// AreaOfEffectBase.cs（完整取代舊版）
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public abstract class AreaOfEffectBase : MonoBehaviour
    {
        [Header("=== 基礎設定 ===")]
        [SerializeField, Range(0.05f, 5f)] private float tickRate = 0.5f;
        [SerializeField, Tooltip("效果持續時間，<=0 表示永久")] private float duration = 5f;
        [SerializeField, Tooltip("最大 Tick 次數，<=0 表示無限制")] private int maxTickCount = 0;

        [Header("=== 狀態效果設定（支援多種同時存在）===")]
        [SerializeField]
        protected StatusEffectData[] statusEffects = new StatusEffectData[]
        {
            new StatusEffectData { type = StatusEffectType.Slow, value = 0.4f, duration = 2f, tickRate = 0.5f }
        };

        [Header("=== 即時資料（僅供除錯）===")]
        [SerializeField] private List<EnemyController> enemiesInRange = new();
        [SerializeField] private float lifetimeTimer = 0f;
        [SerializeField] private float tickTimer = 0f;
        [SerializeField] private int currentTickCount = 0;
        [SerializeField] private bool isInitialized = false;

        // ============================================================
        protected virtual void OnEffectStart() { }
        protected virtual void OnEffectEnd() { }

        private void OnEnable()
        {
            Initialize();
            OnEffectStart();
        }

        private void OnDisable()
        {
            ClearAllEnemies();
            OnEffectEnd();
            isInitialized = false;
        }

        private void OnDestroy()
        {
            ClearAllEnemies();
            OnEffectEnd();
        }

        public virtual void Initialize(float customDuration = -1f, int customMaxTickCount = -1)
        {
            lifetimeTimer = 0f;
            tickTimer = 0f;
            currentTickCount = 0;
            enemiesInRange.Clear();

            if (customDuration > 0f) duration = customDuration;
            if (customMaxTickCount > 0) maxTickCount = customMaxTickCount;

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
                    ApplyEffectsTo(enemy);
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
                    RemoveEffectsFrom(enemy);
                }
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (duration > 0f)
            {
                lifetimeTimer += Time.deltaTime;
                if (lifetimeTimer >= duration)
                {
                    Expire();
                    return;
                }
            }

            tickTimer += Time.deltaTime;
            if (tickTimer >= tickRate)
            {
                tickTimer -= tickRate;
                PerformTick();
                currentTickCount++;

                if (maxTickCount > 0 && currentTickCount >= maxTickCount)
                {
                    Expire();
                }
            }
        }

        private void LateUpdate()
        {
            enemiesInRange.RemoveAll(e => e == null || e.IsDead);
        }

        private void PerformTick()
        {
            foreach (var enemy in enemiesInRange)
            {
                if (enemy != null)
                {
                    ApplyEffectsTo(enemy); // 🔥 每 Tick 都重新 Apply → 持續刷新時間
                }
            }
        }

        // 在 AreaOfEffectBase.cs 的 ApplyEffectsTo() 方法內，取代原本的 if (is SlowEffect) 那坨
        private void ApplyEffectsTo(EnemyController enemy)
        {
            foreach (var data in statusEffects)
            {
                var newEffect = CreateEffectInstance(data, gameObject); // 傳入來源
                if (newEffect == null) continue;

                // 通用刷新邏輯：只要 SupportsRefresh == true 就檢查同來源
                if (newEffect.SupportsRefresh)
                {
                    bool refreshed = false;

                    if (enemy.activeEffects.TryGetValue(newEffect.Type, out var existingList))
                    {
                        foreach (var existing in existingList)
                        {
                            if (existing is IStatusEffect existingEffect &&
                                existingEffect.SupportsRefresh &&
                                existingEffect.Source == gameObject)
                            {
                                existingEffect.RefreshDuration();
                                refreshed = true;
                                break;
                            }
                        }
                    }

                    if (!refreshed)
                    {
                        enemy.ApplyStatusEffect(newEffect);
                    }
                }
                else
                {
                    // 不支援刷新的效果直接套（例如一次性護盾、瞬間治療）
                    enemy.ApplyStatusEffect(newEffect);
                }
            }
        }

        private void RemoveEffectsFrom(EnemyController enemy)
        {
            // 不主動移除，讓效果自然過期（更彈性）
            // 如果需要「離開立刻解除」，可在子類覆寫
        }

        protected virtual IStatusEffect CreateEffectInstance(StatusEffectData data, GameObject source)
        {
            return data.type switch
            {
                StatusEffectType.Slow => new SlowEffect(data, gameObject),
                StatusEffectType.Poison => new PoisonEffect(data, gameObject),
                StatusEffectType.Stun => new StunEffect(data, gameObject),

                _ => null
            };
        }

        public void ForceExpire()
        {
            Expire();
        }

        private void Expire()
        {
            ClearAllEnemies();
            //Destroy(gameObject);
            gameObject.SetActive(false);
        }

        private void ClearAllEnemies()
        {
            foreach (var enemy in enemiesInRange)
            {
                if (enemy != null)
                {
                    // 可選：特效消失時強制清除效果
                    // enemy.ClearStatusEffect();
                }
            }
            enemiesInRange.Clear();
        }

        public int CurrentTickCount => currentTickCount;
        public int MaxTickCount => maxTickCount;
        public float RemainingTime => duration > 0f ? duration - lifetimeTimer : -1f;
    }
}