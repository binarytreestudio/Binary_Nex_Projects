using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelController : MonoBehaviour
{
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [SerializeField] private Sprite leftHookSprite;
    [SerializeField] private Sprite uppercutSprite;
    [SerializeField] private Sprite rightHookSprite;

    public void SetTutorialImage(Sprite sprite)
    {
        tutorialImage.sprite = sprite;
    }
    public void SetTutorialImageLeftHook()
    {
        SetTutorialImage(leftHookSprite);
    }
    public void SetTutorialImageUppercut()
    {
        SetTutorialImage(uppercutSprite);
    }
    public void SetTutorialImageRightHook()
    {
        SetTutorialImage(rightHookSprite);
    }

    public void SetTutorialText(string text)
    {
        tutorialText.text = text;
    }
}
