using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyController : Singleton<EnemyController>
{
    #region Enums
    public enum PlayerAttackPath
    {
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
    [SerializeField] private float enemyAttackChanceIncreasePerHit = 0.05f;

    #endregion

    #region private Vars

    private EnemyType currentEnemyType;
    private float enemyHealth;

    private float enemyAttackDamage;
    private float enemyAttackDelay;
    private float enemyAttackChance;
    private bool attacking;
    private PlayerAttackPath previousPlayerAttackPath = PlayerAttackPath.CrossFinisher;

    private float bubbleFrequency;
    private float bubbleDuration;
    private int bubbleLimit;
    private float bubbleTimer;
    private int bubbleCount = 0;
    private bool finisherBubbleActive = false;

    private bool gameStarted = false;
    private int playerCount;

    #endregion

    #region Events

    public Action<PlayerAttackPath, float> OnEnemyCreateBubble;     //float: bubble duration
    public Action<EnemyIncomingAttack, float> OnEnemyAttackSelected;    //float: attack delay
    public Action<float> OnEnemyHPChanged;    //float: health percentage
    public Action<EnemyIncomingAttack, float> OnEnemyAttack;    //float: attack damage

    #endregion

    #region Start

    private void Start()
    {
        BattleManager.Instance.OnPlayerAttackSuccess += TakeDamage;
        BattleManager.Instance.OnGameStarted += OnGameStarted;
        BattleManager.Instance.OnBubbleExpired += OnBubbleExpired;

        EnemyManager.Instance.OnEnemyReset += InitEnemy;
    }

    #endregion

    #region OnDestroy

    protected override void OnDestroy()
    {
        BattleManager.Instance.OnPlayerAttackSuccess -= TakeDamage;
        BattleManager.Instance.OnGameStarted -= OnGameStarted;
        BattleManager.Instance.OnBubbleExpired -= OnBubbleExpired;

        EnemyManager.Instance.OnEnemyReset -= InitEnemy;

        base.OnDestroy();
    }

    #endregion

    #region Update

    void Update()
    {
        if (!gameStarted || attacking || finisherBubbleActive || enemyHealth <= 0) return;

        if (bubbleCount <= 0)
        {
            bubbleCount = 0;
            EnemyRandom();
            return;
        }
        bubbleTimer -= Time.deltaTime;
        if (bubbleTimer <= 0)
        {
            if (bubbleCount < bubbleLimit)
            {
                EnemyRandom();
            }
        }
    }

    #endregion

    void OnGameStarted(int playerCount)
    {
        gameStarted = true;
        this.playerCount = playerCount;
    }

    void InitEnemy(EnemyType enemyType, int level)
    {
        animator.Play("Idle");

        currentEnemyType = enemyType;

        enemyHealth = currentEnemyType.maxHealth;
        OnEnemyHPChanged?.Invoke(enemyHealth / currentEnemyType.maxHealth);

        enemyAttackDamage = currentEnemyType.attackDamage;
        enemyAttackDelay = currentEnemyType.attackDelay;
        enemyAttackChance = currentEnemyType.attackChance;

        bubbleDuration = currentEnemyType.bubbleDuration;
        bubbleFrequency = currentEnemyType.bubbleFrequency; ;
        bubbleLimit = currentEnemyType.bubbleLimit;

        bubbleCount = 0;

        EnemyRandom(0);
    }

    #region TakeDamage

    void TakeDamage(int playerIndex, BattleManager.HitType hitType, float damage, PlayerAttackPath path)
    {
        enemyHealth -= damage;
        OnEnemyHPChanged?.Invoke(enemyHealth / currentEnemyType.maxHealth);
        bubbleCount--;

        if (enemyHealth > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Die");
        finisherBubbleActive = false;
    }

    #endregion

    #region Enemy Attack

    void EnemyRandom(int actionOverrided = -1)      //actionOverrided: -1: random, 0: stand, 1: attack
    {
        float i = UnityEngine.Random.Range(0.01f, 1.00f);
        if ((!enemyAttack || attacking || enemyHealth <= (int)BattleManager.HitType.Finisher || i > enemyAttackChance || actionOverrided == 0) && actionOverrided != 1)
        {
            if (enemyHealth > (int)BattleManager.HitType.Finisher)
            {
                int randomPath = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerAttackPath)).Length - 1);
                // Ensure not the same as previous path
                while ((PlayerAttackPath)randomPath == previousPlayerAttackPath)
                {
                    randomPath = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerAttackPath)).Length - 1);
                }
                previousPlayerAttackPath = (PlayerAttackPath)randomPath;

                OnEnemyCreateBubble?.Invoke((PlayerAttackPath)randomPath, bubbleDuration);
            }
            else
            {
                OnEnemyCreateBubble?.Invoke(PlayerAttackPath.CrossFinisher, -1);
                finisherBubbleActive = true;
            }
            enemyAttackChance += enemyAttackChanceIncreasePerHit;

            bubbleCount++;
        }
        else
        {
            EnemyIncomingAttack enemyIncomingAttack = (EnemyIncomingAttack)UnityEngine.Random.Range(1, Enum.GetValues(typeof(EnemyIncomingAttack)).Length);
            OnEnemyAttackSelected?.Invoke(enemyIncomingAttack, enemyAttackDelay);

            animator.SetTrigger("Attack");
            animator.SetFloat("AttackSpeedMultiplier", 2.167f / enemyAttackDelay); // 2.167f is the base attack animation duration
            animator.SetBool("MirrorAttack", enemyIncomingAttack == EnemyIncomingAttack.Right);
            BattleManager.Instance.enemyAttacking = true;
            enemyAttackChance = currentEnemyType.attackChance;
            attacking = true;
            DOVirtual.DelayedCall(enemyAttackDelay, () =>
            {
                OnEnemyAttack?.Invoke(enemyIncomingAttack, enemyAttackDamage);
                EnemyRandom(0);
                attacking = false;
            });
        }
        bubbleTimer = bubbleFrequency;
    }

    #endregion

    void OnBubbleExpired(PlayerAttackPath path)
    {
        bubbleCount--;
        EnemyRandom(1);
    }

}
