using UnityEngine;
using UnityEngine.Serialization;
using Nex.Essentials;
using SignalPolarity = Nex.Essentials.SignalPolarityDetector.SignalPolarity;
using NodeIndex = Nex.Essentials.SimplePose.NodeIndex;
using System;
using Cysharp.Threading.Tasks.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float playerMaxHealth = 100f;
    private float playerHealth;

    [Header("Slash Detectors")]
    [SerializeField] SlashDetector leftSlashDetector = null!;
    [SerializeField] SlashDetector rightSlashDetector = null!;


    [Header("Crouch Detection")]
    [SerializeField] private SignalPolarityDetector verticalSignalDetector = null!;
    private SignalPolarity currentPolarity = SignalPolarity.Neutral;

    [Header("Block Detection")]
    [SerializeField] private BodyPoseController bodyPoseController = null!;

    [Header("Lean Detection")]
    [SerializeField] private AngularSignalProducer leanSignalProducer = null!;
    [FormerlySerializedAs("signalDetector")][SerializeField] private SignalPolarityDetector leanSignalPolarityDetector = null!;
    private bool playerLeaningLeft = false;
    private bool playerLeaningRight = false;

    [Header("IK Avatar Controller")]
    [SerializeField] private IKAvatarController ikAvatarController = null!;

    private bool gameStarted = false;
    int playerIndex;
    int playerCombo = 0;
    int shieldStacks = 0;

    public Action<int, float> OnPlayerHPChanged;    // int: player index, float: health percentage
    public Action<int> OnPlayerDied;    // int: player index
    public Action<int, Jazz.Handedness, Vector2, int> OnSlashDetected;   // int: player index, Jazz.Handedness: handedness, Vector2: direction, int: combo
    public Action<int, int> OnPlayerComboChanged; // int: player index, int: combo
    public Action<int> OnPlayerAvoidedAttack; // int: player index

    public void Init(int playerIndex)
    {
        this.playerIndex = playerIndex;
        //leftSlashDetector.Init(playerIndex);
        //rightSlashDetector.Init(playerIndex);
        leanSignalProducer.Init(playerIndex);
        ikAvatarController.Init(playerIndex);

        playerHealth = playerMaxHealth;
        OnPlayerHPChanged?.Invoke(playerIndex, playerHealth / playerMaxHealth);

        leanSignalPolarityDetector.SignalStream.Subscribe(HandleLeanSignal, destroyCancellationToken);
        leftSlashDetector.OnSlashDetected += OnLeftSlashDetected;
        rightSlashDetector.OnSlashDetected += OnRightSlashDetected;

        BattleManager.Instance.OnGameStarted += OnGameStarted;
        BattleManager.Instance.OnPlayerAttackSuccess += OnPlayerAttackSuccess;
        //BattleManager.Instance.OnBubbleExpired += OnBubbleExpired;
        EnemyController.Instance.OnEnemyAttack += OnEnemyAttack;

    }

    void OnDestroy()
    {
        leftSlashDetector.OnSlashDetected -= OnLeftSlashDetected;
        rightSlashDetector.OnSlashDetected -= OnRightSlashDetected;

        BattleManager.Instance.OnGameStarted -= OnGameStarted;
        BattleManager.Instance.OnPlayerAttackSuccess -= OnPlayerAttackSuccess;
        //BattleManager.Instance.OnBubbleExpired -= OnBubbleExpired;
        EnemyController.Instance.OnEnemyAttack -= OnEnemyAttack;
    }

    private void Update()
    {
        if (!gameStarted) return;

        SignalPolarity polarity = verticalSignalDetector.Signal;
        if (!IsPlayerCrouching() && polarity == SignalPolarity.Negative)
        {
            // Player started crouching
            currentPolarity = SignalPolarity.Negative;
        }
        else if (IsPlayerCrouching() && polarity == SignalPolarity.Positive)
        {
            // Player stopped crouching
            currentPolarity = SignalPolarity.Neutral;
        }
    }

    void TakeDamage(float damage)
    {
        playerHealth -= damage;
        OnPlayerHPChanged?.Invoke(playerIndex, playerHealth / playerMaxHealth);

        ResetPlayerCombo();

        if (playerHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        UIManager.Instance.ShowGameOverPanel();
        OnPlayerDied?.Invoke(playerIndex);
    }

    #region Slash Detection

    void OnLeftSlashDetected(Vector2 direction)
    {
        SlashDetected(Jazz.Handedness.Left, direction, playerCombo);
    }

    void OnRightSlashDetected(Vector2 direction)
    {
        SlashDetected(Jazz.Handedness.Right, direction, playerCombo);
    }

    void SlashDetected(Jazz.Handedness handedness, Vector2 direction, int combo)
    {
        OnSlashDetected?.Invoke(playerIndex, handedness, direction, combo);
    }

    #endregion


    #region Player Crouch

    public bool IsPlayerCrouching()
    {
        return currentPolarity == SignalPolarity.Negative;
    }

    #endregion

    #region Player Block

    public bool IsPlayerBlockingLeft()
    {
        if (!bodyPoseController.TryGetBodyPose(playerIndex, BodyPoseController.PoseFlavor.Raw, out var bodyPose))
        {
            Debug.LogError("Failed to get body pose.");
            return false;
        }

        var leftHandPosition = bodyPose[NodeIndex.LeftWrist];
        var nosePosition = bodyPose[NodeIndex.Nose];

        if (!leftHandPosition.HasValue || !nosePosition.HasValue)
        {
            Debug.LogWarning("Left hand or nose position not available.");
            return false;
        }

        var left = leftHandPosition.Value;
        var nose = nosePosition.Value;

        // Left block: left hand above the nose and to the left of the nose
        return left.y > nose.y && left.x < nose.x;
    }

    public bool IsPlayerBlockingRight()
    {
        if (!bodyPoseController.TryGetBodyPose(playerIndex, BodyPoseController.PoseFlavor.Raw, out var bodyPose))
        {
            Debug.LogError("Failed to get body pose.");
            return false;
        }

        var rightHandPosition = bodyPose[NodeIndex.RightWrist];
        var nosePosition = bodyPose[NodeIndex.Nose];

        if (!rightHandPosition.HasValue || !nosePosition.HasValue)
        {
            Debug.LogWarning("Right hand or nose position not available.");
            return false;
        }

        var right = rightHandPosition.Value;
        var nose = nosePosition.Value;

        // Right block: right hand above the nose and to the right of the nose
        return right.y > nose.y && right.x > nose.x;
    }

    #endregion

    #region Lean Detection

    private void HandleLeanSignal(SignalPolarity signal)
    {
        playerLeaningLeft = signal == SignalPolarity.Positive;
        playerLeaningRight = signal == SignalPolarity.Negative;
    }

    #endregion

    void OnGameStarted(int playerCount)
    {
        gameStarted = true;
    }

    void OnEnemyAttack(EnemyController.EnemyIncomingAttack attack, float damage)
    {
        bool avoided = false;
        switch (attack)
        {
            case EnemyController.EnemyIncomingAttack.Left:
                avoided = playerLeaningRight;
                break;
            case EnemyController.EnemyIncomingAttack.Right:
                avoided = playerLeaningLeft;
                break;
            default:
                break;
        }

        if (avoided)
        {
            AvoidedAttack();
        }
        else
        {
            if (shieldStacks > 0)
            {
                shieldStacks--;
                AvoidedAttack();
                return;
            }
            TakeDamage(damage);
        }
    }

    void OnPlayerAttackSuccess(int playerIndex, BattleManager.HitType hitType, float damage, EnemyController.PlayerAttackPath path)
    {
        if (this.playerIndex != playerIndex) return;

        AddPlayerCombo();
    }

    void OnBubbleExpired(int playerIndex)
    {
        if (this.playerIndex != playerIndex) return;

        ResetPlayerCombo();
    }

    void AvoidedAttack()
    {
        AddPlayerCombo();
        OnPlayerAvoidedAttack?.Invoke(playerIndex);
    }

    void AddPlayerCombo()
    {
        playerCombo++;
        OnPlayerComboChanged?.Invoke(playerIndex, playerCombo);
    }

    void ResetPlayerCombo()
    {
        playerCombo = 0;
        OnPlayerComboChanged?.Invoke(playerIndex, playerCombo);
    }

    public void AddShield()
    {
        shieldStacks++;
    }

    public void RecoverHP(float amount)
    {
        playerHealth = Mathf.Min(playerHealth + amount, playerMaxHealth);
        OnPlayerHPChanged?.Invoke(playerIndex, playerHealth / playerMaxHealth);
    }

    public void IncreaseMaxHP(float amount)
    {
        playerMaxHealth += amount;
        playerHealth += amount;
        OnPlayerHPChanged?.Invoke(playerIndex, playerHealth / playerMaxHealth);
    }

    public bool IsDamaged()
    {
        return playerHealth < playerMaxHealth;
    }
}
