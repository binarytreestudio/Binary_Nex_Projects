using UnityEngine;
using Nex.Essentials;
using SignalPolarity = Nex.Essentials.SignalPolarityDetector.SignalPolarity;


public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private int playerMaxHealth = 3;
    private float playerHealth;
    public float PlayerHealth => playerHealth;
    [SerializeField] private TMPro.TextMeshProUGUI playerHealthText;

    [Header("Crouch Detection")]
    [SerializeField] private SignalPolarityDetector verticalSignalDetector = null!;
    private SignalPolarity currentPolarity = SignalPolarity.Neutral;

    [Header("Debug")]
    [SerializeField] private TMPro.TextMeshProUGUI debugCrouchText;


    private void Start()
    {
        playerHealth = playerMaxHealth;
        playerHealthText.text = "Player HP: " + playerHealth.ToString("F1");
    }

    private void Update()
    {
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
}
