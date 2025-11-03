using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : Singleton<EnemyController>
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
    }

    [Header("Enemy Settings")]
    [SerializeField] private float enemyMaxHealth = 100;
    private float enemyHealth;
    [Tooltip("If false, disable enemy attack")]
    public bool enemyAttack = false;
    [Tooltip("Delay before enemy attack executes")]
    [SerializeField] private float enemyAttackDelay = 2.0f;
    [SerializeField] private float enemyStandDuration = 1.0f;
    [SerializeField] private Animator animator;
    [SerializeField] private float delayBeforeReset = 7;

    private bool attacked = false;
    [HideInInspector] public AttackPath playerAttackPath = AttackPath.None;
    private EnemyIncomingAttack enemyIncomingAttack = EnemyIncomingAttack.None;
    private float enemyStandTimer;
    private int playerCombo = 0;
    private float runTimeStandTime;
    private float runTimeAttackDelay;

    public Action<AttackPath, float> OnEnemyStandSelected;
    public Action<EnemyIncomingAttack, float> OnEnemyAttackSelected;
    public Action<float> OnEnemyHPChanged;


    private void Start()
    {
        BattleManager.Instance.OnGameStarted += OnGameStarted;
        BattleManager.Instance.OnPlayerComboChanged += OnPlayerComboChanged;
        enemyHealth = enemyMaxHealth;
        OnEnemyHPChanged?.Invoke(enemyHealth / enemyMaxHealth);
    }

    void OnGameStarted(bool isStarted)
    {
        EnemyRandom();
    }

    void OnPlayerComboChanged(int combo)
    {
        playerCombo = combo;
        runTimeStandTime = Mathf.Max(0.5f, enemyStandDuration * (1 - playerCombo * 0.01f));
        runTimeAttackDelay = Mathf.Max(0.5f, enemyAttackDelay * (1 - playerCombo * 0.01f));
    }

    void Update()
    {
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

    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        OnEnemyHPChanged?.Invoke(enemyHealth / enemyMaxHealth);

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

        //prototype reset enemy health after a delay
        DOVirtual.DelayedCall(delayBeforeReset, () =>
        {
            animator.Play("Idle");
            enemyHealth = enemyMaxHealth;
            OnEnemyHPChanged?.Invoke(enemyHealth / enemyMaxHealth);
            EnemyRandom();
        });
    }

    void EnemyRandom(int actionOverrided = -1)      // -1: random, 0: stand, 1: attack
    {
        int i = UnityEngine.Random.Range(0, 100);
        if ((!enemyAttack || attacked || enemyHealth <= 30 || i < BattleManager.Instance.playerAttackChance || actionOverrided == 0) && actionOverrided != 1)
        {
            enemyIncomingAttack = EnemyIncomingAttack.None;
            if (enemyHealth > 30)
            {
                int randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length - 1);
                while (randomPath == (int)playerAttackPath)
                {
                    randomPath = UnityEngine.Random.Range(1, Enum.GetValues(typeof(AttackPath)).Length - 1);
                }
                playerAttackPath = (AttackPath)randomPath;
                OnEnemyStandSelected?.Invoke(playerAttackPath, runTimeStandTime);

            }
            else
            {
                playerAttackPath = AttackPath.CrossFinisher;
                OnEnemyStandSelected?.Invoke(playerAttackPath, -1);
            }
            attacked = false;
            enemyStandTimer = runTimeStandTime;
        }
        else
        {
            playerAttackPath = AttackPath.None;
            enemyIncomingAttack = (EnemyIncomingAttack)UnityEngine.Random.Range(1, Enum.GetValues(typeof(EnemyIncomingAttack)).Length);
            OnEnemyAttackSelected?.Invoke(enemyIncomingAttack, runTimeAttackDelay);
            StartCoroutine(EnemyAttackCoroutine());
            BattleManager.Instance.enemyAttacking = true;
            attacked = true;
        }
    }

    IEnumerator EnemyAttackCoroutine()
    {
        yield return new WaitForSeconds(runTimeAttackDelay);
        BattleManager.Instance.EnemyAttack(enemyIncomingAttack);
        EnemyRandom();
    }

    protected override void OnDestroy()
    {
        BattleManager.Instance.OnGameStarted -= OnGameStarted;
        BattleManager.Instance.OnPlayerComboChanged -= OnPlayerComboChanged;
        base.OnDestroy();
    }
}
