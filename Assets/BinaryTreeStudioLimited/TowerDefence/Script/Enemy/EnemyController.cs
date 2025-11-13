using UnityEngine;

namespace TowerDefence
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] GameObject hitParticleEffect;
        [SerializeField] float hitParticleEffectYOffset = 2f;

        private float health;
        private float speed;
        private float damage = 10f;

        public void Init(float hp, float spd, float dmg)
        {
            health = hp;
            speed = spd;
            damage = dmg;
        }

        void Update()
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);
            if (transform.position.z < 0)
            {
                PlayerManager.Instance.PlayerTakeDamage(damage);
                Destroy(gameObject);
            }
        }

        public void TakeDamage(float dmg)
        {
            health -= dmg;
            if (health <= 0)
            {
                Instantiate(hitParticleEffect, transform.position + Vector3.up * hitParticleEffectYOffset, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
