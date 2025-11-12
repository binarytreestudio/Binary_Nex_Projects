using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpDatabase", menuName = "Scriptable Objects/PowerUpDatabase")]
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
        Strength,
        DoubleDamage,
        Shield,
        RecoverHP,
        IncreaseMaxHP,
    }

    [SerializeField] private List<PowerUpData> powerUpDataList = new();

    public PowerUpData GetPowerUpData(PowerUpType powerUpType)
    {
        return powerUpDataList.Find(data => data.powerUpType == powerUpType);
    }
}
