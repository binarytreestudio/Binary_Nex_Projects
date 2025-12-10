using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace TowerDefence
{
    public enum EnemyStatsType
    {
        None = 0,
        Speed = 5,
        All = 100,
    }

    [Serializable]
    public class EnemyStats
    {
        public int health;
        public float speed;
        public float damage;
    }

    public class EnemyController : MonoBehaviour
    {
        [Header("Hit Particle Effect")]
        [SerializeField] private float particleEffectYOffset = 1f;

        [Header("Level Scaling")]
        [Tooltip("Value between 0 and 1 representing percentage increase")]
        //[Range(0f, 1f)][SerializeField] private float healthIncreasePerLevel = 0.15f;
        [Range(0f, 1f)][SerializeField] private float speedIncreasePerLevel = 0.05f;
        [Range(0f, 1f)][SerializeField] private float damageIncreasePerLevel = 0.1f;

        [Header("Stats")]
        [SerializeField] private EnemyStats baseStats;
        [SerializeField] private EnemyStats modifiedStats;
        [SerializeField] private StatusParticleController buffController;

        [Header("Fly away Settings")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float force = 5f;
        [SerializeField] private int maxCollisions = 3;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        // ==================== 狀態效果系統 ====================
        public SerializedDictionary<StatusEffectType, List<IStatusEffect>> activeEffects = new();
        [SerializeField] private float statusEffectTickTimer = 0f;

        // ==================== 內部狀態 ====================
        private bool isDead = false;
        private int collidedCount = 0;
        private bool brokeThrough = false;

        private List<GameObject> lanes;
        private int laneIndex = 0;
        private Transform laneDestination;

        // =========================================================
        // 初始化（由 EnemyManager 呼叫）straight lane logic
        // =========================================================
        public void Init(int level, GameObject lane)
        {
            laneDestination = lane.transform.Find("Destination");

            // 計算等級加成
            float levelMultiplier = level - 1;

            //baseStats.health = Mathf.CeilToInt(baseStats.health * (1f + healthIncreasePerLevel * levelMultiplier));
            baseStats.speed = baseStats.speed * (1f + speedIncreasePerLevel * levelMultiplier);
            baseStats.damage = baseStats.damage * (1f + damageIncreasePerLevel * levelMultiplier);

            ResetModifiedStats();
        }


        // =========================================================
        // 初始化（由 EnemyManager 呼叫）S lane logic
        // =========================================================
        public void Init(int level, List<GameObject> lanes)
        {
            this.lanes = lanes;

            // 計算等級加成
            float levelMultiplier = level - 1;

            //baseStats.health = Mathf.CeilToInt(baseStats.health * (1f + healthIncreasePerLevel * levelMultiplier));
            baseStats.speed = baseStats.speed * (1f + speedIncreasePerLevel * levelMultiplier);
            baseStats.damage = baseStats.damage * (1f + damageIncreasePerLevel * levelMultiplier);

            ResetModifiedStats();
        }

        private void ResetModifiedStats()
        {
            int currentHealth = modifiedStats.health; // 保留當前血量
            modifiedStats = baseStats.DeepClone();
            modifiedStats.health = currentHealth > 0 ? currentHealth : modifiedStats.health;
        }

        // =========================================================
        // Unity 生命周期
        // =========================================================
        private void Update()
        {
            if (isDead)
            {
                if (transform.position.y < -10f)
                    Destroy(gameObject);
                return;
            }

            // 狀態效果 Tick
            UpdateStatusEffects();

            // 移動邏輯

            switch (BattleManager.Instance.LaneType)
            {
                case BattleManager.LaneSetting.Straight:
                    break;
                case BattleManager.LaneSetting.SShape:
                    laneDestination = lanes[Mathf.Min(laneIndex, lanes.Count - 1)]
                    .transform.Find("Destination");
                    break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                laneDestination.position,
                modifiedStats.speed * Time.deltaTime);

            if (!brokeThrough)
                transform.LookAt(laneDestination);

            switch (BattleManager.Instance.LaneType)
            {
                case BattleManager.LaneSetting.Straight:
                    if (Vector3.Distance(transform.position, laneDestination.position) < 0.1f && !brokeThrough)
                    {
                        brokeThrough = true;
                        PlayerManager.Instance.PlayerTakeDamage(modifiedStats.damage);
                        animator?.SetTrigger("Jump");
                        transform.DOJump(transform.position + transform.forward * 2f, 1.5f, 1, 1f).OnComplete(() =>
                        {
                            Destroy(gameObject);
                        });
                    }
                    break;
                case BattleManager.LaneSetting.SShape:
                    if (Vector3.Distance(transform.position, laneDestination.position) < 0.1f)
                    {
                        laneIndex++;
                    }
                    var finalDest = lanes[lanes.Count - 1].transform.Find("Destination").position;
                    if (Vector3.Distance(transform.position, finalDest) < 0.1f && !brokeThrough)
                    {
                        brokeThrough = true;
                        PlayerManager.Instance.PlayerTakeDamage(modifiedStats.damage);
                        animator?.SetTrigger("Jump");
                        Destroy(gameObject, 1f);
                    }
                    break;
            }
        }

        private void LateUpdate()
        {
            // Clear null effects
            foreach (var list in activeEffects.Values)
            {
                list.RemoveAll(e => e == null);
            }
        }

        // =========================================================
        // Core Of Status Effect System
        // =========================================================
        private void UpdateStatusEffects()
        {
            if (activeEffects.Count == 0) return;

            statusEffectTickTimer += Time.deltaTime;
            if (statusEffectTickTimer < 0.1f) return; // 每 0.1 秒統一 Tick 一次

            float delta = statusEffectTickTimer;
            statusEffectTickTimer = 0f;

            var effectsToProcess = new List<(StatusEffectType type, IStatusEffect effect)>();

            foreach (var kvp in activeEffects)
            {
                var type = kvp.Key;
                var list = kvp.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    var effect = list[i];
                    if (effect != null)
                    {
                        effectsToProcess.Add((type, effect));
                    }
                }
            }

            foreach (var (type, effect) in effectsToProcess)
            {
                effect.Tick(this, delta);
            }

            // 清理空的列表（可選，保持乾淨）
            var emptyKeys = new List<StatusEffectType>();
            foreach (var kvp in activeEffects)
            {
                if (kvp.Value.Count == 0 || kvp.Value.TrueForAll(e => e == null))
                    emptyKeys.Add(kvp.Key);
            }
            foreach (var key in emptyKeys)
            {
                activeEffects.Remove(key);
            }
        }

        public void ApplyStatusEffect(IStatusEffect effect)
        {
            if (isDead || effect == null) return;

            var type = effect.Type;

            if (!activeEffects.ContainsKey(type))
                activeEffects[type] = new List<IStatusEffect>();

            // Check for existing effect of the same type
            if (activeEffects[type].Contains(effect))
                return;

            activeEffects[type].Add(effect);
            effect.Apply(this);
            Debug.LogError("apply effect: " + type.ToString());
            buffController.ShowBuff(type);
        }

        public void RemoveStatusEffect(IStatusEffect effect)
        {
            if (effect == null) return;

            var type = effect.Type;
            if (activeEffects.TryGetValue(type, out var list))
            {
                if (list.Remove(effect))
                {
                    effect.Remove(this);

                    if (list.Count == 0)
                        activeEffects.Remove(type);
                }
            }
            Debug.LogError("hide effect: " + type.ToString());
            buffController.HideBuff(type);
        }

        public void ClearStatusEffect(StatusEffectType type = StatusEffectType.None)
        {
            if (type == StatusEffectType.None)
            {
                foreach (var kvp in new Dictionary<StatusEffectType, List<IStatusEffect>>(activeEffects))
                {
                    foreach (var effect in kvp.Value)
                        effect.Remove(this);
                }
                activeEffects.Clear();
                return;
            }

            if (activeEffects.TryGetValue(type, out var list))
            {
                foreach (var effect in list)
                    effect.Remove(this);
                activeEffects.Remove(type);
            }
        }

        // =========================================================
        // Damage and Death
        // =========================================================
        public void TakeDamage(DamageObject damageObject)
        {
            if (isDead) return;

            modifiedStats.health -= damageObject.damage;

            if (modifiedStats.health <= 0)
            {
                Die();
                return;
            }

            //Instantiate(hitParticleEffect, transform.position + Vector3.up * particleEffectYOffset, Quaternion.identity);
            ObjectPoolManager.Instance.GetEnemyHitParticle().transform.position = transform.position + Vector3.up * particleEffectYOffset;
            AudioManager.Instance.PlayNormalHitAudio();
        }

        private void Die()
        {
            isDead = true;
            gameObject.tag = "Corpse";

            AddExposiveForce();

            animator?.SetTrigger("Die");
            //Instantiate(deathParticleEffect, transform.position + Vector3.up * particleEffectYOffset, Quaternion.identity);
            ObjectPoolManager.Instance.GetEnemyDeathParticle().transform.position = transform.position + Vector3.up * particleEffectYOffset;
            AudioManager.Instance.PlayCriticalHitAudio();

            // Clear all status effects
            ClearStatusEffect();
        }

        private void AddExposiveForce()
        {
            Vector3 randomUpperDirection = UnityEngine.Random.onUnitSphere;
            randomUpperDirection.y = Mathf.Abs(randomUpperDirection.y);
            randomUpperDirection.z = Mathf.Abs(randomUpperDirection.z);

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.AddForce(randomUpperDirection.normalized * force, ForceMode.Impulse);
        }

        // =========================================================
        // Collision 
        // =========================================================
        private void OnCollisionEnter(Collision collision)
        {
            if (isDead)
            {
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    collidedCount++;
                    if (collidedCount >= maxCollisions)
                        Destroy(gameObject);
                }
                return;
            }

            if (collision.gameObject.CompareTag("Corpse"))
            {
                TakeDamage(new DamageObject(1));
            }
        }

        private void OnDestroy()
        {
            EnemyManager.Instance?.OnEnemyDefeated(this);
        }

        // =========================================================
        // 公開屬性（給其他系統讀取）
        // =========================================================
        public bool IsDead => isDead;
        public EnemyStats BaseStats { get => baseStats; }
        public EnemyStats ModifiedStats { get => modifiedStats; }
    }
}