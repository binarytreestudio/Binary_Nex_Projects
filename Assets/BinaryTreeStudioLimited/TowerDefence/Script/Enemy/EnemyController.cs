using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Hit Particle Effect")]
        [SerializeField] private GameObject hitParticleEffect;
        [SerializeField] private GameObject deathParticleEffect;
        [SerializeField] private float particleEffectYOffset = 1f;

        [Header("State")]
        [SerializeField] private int health;
        [SerializeField] private float speed;
        [SerializeField] private float damage;
        [Tooltip("Value between 0 and 1 representing percentage increase")][Range(0f, 1f)][SerializeField] private int healthIncreasePerLevel;
        [Tooltip("Value between 0 and 1 representing percentage increase")][Range(0f, 1f)][SerializeField] private float speedIncreasePerLevel;
        [Tooltip("Value between 0 and 1 representing percentage increase")][Range(0f, 1f)][SerializeField] private float damageIncreasePerLevel;


        [Header("Fly away Settings")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float force = 5f;
        [SerializeField] private int maxCollisions = 3;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private bool isDead = false;
        private int collidedCount = 0;
        private bool brokeThrough = false;
        private List<GameObject> lanes;
        private int laneIndex = 0;

        public void Init(int level, List<GameObject> lanes)
        {
            //health = Mathf.CeilToInt(health * (1 + healthIncreasePerLevel * (level - 1)));
            speed = speed * (1 + speedIncreasePerLevel * (level - 1));
            damage = damage * (1 + damageIncreasePerLevel * (level - 1));

            this.lanes = lanes;
        }

        void Update()
        {
            if (isDead)
            {
                if (transform.position.y < -10f)
                {
                    Destroy(gameObject);
                }
                return;
            }

            Vector3 destination = lanes[laneIndex].transform.Find("Destination").position;
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            transform.LookAt(destination);
            if (Vector3.Distance(transform.position, destination) < 0.1f)
            {
                laneIndex++;
                if (laneIndex >= lanes.Count)
                    laneIndex = lanes.Count - 1;
            }


            if (Vector3.Distance(transform.position, lanes[lanes.Count - 1].transform.Find("Destination").position) < 0.1f && !brokeThrough)
            {
                PlayerManager.Instance.PlayerTakeDamage(damage);
                AudioManager.Instance.PlayPlayerHurtAudio();
                animator.SetTrigger("Jump");
                Destroy(gameObject, 1f);

                brokeThrough = true;
                //BreakThroughPanelController.Instance.SpawnBreakThroughEnemies(lane, enemyType);
            }
        }

        public void TakeDamage(int dmg)
        {
            if (isDead)
                return;
            health -= dmg;
            if (health <= 0)
            {
                isDead = true;
                gameObject.tag = "Corpse";

                Vector3 randomUpperDirection = Random.onUnitSphere;
                randomUpperDirection.y = Mathf.Abs(randomUpperDirection.y); // Force y to be positive
                randomUpperDirection.z = Mathf.Abs(randomUpperDirection.z); // Force z to be positive

                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(randomUpperDirection.normalized * force, ForceMode.Impulse);
                rb.useGravity = true;

                animator.SetTrigger("Die");
                Instantiate(deathParticleEffect, transform.position + Vector3.up * particleEffectYOffset, Quaternion.identity);
                AudioManager.Instance.PlayCriticalHitAudio();

                return;
            }
            Instantiate(hitParticleEffect, transform.position + Vector3.up * particleEffectYOffset, Quaternion.identity);
            AudioManager.Instance.PlayNormalHitAudio();
        }

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
                TakeDamage(1);
            }
        }

        private void OnDestroy()
        {
            EnemyManager.Instance.OnEnemyDefeated(this);
        }
    }
}
