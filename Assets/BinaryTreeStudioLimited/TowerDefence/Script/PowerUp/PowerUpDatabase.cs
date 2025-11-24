using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TowerDefence
{
    [CreateAssetMenu(fileName = "PowerUpDatabase", menuName = "Scriptable Objects/TowerDefence/PowerUpDatabase")]
    public class PowerUpDatabase : ScriptableObject
    {
        [Serializable]
        public struct PowerUpData
        {
            public PowerUpType powerUpType;
            public Sprite icon;
            public string powerUpName;
            public string description;
            public int maxStack;
        }

        [Serializable]
        public enum PowerUpType
        {
            RecoverHP = -50,

            NormalPunch = 0,

            Ice = 10,
            Poison,
            FireballCount,
            Stone,
        }

        [SerializeField] private List<PowerUpData> powerUpDataList = new();
        [SerializeField] private List<PowerUpType> rewardList_level1 = new();

        public PowerUpData GetPowerUpData(PowerUpType powerUpType)
        {
            return powerUpDataList.Find(data => data.powerUpType == powerUpType);
        }

        public List<PowerUpType> RandomPowerUps(int amount)
        {
            //List<PowerUpType> randomPowerUpTypes = new();
            //for (int i = 0; i < amount; i++)
            //{
            //    var randomPowerUpType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(PowerUpType)).Length);
            //    bool fullHealthButRecovery = !PlayerManager.Instance.IsPlayerDamaged() && (PowerUpType)randomPowerUpType == PowerUpType.RecoverHP;

            //    while (randomPowerUpTypes.Contains((PowerUpType)randomPowerUpType) || fullHealthButRecovery)
            //    {
            //        randomPowerUpType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(PowerUpType)).Length);
            //        fullHealthButRecovery = !PlayerManager.Instance.IsPlayerDamaged() && (PowerUpType)randomPowerUpType == PowerUpType.RecoverHP;
            //    }
            //    randomPowerUpTypes.Add((PowerUpType)randomPowerUpType);
            //}
            //return randomPowerUpTypes;
            List<PowerUpType> result = rewardList_level1.DeepClone();
            bool PlayerDamaged = PlayerManager.Instance.IsPlayerDamaged();
            if (PlayerDamaged)
                result.Add(PowerUpType.RecoverHP);

            for (int i = 0; amount < result.Count; i++)
                result.Pop(UnityEngine.Random.Range(0, result.Count - 1));

            return result;
        }
    }
}