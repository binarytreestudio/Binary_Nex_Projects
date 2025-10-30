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
        Up
    }

    [Header("Enemy Settings")]
    [SerializeField] private float enemyMaxHealth = 100;
    private float enemyHealth;
    public float EnemyHealth => enemyHealth;
    [Tooltip("If false, disable enemy attack")]
    public bool enemyAttack = false;
    [Tooltip("Delay before enemy attack executes")]
    [SerializeField] private float enemyAttackDelay = 2.0f;
    [SerializeField] private float enemyStandDuration = 1.0f;
    [SerializeField] private Animator animator;

    private bool attacked = false;
    [HideInInspector] public AttackPath playerAttackPath = AttackPath.None;
    [SerializeField] private TMPro.TextMeshProUGUI enemyHealthText;
    private EnemyIncomingAttack enemyIncomingAttack = EnemyIncomingAttack.None;
    private float enemyStandTimer;

    private void Start()
    {
        BattleManager.Instance.OnGameStarted += OnGameStarted;
        enemyHealth = enemyMaxHealth;
    }

    void OnGameStarted(bool isStarted)
    {
        EnemyRandom();
    }

    void Update()
    {
        switch (playerAttackPath)
        {
            case AttackPath.None:
                break;
            default:
                enemyStandTimer -= Time.deltaTime;
                if (enemyStandTimer <= 0)
                {
                    BattleManager.Instance.AttackFail();
                    EnemyRandom();
                }
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        if (enemyHealth > 0)
        {
            enemyHealthText.text = "Enemy HP: " + enemyHealth.ToString("F1");
            animator.SetTrigger("Hit");
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

    void EnemyRandom()
    {
        int i = UnityEngine.Random.Range(0, 100);
        if (enemyAttack ? ((attacked || EnemyHealth <= 30) ? i < 100 : i < BattleManager.Instance.playerAttackChance) : i < 100)
        {
            enemyIncomingAttack = EnemyIncomingAttack.None;
            if (EnemyHealth > 30)
            {
                int randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length - 1);
                while (randomPath == (int)playerAttackPath)
                {
                    randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length - 1);
                }
                playerAttackPath = (AttackPath)randomPath;
            }
            else
            {
                playerAttackPath = AttackPath.CrossFinisher;
            }
            AttackPathIndicatorManager.Instance.ShowAttackIndicator(playerAttackPath, enemyStandDuration);
            attacked = false;
            enemyStandTimer = enemyStandDuration;
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
        EnemyRandom();
    }
}
