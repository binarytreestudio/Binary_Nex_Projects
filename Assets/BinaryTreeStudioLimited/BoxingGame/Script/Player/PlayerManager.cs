using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [Serializable]
    private struct PlayerMapping
    {
        public int playerIndex;
        public PlayerController playerController;
    }
    [SerializeField] private List<PlayerMapping> playerMapping = new();
    [SerializeField] private float leftBorder = -1f;
    [SerializeField] private float rightBorder = 1.5f;

    public Action<int, Jazz.Handedness, Vector2, int> OnPlayerSlashDetected;    // int: player index, Jazz.Handedness: handedness, Vector2: direction, int: combo
    public Action<int, float> OnPlayerHPChanged;
    public Action<int, int> OnPlayerComboChanged;
    public Action<int> OnPlayerAvoidedAttack;

    public void RegisterPlayerController(PlayerController playerController)
    {
        if (!playerMapping.Exists(info => info.playerController == playerController))
        {
            playerMapping.Add(new PlayerMapping { playerIndex = playerMapping.Count, playerController = playerController });
            playerController.Init(playerMapping.Count - 1);
            playerController.OnSlashDetected += HandlePlayerSlashDetected;
            playerController.OnPlayerHPChanged += HandlePlayerHPChanged;
            playerController.OnPlayerComboChanged += HandlePlayerComboChanged;
            playerController.OnPlayerAvoidedAttack += HandlePlayerAvoidedAttack;
        }
        float screenLength = rightBorder - leftBorder;
        float segmentLength = screenLength / playerMapping.Count;
        for (int i = 0; i < playerMapping.Count; i++)
        {
            float xPos = leftBorder + segmentLength * (i + 0.5f);
            Vector3 playerPos = playerMapping[i].playerController.transform.position;
            playerMapping[i].playerController.transform.position = new Vector3(xPos, playerPos.y, playerPos.z);
        }
    }

    protected override void OnDestroy()
    {
        foreach (var playerInfo in playerMapping)
        {
            playerInfo.playerController.OnSlashDetected -= HandlePlayerSlashDetected;
            playerInfo.playerController.OnPlayerHPChanged -= HandlePlayerHPChanged;
            playerInfo.playerController.OnPlayerComboChanged -= HandlePlayerComboChanged;
            playerInfo.playerController.OnPlayerAvoidedAttack -= HandlePlayerAvoidedAttack;
        }

        base.OnDestroy();
    }

    private void HandlePlayerSlashDetected(int playerIndex, Jazz.Handedness handedness, Vector2 direction, int combo)
    {
        OnPlayerSlashDetected?.Invoke(playerIndex, handedness, direction, combo);
    }

    private void HandlePlayerHPChanged(int playerIndex, float healthPercentage)
    {
        OnPlayerHPChanged?.Invoke(playerIndex, healthPercentage);
    }

    private void HandlePlayerComboChanged(int playerIndex, int combo)
    {
        OnPlayerComboChanged?.Invoke(playerIndex, combo);
    }

    private void HandlePlayerAvoidedAttack(int playerIndex)
    {
        OnPlayerAvoidedAttack?.Invoke(playerIndex);
    }

    public void AddShieldToPlayer(int playerIndex)
    {
        var playerInfo = playerMapping.Find(info => info.playerIndex == playerIndex);
        if (playerInfo.playerController != null)
        {
            playerInfo.playerController.AddShield();
        }
    }

    public void RecoverHPForPlayer(int playerIndex, float amount)
    {
        var playerInfo = playerMapping.Find(info => info.playerIndex == playerIndex);
        if (playerInfo.playerController != null)
        {
            playerInfo.playerController.RecoverHP(amount);
        }
    }

    public void IncreaseMaxHPForPlayer(int playerIndex, float amount)
    {
        var playerInfo = playerMapping.Find(info => info.playerIndex == playerIndex);
        if (playerInfo.playerController != null)
        {
            playerInfo.playerController.IncreaseMaxHP(amount);
        }
    }

    public bool IsPlayerDamaged()
    {
        foreach (var playerInfo in playerMapping)
        {
            if (playerInfo.playerController.IsDamaged())
            {
                return true;
            }
        }
        return false;
    }
}
