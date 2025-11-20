using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        public enum EnemyType
        {
            Normal = 0,
            Elite = 1,
            Golem = 2,
        }

        [Header("Level Settings")]
        [SerializeField] private int levelEnemyCount = 20;
        [SerializeField][Range(0f, 1f)] private float enemiesPerLevelIncrease = 0.1f;

        [Header("Enemy Spawn Settings")]
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField][Range(0f, 1f)] private float intervalDecreasePercentagePerLevel = 0.01f;

        [Header("Enemy Type")]
        [SerializeField] private GameObject normalEnemyPrefab;
        [SerializeField] private GameObject eliteEnemyPrefab;
        [SerializeField] private GameObject golemEnemyPrefab;
        [Range(0f, 1f)][SerializeField] private float eliteEnemyChance = 0.1f;
        [SerializeField] private int eliteEnemyQuotaCost = 3;
        [Range(0f, 1f)][SerializeField] private float golemEnemyChance = 0.05f;
        [SerializeField] private int golemEnemyQuotaCost = 8;

        float spawnTimer = 0f;
        bool gameStarted = false;
        private int playerCount = 1;
        private int level = 1;
        private int enemiesSpawnedThisLevel = 0;
        List<EnemyController> spawnedEnemies = new();
        private List<GameObject> lanes;

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
            if (!gameStarted || enemiesSpawnedThisLevel >= levelEnemyCount * Mathf.Exp(enemiesPerLevelIncrease * (level - 1)))
                return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval * Mathf.Exp(-intervalDecreasePercentagePerLevel * (level - 1));
            }
        }

        public void SetLanes(List<GameObject> lanes)
        {
            this.lanes = lanes;
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

            switch (BattleManager.Instance.LaneType)
            {
                case BattleManager.LaneSetting.Straight:
                    GameObject enemy = Instantiate(enemyPrefab, lanes[randomInt].transform.Find("Start").position, Quaternion.identity);
                    var enemyController = enemy.GetComponent<EnemyController>();
                    enemyController.Init(level, lanes[randomInt]);
                    spawnedEnemies.Add(enemyController);
                    break;
                case BattleManager.LaneSetting.SShape:
                    enemy = Instantiate(enemyPrefab, lanes[0].transform.Find("Start").position, Quaternion.identity);
                    enemyController = enemy.GetComponent<EnemyController>();
                    enemyController.Init(level, lanes);
                    spawnedEnemies.Add(enemyController);
                    break;
            }

            if (enemyPrefab == golemEnemyPrefab)
            {
                enemiesSpawnedThisLevel += golemEnemyQuotaCost;
            }
            else if (enemyPrefab == eliteEnemyPrefab)
            {
                enemiesSpawnedThisLevel += eliteEnemyQuotaCost;
            }
            else
            {
                enemiesSpawnedThisLevel += 1;
            }
        }

        public void OnEnemyDefeated(EnemyController enemy)
        {
            spawnedEnemies.Remove(enemy);

            if (enemiesSpawnedThisLevel >= levelEnemyCount * Mathf.Exp(enemiesPerLevelIncrease * (level - 1)) &&
                spawnedEnemies.Count == 0)
            {
                BattleManager.Instance.LevelComplete();
            }
        }

        public void StartNextLevel()
        {
            level++;
            enemiesSpawnedThisLevel = 0;
            GameplayHUDController.Instance.SetLevelText(level);
        }
    }
}