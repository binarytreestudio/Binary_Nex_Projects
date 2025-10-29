using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum AttackPath
    {
        None,
        LeftHook,
        RightHook,
        Uppercut,
        CrossFinisher,
    }
    public enum EnemyIncomingAttack
    {
        None,
        Left,
        Right,
        Up
    }

    [Tooltip("If false, disable enemy attack")]
    public bool enemyAttack = false;
    [Tooltip("Delay before enemy attack executes")]
    [SerializeField] private float enemyMaxHealth = 100;
    private float enemyHealth;
    public float EnemyHealth => enemyHealth;

    [SerializeField] private float enemyAttackDelay = 2.0f;
    private bool attacked = false;
    public AttackPath playerAttackPath = AttackPath.None;
    [SerializeField] private TMPro.TextMeshProUGUI enemyHealthText;
    private EnemyIncomingAttack enemyIncomingAttack = EnemyIncomingAttack.None;

    private void Start()
    {
        enemyHealth = enemyMaxHealth;
    }

    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        if (enemyHealth > 0)
        {
            enemyHealthText.text = "Enemy HP: " + enemyHealth.ToString("F1");
        }
        else
        {
            Die();

            //prototype infinite health
            enemyHealth = enemyMaxHealth;
            enemyHealthText.text = "Enemy HP: " + enemyHealth.ToString("F1");
            enemyHealthText.text += "\nEnemy Defeated!";
        }
        EnemyRandom();
    }

    void Die()
    {

    }

    public void EnemyRandom()
    {
        int i = UnityEngine.Random.Range(0, 100);
        if (enemyAttack ? (attacked ? i < 100 : i < BattleManager.Instance.playerAttackChance) : i < 100)
        {
            enemyIncomingAttack = EnemyIncomingAttack.None;
            int randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length);
            while (randomPath == (int)playerAttackPath)
            {
                randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length);
            }
            playerAttackPath = (AttackPath)randomPath;
            AttackPathManager.Instance.ShowAttackIndicator(playerAttackPath);
            attacked = false;
        }
        else
        {
            playerAttackPath = AttackPath.None;
            enemyIncomingAttack = (EnemyIncomingAttack)UnityEngine.Random.Range(1, Enum.GetValues(typeof(EnemyIncomingAttack)).Length);
            EnemyAttackIndicatorManager.Instance.Show(enemyIncomingAttack, enemyAttackDelay);
            StartCoroutine(EnemyAttackCoroutine());
            BattleManager.Instance.enemyAttacking = true;
            attacked = true;
        }
    }

    IEnumerator EnemyAttackCoroutine()
    {
        yield return new WaitForSeconds(enemyAttackDelay);
        BattleManager.Instance.EnemyAttack(enemyIncomingAttack);
    }
}
