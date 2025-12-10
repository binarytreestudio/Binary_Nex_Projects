using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameplayHUDController : Singleton<GameplayHUDController>
{
    [SerializeField] private Image playerHealthBarImage;
    [SerializeField] private TextMeshProUGUI levelText;

    public void SetPlayerHealthBarValue(float percentage)
    {
        playerHealthBarImage.fillAmount = percentage;
    }

    public void SetLevelText(int level)
    {
        levelText.text = "Level " + level.ToString();
    }
}

