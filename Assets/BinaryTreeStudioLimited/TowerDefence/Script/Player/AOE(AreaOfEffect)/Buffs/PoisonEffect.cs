// PoisonEffect.cs
using UnityEngine;

namespace TowerDefence
{
    public class PoisonEffect : IStatusEffect
    {
        public StatusEffectType Type => StatusEffectType.Poison;

        public float Value { get; }                    // 每秒扣血量（例如 10 = 每秒扣 10 血）
        public float Duration { get; private set; }    // 剩餘持續時間
        public float TickRate => 1f;                   // 每 1 秒扣一次血
        public bool SupportsRefresh => true;

        private readonly float originalDuration;
        private readonly GameObject sourceObject;      // 用來判斷是否同一來源
        public GameObject Source { get => sourceObject; }
        private float damageTimer = 0f;                // 累積時間，用來控制扣血頻率

        public PoisonEffect(StatusEffectData data, GameObject source = null)
        {
            Value = data.value;                 // 這裡直接當作「每秒扣血量」
            originalDuration = data.duration;
            Duration = data.duration;
            sourceObject = source;
        }

        public void Apply(EnemyController target)
        {
        }

        public void Tick(EnemyController target, float deltaTime)
        {
            if (target.IsDead) return;

            // 持續時間倒數
            if (Duration > 0f)
            {
                Duration -= deltaTime;
                if (Duration <= 0f)
                {
                    target.RemoveStatusEffect(this);
                    return;
                }
            }

            // 扣血邏輯（每秒一次）
            damageTimer += deltaTime;
            if (damageTimer >= 1f)  // 每 1 秒扣一次（跟 TickRate 同步）
            {
                int damageThisTick = Mathf.FloorToInt(damageTimer * Value); // 支援非整數（如 8.5/秒）
                if (damageThisTick > 0)
                {
                    target.TakeDamage(new DamageObject(damageThisTick));
                }

                damageTimer -= damageThisTick / Value; // 精確剩餘時間
            }
        }

        public void Remove(EnemyController target)
        {
        }


        public void RefreshDuration()
        {
            Duration = originalDuration;
        }

        public bool IsFromSameSource(GameObject source)
        {
            return sourceObject == source;
        }
    }
}