using System.Collections.Generic;
using UnityEngine;
namespace TowerDefence
{
    public enum EnemyStats
    {
        Speed,
    }
    public abstract class AreaOfEffectBase : MonoBehaviour
    {
        [SerializeField] protected List<EnemyController> enemiesInRange = new List<EnemyController>();

        [Header("Tick Settings")]
        [SerializeField] private float tickRate = 0.5f;
        [SerializeField] private float tickTimer = 0f;

        protected virtual void OnEnemyEnter(EnemyController enemy) { }
        protected virtual void OnEnemyTick(EnemyController enemy) { }
        protected virtual void OnEnemyExit(EnemyController enemy) { }


        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;

            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                if (!enemiesInRange.Contains(enemy))
                {
                    enemiesInRange.Add(enemy);
                    OnEnemyEnter(enemy);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;

            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                if (enemiesInRange.Remove(enemy))
                {
                    OnEnemyExit(enemy);
                }
            }
        }

        private void Update()
        {
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickRate)
            {
                tickTimer -= tickRate;
                PerformTick();
            }
        }

        /// <summary>
        /// 每隔 tickRate 秒會呼叫一次，對範圍內所有敵人執行 Tick 效果
        /// </summary>
        protected void PerformTick()
        {
            print(enemiesInRange.Count);
            for (int i = enemiesInRange.Count - 1; i >= 0; i--)
            {
                var enemy = enemiesInRange[i];
                if (enemy != null)
                {
                    OnEnemyTick(enemy);
                    print(enemy.name);
                }
            }
        }

        // 清理已死亡（null）的敵人，建議用 LateUpdate 或在死亡事件中主動移除
        private void LateUpdate()
        {
            enemiesInRange.RemoveAll(enemy => enemy == null);
        }

        // 可選：提供公開方法讓外部強制清除或重新計算（例如特效重置時）
        protected void ClearEnemies()
        {
            enemiesInRange.Clear();
        }
    }
}