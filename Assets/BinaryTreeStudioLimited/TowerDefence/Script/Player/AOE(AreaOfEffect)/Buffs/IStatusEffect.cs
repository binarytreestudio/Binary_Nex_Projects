using UnityEngine;

namespace TowerDefence
{
    public interface IStatusEffect
    {
        StatusEffectType Type { get; }
        float Value { get; }
        float Duration { get; }
        float TickRate { get; }

        void Apply(EnemyController target);

        void Tick(EnemyController target, float deltaTime);

        void Remove(EnemyController target);

        bool SupportsRefresh { get; }
        void RefreshDuration();
        GameObject Source { get; }
    }
}