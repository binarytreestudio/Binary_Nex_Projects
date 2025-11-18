using UnityEngine;

namespace TowerDefence
{
    public class StunEffect : IStatusEffect
    {
        public StatusEffectType Type => StatusEffectType.Stun;
        public float Value { get; } 
        public float Duration { get; private set; }
        public float TickRate => 0.1f; 

        public bool SupportsRefresh => true;      
        public GameObject Source { get; }    

        private readonly float originalDuration;

        public StunEffect(StatusEffectData data, GameObject source = null)
        {
            Value = data.value;
            originalDuration = data.duration;
            Duration = data.duration;
            Source = source;
        }

        public void Apply(EnemyController target)
        {
            target.ModifiedStats.speed = 0f;
        }

        public void Remove(EnemyController target)
        {
            RecalculateSpeed(target);
        }

        public void Tick(EnemyController target, float deltaTime)
        {
            if (Duration > 0f)
            {
                Duration -= deltaTime;
                if (Duration <= 0f)
                {
                    target.RemoveStatusEffect(this);
                }
            }
        }

        public void RefreshDuration()
        {
            Duration = originalDuration;
        }

        public static void RecalculateSpeed(EnemyController target)
        {
            bool hasStun = target.activeEffects.ContainsKey(StatusEffectType.Stun) &&
                           target.activeEffects[StatusEffectType.Stun].Count > 0;

            if (!hasStun)
            {
                // 恢復原始速度（會被其他 SlowEffect 等重新計算）
                target.ModifiedStats.speed = target.BaseStats.speed;

                // 讓 SlowEffect 重新計算（如果有的話）
                if (target.activeEffects.TryGetValue(StatusEffectType.Slow, out var slows))
                {
                    SlowEffect.RecalculateSpeed(target); // 呼叫 SlowEffect 的靜態方法
                }
            }
            else
            {
                target.ModifiedStats.speed = 0f;
            }
        }
    }
}