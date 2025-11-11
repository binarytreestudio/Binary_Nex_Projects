using System;
using UnityEngine;

namespace TowerDefence
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private GameObject enemyPrefab = null!;

        [SerializeField] private float enemyHP = 100f;
        [SerializeField] private float enemySpeed = 2f;
        [SerializeField] private float enemyDamage = 10f;

        float spawnTimer = 0f;
        bool gameStarted = false;
        private int playerCount = 1;

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
                spawnTimer = spawnInterval;
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

            GameObject enemy = Instantiate(enemyPrefab, new Vector3(xPos, 0, 20), Quaternion.identity);
            var enemyController = enemy.GetComponent<EnemyController>();
            enemyController.Init(enemyHP, enemySpeed, enemyDamage);
        }
    }
}
