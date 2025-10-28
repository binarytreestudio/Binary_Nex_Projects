using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nex.Essentials;
using UnityEngine;
using TMPro;
using Jazz;

public class BattleManager : Singleton<BattleManager>
{
    #region Public var

    [Serializable]
    private class SetupConfig
    {
        public GameObject setupPanel = null!;
        public TMP_Text setupInstruction = null!;
        public GameObject setupPrefab = null!;
        public float setupPrefabYPosition = 432;
        public float canvasWidth = 1920;
    }
    [Header("Nex Setup")]
    [SerializeField] private SetupConfig setupConfig = null!;

    [SerializeField] private MdkController mdkController = null!;
    [SerializeField] private PlayAreaController playAreaController = null!;
    [SerializeField] private BodyPoseController bodyPoseController = null!;
    [SerializeField] private PlayAreaPreviewFrameProvider playAreaPreviewFrameProvider = null!;


    [Header("Slash Detectors")]
    [SerializeField] SlashDetector leftSlashDetector = null!;
    [SerializeField] SlashDetector rightSlashDetector = null!;

    [Header("UI Elements")]
    [SerializeField] BoxingEnemyAttackIndicator upSideIncomingAttackIndicator;
    [SerializeField] TMPro.TextMeshProUGUI comboText;
    [SerializeField] Color successColor = Color.yellow;
    [SerializeField] Color incomingAttackColor = Color.red;
    [SerializeField] private TMPro.TextMeshProUGUI comboTimerText;

