using UnityEngine;
using UnityEngine.UI;

public class BoxingEnemyAttackIndicator : Singleton<BoxingEnemyAttackIndicator>
{
    [SerializeField] Image image;
    [SerializeField] TMPro.TextMeshProUGUI timerText;
    float duration;
    float elapsedTime;

    void Start()
    {
        Hide();
    }

    public void Show(float duration)
    {
        this.duration = duration;
        elapsedTime = 0f;
        timerText.text = duration.ToString("F1");
        EnableImage();
    }

    void Update()
    {
        if (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float remainingTime = Mathf.Max(0f, duration - elapsedTime);
            timerText.text = remainingTime.ToString("F1");
        }
        else
        {
            Hide();
        }
    }

    private void EnableImage()
    {
        image.enabled = true;
    }

    private void Hide()
    {
        image.enabled = false;
        timerText.text = "";
    }
}
