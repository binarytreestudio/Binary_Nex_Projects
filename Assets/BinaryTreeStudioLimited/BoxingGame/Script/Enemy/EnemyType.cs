using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Scriptable Objects/EnemyType")]
public class EnemyType : ScriptableObject
{
    public float maxHealth;
    public float attackDamage;
    public float attackDelay;
    [Tooltip("How often the bubble appears in seconds")]
    public float bubbleFrequency;
    [Tooltip("how long the bubble lasts in seconds")]
    public float bubbleDuration;
    [Tooltip("Max number of bubbles")]
    public int bubbleLimit;
    [Range(0f, 1f)]
    public float attackChance;

    public EnemyType(EnemyType other)
    {
        maxHealth = other.maxHealth;
        attackDamage = other.attackDamage;
        attackDelay = other.attackDelay;
        bubbleFrequency = other.bubbleFrequency;
        bubbleDuration = other.bubbleDuration;
        bubbleLimit = other.bubbleLimit;
        attackChance = other.attackChance;
    }
}
