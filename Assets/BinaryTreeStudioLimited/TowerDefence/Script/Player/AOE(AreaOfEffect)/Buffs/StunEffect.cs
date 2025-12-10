using UnityEngine;
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
            // ��_��l�t�ס]�|�Q��L SlowEffect �����s�p��^
            target.ModifiedStats.speed = target.BaseStats.speed;

            // �� SlowEffect ���s�p��]�p�G�����ܡ^
            if (target.activeEffects.TryGetValue(StatusEffectType.Slow, out var slows))
            {
                SlowEffect.RecalculateSpeed(target); // �I�s SlowEffect ���R�A��k
            }
        }
        else
        {
            target.ModifiedStats.speed = 0f;
        }
    }
}

