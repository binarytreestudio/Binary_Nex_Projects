using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameOverPanelController gameOverPanelController;
    
    public void ShowGameOverPanel() => Show(gameOverPanelController);

    void Show<T>(T ui) where T : MonoBehaviour
    {
        ui.gameObject.SetActive(true);
    }
}
