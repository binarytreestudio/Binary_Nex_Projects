using System;
using UnityEngine;
namespace TowerDefence
{
    public class FireBallController : MonoBehaviour
    {
        [Header("Damage Setting")]
        [SerializeField] DamageObject damageSetting;
        [SerializeField] private float travelSpeed = 1;

        [Header("Effects")]
        [SerializeField] private GameObject fireballEffect;
        [SerializeField] private GameObject iceEffect;
        [SerializeField] private GameObject poisonEffect;
        [SerializeField] private GameObject rockEffect;

        [Header("Power Up AOE")]
        [SerializeField] private GameObject iceAOEPrefab;
        [SerializeField] private GameObject poisonAOEPrefab;

        private PowerUpDatabase.PowerUpType powerUp;

        void Update()
        {
            transform.Translate(Vector3.forward * travelSpeed * Time.deltaTime);
            if (transform.position.z >= 100f)
            {
                Destroy(gameObject);
            }
        }

        public void Init(float speed, int damage, PowerUpDatabase.PowerUpType power)
        {
            damageSetting.damage = damage;
            travelSpeed = speed;
            powerUp = power;
            switch (powerUp)
            {
                case PowerUpDatabase.PowerUpType.Ice:
                    iceEffect.SetActive(true);
                    break;
                case PowerUpDatabase.PowerUpType.Poison:
                    poisonEffect.SetActive(true);
                    break;
                case PowerUpDatabase.PowerUpType.Stone:
                    rockEffect.SetActive(true);
                    break;
                default:
                    fireballEffect.SetActive(true);
                    break;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Enemy"))
                return;
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy == null)
                return;
            enemy.TakeDamage(damageSetting);
            switch (powerUp)
            {
                case PowerUpDatabase.PowerUpType.Ice:
                    Instantiate(iceAOEPrefab, new Vector3(transform.position.x, 0f, transform.position.z), Quaternion.identity);
                    break;
                case PowerUpDatabase.PowerUpType.Poison:
                    Instantiate(poisonAOEPrefab, new Vector3(transform.position.x, 0f, transform.position.z), Quaternion.identity);
                    break;
            }
            if (powerUp != PowerUpDatabase.PowerUpType.Stone || enemy.BaseStats.health > 3)
            {
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
