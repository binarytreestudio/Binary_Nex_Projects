using UnityEngine;
using System;

public class EnemyManager : Singleton<EnemyManager>
{
    [SerializeField] private int bossFrequency = 10;
    [SerializeField] private EnemyType normalEnemy;
    [SerializeField] private EnemyType boss;
    [SerializeField] private float minimumReactionTime = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float reduceReactionTimePercentagePerLevel = 0.03f;
    [SerializeField] private float enemyHealthIncreasePerLevel = 30f;
    [Range(0f, 1f)]
    [SerializeField] private float bubbleFrequencyDecreasePercentagePerLevel = 0.05f;
    [SerializeField] private float bubbleFrequencyDecreasePercentagePerPlayer = 0.5f;


    private int enemyLevel = 1;
    private int playerCount;

    public Action<EnemyType, int> OnEnemyReset;     // EnemyType: enemy type, int: enemy level

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
        this.playerCount = playerCount;
        enemyLevel = 1;
        ResetEnemy();
    }

    void OnEnemyHPChanged(float healthPercentage)
    {
        if (healthPercentage <= 0)
        {
            enemyLevel++;
        }
    }

    public void ResetEnemy()
    {
        EnemyType enemyType = new((enemyLevel % bossFrequency == 0) ? boss : normalEnemy);
        enemyType.maxHealth += enemyLevel * UnityEngine.Random.Range(enemyHealthIncreasePerLevel / 5, enemyHealthIncreasePerLevel);
        enemyType.attackDelay = Mathf.Max(minimumReactionTime, enemyType.attackDelay * (1 - ((enemyLevel - 1) * reduceReactionTimePercentagePerLevel)));
        enemyType.bubbleDuration = Mathf.Max(minimumReactionTime, enemyType.bubbleDuration * (1 - ((enemyLevel - 1) * reduceReactionTimePercentagePerLevel)));
        enemyType.bubbleFrequency = enemyType.bubbleFrequency * (1 + ((enemyLevel - 1) * bubbleFrequencyDecreasePercentagePerLevel));
        enemyType.bubbleFrequency /= 1 + (playerCount - 1) * bubbleFrequencyDecreasePercentagePerPlayer;

        OnEnemyReset?.Invoke(enemyType, enemyLevel);
    }

}
