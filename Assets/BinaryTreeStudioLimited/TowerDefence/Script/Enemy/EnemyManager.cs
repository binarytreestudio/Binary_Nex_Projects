using UnityEngine;

namespace TowerDefence
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [Header("Enemy Spawn Settings")]
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private float intervalDecreasePerLevel = 0.1f;

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
            if (!gameStarted)
                return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval - intervalDecreasePerLevel * (level - 1);
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
        }
    }
}
