using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    [SerializeField] private DummyPlayerController dummyPlayerController;

    private List<GameObject> spawnedDummies = new();
    private bool leftTutorialCompleted = false;
    private bool centerTutorialCompleted = false;
    private bool rightTutorialCompleted = false;

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
            SaveManager.Instance.CompleteTutorial();
            UIManager.Instance?.HideTutorialPanel();
            dummyPlayerController.gameObject.SetActive(false);
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
        UIManager.Instance?.ShowTutorialPanel();
        UIManager.Instance?.gameplayHUDController.SetLevelText("Tutorial");
        dummyPlayerController.gameObject.SetActive(true);

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

        UIManager.Instance?.tutorialPanelController.SetTutorialText("Perform left hook to cast a fireball to your left!");

        PlayerManager.Instance.UnlockLeftFireball();
        PlayerManager.Instance.LockMiddleFireball();
        PlayerManager.Instance.LockRightFireball();

        dummyPlayerController.SetTrigger("RightHook");
        dummyPlayerController.SetBool("Mirror", true);
    }

    private void StartCenterTutorial()
    {
        for (int i = 0; i < playerCount; i++)
        {
            var dummy = EnemyManager.Instance.InstantiateEnemy(EnemyManager.EnemyType.Dummy);
            dummy.transform.position = new Vector3(PlayerManager.Instance.GetPlayerXPosition(i), dummy.transform.position.y, 10);
            spawnedDummies.Add(dummy);
        }

        UIManager.Instance?.tutorialPanelController.SetTutorialText("Perform uppercut to cast a fireball in front of you!");

        PlayerManager.Instance.LockLeftFireball();
        PlayerManager.Instance.UnlockMiddleFireball();
        PlayerManager.Instance.LockRightFireball();

        dummyPlayerController.SetTrigger("Uppercut");
        dummyPlayerController.SetBool("Mirror", false);

    }

    private void StartRightTutorial()
    {
        for (int i = 0; i < playerCount; i++)
        {
            var dummy = EnemyManager.Instance.InstantiateEnemy(EnemyManager.EnemyType.Dummy);
            dummy.transform.position = new Vector3(PlayerManager.Instance.GetPlayerXPosition(i) + laneSpace, dummy.transform.position.y, 10);
            spawnedDummies.Add(dummy);
        }

        UIManager.Instance?.tutorialPanelController.SetTutorialText("Perform right hook to cast a fireball to your right!");

        PlayerManager.Instance.LockLeftFireball();
        PlayerManager.Instance.LockMiddleFireball();
        PlayerManager.Instance.UnlockRightFireball();

        dummyPlayerController.SetTrigger("RightHook");
        dummyPlayerController.SetBool("Mirror", false);
    }
}
