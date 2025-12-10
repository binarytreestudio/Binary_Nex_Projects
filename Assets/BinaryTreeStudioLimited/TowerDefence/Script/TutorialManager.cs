using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    [SerializeField] private List<GameObject> spawnedDummies = new();
    [SerializeField] private bool leftTutorialCompleted = false;
    [SerializeField] private bool centerTutorialCompleted = false;
    [SerializeField] private bool rightTutorialCompleted = false;

    private float laneSpace = 2f;
    private int playerCount = 1;

    private void Update()
    {
        if (leftTutorialCompleted && centerTutorialCompleted && rightTutorialCompleted)
        {
            spawnedDummies.ForEach(dummy => Destroy(dummy));
            spawnedDummies.Clear();
            EnemyManager.Instance.GameStarted();
            this.enabled = false;
            PlayerManager.Instance.UnlockLeftFireball();
            PlayerManager.Instance.UnlockMiddleFireball();
            PlayerManager.Instance.UnlockRightFireball();
            return;
        }
        if (spawnedDummies.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < spawnedDummies.Count; i++)
        {
            if (spawnedDummies[i] != null) return;
        }
        spawnedDummies.Clear();
        if (!leftTutorialCompleted)
        {
            leftTutorialCompleted = true;
            StartCenterTutorial();
            return;
        }
        else if (!centerTutorialCompleted)
        {
            centerTutorialCompleted = true;
            StartRightTutorial();
            return;
        }
        else if (!rightTutorialCompleted)
        {
            rightTutorialCompleted = true;
        }
    }

    public void StartTutorial(int playerCount)
    {
        this.playerCount = playerCount;

        switch (BattleManager.Instance.LaneType)
        {
            case BattleManager.LaneSetting.Straight:
                laneSpace = 2;
                break;
            case BattleManager.LaneSetting.SShape:
                laneSpace = 5f / playerCount;
                break;
        }

        UIManager.Instance?.gameplayHUDController.SetLevelText("Tutorial");

        StartLeftTutorial();
    }

    private void StartLeftTutorial()
    {
        for (int i = 0; i < playerCount; i++)
        {
            var dummy = EnemyManager.Instance.InstantiateEnemy(EnemyManager.EnemyType.Dummy);
            dummy.transform.position = new Vector3(PlayerManager.Instance.GetPlayerXPosition(i) - laneSpace, dummy.transform.position.y, 10);
            spawnedDummies.Add(dummy);
        }
        PlayerManager.Instance.UnlockLeftFireball();
        PlayerManager.Instance.LockMiddleFireball();
        PlayerManager.Instance.LockRightFireball();
    }

    private void StartCenterTutorial()
    {
        for (int i = 0; i < playerCount; i++)
        {
            var dummy = EnemyManager.Instance.InstantiateEnemy(EnemyManager.EnemyType.Dummy);
            dummy.transform.position = new Vector3(PlayerManager.Instance.GetPlayerXPosition(i), dummy.transform.position.y, 10);
            spawnedDummies.Add(dummy);
        }
        PlayerManager.Instance.LockLeftFireball();
        PlayerManager.Instance.UnlockMiddleFireball();
        PlayerManager.Instance.LockRightFireball();
    }

    private void StartRightTutorial()
    {
        for (int i = 0; i < playerCount; i++)
        {
            var dummy = EnemyManager.Instance.InstantiateEnemy(EnemyManager.EnemyType.Dummy);
            dummy.transform.position = new Vector3(PlayerManager.Instance.GetPlayerXPosition(i) + laneSpace, dummy.transform.position.y, 10);
            spawnedDummies.Add(dummy);
        }
        PlayerManager.Instance.LockLeftFireball();
        PlayerManager.Instance.LockMiddleFireball();
        PlayerManager.Instance.UnlockRightFireball();
    }
}
