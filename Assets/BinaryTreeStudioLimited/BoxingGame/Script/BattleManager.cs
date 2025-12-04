using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Nex.Essentials;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using Jazz;

public class BattleManager : Singleton<BattleManager>
{
    public struct Bubble
    {
        public EnemyController.PlayerAttackPath path;
        public float spawnTime;
        public float duration;
    }

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
    //[SerializeField] private bool skipNexSetup = false;
    [SerializeField] private SetupConfig setupConfig = null!;
    [SerializeField] private MdkController mdkController = null!;
    [SerializeField] private PlayAreaController playAreaController = null!;
    [SerializeField] private BodyPoseController bodyPoseController = null!;
    [SerializeField] private PlayAreaPreviewFrameProvider playAreaPreviewFrameProvider = null!;

    [Serializable]
    private class MenuScreenConfig
    {
        public GameObject menuPanel = null!;
        public Button singlePlayerButton = null!;
        public Button twoPlayerButton = null!;
    }

    [Header("Player Config")]
    [SerializeField] private MenuScreenConfig menuScreenConfig = null!;
    [Serializable]
    private class PlayerConfig
    {
        public float[] singlePlayerPosition = { 0.5f };
        public float[] twoPlayerPositions = { 0.3f, 0.6f };
    }
    [SerializeField] private PlayerConfig playerPositionsConfig = null!;
    [SerializeField] private GameObject playerPrefab;


    [Header("VFX")]
    [SerializeField] ParticleSystem perfectHitEffect;
    [SerializeField] ParticleSystem missEffect;
    [SerializeField] ParticleSystem goodHitEffect;


    [Header("Settings")]
    [Range(0f, 360f)]
    [SerializeField] private float goodHitAngle = 60f;
    [Range(0f, 360f)]
    [SerializeField] private float perfectHitAngle = 30f;
    [SerializeField] private int comboThresholdForDamageMultiplier = 9;
    [Range(0f, 1f)]
    [SerializeField] private float comboDamageMultiplier = .5f;

    #endregion

    #region private var
    private bool gameStarted = false;
    public bool GameStarted => gameStarted;
    [HideInInspector] public bool enemyAttacking = false;
    private bool crossFinisherLeftSuccess = false;
    private bool crossFinisherRightSuccess = false;
    [Serializable]
    private class HitAngleMapping
    {
        public EnemyController.PlayerAttackPath attackPath;
        [Range(0f, 360f)]
        public float angle;
    }
    [SerializeField] private List<HitAngleMapping> hitAngleMappings = new List<HitAngleMapping>();

    private int playerScore = 0;
    private int playerCount;
    private List<Bubble> createdBubbles = new();

    private List<int> playerHitCounter = new();


    #endregion

    #region Observer Pattern

    public Action<int> OnGameStarted;   // int: player count
    public Action<int, ScoreType> OnPlayerScoreChanged;  // int: score, ScoreType: score type
    public Action<int, HitType, float, EnemyController.PlayerAttackPath> OnPlayerAttackSuccess;    // int: player index, HitType: hit type, float: damage, EnemyController.PlayerAttackPath: attack path
    public Action<EnemyController.PlayerAttackPath> OnBubbleExpired;    // EnemyController.PlayerAttackPath: bubble path

    #endregion

    #region Start

    void Start()
    {
        mdkController.StartRunning().Forget();

        //if (skipNexSetup)
        //{
        //    RunGame();
        //    return;
        //}

        Run(destroyCancellationToken).Forget();
    }

    #endregion

    #region Update

