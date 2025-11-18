using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public abstract class AreaOfEffectBase : MonoBehaviour
    {
        [Header("Target & List")]
        [SerializeField] protected List<EnemyController> enemiesInRange = new List<EnemyController>();

        [Header("Tick Settings")]
        [SerializeField, Range(0.05f, 5f)] private float tickRate = 0.5f;

        [Header("Duration Settings")]
        [SerializeField, Tooltip("效果持續時間，0 或負數代表永久存在")]
        private float duration = 5f;

        [Header("Tick Count Limit")] 
        [SerializeField, Tooltip("最大生效次數（每次 Tick 算一次），0 或負數代表無限制")]
        private int maxTickCount = 0; // 0 或負數 = 不限制

        [Header("Real-time Data")]
        [SerializeField] private float tickTimer = 0f;
        [SerializeField] private float lifetimeTimer = 0f;
        [SerializeField] private int currentTickCount = 0;     // 目前已觸發次數
        [SerializeField] private bool isInitialized = false;

        protected virtual void OnEnemyEnter(EnemyController enemy) { }
        protected virtual void OnEnemyTick(EnemyController enemy) { }
        protected virtual void OnEnemyExit(EnemyController enemy) { }
        protected virtual void OnEffectEnd() { }

        private void OnEnable()
        {
            Initialize();
        }

        public virtual void Initialize(float customDuration = -1f, int customMaxTickCount = 0)
        {
            lifetimeTimer = 0f;
            tickTimer = 0f;
            currentTickCount = 0;
            enemiesInRange.Clear();

            if (customDuration > 0f)
                duration = customDuration;

            if (customMaxTickCount > 0)
                maxTickCount = customMaxTickCount;

            isInitialized = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;

            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                if (enemy != null && !enemiesInRange.Contains(enemy))
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
                if (enemy != null && enemiesInRange.Remove(enemy))
                {
                    OnEnemyExit(enemy);
                }
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            tickTimer += Time.deltaTime;

            if (tickTimer >= tickRate)
            {
                tickTimer -= tickRate;
                PerformTick();

                // 每次 Tick 完畢就累計一次
                currentTickCount++;

                // 檢查是否達到次數上限
                if (maxTickCount > 0 && currentTickCount >= maxTickCount)
                {
                    Expire();
                    return; // 直接結束，避免後續時間判斷又重複觸發
                }
            }

            // 原有的時間到期機制
            if (duration > 0f)
            {
                lifetimeTimer += Time.deltaTime;
                if (lifetimeTimer >= duration)
                {
                    Expire();
                }
            }
        }

        protected virtual void PerformTick()
        {
            for (int i = enemiesInRange.Count - 1; i >= 0; i--)
            {
                var enemy = enemiesInRange[i];
                if (enemy != null)
                {
                    OnEnemyTick(enemy);
                }
            }
        }

        private void LateUpdate()
        {
            // 清理已死亡的敵人（null）
            enemiesInRange.RemoveAll(e => e == null);
        }

        private void Expire()
        {
            OnEffectEnd();
            Destroy(gameObject);
        }

        // 外部強制結束
        public void ForceExpire()
        {
            Expire();
        }

        // 外部強制清除敵人列表
        protected void ClearEnemies()
        {
            foreach (var enemy in enemiesInRange)
            {
                if (enemy != null)
                    OnEnemyExit(enemy);
            }
            enemiesInRange.Clear();
        }

        // 提供給子類或外部查詢目前次數與上限
        public int CurrentTickCount => currentTickCount;
        public int MaxTickCount => maxTickCount;
    }
}