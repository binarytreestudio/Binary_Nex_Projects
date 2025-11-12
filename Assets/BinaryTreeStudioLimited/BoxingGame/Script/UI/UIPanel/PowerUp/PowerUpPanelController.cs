using System.Collections.Generic;
using UnityEngine;

public class PowerUpPanelController : MonoBehaviour
{
    [SerializeField] private List<PowerUpItemController> powerUpItemControllers = new();

    public void OnPowerUpShown(List<PowerUpDatabase.PowerUpType> powerUpTypes)
    {
        for (int i = 0; i < powerUpItemControllers.Count; i++)
        {
            powerUpItemControllers[i].Init(powerUpTypes[i]);
        }
    }
}
