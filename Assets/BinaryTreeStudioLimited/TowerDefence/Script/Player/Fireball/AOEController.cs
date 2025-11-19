using UnityEngine;

namespace TowerDefence
{
    public class AOEController : MonoBehaviour
    {
        [Header("Effects")]
        [SerializeField] private GameObject iceAOEEffect;
        [SerializeField] private GameObject poisonAOEEffect;

        private PowerUpDatabase.PowerUpType powerUpType;

        public void Init(PowerUpDatabase.PowerUpType powerUpType)
        {
            this.powerUpType = powerUpType;

            switch (powerUpType)
            {
                case PowerUpDatabase.PowerUpType.Ice:
                    iceAOEEffect.SetActive(true);
                    gameObject.AddComponent<IceAOE>();
                    break;
                case PowerUpDatabase.PowerUpType.Poison:
                    poisonAOEEffect.SetActive(true);
                    gameObject.AddComponent<PoisonAOE>();
                    break;
                default:
                    break;
            }
        }
    }
}