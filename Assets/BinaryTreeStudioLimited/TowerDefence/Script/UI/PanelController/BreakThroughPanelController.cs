using System;
using UnityEngine;
public class BreakThroughPanelController : MonoBehaviour
{
    [SerializeField] private GameObject pilar1Object;
    [SerializeField] private GameObject pilar2Object;

    private void Start()
    {
        BattleManager.Instance.OnGameStarted += SetPilarXPositions;
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnGameStarted -= SetPilarXPositions;
    }

    private void SetPilarXPositions(int playerCount)
    {
        if (playerCount % 2 == 0)
        {
            pilar1Object.transform.localPosition += Vector3.left * 200f;
            pilar2Object.transform.localPosition += Vector3.right * 200f;
        }
    }
}