    [Header("VFX")]
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] ParticleSystem missEffect;

    [Header("Settings")]
    [Tooltip("Percentage chance of player attack instead of enemy attack")]
    public int playerAttackChance = 70;
    [SerializeField] private float slashDirectionThreshold = 0.5f;
    [SerializeField] private float comboLastDuration = 2.0f;
    [SerializeField] private float crossFinisherBufferTime = 1;
    [SerializeField] private EnemyController enemyController;

    #endregion

    #region private var
    int playerCombo = 0;
    private float comboTimer = 0.0f;
    [HideInInspector] public bool enemyAttacking = false;
    private bool crossFinisherLeftSuccess = false;
    private bool crossFinisherRightSuccess = false;
    private float crossFinisherTimer;

    #endregion

    #region Start

    void Start()
    {
        mdkController.StartRunning().Forget();

        Run(destroyCancellationToken).Forget();
    }

    #endregion

    #region Update

    void Update()
    {
        if (playerCombo > 0 && !enemyAttacking)
        {
            comboTimerText.text = "Timer: " + comboTimer.ToString("F2") + "s";
            if (comboTimer > 0)
            {
                comboTimer -= Time.deltaTime;
            }
            else
            {
                playerCombo = 0;
                comboText.text = "Combo: " + playerCombo;
                comboText.color = incomingAttackColor;
                comboTimerText.text = "";
            }
        }

        if (crossFinisherLeftSuccess != crossFinisherRightSuccess)
        {
            if (crossFinisherTimer > 0)
            {
                crossFinisherTimer -= Time.deltaTime;
            }
            else
            {
                crossFinisherLeftSuccess = false;
                crossFinisherRightSuccess = false;
                AttackFail();
                AttackPathManager.Instance.ShowAttackIndicator(EnemyController.AttackPath.CrossFinisher);
            }
        }
    }

    #endregion

    #region Async Methods
    private async UniTaskVoid Run(CancellationToken cancellationToken)
    {
        mdkController.DewarpLocked = false;
        mdkController.EnableConsistency = false;
        playAreaController.Locked = false;
        await RunSetup(cancellationToken);

        mdkController.DewarpLocked = true;
        mdkController.EnableConsistency = true;
        playAreaController.Locked = true;
        RunGame();
    }


    private async UniTask RunSetup(CancellationToken cancellationToken)
    {
        setupConfig.setupPanel.SetActive(true);

        // For each player, we want to figure out if they are holding up one of their hands or not.
        // And which hand they are holding up.
        // If both are raised, default to right hand.
        const int playerCount = 1;
        var playerSetupDetectors = new OnePlayerSetupDetector[playerCount];
        for (var playerIndex = 0; playerIndex < playerCount; ++playerIndex)
        {
            var playerPosition = playAreaController.PlayerPositions[playerIndex];
            var framePosition = new Vector2(playerPosition * setupConfig.canvasWidth,
                setupConfig.setupPrefabYPosition);

            // Instantiate the player setup detector prefab inside the setup panel
            var obj = Instantiate(setupConfig.setupPrefab, setupConfig.setupPanel.transform, false);

            // Initialize setup controller, pass controllers and params dynamically
            var setupController = obj.GetComponent<OnePlayerSetupDetector>();
            setupController.Initialize(playAreaController, bodyPoseController, playerIndex, framePosition);
            playerSetupDetectors[playerIndex] = setupController;

            // Initialize preview frame provider
            var provider = obj.GetComponent<PlayAreaMaskedPreviewFrameProvider>();
            provider.Initialize(playAreaPreviewFrameProvider, playerPosition);
        }

        await UniTask.WhenAll(playerSetupDetectors.Select(detector =>
            detector.WaitUntilIsReady(cancellationToken)));

        setupConfig.setupPanel.SetActive(false);
    }

    void RunGame()
    {
        leftSlashDetector.OnSlashDetected += OnLeftSlashDetected;
        rightSlashDetector.OnSlashDetected += OnRightSlashDetected;
        enemyController.playerAttackPath = (EnemyController.AttackPath)UnityEngine.Random.Range(1, Enum.GetValues(typeof(EnemyController.AttackPath)).Length);
        AttackPathManager.Instance.ShowAttackIndicator(enemyController.playerAttackPath);
        comboText.text = "Combo: " + playerCombo;
        comboTimerText.text = "";
    }

    #endregion

    #region Slash Detection

    void OnLeftSlashDetected(Vector2 direction)
    {
        SlashDetected(Handedness.Left, direction);
    }

    void OnRightSlashDetected(Vector2 direction)
    {
        SlashDetected(Handedness.Right, direction);
    }

    void SlashDetected(Handedness handedness, Vector2 direction)
    {
        Debug.Log(direction.normalized.x + ", " + direction.normalized.y);
        switch (enemyController.playerAttackPath)
        {
            case EnemyController.AttackPath.LeftHook:
                if (handedness != Handedness.Left)
                    break; // Must be left hand
                if (Mathf.Abs(direction.normalized.x) < slashDirectionThreshold)
                    break; // Must be mostly horizontal
                if (direction.x < 0)
                    break; // Left hand must move right to perform left hook
                AttackSuccess();
                return;
            case EnemyController.AttackPath.RightHook:
                if (handedness != Handedness.Right)
                    break; // Must be right hand
                if (Mathf.Abs(direction.normalized.x) < slashDirectionThreshold)
                    break; // Must be mostly horizontal
                if (direction.x > 0)
                    break; // Right hand must move left to perform right hook
                AttackSuccess();
                return;
            case EnemyController.AttackPath.Uppercut:
                if (direction.y < 0)
                    break; // Must be upward
                if (Mathf.Abs(direction.normalized.y) < slashDirectionThreshold)
                    break; // Must be mostly vertical
                AttackSuccess();
                return;
            case EnemyController.AttackPath.CrossFinisher:
                if (direction.x > 0 && direction.y < 0)
                {
                    crossFinisherLeftSuccess = true;
                    AttackPathManager.Instance.HideAttackIndicator(EnemyController.AttackPath.CrossFinisher, Handedness.Left);
                    crossFinisherTimer = crossFinisherBufferTime;
                }
                if (direction.x < 0 && direction.y < 0)
                {
                    crossFinisherRightSuccess = true;
                    AttackPathManager.Instance.HideAttackIndicator(EnemyController.AttackPath.CrossFinisher, Handedness.Right);
                    crossFinisherTimer = crossFinisherBufferTime;
                }
                if (crossFinisherLeftSuccess && crossFinisherRightSuccess)
                {
                    crossFinisherLeftSuccess = false;
                    crossFinisherRightSuccess = false;
                    AttackSuccess();
                }
                return;
            default:
                return;
        }
        AttackFail();
    }

    #endregion

    #region Attack Results

    void AttackSuccess()
    {
        comboTimer = comboLastDuration;
        playerCombo++;

        AttackPathManager.Instance.HideAllIndicators();
        comboText.text = "Combo: " + playerCombo;
        comboText.color = successColor;

        AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);
        hitEffect.Play();

        enemyController.TakeDamage(10 + playerCombo * 2 - 2);
    }

    void AttackFail()
    {
        playerCombo = 0;
        comboText.text = "Combo: " + playerCombo;
        comboText.color = incomingAttackColor;

        AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Miss);
        missEffect.Play();
    }

    #endregion

    #region Enemy Attack

    public void EnemyUpSideAttack()
    {
        if (PlayerController.Instance.IsPlayerCrouching())
        {
            playerCombo++;
            AttackPathManager.Instance.HideAllIndicators();
            comboText.text = "Combo: " + playerCombo;
            comboText.color = successColor;

            AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Miss);
            missEffect.Play();
        }
        else
        {
            playerCombo = 0;
            comboText.text = "Combo: " + playerCombo;
            comboText.color = incomingAttackColor;

            AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);
            hitEffect.Play();
            PlayerController.Instance.TakeDamage(1);
        }
        enemyAttacking = false;
        enemyController.EnemyRandom();
    }

    #endregion



    protected override void OnDestroy()
    {
        leftSlashDetector.OnSlashDetected -= OnLeftSlashDetected;
        rightSlashDetector.OnSlashDetected -= OnRightSlashDetected;
        base.OnDestroy();
    }
}
