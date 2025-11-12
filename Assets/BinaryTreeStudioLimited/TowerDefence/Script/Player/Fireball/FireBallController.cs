using UnityEngine;
namespace TowerDefence
{
    public class FireBallController : MonoBehaviour
    {
        float speed;
        float damage;

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            if (transform.position.z >= 100f)
            {
                Destroy(gameObject);
            }
        }

        public void Init(float speed, float damage)
        {
            this.speed = speed;
            this.damage = damage;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}
