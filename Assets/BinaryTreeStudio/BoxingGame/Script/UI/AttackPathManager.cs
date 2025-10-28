using System;
using System.Collections.Generic;
using Jazz;
using UnityEngine;

public class AttackPathManager : Singleton<AttackPathManager>
{
    [Serializable]
    public class AttackIndicatorMapping
    {
        public GameObject indicatorObject;
        public EnemyController.AttackPath path;
    }
    [SerializeField] List<AttackIndicatorMapping> attackIndicators = new();

    void Start()
    {
        HideAllIndicators();
    }

    public void ShowAttackIndicator(EnemyController.AttackPath attackPath)
    {
        HideAllIndicators();
        GameObject attackIndicator = attackIndicators.Find(indicator => indicator.path == attackPath).indicatorObject;
        attackIndicator.SetActive(true);
        if (attackPath == EnemyController.AttackPath.CrossFinisher)
        {
            attackIndicator.transform.GetChild(0).gameObject.SetActive(true);
            attackIndicator.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void HideAllIndicators()
    {
        attackIndicators.ForEach(indicator => indicator.indicatorObject.SetActive(false));
    }

    public void HideAttackIndicator(EnemyController.AttackPath attackPath, Handedness handedness = default)
    {
        if (attackPath == EnemyController.AttackPath.CrossFinisher)
        {
            if (handedness == Handedness.Left)
            {
                attackIndicators.Find(indicator => indicator.path == EnemyController.AttackPath.CrossFinisher).indicatorObject.transform.GetChild(0).gameObject.SetActive(false);
            }
            else if (handedness == Handedness.Right)
            {
                attackIndicators.Find(indicator => indicator.path == EnemyController.AttackPath.CrossFinisher).indicatorObject.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
        else
        {
            attackIndicators.Find(indicator => indicator.path == attackPath).indicatorObject.SetActive(false);
        }
    }
}
