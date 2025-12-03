using System;
using UnityEngine;

namespace TowerDefence
{
    public class BreakThroughPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject pilar1Object;
        [SerializeField] private GameObject pilar2Object;

        private void Start()
        {
            BattleManager.Instance.OnGameStarted += SetPilarXPositions;
        }

        private void OnDestroy()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnGameStarted -= SetPilarXPositions;
        }

        private void SetPilarXPositions(int playerCount)
        {
            if (playerCount % 2 == 0)
            {
                pilar1Object.transform.localPosition += Vector3.left * 200f;
                pilar2Object.transform.localPosition += Vector3.right * 200f;
            }
        }


        //[SerializeField] private Transform leftEnemySpawnPoint;
        //[SerializeField] private Transform centerEnemySpawnPoint;
        //[SerializeField] private Transform rightEnemySpawnPoint;
        //[SerializeField] private GameObject breakThroughEnemyPrefab;
        //[SerializeField] private Transform breakThroughY;
        //[SerializeField] private Transform breakThroughDestination;
        //
        //public void SpawnBreakThroughEnemies(int lane, EnemyManager.EnemyType enemyType) //lane -1: left, 0: center, 1: right
        //{
        //    Transform spawnPoint = lane switch
        //    {
        //        -1 => leftEnemySpawnPoint,
        //        0 => centerEnemySpawnPoint,
        //        1 => rightEnemySpawnPoint,
        //        _ => centerEnemySpawnPoint
        //    };
        //
        //    GameObject breakThroughEnemyObj = Instantiate(breakThroughEnemyPrefab, transform);
        //    breakThroughEnemyObj.transform.position = spawnPoint.position;
        //    breakThroughEnemyObj.transform.rotation = spawnPoint.rotation;
        //
        //    BreakThroughEnemyController breakThroughEnemyController = breakThroughEnemyObj.GetComponent<BreakThroughEnemyController>();
        //    breakThroughEnemyController.Init(enemyType, breakThroughY, breakThroughDestination);
        //}
    }
}