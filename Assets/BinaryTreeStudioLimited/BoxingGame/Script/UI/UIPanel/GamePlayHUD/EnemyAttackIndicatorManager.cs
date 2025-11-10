using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyAttackIndicatorManager : MonoBehaviour
{
    [Serializable]
    private struct AttackIndicatorMapping
    {
        public EnemyController.EnemyIncomingAttack attackType;
        public Image image;
        public TMPro.TextMeshProUGUI timerText;
    }
    [SerializeField] private List<AttackIndicatorMapping> attackIndicators = new();
    [SerializeField] private int blinkPerSecond = 2;
    private float duration;

    void Start()
    {
        HideAllIndicator();
        EnemyController.Instance.OnEnemyAttackSelected += Show;
    }

    public void Show(EnemyController.EnemyIncomingAttack attackType, float duration)
    {
        this.duration = duration;
        AttackIndicatorMapping mapping = attackIndicators.Find(indicator => indicator.attackType == attackType);
        EnableIndicator(mapping);
    }

    private void EnableIndicator(AttackIndicatorMapping mapping)
    {
        mapping.image.enabled = true;
        mapping.image.color = new Color(mapping.image.color.r, mapping.image.color.g, mapping.image.color.b, 1f);
        mapping.timerText.text = duration.ToString("F1");
        DOVirtual.Float(duration, 0f, duration, value =>
        {
            mapping.timerText.text = value.ToString("F1");
        }).SetEase(Ease.Linear).OnComplete(() =>
        {
            HideIndicator(mapping);
        });
        mapping.image.DOFade(0f, 1f / blinkPerSecond).SetLoops(Mathf.Max(1, Mathf.CeilToInt(blinkPerSecond * duration)), LoopType.Yoyo);
    }

    private void HideIndicator(AttackIndicatorMapping mapping)
    {
        mapping.image.enabled = false;
        mapping.timerText.text = "";
    }

    private void HideAllIndicator()
    {
        foreach (var mapping in attackIndicators)
        {
            HideIndicator(mapping);
        }
    }

    void OnDestroy()
    {
        EnemyController.Instance.OnEnemyAttackSelected -= Show;
    }
}
