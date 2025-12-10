using UnityEngine;

public enum StatusEffectType
{
    None = 0,

    // Debuff（負面效果）
    Slow = 1,      // 減速
    Burn = 2,      // 灼燒（持續扣血）
    Poison = 3,    // 中毒（持續扣血）
    Stun = 4,      // 暈眩（停止移動）

    // Buff（正面效果）


    // 其他
    AllDebuffs = 100,
    AllBuffs = 101
}

[System.Serializable]
public struct StatusEffectData
{
    public StatusEffectType type;
    public float value;      // 效果數值（減速40% = 0.4f）
    public float duration;   // 持續時間
    public float tickRate;   // Tick 頻率（灼燒每秒扣血）
}