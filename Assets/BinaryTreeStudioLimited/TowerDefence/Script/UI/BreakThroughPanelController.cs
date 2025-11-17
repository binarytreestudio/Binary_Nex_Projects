using System;
using UnityEngine;

namespace TowerDefence
{
    public class BreakThroughPanelController : Singleton<BreakThroughPanelController>
    {
        [SerializeField] private Transform leftEnemySpawnPoint;
        [SerializeField] private Transform centerEnemySpawnPoint;
        [SerializeField] private Transform rightEnemySpawnPoint;
        [SerializeField] private GameObject breakThroughEnemyPrefab;
        [SerializeField] private Transform breakThroughY;
        [SerializeField] private Transform breakThroughDestination;

        public void SpawnBreakThroughEnemies(int lane, EnemyManager.EnemyType enemyType) //lane -1: left, 0: center, 1: right
        {
            Transform spawnPoint = lane switch
            {
                -1 => leftEnemySpawnPoint,
                0 => centerEnemySpawnPoint,
                1 => rightEnemySpawnPoint,
                _ => centerEnemySpawnPoint
            };

            GameObject breakThroughEnemyObj = Instantiate(breakThroughEnemyPrefab, transform);
            breakThroughEnemyObj.transform.position = spawnPoint.position;
            breakThroughEnemyObj.transform.rotation = spawnPoint.rotation;

            BreakThroughEnemyController breakThroughEnemyController = breakThroughEnemyObj.GetComponent<BreakThroughEnemyController>();
            breakThroughEnemyController.Init(enemyType, breakThroughY, breakThroughDestination);
        }
    }
}