using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyAttackIndicatorManager : Singleton<EnemyAttackIndicatorManager>
{
    [Serializable]
    private struct AttackIndicatorMapping
    {
        public EnemyController.EnemyIncomingAttack attackType;
        public Image image;
        public TMPro.TextMeshProUGUI timerText;
    }
    [SerializeField] private List<AttackIndicatorMapping> attackIndicators = new();
    private float duration;

    void Start()
    {
        HideAllIndicator();
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
        mapping.timerText.text = duration.ToString("F1");
        DOVirtual.Float(duration, 0f, duration, value =>
        {
            mapping.timerText.text = value.ToString("F1");
        }).SetEase(Ease.Linear).OnComplete(() =>
        {
            HideIndicator(mapping);
        });
        mapping.image.DOFade(0f, duration / 2.1f).SetLoops(2, LoopType.Yoyo);
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
}
