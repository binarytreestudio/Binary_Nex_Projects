using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameplayHUDController : MonoBehaviour
{
    [SerializeField] private Image playerHealthBarImage;
    [SerializeField] private TextMeshProUGUI levelText;

    public void SetPlayerHealthBarValue(float percentage)
    {
        playerHealthBarImage.fillAmount = percentage;
    }

    public void SetLevelText(string text)
    {
        levelText.text = $"Level: {text}";
    }
}

