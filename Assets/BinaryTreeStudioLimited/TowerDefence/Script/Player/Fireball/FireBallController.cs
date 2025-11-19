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
        [SerializeField] private GameObject powerUpAOEPrefab;

        private int powerUp;

        void Update()
        {
            transform.Translate(Vector3.forward * travelSpeed * Time.deltaTime);
            if (transform.position.z >= 100f)
            {
                Destroy(gameObject);
            }
        }

        public void Init(float speed, int damage, int power)
        {
            damageSetting.damage = damage;
            travelSpeed = speed;
            powerUp = power;
            switch (powerUp)
            {
                case (int)PowerUpDatabase.PowerUpType.Ice:
                    iceEffect.SetActive(true);
                    break;
                case (int)PowerUpDatabase.PowerUpType.Poison:
                    poisonEffect.SetActive(true);
                    break;
                case (int)PowerUpDatabase.PowerUpType.Stone:
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
            if (powerUp != -1)
            {
                GameObject aoeObject = Instantiate(powerUpAOEPrefab, transform.position, Quaternion.identity);
                aoeObject.transform.position = new Vector3(aoeObject.transform.position.x, 0f, aoeObject.transform.position.z);
                AOEController aoeController = aoeObject.GetComponent<AOEController>();
                aoeController.Init((PowerUpDatabase.PowerUpType)powerUp);
            }
            if (powerUp != (int)PowerUpDatabase.PowerUpType.Stone)
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
