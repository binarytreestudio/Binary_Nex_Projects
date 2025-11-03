using UnityEngine;
using Nex.Essentials;
using SignalPolarity = Nex.Essentials.SignalPolarityDetector.SignalPolarity;
using NodeIndex = Nex.Essentials.SimplePose.NodeIndex;
using System;

public class PlayerController : Singleton<PlayerController>
{
    [Header("Health")]
    [SerializeField] private int playerMaxHealth = 3;
    private int playerHealth;

    [Header("Crouch Detection")]
    [SerializeField] private SignalPolarityDetector verticalSignalDetector = null!;
    private SignalPolarity currentPolarity = SignalPolarity.Neutral;

    [Header("Block Detection")]
    [SerializeField] private BodyPoseController bodyPoseController = null!;

    private bool gameStarted = false;

    public Action<int> OnPlayerHPChanged;
    public Action OnPlayerDied;


    private void Start()
    {
        playerHealth = playerMaxHealth;
        OnPlayerHPChanged?.Invoke(playerHealth);
        BattleManager.Instance.OnGameStarted += OnGameStarted;
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

    public void TakeDamage(int damage)
    {
        playerHealth -= damage;
        OnPlayerHPChanged?.Invoke(playerHealth);
        if (playerHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        UIManager.Instance.ShowGameOverPanel();
        OnPlayerDied?.Invoke();
    }

    #region Player Crouch

    public bool IsPlayerCrouching()
    {
        return currentPolarity == SignalPolarity.Negative;
    }

    #endregion

    #region Player Block

    public bool IsPlayerBlockingLeft()
    {
        if (!bodyPoseController.TryGetBodyPose(0, BodyPoseController.PoseFlavor.Raw, out var bodyPose))
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
        if (!bodyPoseController.TryGetBodyPose(0, BodyPoseController.PoseFlavor.Raw, out var bodyPose))
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

    void OnGameStarted(bool started)
    {
        gameStarted = started;
    }

    protected override void OnDestroy()
    {
        BattleManager.Instance.OnGameStarted -= OnGameStarted;
        base.OnDestroy();
    }
}
