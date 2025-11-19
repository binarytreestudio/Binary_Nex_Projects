using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace TowerDefence
{
    public class PlayerController : MonoBehaviour
    {
        public enum HitAngle
        {
            LeftHook = 45,
            RightHook = 135,
            UpperCut = 90
        }

        [Header("Slash Detectors")]
        [SerializeField] SlashDetector leftSlashDetector = null!;
        [SerializeField] SlashDetector rightSlashDetector = null!;
        [SerializeField] private float hookAngleRange = 60f;

        [Header("IK Avatar Controller")]
        [SerializeField] private IKAvatarController ikAvatarController = null!;

        [Header("Fireball")]
        [SerializeField] private GameObject fireBallPrefab = null!;
        [SerializeField] private float fireBallSpeed = 10f;
        [SerializeField] private int fireBallDamage = 1;
        [SerializeField] private float laneTimer = 0.5f;

        private bool gameStarted = false;
        int playerIndex;
        private float leftLaneCooldownTimer = 0;
        private float rightLaneCooldownTimer = 0;
        private float middleLaneCooldownTimer = 0;
        int playerCount;
        private float laneSpace;
        [SerializeField] private List<PlayerManager.AppliedPowerUp> appliedPowerUps;

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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SlashDetected(Jazz.Handedness.Left, (Vector2.right + Vector2.up).normalized);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SlashDetected(Jazz.Handedness.Right, (Vector2.left + Vector2.up).normalized);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SlashDetected(Jazz.Handedness.Right, Vector2.up);
            }
            if (leftLaneCooldownTimer >= 0)
                leftLaneCooldownTimer -= Time.deltaTime;
            if (middleLaneCooldownTimer >= 0)
                middleLaneCooldownTimer -= Time.deltaTime;
            if (rightLaneCooldownTimer >= 0)
                rightLaneCooldownTimer -= Time.deltaTime;
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

            float leftHookDifference = Mathf.Abs(angleDegrees - (float)HitAngle.LeftHook);
            float rightHookDifference = Mathf.Abs(angleDegrees - (float)HitAngle.RightHook);
            float upperCutDifference = Mathf.Abs(angleDegrees - (float)HitAngle.UpperCut);

            bool leftHookAngleCheck = angleDegrees > (float)HitAngle.LeftHook - hookAngleRange / 2 && angleDegrees < (float)HitAngle.LeftHook + hookAngleRange / 2;
            if (handedness == Jazz.Handedness.Left && leftHookAngleCheck && leftHookDifference < upperCutDifference)
            {
                //Left Hook
                Shoot(-1);
                return;
            }
            bool rightHookAngleCheck = angleDegrees > (float)HitAngle.RightHook - hookAngleRange / 2 && angleDegrees < (float)HitAngle.RightHook + hookAngleRange / 2;
            if (handedness == Jazz.Handedness.Right && rightHookAngleCheck && rightHookDifference < upperCutDifference)
            {
                //Right Hook
                Shoot(1);
                return;
            }
            bool upperCutAngleCheck = angleDegrees > (float)HitAngle.UpperCut - hookAngleRange / 2 && angleDegrees < (float)HitAngle.UpperCut + hookAngleRange / 2;
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
            this.playerCount = playerCount;

            laneSpace = 5f / playerCount;

            gameStarted = true;
        }

        void Shoot(int index)   //-1: left, 0: middle, 1: right
        {
            if (!gameStarted)
                return;

            switch (index)
            {
                case -1:
                    if (leftLaneCooldownTimer > 0)
                        return;
                    leftLaneCooldownTimer = laneTimer;
                    break;
                case 0:
                    if (middleLaneCooldownTimer > 0)
                        return;
                    middleLaneCooldownTimer = laneTimer;
                    break;
                case 1:
                    if (rightLaneCooldownTimer > 0)
                        return;
                    rightLaneCooldownTimer = laneTimer;
                    break;
            }

            int power = -1;
            if (appliedPowerUps.Count > 0)
            {
                float totalNotPowerUpChance = 1f;
                appliedPowerUps.ForEach(powerUp =>
                {
                    totalNotPowerUpChance *= 1 - (20 + 5 * (powerUp.stackCount - 1)) / 100f;
                });
                float randomValue = Random.value;
                if (randomValue > totalNotPowerUpChance)
                {
                    List<float> powerUpChances = new List<float>();
                    appliedPowerUps.ForEach(powerUp =>
                    {
                        powerUpChances.Add((20 + 5f * (powerUp.stackCount - 1)) / 100f);
                    });
                    randomValue = Random.Range(0, powerUpChances.Sum());
                    for (int i = 0; i < powerUpChances.Count; i++)
                    {
                        if (randomValue > powerUpChances.GetRange(0, i + 1).Sum())
                        {
                            continue;
                        }
                        power = (int)appliedPowerUps[i].powerUpType;
                        break;
                    }
                }
            }

            // Instantiate fireball at the lane position
            Vector3 spawnPosition = new Vector3(transform.position.x + laneSpace * index, transform.position.y, transform.position.z);
            GameObject fireball = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
            var fireballController = fireball.GetComponent<FireBallController>();
            fireballController.Init(fireBallSpeed, fireBallDamage, power);
            AudioManager.Instance.PlayFireBallAudio();

            if (power == (int)PowerUpDatabase.PowerUpType.FireballCount)
            {
                DOVirtual.DelayedCall(0.2f, () =>
                {
                    // Instantiate fireball at the lane position
                    fireball = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
                    fireballController = fireball.GetComponent<FireBallController>();
                    fireballController.Init(fireBallSpeed, fireBallDamage, power);
                    AudioManager.Instance.PlayFireBallAudio();
                });
            }
        }

        public List<PlayerManager.AppliedPowerUp> GetAppliedPowerUps()
        {
            return appliedPowerUps;
        }

        public void SetAppliedPowerUps(List<PlayerManager.AppliedPowerUp> appliedPowerUps)
        {
            this.appliedPowerUps = appliedPowerUps;
        }
    }
}
