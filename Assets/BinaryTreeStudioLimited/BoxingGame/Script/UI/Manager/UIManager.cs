using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameOverPanelController gameOverPanelController;
    [SerializeField] private StartScreenController startScreenController;

    public void ShowGameOverPanel() => Show(gameOverPanelController);
    public void ShowStartScreen() => Show(startScreenController);

    void Show<T>(T ui) where T : MonoBehaviour
    {
        ui.gameObject.SetActive(true);
    }
}
