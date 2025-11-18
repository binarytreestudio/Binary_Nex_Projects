using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class PowerUpManager : Singleton<PowerUpManager>
    {
        [SerializeField] private PowerUpDatabase powerUpDatabase;

        //public List<PowerUpDatabase.PowerUpType> RandomPowerUps(int amount)
        //{
        //    UIManager.Instance.ShowPowerUpPanel();
        //    List<PowerUpDatabase.PowerUpType> randomPowerUpTypes = new();
        //    for (int i = 0; i < amount; i++)
        //    {
        //        var randomPowerUpType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(PowerUpDatabase.PowerUpType)).Length);
        //        bool fullHealthButRecovery = !PlayerManager.Instance.IsPlayerDamaged() && (PowerUpDatabase.PowerUpType)randomPowerUpType == PowerUpDatabase.PowerUpType.RecoverHP;
        //
        //        while (randomPowerUpTypes.Contains((PowerUpDatabase.PowerUpType)randomPowerUpType) || fullHealthButRecovery)
        //        {
        //            randomPowerUpType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(PowerUpDatabase.PowerUpType)).Length);
        //            fullHealthButRecovery = !PlayerManager.Instance.IsPlayerDamaged() && (PowerUpDatabase.PowerUpType)randomPowerUpType == PowerUpDatabase.PowerUpType.RecoverHP;
        //        }
        //        randomPowerUpTypes.Add((PowerUpDatabase.PowerUpType)randomPowerUpType);
        //    }
        //    return randomPowerUpTypes;
        //}
    }
}