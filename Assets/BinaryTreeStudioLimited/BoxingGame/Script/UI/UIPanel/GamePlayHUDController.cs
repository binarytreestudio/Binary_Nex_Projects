using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class GamePlayHUDController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Image playerHPImage;
    [SerializeField] private TextMeshProUGUI playerComboText;
    [SerializeField] private Color successComboColor = Color.yellow;
    [SerializeField] private Color failedComboColor = Color.red;

    [Header("Enemy")]
    [SerializeField] private Image enemyHPImage;
    [SerializeField] private TextMeshProUGUI enemyLevelText;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private Transform scorePopupParent;
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private int playerScorePopupLimit = 3;

    private int previousPlayerCombo = 0;
    List<GameObject> scorePopups = new();

    private void Start()
    {
        PlayerController.Instance.OnPlayerHPChanged += UpdatePlayerHP;

        EnemyManager.Instance.OnEnemyReset += OnEnemyReset;

        EnemyController.Instance.OnEnemyHPChanged += UpdateEnemyHP;

        BattleManager.Instance.OnPlayerComboChanged += UpdatePlayerCombo;
        BattleManager.Instance.OnPlayerScoreChanged += OnScoreChanged;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnPlayerHPChanged -= UpdatePlayerHP;

        EnemyManager.Instance.OnEnemyReset -= OnEnemyReset;

        EnemyController.Instance.OnEnemyHPChanged -= UpdateEnemyHP;

        BattleManager.Instance.OnPlayerComboChanged -= UpdatePlayerCombo;
        BattleManager.Instance.OnPlayerScoreChanged -= OnScoreChanged;
    }

    private void UpdatePlayerHP(float normalizedHP)
    {
        playerHPImage.fillAmount = normalizedHP;
    }

    private void OnEnemyReset(EnemyType enemyType, int level)
    {
        enemyLevelText.text = $"Level: {level}";
    }

    private void UpdateEnemyHP(float normalizedHP)
    {
        if (normalizedHP <= 0)
        {
            //Hide enemy HP bar when enemy is dead
            enemyHPImage.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            enemyHPImage.transform.parent.gameObject.SetActive(true);
        }

        enemyHPImage.fillAmount = normalizedHP;
    }

    private void UpdatePlayerCombo(int combo)
    {
        if (combo > previousPlayerCombo)
        {
            // Flash success color
            playerComboText.color = successComboColor;
            var scorePopup = Instantiate(scorePopupPrefab, scorePopupParent);
            var popupText = scorePopup.GetComponent<TextMeshProUGUI>();
            popupText.text = $"{combo} Combo + {(int)BattleManager.ScoreType.AdditionalScorePerCombo * combo}";
            AddScorePopup(scorePopup);
        }
        else if (combo == 0 && previousPlayerCombo > 0)
        {
            // Flash incoming attack color
            playerComboText.color = failedComboColor;
        }
        playerComboText.text = "Combo: " + combo;
        previousPlayerCombo = combo;
    }

    private void OnScoreChanged(int newScore, BattleManager.ScoreType scoreType)
    {
        playerScoreText.text = "Score: " + newScore;
        if (scoreType == 0) return;

        // Update score popup based on score type
        var scorePopup = Instantiate(scorePopupPrefab, scorePopupParent);
        var popupText = scorePopup.GetComponent<TextMeshProUGUI>();
        switch (scoreType)
        {
            case BattleManager.ScoreType.Good:
                popupText.text = $"Good Hit + {(int)BattleManager.ScoreType.Good}";
                break;
            case BattleManager.ScoreType.Perfect:
                popupText.text = $"Perfect Hit + {(int)BattleManager.ScoreType.Perfect}";
                break;
            case BattleManager.ScoreType.Finisher:
                popupText.text = $"Finisher Hit + {(int)BattleManager.ScoreType.Finisher}";
                break;
            case BattleManager.ScoreType.Block:
                popupText.text = $"Successful Block + {(int)BattleManager.ScoreType.Block}";
                break;
        }
        AddScorePopup(scorePopup);
    }

    private void AddScorePopup(GameObject popup)
    {
        scorePopups.Add(popup);
        if (scorePopups.Count > playerScorePopupLimit)
        {
            Destroy(scorePopups[0]);
            scorePopups.RemoveAt(0);
        }
        for (int i = 0; i < scorePopups.Count; i++)
        {
            scorePopups[i].GetComponent<TextMeshProUGUI>().alpha = (i + 1) * (1f / scorePopups.Count);
        }
    }
}

