using UnityEngine;

namespace TowerDefence
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Hit Particle Effect")]
        [SerializeField] GameObject hitParticleEffect;
        [SerializeField] float hitParticleEffectYOffset = 2f;

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

        public void Init(int level)
        {
            //health = Mathf.CeilToInt(health * (1 + healthIncreasePerLevel * (level - 1)));
            speed = speed * (1 + speedIncreasePerLevel * (level - 1));
            damage = damage * (1 + damageIncreasePerLevel * (level - 1));
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

            transform.Translate(Vector3.back * speed * Time.deltaTime);
            if (transform.position.z < 0)
            {
                PlayerManager.Instance.PlayerTakeDamage(damage);
                Destroy(gameObject);
            }
        }

        public void TakeDamage(int dmg)
        {
            if (isDead)
                return;
            health -= dmg;
            animator.SetTrigger("Hit");
            Instantiate(hitParticleEffect, transform.position + Vector3.up * hitParticleEffectYOffset, Quaternion.identity);
            if (health <= 0)
            {
                FlyAway();
            }
        }

        private void FlyAway()
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
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Corpse"))
            {
                if (isDead)
                {
                    collidedCount++;
                    if (collidedCount >= maxCollisions)
                        Destroy(gameObject);
                    return;
                }
                TakeDamage(1);
            }
        }
    }
}
