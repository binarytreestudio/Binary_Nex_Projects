using UnityEngine;

namespace TowerDefence
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Slash Detectors")]
        [SerializeField] SlashDetector leftSlashDetector = null!;
        [SerializeField] SlashDetector rightSlashDetector = null!;

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
            if (handedness == Jazz.Handedness.Left && Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && direction.x > 0)
            {
                //Left Hook
                Shoot(1);
                return;
            }
            if (handedness == Jazz.Handedness.Right && Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && direction.x < 0)
            {
                //Right Hook
                Shoot(-1);
                return;
            }
            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) && direction.y > 0)
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
