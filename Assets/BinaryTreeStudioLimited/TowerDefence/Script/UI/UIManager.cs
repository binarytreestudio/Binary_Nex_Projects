using UnityEngine;
public class UIManager : Singleton<UIManager>
{
    public GameplayHUDController gameplayHUDController;
    [SerializeField] private GameOverPanelController gameOverPanelController;
    [SerializeField] private PowerUpPanelController powerUpPanelController;
    public TutorialPanelController tutorialPanelController;

    public void ShowGameplayHUD() => Show(gameplayHUDController);
    public void HideGameplayHUD() => Hide(gameplayHUDController);

    public void ShowGameOverPanel() => Show(gameOverPanelController);
    public void HideGameOverPanel() => Hide(gameOverPanelController);

    public void ShowPowerUpPanel() => Show(powerUpPanelController);
    public void HidePowerUpPanel() => Hide(powerUpPanelController);

    public void ShowTutorialPanel() => Show(tutorialPanelController);
    public void HideTutorialPanel() => Hide(tutorialPanelController);

    void Show<T>(T ui) where T : MonoBehaviour
    {
        ui.gameObject.SetActive(true);
    }

    void Hide<T>(T ui) where T : MonoBehaviour
    {
        ui.gameObject.SetActive(false);
    }
}

