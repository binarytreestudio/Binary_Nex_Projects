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
        [SerializeField][Range(0f, 1f)] private float eliteEnemyChanceIncreasePerLevel = 0.2f;
        [SerializeField] private int eliteEnemyQuotaCost = 3;
        [Range(0f, 1f)][SerializeField] private float golemEnemyChance = 0.05f;
        [SerializeField][Range(0f, 1f)] private float golemEnemyChanceIncreasePerLevel = 0.1f;
        [SerializeField] private int golemEnemyQuotaCost = 8;

        float spawnTimer = 0f;
        bool gameStarted = false;
        private int playerCount = 1;
        private int level = 0;
        private int enemiesSpawnedThisLevel = 0;
        List<EnemyController> spawnedEnemies = new();
        private List<GameObject> lanes;

        void Update()
        {
            if (!gameStarted || spawnedEnemies.Count <= 0 || !spawnedEnemies.Find(enemy => enemy.gameObject.activeSelf == false))
                return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnedEnemies.Find(enemy => enemy.gameObject.activeSelf == false).gameObject.SetActive(true);
                spawnTimer = spawnInterval * Mathf.Exp(-intervalDecreasePercentagePerLevel * (level - 1)) / playerCount;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                spawnedEnemies.ForEach(enemy => Destroy(enemy.gameObject));
                spawnedEnemies.Clear();
            }
        }

        public void SetLanes(List<GameObject> lanes)
        {
            this.lanes = lanes;
        }

        public void OnGameStarted(int playerCount)
        {
            this.playerCount = playerCount;

            Debug.Log("Enemy Manager Game Started");

            StartNextLevel();

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
            GameObject enemyPrefab = random < golemEnemyChance * Mathf.Exp(golemEnemyChanceIncreasePerLevel * (level - 1)) ? golemEnemyPrefab :
                                     random < eliteEnemyChance * Mathf.Exp(eliteEnemyChanceIncreasePerLevel * (level - 1)) ? eliteEnemyPrefab :
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

            Debug.Log(enemyPrefab);

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

            if (enemiesSpawnedThisLevel >= levelEnemyCount * Mathf.Exp(enemiesPerLevelIncrease * (level - 1)) * playerCount &&
                spawnedEnemies.Count == 0)
            {
                BattleManager.Instance.LevelComplete();
            }
        }

        public void StartNextLevel()
        {
            level++;
            enemiesSpawnedThisLevel = 0;
            Debug.Log($"Spawn enemy count {spawnedEnemies.Count}");
            while (enemiesSpawnedThisLevel < levelEnemyCount * Mathf.Exp(enemiesPerLevelIncrease * (level - 1)) * playerCount)
            {
                SpawnEnemy();
            }
            spawnedEnemies.ForEach(enemy => { enemy.gameObject.SetActive(false); });

            GameplayHUDController.Instance?.SetLevelText(level);
        }
    }
}