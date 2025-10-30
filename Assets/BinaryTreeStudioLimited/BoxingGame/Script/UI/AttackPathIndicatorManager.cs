using System;
using System.Collections.Generic;
using Jazz;
using UnityEngine;

public class AttackPathIndicatorManager : Singleton<AttackPathIndicatorManager>
{
    [Serializable]
    private class AttackIndicatorMapping
    {
        public AttackPathIndicatorController attackPathIndicatorController;
        public EnemyController.AttackPath path;
    }
    [SerializeField] List<AttackIndicatorMapping> attackIndicators = new();

    void Start()
    {
        HideAllIndicators();
    }

    public void ShowAttackIndicator(EnemyController.AttackPath path, float duration)
    {
        HideAllIndicators();
        attackIndicators.Find(indicator => indicator.path == path).attackPathIndicatorController.Show(duration);
    }

    public void HideAttackIndicator(EnemyController.AttackPath attackPath)
    {
        attackIndicators.Find(indicator => indicator.path == attackPath).attackPathIndicatorController.Hide();
    }
    public void HideAttackIndicator(EnemyController.AttackPath attackPath, Handedness handedness)
    {
        // Currently, only CrossFinisher uses handedness
        if (attackPath != EnemyController.AttackPath.CrossFinisher) return;

        attackIndicators.Find(indicator => indicator.path == attackPath).attackPathIndicatorController.Hide();
    }

    public void HideAllIndicators()
    {
        attackIndicators.ForEach(indicator => indicator.attackPathIndicatorController.Hide());
    }
}
