using UnityEngine;

namespace TowerDefence
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private GameOverPanelController gameOverPanelController;
        [SerializeField] private PowerUpPanelController powerUpPanelController;

        public void ShowGameOverPanel() => Show(gameOverPanelController);
        public void HideGameOverPanel() => Hide(gameOverPanelController);

        public void ShowPowerUpPanel() => Show(powerUpPanelController);
        public void HidePowerUpPanel() => Hide(powerUpPanelController);

        void Show<T>(T ui) where T : MonoBehaviour
        {
            ui.gameObject.SetActive(true);
        }

        void Hide<T>(T ui) where T : MonoBehaviour
        {
            ui.gameObject.SetActive(false);
        }
    }
}