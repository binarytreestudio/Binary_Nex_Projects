using UnityEngine;
using UnityEngine.UI;

namespace TowerDefence
{
    public class GameplayHUDController : Singleton<GameplayHUDController>
    {
        [SerializeField] private Image playerHealthBarImage;

        public void SetPlayerHealthBarValue(float percentage)
        {
            playerHealthBarImage.fillAmount = percentage;
        }
    }
}
