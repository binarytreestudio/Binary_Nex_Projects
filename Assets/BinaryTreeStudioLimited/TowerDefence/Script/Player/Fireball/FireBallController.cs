using System;
using UnityEngine;
namespace TowerDefence
{
    public class FireBallController : MonoBehaviour
    {
        [SerializeField] DamageObject damageSetting;
        [SerializeField] private float travelSpeed = 1;

        void Update()
        {
            transform.Translate(Vector3.back * travelSpeed * Time.deltaTime);
            if (transform.position.z <= -100f)
            {
                Destroy(gameObject);
            }
        }

        public void Init(float speed, int damage)
        {
            damageSetting.damage = damage;
            travelSpeed = speed;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damageSetting);
                }
                Destroy(gameObject);
            }
        }
    }
    [Serializable]
    public struct DamageObject
    {
        public DamageObject(int damage)
        {
            this.damage = damage;
        }

        public int damage;
    }
}
