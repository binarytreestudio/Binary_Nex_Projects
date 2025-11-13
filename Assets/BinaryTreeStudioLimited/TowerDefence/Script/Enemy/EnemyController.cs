using UnityEngine;

namespace TowerDefence
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Hit Particle Effect")]
        [SerializeField] GameObject hitParticleEffect;
        [SerializeField] float hitParticleEffectYOffset = 2f;

        [Header("Fly away Settings")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float force = 5f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private float health;
        private float speed;
        private float damage = 10f;
        private bool isDead = false;

        public void Init(float hp, float spd, float dmg)
        {
            health = hp;
            speed = spd;
            damage = dmg;
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

        public void TakeDamage(float dmg)
        {
            if (isDead)
                return;
            health -= dmg;
            animator.SetTrigger("Hit");
            if (health <= 0)
            {
                FlyAway();
            }
        }

        private void FlyAway()
        {
            isDead = true;

            Instantiate(hitParticleEffect, transform.position + Vector3.up * hitParticleEffectYOffset, Quaternion.identity);

            Vector3 randomUpperDirection = Random.onUnitSphere;
            randomUpperDirection.y = Mathf.Abs(randomUpperDirection.y); // Force y to be positive
            randomUpperDirection.z = Mathf.Abs(randomUpperDirection.z); // Force z to be positive

            rb.AddForce(randomUpperDirection.normalized * force, ForceMode.Impulse);
            rb.useGravity = true;

            animator.SetTrigger("Die");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isDead)
                return;
            if (collision.gameObject.CompareTag("Enemy"))
            {
                FlyAway();
            }
        }
    }
}
