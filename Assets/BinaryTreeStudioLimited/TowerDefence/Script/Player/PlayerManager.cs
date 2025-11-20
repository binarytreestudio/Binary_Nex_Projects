using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [Header("player health")]
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        private struct PlayerMapping
        {
            public int playerIndex;
            public PlayerController playerController;
        }
        private List<PlayerMapping> playerMapping = new();

        public Action<int, Jazz.Handedness, Vector2> OnPlayerSlashDetected;

        void Start()
        {
            currentHealth = maxHealth;
            GameplayHUDController.Instance.SetPlayerHealthBarValue(1f);
        }

        public void RegisterPlayerController(PlayerController playerController)
        {
            if (!playerMapping.Exists(info => info.playerController == playerController))
            {
                playerMapping.Add(new PlayerMapping { playerIndex = playerMapping.Count, playerController = playerController });
                playerController.Init(playerMapping.Count - 1);
            }
        }

        public void PlayerTakeDamage(float damage)
        {
            currentHealth -= damage;
            GameplayHUDController.Instance.SetPlayerHealthBarValue(currentHealth / maxHealth);
            AudioManager.Instance.PlayPlayerHurtAudio();
            if (currentHealth <= 0)
            {
                BattleManager.Instance.GameOver();
            }
        }

        public void PlayerSlashDetected(int playerIndex, Jazz.Handedness handedness, Vector2 direction)
        {
            OnPlayerSlashDetected?.Invoke(playerIndex, handedness, direction);
        }

        public bool IsPlayerDamaged()
        {
            return currentHealth < maxHealth;
        }

        public void RecoverHP(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            GameplayHUDController.Instance.SetPlayerHealthBarValue(currentHealth / maxHealth);
        }

        //public List<AppliedPowerUp> GetPlayerPowerUps(int playerIndex)
        //{
        //    return playerMapping.Find(player => player.playerIndex == playerIndex).playerController.GetAppliedPowerUps();
        //}

        public void ApplyPowerUp(int playerIndex, PowerUpDatabase.PowerUpType powerUpType)
        {
            switch (powerUpType)
            {
                //Global effects
                case PowerUpDatabase.PowerUpType.RecoverHP:
                    RecoverHP(20);
                    break;
                //player specific effects
                default:
                    ApplyPowerUpToPlayer(playerIndex, powerUpType);
                    break;
            }
            EnemyManager.Instance.StartNextLevel();
            UIManager.Instance.HidePowerUpPanel();
            LevelStart();
        }

        public void ApplyPowerUpToPlayer(int playerIndex, PowerUpDatabase.PowerUpType powerUpType)
        {
            //var playerPowerUps = GetPlayerPowerUps(playerIndex);
            //var appliedPowerUpIndex = playerPowerUps.FindIndex(p => p.powerUpType == powerUpType);
            //if (appliedPowerUpIndex >= 0)
            //{
            //    var existing = playerPowerUps[appliedPowerUpIndex];
            //    existing.stackCount++;
            //    playerPowerUps[appliedPowerUpIndex] = existing;
            //}
            //else
            //{
            //    AppliedPowerUp appliedPowerUp = new AppliedPowerUp
            //    {
            //        powerUpType = powerUpType,
            //        stackCount = 1
            //    };
            //    playerPowerUps.Add(appliedPowerUp);
            //}
            playerMapping.Find(player => player.playerIndex == playerIndex).playerController.SetAppliedPowerUps(powerUpType);
        }

        public void LevelComplete()
        {
            foreach (var player in playerMapping)
            {
                player.playerController.LevelComplete();
            }
        }

        void LevelStart()
        {
            foreach (var player in playerMapping)
            {
                player.playerController.LevelStart();
            }
        }
    }

}
