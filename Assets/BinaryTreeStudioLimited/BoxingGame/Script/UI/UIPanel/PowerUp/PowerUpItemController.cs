using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpItemController : MonoBehaviour
{
    [SerializeField] private PowerUpDatabase powerUpDatabase;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    PowerUpDatabase.PowerUpType currentPowerUpType;

    public void Init(PowerUpDatabase.PowerUpType powerUpType)
    {
        currentPowerUpType = powerUpType;
        var powerUpData = powerUpDatabase.GetPowerUpData(powerUpType);
        nameText.text = powerUpData.powerUpName;
        iconImage.sprite = powerUpData.icon;
        descriptionText.text = powerUpData.description;
    }

    public void OnClickPowerUp()
    {
        PowerUpManager.Instance.ApplyPowerUp(0, currentPowerUpType);
    }
}
