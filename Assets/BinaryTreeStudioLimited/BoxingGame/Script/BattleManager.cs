using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nex.Essentials;
using UnityEngine;
using TMPro;
using Jazz;
using System.Collections.Generic;

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
    [SerializeField] EnemyAttackIndicatorManager upSideIncomingAttackIndicator;
    [SerializeField] TMPro.TextMeshProUGUI comboText;
    [SerializeField] Color successColor = Color.yellow;
    [SerializeField] Color incomingAttackColor = Color.red;

    [Header("VFX")]
    [SerializeField] ParticleSystem perfectHitEffect;
    [SerializeField] ParticleSystem missEffect;
    [SerializeField] ParticleSystem goodHitEffect;


    [Header("Settings")]
    [Tooltip("Percentage chance of player attack instead of enemy attack")]
    public int playerAttackChance = 70;
    [Range(0f, 360f)]
    [SerializeField] private float goodHitAngle = 60f;
    [Range(0f, 360f)]
    [SerializeField] private float perfectHitAngle = 30f;
    [SerializeField] private float crossFinisherBufferTime = 1;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private float goodHitDamage = 10;
    [SerializeField] private float perfectHitDamage = 15;
    [SerializeField] private float finisherHitDamage = 30;
    [SerializeField] private float comboDamageMultiplier = 2;

    #endregion

    #region private var
    int playerCombo = 0;
    [HideInInspector] public bool enemyAttacking = false;
    private bool crossFinisherLeftSuccess = false;
    private bool crossFinisherRightSuccess = false;
    [Serializable]
    private class HitAngleMapping
    {
        public EnemyController.AttackPath attackPath;
        [Range(0f, 360f)]
        public float angle;
    }
    [SerializeField] private List<HitAngleMapping> hitAngleMappings = new List<HitAngleMapping>();

    private enum HitType
    {
        Good,
        Perfect,
        Finisher
    }

    #endregion

    #region Observer Pattern

    private bool gameStarted = false;
    public Action<bool> OnGameStarted;

    #endregion

    #region Start

    void Start()
    {
        mdkController.StartRunning().Forget();

        Run(destroyCancellationToken).Forget();
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
        comboText.text = "Combo: " + playerCombo;
        gameStarted = true;
        OnGameStarted?.Invoke(gameStarted);
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
        float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angleDegrees = (angleDegrees + 360) % 360;
        var hitAngle = hitAngleMappings.Find(mapping => mapping?.attackPath == enemyController.playerAttackPath)?.angle ?? 0f;
        switch (enemyController.playerAttackPath)
        {
            case EnemyController.AttackPath.LeftHook:
                if (handedness != Handedness.Left)
                    break; // Must be left hand
                if (direction.x < 0)
                    break; // Left hand must move right to perform left hook
                if (angleDegrees < hitAngle - goodHitAngle || angleDegrees > hitAngle + goodHitAngle)
                    break; // Must be in good angle range
                if (angleDegrees > hitAngle - perfectHitAngle && angleDegrees < hitAngle + perfectHitAngle)
                {
                    AttackSuccess(HitType.Perfect);    // Perfect hit
                    return;
                }
                AttackSuccess(HitType.Good);    // Good hit
                return;
            case EnemyController.AttackPath.RightHook:
                if (handedness != Handedness.Right)
                    break; // Must be right hand
                if (direction.x > 0)
                    break; // Right hand must move left to perform right hook
                if (angleDegrees < hitAngle - goodHitAngle || angleDegrees > hitAngle + goodHitAngle)
                    break; // Must be in good angle range
                if (angleDegrees > hitAngle - perfectHitAngle && angleDegrees < hitAngle + perfectHitAngle)
                {
                    AttackSuccess(HitType.Perfect);    // Perfect hit
                    return;
                }
                AttackSuccess(HitType.Good);    // Good hit
                return;
            case EnemyController.AttackPath.Uppercut:
                if (direction.y < 0)
                    break; // Must be upward
                if (angleDegrees < hitAngle - goodHitAngle || angleDegrees > hitAngle + goodHitAngle)
                    break; // Must be in good angle range
                if (angleDegrees > hitAngle - perfectHitAngle && angleDegrees < hitAngle + perfectHitAngle)
                {
                    AttackSuccess(HitType.Perfect);    // Perfect hit
                    return;
                }
                AttackSuccess(HitType.Good);    // Good hit
                return;
            case EnemyController.AttackPath.CrossFinisher:
                if (direction.x > 0 && direction.y < 0)
                {
                    crossFinisherLeftSuccess = true;
                    AttackPathIndicatorManager.Instance.HideAttackIndicator(EnemyController.AttackPath.CrossFinisher, Handedness.Left);
                }
                if (direction.x < 0 && direction.y < 0)
                {
                    crossFinisherRightSuccess = true;
                    AttackPathIndicatorManager.Instance.HideAttackIndicator(EnemyController.AttackPath.CrossFinisher, Handedness.Right);
                }
                if (crossFinisherLeftSuccess && crossFinisherRightSuccess)
                {
                    crossFinisherLeftSuccess = false;
                    crossFinisherRightSuccess = false;
                    AttackSuccess(HitType.Finisher);
                }
                return;
            default:
                return;
        }
        //AttackFail();
    }

    #endregion

    #region Attack Results

    void AttackSuccess(HitType hitType)
    {
        playerCombo++;

        AttackPathIndicatorManager.Instance.HideAllIndicators();
        comboText.text = "Combo: " + playerCombo;
        comboText.color = successColor;

        switch (hitType)
        {
            case HitType.Good:
                enemyController.TakeDamage(goodHitDamage + playerCombo * comboDamageMultiplier - comboDamageMultiplier);
                // VFX & SFX for good hit
                goodHitEffect.Play();
                AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);
                break;
            case HitType.Perfect:
                enemyController.TakeDamage(perfectHitDamage + playerCombo * comboDamageMultiplier - comboDamageMultiplier);
                // VFX & SFX for perfect hit
                perfectHitEffect.Play();
                AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);
                break;
            case HitType.Finisher:
                enemyController.TakeDamage(finisherHitDamage + playerCombo * comboDamageMultiplier - comboDamageMultiplier);
                // VFX & SFX for finisher hit
                perfectHitEffect.Play();
                AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);
                break;
            default:
                break;
        }
    }

    public void AttackFail()
    {
        playerCombo = 0;
        comboText.text = "Combo: " + playerCombo;
        comboText.color = incomingAttackColor;

        AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Miss);
        missEffect.Play();
    }

    #endregion

    #region Enemy Attack

    public void EnemyAttack(EnemyController.EnemyIncomingAttack attackPath)
    {
        switch (attackPath)
        {
            case EnemyController.EnemyIncomingAttack.Up:
                if (PlayerController.Instance.IsPlayerBlockingLeft() && PlayerController.Instance.IsPlayerBlockingRight())
                    PlayerBlockSuccess();
                else
                    PlayerBlockFail();
                break;
            default:
                break;
        }
        enemyAttacking = false;
    }

    void PlayerBlockSuccess()
    {
        playerCombo++;
        AttackPathIndicatorManager.Instance.HideAllIndicators();
        comboText.text = "Combo: " + playerCombo;
        comboText.color = successColor;

        //audio & vfx
    }

    void PlayerBlockFail()
    {
        playerCombo = 0;
        comboText.text = "Combo: " + playerCombo;
        comboText.color = incomingAttackColor;
        PlayerController.Instance.TakeDamage(1);

        //audio & vfx
    }

    #endregion



    protected override void OnDestroy()
    {
        leftSlashDetector.OnSlashDetected -= OnLeftSlashDetected;
        rightSlashDetector.OnSlashDetected -= OnRightSlashDetected;
        base.OnDestroy();
    }
}
