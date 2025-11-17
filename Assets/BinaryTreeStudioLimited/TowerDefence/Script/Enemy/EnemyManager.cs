using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [Header("Level Settings")]
        [SerializeField] private int levelEnemyCount = 20;
        [SerializeField] private int enemiesPerLevelIncrease = 5;

        [Header("Enemy Spawn Settings")]
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField][Range(0f, 1f)] private float intervalDecreasePercentagePerLevel = 0.01f;

        [Header("Enemy Type")]
        [SerializeField] private GameObject normalEnemyPrefab;
        [SerializeField] private GameObject eliteEnemyPrefab;
        [SerializeField] private GameObject golemEnemyPrefab;
        [Range(0f, 1f)][SerializeField] private float eliteEnemyChance = 0.1f;
        [Range(0f, 1f)][SerializeField] private float golemEnemyChance = 0.05f;

        float spawnTimer = 0f;
        bool gameStarted = false;
        private int playerCount = 1;
        private int level = 1;
        private int enemiesSpawnedThisLevel = 0;
        List<EnemyController> spawnedEnemies = new();

        void Start()
        {
            BattleManager.Instance.OnGameStarted += OnGameStarted;
        }

        protected override void OnDestroy()
        {
            BattleManager.Instance.OnGameStarted -= OnGameStarted;

            base.OnDestroy();
        }

        void Update()
        {
            if (!gameStarted || enemiesSpawnedThisLevel >= levelEnemyCount + enemiesPerLevelIncrease * (level - 1))
                return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval * (1f - intervalDecreasePercentagePerLevel * (level - 1));
            }
        }

        void OnGameStarted(int playerCount)
        {
            this.playerCount = playerCount;

            gameStarted = true;
        }

        void SpawnEnemy()
        {
            int totalLanes = playerCount + 2;
            float spacing = 2f;
            float startX = -spacing * Mathf.Floor((totalLanes - 1) / 2f);
            int randomInt = UnityEngine.Random.Range(0, totalLanes);

            float xPos = startX + randomInt * spacing;

            float random = UnityEngine.Random.value;
            GameObject enemyPrefab = random < golemEnemyChance ? golemEnemyPrefab :
                                     random < eliteEnemyChance ? eliteEnemyPrefab :
                                     normalEnemyPrefab;
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(xPos, 0, 20), Quaternion.identity);
            var enemyController = enemy.GetComponent<EnemyController>();
            enemyController.Init(level);
            spawnedEnemies.Add(enemyController);

            enemiesSpawnedThisLevel++;
        }

        public void OnEnemyDefeated(EnemyController enemy)
        {
            spawnedEnemies.Remove(enemy);

            if (enemiesSpawnedThisLevel >= levelEnemyCount + enemiesPerLevelIncrease * (level - 1) &&
                spawnedEnemies.Count == 0)
            {
                BattleManager.Instance.LevelComplete();
            }
        }

        public void StartNextLevel()
        {
            level++;
            enemiesSpawnedThisLevel = 0;
        }
    }
}