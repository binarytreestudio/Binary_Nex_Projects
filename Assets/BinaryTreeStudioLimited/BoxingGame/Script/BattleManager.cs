using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Nex.Essentials;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using Jazz;
using System.Collections.Generic;

public class BattleManager : Singleton<BattleManager>
{
    #region Public Enums

    public enum ScoreType
    {
        Good = 100,
        Perfect = 200,
        Finisher = 300,
        Block = 150,
        AdditionalScorePerCombo = 25
    }

    public enum HitType
    {
        Good = 10,
        Perfect = 15,
        Finisher = 30
    }

    #endregion

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
    [SerializeField] private bool skipNexSetup = false;
    [SerializeField] private SetupConfig setupConfig = null!;

    [SerializeField] private MdkController mdkController = null!;
    [SerializeField] private PlayAreaController playAreaController = null!;
    [SerializeField] private BodyPoseController bodyPoseController = null!;
    [SerializeField] private PlayAreaPreviewFrameProvider playAreaPreviewFrameProvider = null!;


    [Header("Motion Detectors")]
    [SerializeField] SlashDetector leftSlashDetector = null!;
    [SerializeField] SlashDetector rightSlashDetector = null!;
    [FormerlySerializedAs("signalDetector")][SerializeField] private SignalPolarityDetector leanSignalPolarityDetector = null!;

    [Header("VFX")]
    [SerializeField] ParticleSystem perfectHitEffect;
    [SerializeField] ParticleSystem missEffect;
    [SerializeField] ParticleSystem goodHitEffect;


    [Header("Settings")]
    [Range(0f, 360f)]
    [SerializeField] private float goodHitAngle = 60f;
    [Range(0f, 360f)]
    [SerializeField] private float perfectHitAngle = 30f;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private int comboThresholdForDamageMultiplier = 9;
    [Range(0f, 1f)]
    [SerializeField] private float comboDamageMultiplier = .5f;

    #endregion

    #region private var
    private bool gameStarted = false;
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

    private bool playerLeaningLeft = false;
    private bool playerLeaningRight = false;
    private int playerScore = 0;

    #endregion

    #region Observer Pattern

    public Action<bool> OnGameStarted;
    public Action<int> OnPlayerComboChanged;
    public Action<int, ScoreType> OnPlayerScoreChanged;
    public Action<HitType, float> OnAttackSuccess;

    #endregion

    #region Start

    void Start()
    {
        mdkController.StartRunning().Forget();

        if (skipNexSetup)
        {
            RunGame();
            return;
        }
        Run(destroyCancellationToken).Forget();
    }

    #endregion

    #region Update

    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameStarted = true;
                OnGameStarted?.Invoke(gameStarted);
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttackSuccess(HitType.Finisher);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            playerLeaningLeft = true;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            playerLeaningRight = true;
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
        OnPlayerComboChanged?.Invoke(playerCombo);
        OnPlayerScoreChanged?.Invoke(playerScore, 0);
        leanSignalPolarityDetector.SignalStream.Subscribe(HandleLeanSignal, destroyCancellationToken);
        UIManager.Instance.ShowStartScreen();
        EnemyController.Instance.OnEnemyAttack += EnemyAttack;
    }

    #endregion

    protected override void OnDestroy()
    {
        leftSlashDetector.OnSlashDetected -= OnLeftSlashDetected;
        rightSlashDetector.OnSlashDetected -= OnRightSlashDetected;
        EnemyController.Instance.OnEnemyAttack -= EnemyAttack;
        base.OnDestroy();
    }

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

        if (!gameStarted)
        {
            if (direction.y < 0)
                return; // Must be upward
            if (angleDegrees < 90 - goodHitAngle || angleDegrees > 90 + goodHitAngle)
                return; // Must be in good angle range
            gameStarted = true;
            OnGameStarted?.Invoke(gameStarted);
            return;
        }

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
                }
                if (direction.x < 0 && direction.y < 0)
                {
                    crossFinisherRightSuccess = true;
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

    #region Lean Detection

    private void HandleLeanSignal(SignalPolarityDetector.SignalPolarity signal)
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            return;
        playerLeaningLeft = signal == SignalPolarityDetector.SignalPolarity.Positive;
        playerLeaningRight = signal == SignalPolarityDetector.SignalPolarity.Negative;
    }

    #endregion

    #region Attack Results

    void AttackSuccess(HitType hitType)
    {
        playerCombo++;

        OnPlayerComboChanged?.Invoke(playerCombo);

        float damage = (int)hitType;
        damage *= playerCombo > comboThresholdForDamageMultiplier / 2 ? (playerCombo > comboThresholdForDamageMultiplier ? 1 + comboDamageMultiplier : 1 + comboDamageMultiplier / 2) : 1f;
        OnAttackSuccess?.Invoke(hitType, damage);

        switch (hitType)
        {
            case HitType.Good:
                // VFX & SFX for good hit
                goodHitEffect.Play();
                AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);

                playerScore += (int)ScoreType.Good + ((int)ScoreType.AdditionalScorePerCombo * playerCombo);
                OnPlayerScoreChanged?.Invoke(playerScore, ScoreType.Good);
                break;
            case HitType.Perfect:
                // VFX & SFX for perfect hit
                perfectHitEffect.Play();
                AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);

                playerScore += (int)ScoreType.Perfect + ((int)ScoreType.AdditionalScorePerCombo * playerCombo);
                OnPlayerScoreChanged?.Invoke(playerScore, ScoreType.Perfect);
                break;
            case HitType.Finisher:
                // VFX & SFX for finisher hit
                perfectHitEffect.Play();
                AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Hit);

                playerScore += (int)ScoreType.Finisher + ((int)ScoreType.AdditionalScorePerCombo * playerCombo);
                OnPlayerScoreChanged?.Invoke(playerScore, ScoreType.Finisher);
                break;
            default:
                break;
        }
    }

    public void AttackFail()
    {
        playerCombo = 0;

        OnPlayerComboChanged?.Invoke(playerCombo);

        AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Miss);
        missEffect.Play();
    }

    #endregion

    #region Enemy Attack

    void EnemyAttack(EnemyController.EnemyIncomingAttack attackPath, float damage)
    {
        switch (attackPath)
        {
            case EnemyController.EnemyIncomingAttack.Left:
                if (playerLeaningRight)
                {
                    PlayerBlockSuccess();
                }
                else
                {
                    PlayerBlockFail(damage);
                }
                break;
            case EnemyController.EnemyIncomingAttack.Right:
                if (playerLeaningLeft)
                {
                    PlayerBlockSuccess();
                }
                else
                {
                    PlayerBlockFail(damage);
                }
                break;
            default:
                break;
        }
        enemyAttacking = false;
    }

    void PlayerBlockSuccess()
    {
        playerCombo++;
        playerScore += (int)ScoreType.Block + ((int)ScoreType.AdditionalScorePerCombo * playerCombo);

        OnPlayerComboChanged?.Invoke(playerCombo);
        OnPlayerScoreChanged?.Invoke(playerScore, ScoreType.Block);

        //audio & vfx
    }

    void PlayerBlockFail(float damage)
    {
        playerCombo = 0;
        PlayerController.Instance.TakeDamage(damage);

        OnPlayerComboChanged?.Invoke(playerCombo);

        //audio & vfx
    }

    #endregion
}
