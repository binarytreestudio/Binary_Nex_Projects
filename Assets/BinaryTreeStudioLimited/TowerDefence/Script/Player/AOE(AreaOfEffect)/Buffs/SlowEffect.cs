// SlowEffect.cs
using UnityEngine;

namespace TowerDefence
{
    public class SlowEffect : IStatusEffect
    {
        public StatusEffectType Type => StatusEffectType.Slow;
        public float Value { get; }          // 減速比例，例如 0.4f = 減 40%
        public float Duration { get; private set; } // 剩餘持續時間
        public float TickRate => 0.2f;       // 固定 0.2 秒檢查一次

        public bool SupportsRefresh => true;

        public float originalDuration;
        public GameObject sourceObject;
        public GameObject Source { get => sourceObject; }
        public SlowEffect(StatusEffectData data, GameObject source = null)
        {
            Value = Mathf.Clamp01(data.value);
            originalDuration = data.duration;
            Duration = data.duration;
            sourceObject = source;
        }

        public void Apply(EnemyController target)
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

        public void Remove(EnemyController target)
        {
            RecalculateSpeed(target);
        }

        // 🔥 關鍵：支援持續刷新時間
        public void RefreshDuration()
        {
            Duration = originalDuration;
        }


        // 靜態方法：重新計算最終速度
        public static void RecalculateSpeed(EnemyController target)
        {
            if (!target.activeEffects.TryGetValue(StatusEffectType.Slow, out var slowEffects) || slowEffects.Count == 0)
            {
                target.ModifiedStats.speed = target.BaseStats.speed;
                return;
            }

            float strongestSlow = 0f;
            foreach (var effect in slowEffects)
            {
                if (effect is SlowEffect slow)
                {
                    strongestSlow = Mathf.Max(strongestSlow, slow.Value);
                }
            }

            float multiplier = Mathf.Max(1f - strongestSlow, 0.05f);
            target.ModifiedStats.speed = target.BaseStats.speed * multiplier;
        }
    }
}