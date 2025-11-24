using Jazz;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefence
{
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


            //hardcode for now
            if (powerUpData.powerUpType == PowerUpDatabase.PowerUpType.RecoverHP)
            {
                punchIcon.gameObject.SetActive(false);
                iconImage.rectTransform.localPosition = Vector2.zero;
            }
            else if (powerUpData.powerUpType != PowerUpDatabase.PowerUpType.RecoverHP)  // if it is power up
            {
                punchIcon.gameObject.SetActive(true);
                iconImage.rectTransform.localPosition = new Vector2(87, 26);
            }

        }

        public void ApplyPowerUp(int playerIndex)
        {
            PlayerManager.Instance.ApplyPowerUp(playerIndex, currentPowerUpType);

            UIManager.Instance.HidePowerUpPanel();
            Instantiate(chooseParticleEffect, transform.position, Quaternion.identity);
            AudioManager.Instance.PlayCollectPowerup();
        }
    }
}