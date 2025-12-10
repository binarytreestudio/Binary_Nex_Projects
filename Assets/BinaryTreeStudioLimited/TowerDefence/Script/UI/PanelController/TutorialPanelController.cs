using TMPro;
using UnityEngine;

public class TutorialPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;

    public void SetTutorialText(string text)
    {
        tutorialText.text = text;
    }
}
