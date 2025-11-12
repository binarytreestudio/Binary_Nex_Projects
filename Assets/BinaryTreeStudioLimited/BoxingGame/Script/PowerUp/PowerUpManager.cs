using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : Singleton<PowerUpManager>
{
    [Serializable]
    public struct AppliedPowerUp
    {
        public PowerUpDatabase.PowerUpType powerUpType;
        public int stackCount;
    }
    [Serializable]
    public struct PlayerPowerUps
    {
        public int playerIndex;
        public List<AppliedPowerUp> appliedPowerUps;
    }

    [SerializeField] private PowerUpDatabase powerUpDatabase;

    [SerializeField] private List<PlayerPowerUps> playerPowerUpsList = new();

    void Start()
    {
        BattleManager.Instance.OnGameStarted += OnGameStarted;

        EnemyController.Instance.OnEnemyHPChanged += OnEnemyHPChanged;
    }
    protected override void OnDestroy()
    {
        BattleManager.Instance.OnGameStarted -= OnGameStarted;

        EnemyController.Instance.OnEnemyHPChanged -= OnEnemyHPChanged;

        base.OnDestroy();
    }
    void OnGameStarted(int playerCount)
    {
        playerPowerUpsList.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            PlayerPowerUps playerPowerUps = new PlayerPowerUps
            {
                playerIndex = i,
                appliedPowerUps = new List<AppliedPowerUp>()
            };
            playerPowerUpsList.Add(playerPowerUps);
        }
    }

    void OnEnemyHPChanged(float healthPercentage)
    {
        if (healthPercentage <= 0)
        {
            UIManager.Instance.ShowPowerUpPanel();
            List<PowerUpDatabase.PowerUpType> randomPowerUpTypes = new();
            for (int i = 0; i < 3; i++)
            {
                var randomPowerUpType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(PowerUpDatabase.PowerUpType)).Length);
                randomPowerUpType = (int)PowerUpDatabase.PowerUpType.DoubleDamage; // For testing purpose, always DoubleDamage
                bool fullHealthButRecovery = !PlayerManager.Instance.IsPlayerDamaged() && (PowerUpDatabase.PowerUpType)randomPowerUpType == PowerUpDatabase.PowerUpType.RecoverHP;
                bool powerUpAlreadyMaxStacked;
                var playerPowerUps = playerPowerUpsList[0]; // Assuming single player for now
                var appliedPowerUpIndex = playerPowerUps.appliedPowerUps.FindIndex(p => p.powerUpType == (PowerUpDatabase.PowerUpType)randomPowerUpType);
                if (appliedPowerUpIndex >= 0)
                {
                    var existing = playerPowerUps.appliedPowerUps[appliedPowerUpIndex];
                    int maxStack = powerUpDatabase.GetPowerUpData(existing.powerUpType).maxStack;
                    if (maxStack < 0)
                    {
                        maxStack = int.MaxValue;
                    }
                    powerUpAlreadyMaxStacked = existing.stackCount >= maxStack;
                }
                else
                {
                    powerUpAlreadyMaxStacked = false;
                }

                while (randomPowerUpTypes.Contains((PowerUpDatabase.PowerUpType)randomPowerUpType) || fullHealthButRecovery || powerUpAlreadyMaxStacked)
                {
                    randomPowerUpType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(PowerUpDatabase.PowerUpType)).Length);
                    fullHealthButRecovery = !PlayerManager.Instance.IsPlayerDamaged() && (PowerUpDatabase.PowerUpType)randomPowerUpType == PowerUpDatabase.PowerUpType.RecoverHP;
                    appliedPowerUpIndex = playerPowerUps.appliedPowerUps.FindIndex(p => p.powerUpType == (PowerUpDatabase.PowerUpType)randomPowerUpType);
                    if (appliedPowerUpIndex >= 0)
                    {
                        var existing = playerPowerUps.appliedPowerUps[appliedPowerUpIndex];
                        int maxStack = powerUpDatabase.GetPowerUpData(existing.powerUpType).maxStack;
                        if (maxStack < 0)
                        {
                            maxStack = int.MaxValue;
                        }
                        powerUpAlreadyMaxStacked = existing.stackCount >= maxStack;
                    }
                    else
                    {
                        powerUpAlreadyMaxStacked = false;
                    }
                }
                randomPowerUpTypes.Add((PowerUpDatabase.PowerUpType)randomPowerUpType);
            }
            UIManager.Instance.GetPowerUpPanelController().OnPowerUpShown(randomPowerUpTypes);
        }
    }

    public void ApplyPowerUp(int playerIndex, PowerUpDatabase.PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUpDatabase.PowerUpType.Shield:
                PlayerManager.Instance.AddShieldToPlayer(playerIndex);
                break;
            case PowerUpDatabase.PowerUpType.RecoverHP:
                PlayerManager.Instance.RecoverHPForPlayer(playerIndex, 20);
                break;
            case PowerUpDatabase.PowerUpType.IncreaseMaxHP:
                PlayerManager.Instance.IncreaseMaxHPForPlayer(playerIndex, 10);
                break;
            default:
                var playerPowerUps = playerPowerUpsList.Find(p => p.playerIndex == playerIndex);
                var appliedPowerUpIndex = playerPowerUps.appliedPowerUps.FindIndex(p => p.powerUpType == powerUpType);
                if (appliedPowerUpIndex >= 0)
                {
                    var existing = playerPowerUps.appliedPowerUps[appliedPowerUpIndex];
                    existing.stackCount++;
                    playerPowerUps.appliedPowerUps[appliedPowerUpIndex] = existing;
                }
                else
                {
                    AppliedPowerUp appliedPowerUp = new AppliedPowerUp
                    {
                        powerUpType = powerUpType,
                        stackCount = 1
                    };
                    playerPowerUps.appliedPowerUps.Add(appliedPowerUp);
                }
                break;
        }
        EnemyManager.Instance.ResetEnemy();
        UIManager.Instance.HidePowerUpPanel();
    }

    public PlayerPowerUps GetPlayerPowerUps(int playerIndex)
    {
        return playerPowerUpsList.Find(p => p.playerIndex == playerIndex);
    }
}
