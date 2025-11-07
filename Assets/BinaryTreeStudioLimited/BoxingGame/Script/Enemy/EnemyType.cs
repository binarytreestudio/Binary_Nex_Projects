using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Scriptable Objects/EnemyType")]
public class EnemyType : ScriptableObject
{
    public float maxHealth;
    public float attackDamage;
    public float attackDelay;
    [Tooltip("Duration the player can attack")]
    public float standDuration;
    [Range(0f, 1f)]
    public float attackChance;
}
