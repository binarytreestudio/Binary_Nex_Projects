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
            if (currentHealth <= 0)
            {
                BattleManager.Instance.GameOver();
            }
        }

        public void PlayerSlashDetected(int playerIndex, Jazz.Handedness handedness, Vector2 direction)
        {
            OnPlayerSlashDetected?.Invoke(playerIndex, handedness, direction);
        }
    }

}
