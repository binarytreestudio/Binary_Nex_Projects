using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PowerUpItemController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image punchIcon;
    [SerializeField] private TextMeshProUGUI descriptionText;


    [SerializeField] private GameObject chooseParticleEffect;

    PowerUpDatabase.PowerUpType currentPowerUpType;

    public void Init(PowerUpDatabase.PowerUpType powerUpType)
    {
        currentPowerUpType = powerUpType;
        var powerUpData = DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(powerUpType);
        nameText.text = powerUpData.powerUpName;
        iconImage.sprite = powerUpData.icon;
        descriptionText.text = powerUpData.description;
        punchIcon.gameObject.SetActive(false);

    }

    public void ApplyPowerUp(int playerIndex)
    {
        PlayerManager.Instance.ApplyPowerUp(playerIndex, currentPowerUpType);

        UIManager.Instance.HidePowerUpPanel();
        Instantiate(chooseParticleEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayCollectPowerup();
    }
}