    void Update()
    {
        if (!gameStarted)
        {
            return;
        }
        if (createdBubbles.Count > 0)
        {
            Bubble[] bubblesArray = createdBubbles.ToArray();
            foreach (var bubble in bubblesArray)
            {
                if (bubble.duration <= 0f)
                {
                    continue;
                }
                if (Time.time - bubble.spawnTime > bubble.duration && bubble.duration > 0)
                {
                    OnBubbleExpired?.Invoke(bubble.path);
                    createdBubbles.Remove(bubble);
                }
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

        await RunMenu(cancellationToken);

        await RunSetup(cancellationToken);

        mdkController.DewarpLocked = true;
        mdkController.EnableConsistency = true;
        playAreaController.Locked = true;
        RunGame();
    }

    /// Choose the number of players and update the PlayAreaController
    private async UniTask RunMenu(CancellationToken cancellationToken)
    {
        menuScreenConfig.menuPanel.SetActive(true);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var onePlayerButton = menuScreenConfig.singlePlayerButton.OnClickAsync(linkedCts.Token);
        var twoPlayerButton = menuScreenConfig.twoPlayerButton.OnClickAsync(linkedCts.Token);

        var task = await UniTask.WhenAny(onePlayerButton, twoPlayerButton);

        // One task finished, cancel others
        linkedCts.Cancel();

        // Set the number of players based on the button/key input
        switch (task)
        {
            case 0:
                playerCount = 1;
                playAreaController.PlayerPositions = playerPositionsConfig.singlePlayerPosition;
                break;

            case 1:
                playerCount = 2;
                playAreaController.PlayerPositions = playerPositionsConfig.twoPlayerPositions;
                break;
        }

        menuScreenConfig.menuPanel.SetActive(false);
    }


    private async UniTask RunSetup(CancellationToken cancellationToken)
    {
        // Now we enter setup state.
        // Switch the instructions depending on the number of players
        setupConfig.setupInstruction.text = playerCount switch
        {
            1 => "Stand in the middle and raise your hand to start.",
            2 => "Stand side by side and raise your hand to start.",
            _ => setupConfig.setupInstruction.text
        };

        setupConfig.setupPanel.SetActive(true);

        // For each player, we want to figure out if they are holding up one of their hands or not.
        // And which hand they are holding up.
        // If both are raised, default to right hand.
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
            //setupController.Initialize(playAreaController, bodyPoseController, playerIndex, framePosition);
            playerSetupDetectors[playerIndex] = setupController;

            // Initialize preview frame provider
            var provider = obj.GetComponent<PlayAreaMaskedPreviewFrameProvider>();
            provider.Initialize(playAreaPreviewFrameProvider, playerPosition);
        }

        for (int i = 0; i < playerCount; i++)
        {
            var playerObj = Instantiate(playerPrefab);
            var playerController = playerObj.GetComponent<PlayerController>();
            PlayerManager.Instance.RegisterPlayerController(playerController);

            playerHitCounter.Add(0);
        }

        await UniTask.WhenAll(playerSetupDetectors.Select(detector =>
            detector.WaitUntilIsReady(cancellationToken)));

        setupConfig.setupPanel.SetActive(false);
    }

    void RunGame()
    {
        OnPlayerScoreChanged?.Invoke(playerScore, 0);

        EnemyController.Instance.OnEnemyCreateBubble += OnEnemyCreateBubble;

        PlayerManager.Instance.OnPlayerSlashDetected += OnPlayerSlashDetected;
        PlayerManager.Instance.OnPlayerAvoidedAttack += OnPlayerAvoidedAttack;

        gameStarted = true;
        OnGameStarted?.Invoke(playerCount);
    }

    #endregion

    protected override void OnDestroy()
    {
        PlayerManager.Instance.OnPlayerSlashDetected -= OnPlayerSlashDetected;
        PlayerManager.Instance.OnPlayerAvoidedAttack -= OnPlayerAvoidedAttack;

        EnemyController.Instance.OnEnemyCreateBubble -= OnEnemyCreateBubble;

        base.OnDestroy();
    }

    void OnPlayerSlashDetected(int playerIndex, Handedness handedness, Vector2 direction, int combo)
    {
        if (createdBubbles.Count == 0)
            return;
        float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angleDegrees = (angleDegrees + 360) % 360;
        Debug.Log("player " + playerIndex + ": Slash angle: " + angleDegrees);
        foreach (var bubble in createdBubbles)
        {
            var hitAngle = hitAngleMappings.Find(mapping => mapping?.attackPath == bubble.path)?.angle ?? 0f;
            switch (bubble.path)
            {
                case EnemyController.PlayerAttackPath.LeftHook:
                    if (handedness != Handedness.Left)
                        break; // Must be left hand
                    if (direction.x < 0)
                        break; // Left hand must move right to perform left hook
                    if (angleDegrees < hitAngle - goodHitAngle || angleDegrees > hitAngle + goodHitAngle)
                        break; // Must be in good angle range
                    if (angleDegrees > hitAngle - perfectHitAngle && angleDegrees < hitAngle + perfectHitAngle)
                    {
                        AttackSuccess(playerIndex, HitType.Perfect, combo, bubble.path);    // Perfect hit
                        return;
                    }
                    AttackSuccess(playerIndex, HitType.Good, combo, bubble.path);    // Good hit
                    return;
                case EnemyController.PlayerAttackPath.RightHook:
                    if (handedness != Handedness.Right)
                        break; // Must be right hand
                    if (direction.x > 0)
                        break; // Right hand must move left to perform right hook
                    if (angleDegrees < hitAngle - goodHitAngle || angleDegrees > hitAngle + goodHitAngle)
                        break; // Must be in good angle range
                    if (angleDegrees > hitAngle - perfectHitAngle && angleDegrees < hitAngle + perfectHitAngle)
                    {
                        AttackSuccess(playerIndex, HitType.Perfect, combo, bubble.path);    // Perfect hit
                        return;
                    }
                    AttackSuccess(playerIndex, HitType.Good, combo, bubble.path);    // Good hit
                    return;
                case EnemyController.PlayerAttackPath.Uppercut:
                    if (direction.y < 0)
                        break; // Must be upward
                    if (angleDegrees < hitAngle - goodHitAngle || angleDegrees > hitAngle + goodHitAngle)
                        break; // Must be in good angle range
                    if (angleDegrees > hitAngle - perfectHitAngle && angleDegrees < hitAngle + perfectHitAngle)
                    {
                        AttackSuccess(playerIndex, HitType.Perfect, combo, bubble.path);    // Perfect hit
                        return;
                    }
                    AttackSuccess(playerIndex, HitType.Good, combo, bubble.path);    // Good hit
                    return;
                case EnemyController.PlayerAttackPath.CrossFinisher:
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
                        AttackSuccess(playerIndex, HitType.Finisher, combo, bubble.path);
                    }
                    return;
                default:
                    return;
            }
        }
    }

