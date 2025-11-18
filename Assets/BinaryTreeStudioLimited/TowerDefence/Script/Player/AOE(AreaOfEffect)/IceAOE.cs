using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
namespace TowerDefence
{
    public class IceAOE : AreaOfEffectBase
    {
        [SerializeField] private float slowPercentage = 0.2f; // 20% slow
        [SerializeField] private float slowDuration = 2f; // Slow lasts for 2 seconds

        protected override void OnEnemyEnter(EnemyController enemy)
        {
            if (enemy == null) return;
            //enemy.ApplySlow(slowPercentage);
        }

        protected override void OnEnemyTick(EnemyController enemy)
        {
            if (enemy == null) return;
            enemy.TakeDamage(1);
            //enemy.ApplySlow(slowPercentage, slowDuration);
        }
        protected override void OnEnemyExit(EnemyController enemy)
        {
            if (enemy == null) return;
            //enemy.ResetDefault(EnemyStats.Speed);
        }
    }
}