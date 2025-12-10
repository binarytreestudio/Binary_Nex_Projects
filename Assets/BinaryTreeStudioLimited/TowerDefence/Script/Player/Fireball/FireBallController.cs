using System;
using UnityEngine;
public class FireBallController : MonoBehaviour
{
    [Header("Damage Setting")]
    [SerializeField] DamageObject damageSetting;
    [SerializeField] private float travelSpeed = 1;
    [SerializeField] private float limitZ = 12;

    [Header("Effects")]
    [SerializeField] private GameObject fireballEffect;
    [SerializeField] private GameObject iceEffect;
    [SerializeField] private GameObject poisonEffect;
    [SerializeField] private GameObject rockEffect;

    private PowerUpDatabase.PowerUpType powerUp;

    void Update()
    {
        transform.Translate(Vector3.forward * travelSpeed * Time.deltaTime);
        if (transform.position.z >= limitZ)
        {
            PowerUpAOE();
            gameObject.SetActive(false);
        }
    }

    public void Init(float speed, int damage, PowerUpDatabase.PowerUpType power)
    {
        damageSetting.damage = damage;
        travelSpeed = speed;
        powerUp = power;
        fireballEffect.SetActive(false);
        iceEffect.SetActive(false);
        poisonEffect.SetActive(false);
        rockEffect.SetActive(false);
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
                limitZ = 100f;
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
        PowerUpAOE();
        if (powerUp != PowerUpDatabase.PowerUpType.Stone || enemy.BaseStats.health > 2)
        {
            gameObject.SetActive(false);
        }
    }

    private void PowerUpAOE()
    {
        switch (powerUp)
        {
            case PowerUpDatabase.PowerUpType.Ice:
                //Instantiate(iceAOEPrefab, new Vector3(transform.position.x, 0f, transform.position.z), Quaternion.identity);
                ObjectPoolManager.Instance.GetIceAOE().transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
                AudioManager.Instance.PlayIceAreaAudio();
                break;
            case PowerUpDatabase.PowerUpType.Poison:
                //Instantiate(poisonAOEPrefab, new Vector3(transform.position.x, 0f, transform.position.z), Quaternion.identity);
                ObjectPoolManager.Instance.GetPoisonAOE().transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
                AudioManager.Instance.PlayPoisonAreaAudio();
                break;
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

