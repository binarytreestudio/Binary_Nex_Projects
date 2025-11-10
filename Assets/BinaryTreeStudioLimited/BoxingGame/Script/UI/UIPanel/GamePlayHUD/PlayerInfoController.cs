using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoController : MonoBehaviour
{
    [Header("hp")]
    [SerializeField] private Image healthBarFillImage;

    [Header("combo")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Color successComboColor = Color.yellow;
    [SerializeField] private Color failedComboColor = Color.red;

    int playerIndex;
    int previousPlayerCombo = 0;

    public void Init(int playerIndex)
    {
        this.playerIndex = playerIndex;

        PlayerManager.Instance.OnPlayerHPChanged += HandlePlayerHPChanged;
        PlayerManager.Instance.OnPlayerComboChanged += HandlePlayerComboChanged;

        // Initialize UI
        HandlePlayerHPChanged(playerIndex, 1f);
        HandlePlayerComboChanged(playerIndex, 0);
    }

    private void OnDestroy()
    {
        PlayerManager.Instance.OnPlayerHPChanged -= HandlePlayerHPChanged;
        PlayerManager.Instance.OnPlayerComboChanged -= HandlePlayerComboChanged;
    }

    private void HandlePlayerHPChanged(int playerIndex, float healthNormalized)
    {
        if (this.playerIndex != playerIndex) return;
        healthBarFillImage.fillAmount = healthNormalized;
    }

    private void HandlePlayerComboChanged(int playerIndex, int comboCount)
    {
        if (this.playerIndex != playerIndex) return;
        if (comboCount > previousPlayerCombo)
        {
            // Flash success color
            comboText.color = successComboColor;
            //var scorePopup = Instantiate(scorePopupPrefab, scorePopupParent);
            //var popupText = scorePopup.GetComponent<TextMeshProUGUI>();
            //popupText.text = $"{comboCount} Combo + {(int)BattleManager.ScoreType.AdditionalScorePerCombo * comboCount}";
            //AddScorePopup(scorePopup);
        }
        else if (comboCount == 0 && previousPlayerCombo > 0)
        {
            // Flash incoming attack color
            comboText.color = failedComboColor;
        }
        comboText.text = "Combo x" + comboCount;
        previousPlayerCombo = comboCount;
    }
}
