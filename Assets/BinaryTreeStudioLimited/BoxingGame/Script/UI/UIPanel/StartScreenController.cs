using UnityEngine;

public class StartScreenController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BattleManager.Instance.OnGameStarted += OnGameStarted;
    }

    private void OnGameStarted(bool isStarted)
    {
        gameObject.SetActive(!isStarted);
    }

    private void OnDestroy()
    {
        BattleManager.Instance.OnGameStarted -= OnGameStarted;
    }
}
