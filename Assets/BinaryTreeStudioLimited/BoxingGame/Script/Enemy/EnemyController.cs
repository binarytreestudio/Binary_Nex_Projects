using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : Singleton<EnemyController>
{
    #region Enums
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
    }

    #endregion

    #region public Vars

    [Header("Enemy Settings")]
    [Tooltip("If false, disable enemy attack")][SerializeField] bool enemyAttack = false;
    [SerializeField] private Animator animator;
    [SerializeField] private float minimumReactionTime = 0.3f;
    [Range(0f, 1f)]
    [SerializeField] private float reduceReactionTimePercentagePerLevel = 0.01f;
    [SerializeField] private float enemyHealthIncreasePerLevel = 20f;
    [Range(0f, 1f)]
    [SerializeField] private float enemyAttackChanceIncreasePerHit = 0.03f;

    #endregion

    #region private Vars

    private EnemyType currentEnemyType;
    private float enemyHealth;
    private float enemyAttackDamage;
    private float enemyAttackDelay;
    private float enemyStandDuration;
    private bool attacked = false;
    [HideInInspector] public AttackPath playerAttackPath = AttackPath.None;
    private EnemyIncomingAttack enemyIncomingAttack = EnemyIncomingAttack.None;
    private float enemyStandTimer;
    private int enemyLevel = 1;
    private float enemyAttackChance;
    private bool gameStarted = false;

    #endregion

    #region Events

    public Action<AttackPath, float> OnEnemyStandSelected;
    public Action<EnemyIncomingAttack, float> OnEnemyAttackSelected;
    public Action<float> OnEnemyHPChanged;
    public Action<EnemyIncomingAttack, float> OnEnemyAttack;

    #endregion

    #region Start

    private void Start()
    {
        BattleManager.Instance.OnAttackSuccess += TakeDamage;
        BattleManager.Instance.OnGameStarted += OnGameStarted;
        EnemyManager.Instance.OnEnemyReset += InitEnemy;
    }

    #endregion

    #region OnDestroy

    protected override void OnDestroy()
    {
        BattleManager.Instance.OnAttackSuccess -= TakeDamage;
        BattleManager.Instance.OnGameStarted -= OnGameStarted;
        EnemyManager.Instance.OnEnemyReset -= InitEnemy;

        base.OnDestroy();
    }

    #endregion

    #region Update
    void Update()
    {
        if (!gameStarted) return;
        switch (playerAttackPath)
        {
            case AttackPath.None:
            case AttackPath.CrossFinisher:
                break;
            default:
                enemyStandTimer -= Time.deltaTime;
                if (enemyStandTimer <= 0)
                {
                    BattleManager.Instance.AttackFail();
                    EnemyRandom(1);
                }
                break;
        }
    }

    #endregion

    void OnGameStarted(bool started)
    {
        gameStarted = started;
    }

    void InitEnemy(EnemyType enemyType, int level)
    {
        enemyLevel = level;

        animator.Play("Idle");

        currentEnemyType = enemyType;

        enemyHealth = enemyType.maxHealth + enemyLevel * UnityEngine.Random.Range(enemyHealthIncreasePerLevel / 5, enemyHealthIncreasePerLevel);
        currentEnemyType.maxHealth = enemyHealth;
        OnEnemyHPChanged?.Invoke(enemyHealth / currentEnemyType.maxHealth);

        enemyAttackDamage = enemyType.attackDamage;
        enemyAttackDelay = Mathf.Max(minimumReactionTime, enemyType.attackDelay * (1 - ((enemyLevel - 1) * reduceReactionTimePercentagePerLevel)));
        enemyStandDuration = Mathf.Max(minimumReactionTime, enemyType.standDuration * (1 - ((enemyLevel - 1) * reduceReactionTimePercentagePerLevel)));

        enemyAttackChance = enemyType.attackChance;

        EnemyRandom(0);
    }

    #region TakeDamage

    void TakeDamage(BattleManager.HitType hitType, float damage)
    {
        enemyHealth -= damage;
        OnEnemyHPChanged?.Invoke(enemyHealth / currentEnemyType.maxHealth);

        if (enemyHealth > 0)
        {
            animator.SetTrigger("Hit");
            EnemyRandom();
        }
        else
        {
            Die();
        }

    }

    void Die()
    {
        animator.SetTrigger("Die");
        playerAttackPath = AttackPath.None;
        enemyIncomingAttack = EnemyIncomingAttack.None;
    }

    #endregion

    #region Enemy Attack

    void EnemyRandom(int actionOverrided = -1)      // -1: random, 0: stand, 1: attack
    {
        float i = UnityEngine.Random.Range(0.01f, 1.00f);
        if ((!enemyAttack || attacked || enemyHealth <= (int)BattleManager.HitType.Finisher || i > enemyAttackChance || actionOverrided == 0) && actionOverrided != 1)
        {
            enemyIncomingAttack = EnemyIncomingAttack.None;
            if (enemyHealth > (int)BattleManager.HitType.Finisher)
            {
                int randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length - 1);
                while (randomPath == (int)playerAttackPath)
                {
                    randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length - 1);
                }
                playerAttackPath = (AttackPath)randomPath;
                OnEnemyStandSelected?.Invoke(playerAttackPath, enemyStandDuration);
            }
            else
            {
                playerAttackPath = AttackPath.CrossFinisher;
                OnEnemyStandSelected?.Invoke(playerAttackPath, -1);
            }
            attacked = false;
            enemyStandTimer = enemyStandDuration;

            enemyAttackChance += enemyAttackChanceIncreasePerHit;
        }
        else
        {
            playerAttackPath = AttackPath.None;
            enemyIncomingAttack = (EnemyIncomingAttack)UnityEngine.Random.Range(1, Enum.GetValues(typeof(EnemyIncomingAttack)).Length);
            OnEnemyAttackSelected?.Invoke(enemyIncomingAttack, enemyAttackDelay);
            StartCoroutine(EnemyAttackCoroutine());
            BattleManager.Instance.enemyAttacking = true;
            attacked = true;

            animator.SetTrigger("Attack");
            animator.SetFloat("AttackSpeed", 2.167f / enemyAttackDelay); // 2.167f is the base attack animation duration
            animator.SetBool("MirrorAttack", enemyIncomingAttack == EnemyIncomingAttack.Right);

            enemyAttackChance = currentEnemyType.attackChance;
        }
    }

    IEnumerator EnemyAttackCoroutine()
    {
        yield return new WaitForSeconds(enemyAttackDelay);
        OnEnemyAttack?.Invoke(enemyIncomingAttack, enemyAttackDamage);
        EnemyRandom();
    }

    #endregion
}
