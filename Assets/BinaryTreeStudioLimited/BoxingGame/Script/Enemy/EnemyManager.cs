using UnityEngine;
using DG.Tweening;
using System;

public class EnemyManager : Singleton<EnemyManager>
{
    [SerializeField] private int bossFrequency = 10;
    [SerializeField] private float delayBeforeReset = 7;
    [SerializeField] private EnemyType normalEnemy;
    [SerializeField] private EnemyType boss;

    int enemyLevel = 1;

    public Action<EnemyType, int> OnEnemyReset;

    void Start()
    {
        BattleManager.Instance.OnGameStarted += OnGameStarted;

        EnemyController.Instance.OnEnemyHPChanged += OnEnemyHPChanged;
    }

    protected override void OnDestroy()
    {
        EnemyController.Instance.OnEnemyHPChanged -= OnEnemyHPChanged;
        base.OnDestroy();
    }

    void OnGameStarted(int playerCount)
    {
        enemyLevel = 1;
        OnEnemyReset?.Invoke(normalEnemy, enemyLevel);
    }

    void OnEnemyHPChanged(float healthPercentage)
    {
        if (healthPercentage <= 0)
        {
            enemyLevel++;
            //prototype reset enemy health after a delay
            DOVirtual.DelayedCall(delayBeforeReset, () =>
            {
                if (enemyLevel % bossFrequency == 0)
                {
                    OnEnemyReset?.Invoke(boss, enemyLevel);
                }
                else
                {
                    OnEnemyReset?.Invoke(normalEnemy, enemyLevel);
                }
            });
        }
    }

}
