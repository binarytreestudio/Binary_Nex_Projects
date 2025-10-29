using UnityEngine;
using Nex.Essentials;
using SignalPolarity = Nex.Essentials.SignalPolarityDetector.SignalPolarity;
using NodeIndex = Nex.Essentials.SimplePose.NodeIndex;

public class PlayerController : Singleton<PlayerController>
{
    [Header("Health")]
    [SerializeField] private int playerMaxHealth = 3;
    private float playerHealth;
    public float PlayerHealth => playerHealth;
    [SerializeField] private TMPro.TextMeshProUGUI playerHealthText;

    [Header("Crouch Detection")]
    [SerializeField] private SignalPolarityDetector verticalSignalDetector = null!;
    private SignalPolarity currentPolarity = SignalPolarity.Neutral;

    [Header("Block Detection")]
    [SerializeField] private BodyPoseController bodyPoseController = null!;

    [Header("Debug")]
    [SerializeField] private TMPro.TextMeshProUGUI debugCrouchText;
    [SerializeField] private TMPro.TextMeshProUGUI debugBlockText;

    private bool gameStarted = false;


    private void Start()
    {
        playerHealth = playerMaxHealth;
        playerHealthText.text = "Player HP: " + playerHealth.ToString("F1");
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
        debugCrouchText.text = "verticalSignalDetector.Signal: " + polarity.ToString();
        debugCrouchText.text += "\nIs Crouching: " + IsPlayerCrouching().ToString();

        debugBlockText.text = "Is Blocking Left: " + IsPlayerBlockingLeft().ToString();
        debugBlockText.text += "\nIs Blocking Right: " + IsPlayerBlockingRight().ToString();
    }

    public void TakeDamage(int damage)
    {
        playerHealth -= damage;
        if (playerHealth > 0)
        {
            playerHealthText.text = "Player HP: " + playerHealth.ToString("F1");
        }
        else
        {
            Die();

            //prototype infinite health
            playerHealth = playerMaxHealth;
            playerHealthText.text = "Player HP: " + playerHealth.ToString("F1");
            playerHealthText.text += "\nPlayer Defeated!";
        }
    }

    void Die()
    {

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
