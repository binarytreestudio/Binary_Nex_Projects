using System.Collections.Generic;
using DG.Tweening;
using Jazz;
using UnityEngine;
public class PowerUpPanelController : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Vector3 animationStartPosition = new Vector3(0f, 1000f, 1000f);
    [SerializeField] private Vector3 animationEndPosition = new Vector3(0f, 0f, -400f);
    [SerializeField] private List<PowerUpItemController> powerUpItemControllers = new();

    bool init;

    void OnEnable()
    {
        init = false;
        PlayerManager.Instance.OnPlayerLeftHookDetected += PickLeftPowerUp;
        PlayerManager.Instance.OnPlayerRightHookDetected += PickRightPowerUp;
        PlayerManager.Instance.OnPlayerUppercutDetected += PickMiddlePowerUp;


        var randomPowerUps = DatabaseManager.Instance.powerUpDatabase.RandomPowerUps(powerUpItemControllers.Count);
        ShownPowerUp(randomPowerUps);
        transform.localPosition = animationStartPosition;
        transform.DOLocalMove(animationEndPosition, 1f).OnComplete(() =>
        {
            init = true;
        });
    }

    void OnDisable()
    {
        PlayerManager.Instance.OnPlayerLeftHookDetected -= PickLeftPowerUp;
        PlayerManager.Instance.OnPlayerRightHookDetected -= PickRightPowerUp;
        PlayerManager.Instance.OnPlayerUppercutDetected -= PickMiddlePowerUp;
    }


    public void ShownPowerUp(List<PowerUpDatabase.PowerUpType> powerUpTypes)
    {
        for (int i = 0; i < powerUpItemControllers.Count; i++)
        {
            powerUpItemControllers[i].Init(powerUpTypes[i]);
        }
    }

    private void PickLeftPowerUp(int playerIndex)
    {
        powerUpItemControllers[0].ApplyPowerUp(playerIndex);
    }

    private void PickMiddlePowerUp(int playerIndex)
    {
        powerUpItemControllers[1].ApplyPowerUp(playerIndex);
    }

    private void PickRightPowerUp(int playerIndex)
    {
        powerUpItemControllers[2].ApplyPowerUp(playerIndex);
    }
}