    #region Attack Results

    void AttackSuccess(int playerIndex, HitType hitType, int playerCombo, EnemyController.PlayerAttackPath path)
    {
        float damage = (int)hitType;
        var playerPowerUps = PowerUpManager.Instance.GetPlayerPowerUps(playerIndex);

        // Apply Strength Power-Up effect
        int strengthIndex = playerPowerUps.appliedPowerUps.FindIndex(power => power.powerUpType == PowerUpDatabase.PowerUpType.Strength);
        int strengthStackCount = 0;
        if (strengthIndex >= 0)
        {
            var strengthPowerUp = playerPowerUps.appliedPowerUps[strengthIndex];
            Debug.Log("Player " + playerIndex + " has Strength Power-Up with stack count: " + strengthPowerUp.stackCount);
            strengthStackCount = strengthPowerUp.stackCount;
        }
        damage += strengthStackCount * 5;

        // Apply Combo Damage Multiplier
        damage *= playerCombo > comboThresholdForDamageMultiplier / 2 ? (playerCombo > comboThresholdForDamageMultiplier ? 1 + comboDamageMultiplier : 1 + comboDamageMultiplier / 2) : 1f;

        // Apply Double Damage Power-Up effect
        int doubleDamageIndex = playerPowerUps.appliedPowerUps.FindIndex(power => power.powerUpType == PowerUpDatabase.PowerUpType.DoubleDamage);
        if (doubleDamageIndex >= 0)
        {
            playerHitCounter[playerIndex]++;
            if (playerHitCounter[playerIndex] >= 5)
            {
                damage *= 2;
                playerHitCounter[playerIndex] = 0;
            }
        }

        var hitBubble = createdBubbles.Find(bubble => bubble.path == path);
        createdBubbles.Remove(hitBubble);
        OnPlayerAttackSuccess?.Invoke(playerIndex, hitType, damage, path);

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

    //public void AttackFail()
    //{
    //    playerCombo = 0;
    //
    //    OnPlayerComboChanged?.Invoke(playerCombo);
    //
    //    AudioManager.Instance.PlayAudio(AudioManager.SFXAudioType.Miss);
    //    missEffect.Play();
    //}

    #endregion

    #region Enemy Attack

    //void EnemyAttack(EnemyController.EnemyIncomingAttack attackPath, float damage)
    //{
    //    switch (attackPath)
    //    {
    //        case EnemyController.EnemyIncomingAttack.Left:
    //            if (playerLeaningRight)
    //            {
    //                PlayerBlockSuccess();
    //            }
    //            else
    //            {
    //                PlayerBlockFail(damage);
    //            }
    //            break;
    //        case EnemyController.EnemyIncomingAttack.Right:
    //            if (playerLeaningLeft)
    //            {
    //                PlayerBlockSuccess();
    //            }
    //            else
    //            {
    //                PlayerBlockFail(damage);
    //            }
    //            break;
    //        default:
    //            break;
    //    }
    //    enemyAttacking = false;
    //}

    void OnPlayerAvoidedAttack(int playerIndex)
    {
        playerScore += (int)ScoreType.Block;

        OnPlayerScoreChanged?.Invoke(playerScore, ScoreType.Block);

        //audio & vfx
    }

    #endregion

    private void OnEnemyCreateBubble(EnemyController.PlayerAttackPath path, float duration)
    {
        createdBubbles.Add(new Bubble { path = path, spawnTime = Time.time, duration = duration });
    }
}
