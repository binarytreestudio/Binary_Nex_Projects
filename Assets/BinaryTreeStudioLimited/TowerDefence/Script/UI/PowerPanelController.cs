using System.Collections.Generic;
using DG.Tweening;
using Jazz;
using UnityEngine;

namespace TowerDefence
{
    public class PowerUpPanelController : MonoBehaviour
    {
        [Header("Positions")]
        [SerializeField] private Vector3 animationStartPosition = new Vector3(0f, 1000f, 1000f);
        [SerializeField] private Vector3 animationEndPosition = new Vector3(0f, 0f, -400f);
        [SerializeField] private List<PowerUpItemController> powerUpItemControllers = new();

        bool init;

        void OnEnable()
        {
            PlayerManager.Instance.OnPlayerSlashDetected += OnClickPowerUp;

            var randomPowerUps = PowerUpDatabase.RandomPowerUps(powerUpItemControllers.Count);
            ShownPowerUp(randomPowerUps);
            transform.localPosition = animationStartPosition;
            transform.DOLocalMove(animationEndPosition, 1f).OnComplete(() =>
            {
                init = true;
            });
        }

        void OnDisable()
        {
            PlayerManager.Instance.OnPlayerSlashDetected -= OnClickPowerUp;
        }


        public void ShownPowerUp(List<PowerUpDatabase.PowerUpType> powerUpTypes)
        {
            for (int i = 0; i < powerUpItemControllers.Count; i++)
            {
                powerUpItemControllers[i].Init(powerUpTypes[i]);
            }
        }

        public void OnClickPowerUp(int playerIndex, Handedness handedness, Vector2 direction)
        {
            if (!init)
                return;

            float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angleDegrees = (angleDegrees + 360) % 360;

            float leftHookDifference = Mathf.Abs(angleDegrees - (float)PlayerController.HitAngle.LeftHook);
            float rightHookDifference = Mathf.Abs(angleDegrees - (float)PlayerController.HitAngle.RightHook);
            float upperCutDifference = Mathf.Abs(angleDegrees - (float)PlayerController.HitAngle.UpperCut);

            bool leftHookAngleCheck = angleDegrees > (float)PlayerController.HitAngle.LeftHook - 60 / 2 && angleDegrees < (float)PlayerController.HitAngle.LeftHook + 60 / 2;
            if (handedness == Jazz.Handedness.Left && leftHookAngleCheck && leftHookDifference < upperCutDifference)
            {
                //Left Hook
                powerUpItemControllers[0].ApplyPowerUp(playerIndex);
                return;
            }
            bool rightHookAngleCheck = angleDegrees > (float)PlayerController.HitAngle.RightHook - 60 / 2 && angleDegrees < (float)PlayerController.HitAngle.RightHook + 60 / 2;
            if (handedness == Jazz.Handedness.Right && rightHookAngleCheck && rightHookDifference < upperCutDifference)
            {
                //Right Hook
                powerUpItemControllers[2].ApplyPowerUp(playerIndex);
                return;
            }
            bool upperCutAngleCheck = angleDegrees > (float)PlayerController.HitAngle.UpperCut - 60 / 2 && angleDegrees < (float)PlayerController.HitAngle.UpperCut + 60 / 2;
            if (upperCutAngleCheck)
            {
                //Uppercut
                powerUpItemControllers[1].ApplyPowerUp(playerIndex);
                return;
            }
        }
    }
}