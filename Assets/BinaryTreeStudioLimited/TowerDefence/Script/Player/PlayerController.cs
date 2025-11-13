using UnityEngine;

namespace TowerDefence
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Slash Detectors")]
        [SerializeField] SlashDetector leftSlashDetector = null!;
        [SerializeField] SlashDetector rightSlashDetector = null!;
        [SerializeField] private float leftHookAngle = 45f;
        [SerializeField] private float rightHookAngle = 135f;
        [SerializeField] private float upperCutAngle = 90f;
        [SerializeField] private float hookAngleRange = 60f;

        [Header("IK Avatar Controller")]
        [SerializeField] private IKAvatarController ikAvatarController = null!;

        [Header("Fireball")]
        [SerializeField] private GameObject fireBallPrefab = null!;
        [SerializeField] private float fireBallSpeed = 10f;
        [SerializeField] private float fireBallDamage = 100f;

        private bool gameStarted = false;
        int playerIndex;

        public void Init(int playerIndex)
        {
            this.playerIndex = playerIndex;

            leftSlashDetector.Init(playerIndex);
            rightSlashDetector.Init(playerIndex);
            ikAvatarController.Init(playerIndex);

            leftSlashDetector.OnSlashDetected += OnLeftSlashDetected;
            rightSlashDetector.OnSlashDetected += OnRightSlashDetected;

            BattleManager.Instance.OnGameStarted += OnGameStarted;
        }

        void OnDestroy()
        {
            leftSlashDetector.OnSlashDetected -= OnLeftSlashDetected;
            rightSlashDetector.OnSlashDetected -= OnRightSlashDetected;

            BattleManager.Instance.OnGameStarted -= OnGameStarted;
        }

        #region Slash Detection

        void OnLeftSlashDetected(Vector2 direction)
        {
            SlashDetected(Jazz.Handedness.Left, direction);
        }

        void OnRightSlashDetected(Vector2 direction)
        {
            SlashDetected(Jazz.Handedness.Right, direction);
        }

        void SlashDetected(Jazz.Handedness handedness, Vector2 direction)
        {
            PlayerManager.Instance.PlayerSlashDetected(playerIndex, handedness, direction);
            if (!gameStarted)
                return;
            float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angleDegrees = (angleDegrees + 360) % 360;

            float leftHookDifference = Mathf.Abs(angleDegrees - leftHookAngle);
            float rightHookDifference = Mathf.Abs(angleDegrees - rightHookAngle);
            float upperCutDifference = Mathf.Abs(angleDegrees - upperCutAngle);

            bool leftHookAngleCheck = angleDegrees > leftHookAngle - hookAngleRange / 2 && angleDegrees < leftHookAngle + hookAngleRange / 2;
            if (handedness == Jazz.Handedness.Left && leftHookAngleCheck && leftHookDifference < upperCutDifference)
            {
                //Left Hook
                Shoot(1);
                return;
            }
            bool rightHookAngleCheck = angleDegrees > rightHookAngle - hookAngleRange / 2 && angleDegrees < rightHookAngle + hookAngleRange / 2;
            if (handedness == Jazz.Handedness.Right && rightHookAngleCheck && rightHookDifference < upperCutDifference)
            {
                //Right Hook
                Shoot(-1);
                return;
            }
            bool upperCutAngleCheck = angleDegrees > upperCutAngle - hookAngleRange / 2 && angleDegrees < upperCutAngle + hookAngleRange / 2;
            if (upperCutAngleCheck)
            {
                //Uppercut
                Shoot(0);
                return;
            }
        }

        #endregion

        void OnGameStarted(int playerCount)
        {
            gameStarted = true;
        }

        void Shoot(int index)   //-1: left, 0: middle, 1: right
        {
            if (!gameStarted)
                return;

            // Instantiate fireball at the lane position
            Vector3 spawnPosition = new Vector3(transform.position.x + 2 * index, 1f, -.5f);
            GameObject fireball = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
            var fireballController = fireball.GetComponent<FireBallController>();
            fireballController.Init(fireBallSpeed, fireBallDamage);
        }

    }
}
