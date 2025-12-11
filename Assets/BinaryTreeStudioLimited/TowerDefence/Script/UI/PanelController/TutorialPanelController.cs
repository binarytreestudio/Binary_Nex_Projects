using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;

    public void SetTutorialText(string text)
    {
        tutorialText.text = text;
    }
}
