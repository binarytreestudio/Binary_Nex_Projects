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
            Debug.Log("Player Take Damage: " + damage);
            currentHealth -= damage;
            GameplayHUDController.Instance.SetPlayerHealthBarValue(currentHealth / maxHealth);
            if (currentHealth <= 0)
            {
                BattleManager.Instance.GameOver();
            }
        }
    }

}
