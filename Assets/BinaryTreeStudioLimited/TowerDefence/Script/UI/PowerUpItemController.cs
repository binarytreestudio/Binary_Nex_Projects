using Jazz;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefence
{
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

        public void ApplyPowerUp(int playerIndex)
        {
            PlayerManager.Instance.ApplyPowerUp(playerIndex, currentPowerUpType);

            UIManager.Instance.HidePowerUpPanel();
        }
    }
}