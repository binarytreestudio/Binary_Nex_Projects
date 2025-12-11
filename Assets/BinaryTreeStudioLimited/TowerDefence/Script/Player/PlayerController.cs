using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public enum HitAngle
    {
        LeftHook = 45,
        RightHook = 135,
        UpperCut = 90
    }

    [Serializable]
    public class AppliedPowerUp
    {
        public PowerUpDatabase.PowerUpType powerUpType;
        public int stackCount;
    }

    [Header("Slash Detectors")]
    [SerializeField] Nex.Essentials.SlashDetector leftSlashDetector = null!;
    [SerializeField] Nex.Essentials.SlashDetector rightSlashDetector = null!;
    [SerializeField] private float hookAngleRange = 60f;
    [SerializeField] private float uppercutAngleRange = 30f;

    [Header("IK Avatar Controller")]
    [SerializeField] private IKAvatarController ikAvatarController = null!;

    [Header("Fireball")]
    [SerializeField] private float fireBallSpeed = 10f;
    [SerializeField] private int fireBallDamage = 1;
    [SerializeField] private float fireballCooldown = 0.5f;
    [SerializeField] private CardDisplaySystem punchCardSystem;

    [Header("Debug")]
    [SerializeField] private InputActionReference navigate;

    private bool gameStarted = false;
    private int playerIndex;
    private float lastLeftHandShootTime = 0;
    private float lastRightHandShootTime = 0;
    private float laneSpace;
    [SerializeField] private List<AppliedPowerUp> appliedPowerUps = new();
    //int nextPower = -1;
    private bool levelStarted = false;
    private bool isLeftFireballLocked = false;
    private bool isRightFireballLocked = false;
    private bool isMiddleFireballLocked = false;

    public void Init(int playerIndex)
    {
        this.playerIndex = playerIndex;

        //leftSlashDetector.Init(playerIndex);
        //rightSlashDetector.Init(playerIndex);

        leftSlashDetector = BattleManager.Instance.slashDetectors[playerIndex * 2];
        rightSlashDetector = BattleManager.Instance.slashDetectors[playerIndex * 2 + 1];

        ikAvatarController.Init(playerIndex);

        leftSlashDetector.OnSlashDetected += OnLeftSlashDetected;
        rightSlashDetector.OnSlashDetected += OnRightSlashDetected;

        //powerUpIconImages.ForEach(image => image.sprite = DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(PowerUpDatabase.PowerUpType.NormalPunch).icon);
        punchCardSystem.Initialize(DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(PowerUpDatabase.PowerUpType.NormalPunch).icon);

        for (int i = 0; i < 6; i++)
            appliedPowerUps.Add(null);
    }

    void OnDestroy()
    {
        if (leftSlashDetector != null)
            leftSlashDetector.OnSlashDetected -= OnLeftSlashDetected;
        if (rightSlashDetector != null)
            rightSlashDetector.OnSlashDetected -= OnRightSlashDetected;
    }

    void Update()
    {
        if (navigate.action.ReadValue<Vector2>().x < 0)
        {
            SlashDetected(Jazz.Handedness.Left, (Vector2.right + Vector2.up).normalized);
        }
        if (navigate.action.ReadValue<Vector2>().x > 0)
        {
            SlashDetected(Jazz.Handedness.Right, (Vector2.left + Vector2.up).normalized);
        }
        if (navigate.action.ReadValue<Vector2>().y > 0)
        {
            SlashDetected(Jazz.Handedness.Right, Vector2.up);
        }
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
        if (handedness == Jazz.Handedness.Left && Time.time - lastLeftHandShootTime < fireballCooldown)
            return;
        if (handedness == Jazz.Handedness.Right && Time.time - lastRightHandShootTime < fireballCooldown)
            return;
        float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angleDegrees = (angleDegrees + 360) % 360;
        Debug.Log($"Player Controller {playerIndex} {handedness} slash detected, angle: {angleDegrees}");

        //float leftHookDifference = Mathf.Abs(angleDegrees - (float)HitAngle.LeftHook);
        //float rightHookDifference = Mathf.Abs(angleDegrees - (float)HitAngle.RightHook);
        //float upperCutDifference = Mathf.Abs(angleDegrees - (float)HitAngle.UpperCut);

        bool upperCutAngleCheck = angleDegrees > (float)HitAngle.UpperCut - uppercutAngleRange / 2 && angleDegrees < (float)HitAngle.UpperCut + uppercutAngleRange / 2;
        if (upperCutAngleCheck)
        {
            PlayerManager.Instance.PlayerUppercutDetected(playerIndex);
            if (isMiddleFireballLocked)
                return;

            //Uppercut
            Shoot(0);
            switch (handedness)
            {
                case Jazz.Handedness.Left:
                    lastLeftHandShootTime = Time.time;
                    break;
                case Jazz.Handedness.Right:
                    lastRightHandShootTime = Time.time;
                    break;
            }
        }
        else
        {
            bool leftHookAngleCheck = angleDegrees > (float)HitAngle.LeftHook - hookAngleRange / 2 && angleDegrees < (float)HitAngle.LeftHook + hookAngleRange / 2;
            if (handedness == Jazz.Handedness.Left && leftHookAngleCheck)
            {
                PlayerManager.Instance.PlayerLeftHookDetected(playerIndex);
                if (isLeftFireballLocked)
                    return;

                //Left Hook
                Shoot(-1);
                lastLeftHandShootTime = Time.time;
            }
            bool rightHookAngleCheck = angleDegrees > (float)HitAngle.RightHook - hookAngleRange / 2 && angleDegrees < (float)HitAngle.RightHook + hookAngleRange / 2;
            if (handedness == Jazz.Handedness.Right && rightHookAngleCheck)
            {
                PlayerManager.Instance.PlayerRightHookDetected(playerIndex);
                if (isRightFireballLocked)
                    return;

                //Right Hook
                Shoot(1);
                lastRightHandShootTime = Time.time;
            }
        }
    }

    #endregion

    public void OnGameStarted(int playerCount)
    {
        switch (BattleManager.Instance.LaneType)
        {
            case BattleManager.LaneSetting.Straight:
                laneSpace = 2;
                break;
            case BattleManager.LaneSetting.SShape:
                laneSpace = 5f / playerCount;
                break;
        }
        gameStarted = true;
        levelStarted = true;
    }

    void Shoot(int index)   //-1: left, 0: middle, 1: right
    {
        if (!gameStarted || !levelStarted)
            return;
        //// Instantiate fireball at the lane position
        //Vector3 spawnPosition = new Vector3(transform.position.x + laneSpace * index, transform.position.y + 1f, transform.position.z);
        //GameObject fireball = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
        //var fireballController = fireball.GetComponent<FireBallController>();
        //fireballController.Init(fireBallSpeed, nextPower == (int)PowerUpDatabase.PowerUpType.Stone ? fireBallDamage * 3 : fireBallDamage, nextPower);
        //AudioManager.Instance.PlayFireBallAudio();
        //
        //if (nextPower == (int)PowerUpDatabase.PowerUpType.FireballCount)
        //{
        //    DOVirtual.DelayedCall(0.2f, () =>
        //    {
        //        // Instantiate fireball at the lane position
        //        fireball = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
        //        fireballController = fireball.GetComponent<FireBallController>();
        //        fireballController.Init(fireBallSpeed, fireBallDamage, nextPower);
        //        AudioManager.Instance.PlayFireBallAudio();
        //    });
        //}

        ////Randomly determine next power-up based on applied power-ups
        //if (appliedPowerUps.Count > 0)
        //{
        //    float totalNotPowerUpChance = 1f;
        //    appliedPowerUps.ForEach(powerUp =>
        //    {
        //        totalNotPowerUpChance *= 1 - (20 + 5 * (powerUp.stackCount - 1)) / 100f;
        //    });
        //    float randomValue = Random.value;
        //    if (randomValue > totalNotPowerUpChance)
        //    {
        //        List<float> powerUpChances = new List<float>();
        //        appliedPowerUps.ForEach(powerUp =>
        //        {
        //            powerUpChances.Add((20 + 5f * (powerUp.stackCount - 1)) / 100f);
        //        });
        //        randomValue = Random.Range(0, powerUpChances.Sum());
        //        for (int i = 0; i < powerUpChances.Count; i++)
        //        {
        //            if (randomValue > powerUpChances.GetRange(0, i + 1).Sum())
        //            {
        //                continue;
        //            }
        //            nextPower = (int)appliedPowerUps[i].powerUpType;
        //            break;
        //        }
        //    }
        //    else
        //    {
        //        nextPower = -1;
        //    }
        //}
        //if (nextPower != -1)
        //{
        //    var powerUpData = powerUpDatabase.GetPowerUpData((PowerUpDatabase.PowerUpType)nextPower);
        //    powerUpIconImage.sprite = powerUpData.icon;
        //    powerUpIconImage.gameObject.SetActive(true);
        //}
        //else
        //{
        //    powerUpIconImage.gameObject.SetActive(false);
        //}

        // Instantiate fireball at the lane position
        Vector3 spawnPosition = new Vector3(transform.position.x + laneSpace * index, transform.position.y + 1f, transform.position.z);
        //GameObject fireball = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
        GameObject fireball = ObjectPoolManager.Instance.GetFireBall();
        fireball.transform.position = spawnPosition;
        var fireballController = fireball.GetComponent<FireBallController>();
        int damage = appliedPowerUps[0] != null && appliedPowerUps[0].powerUpType == PowerUpDatabase.PowerUpType.Stone ? fireBallDamage * 3 : fireBallDamage;
        var power = appliedPowerUps[0] != null ? appliedPowerUps[0].powerUpType : (PowerUpDatabase.PowerUpType)(-1);
        fireballController.Init(fireBallSpeed, damage, power);
        switch (power)
        {
            case PowerUpDatabase.PowerUpType.Ice:
                AudioManager.Instance.PlayIceBallAudio();
                break;
            case PowerUpDatabase.PowerUpType.Poison:
                AudioManager.Instance.PlayPoisonBallAudio();
                break;
            case PowerUpDatabase.PowerUpType.Stone:
                AudioManager.Instance.PlayRockBallAudio();
                break;
            default:
                AudioManager.Instance.PlayFireBallAudio();
                break;
        }

        if (power == PowerUpDatabase.PowerUpType.FireballCount)
        {
            DOVirtual.DelayedCall(0.2f, () =>
            {
                // Instantiate fireball at the lane position
                //fireball = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
                fireball = ObjectPoolManager.Instance.GetFireBall();
                fireball.transform.position = spawnPosition;
                fireballController = fireball.GetComponent<FireBallController>();
                fireballController.Init(fireBallSpeed, fireBallDamage, PowerUpDatabase.PowerUpType.FireballCount);
                AudioManager.Instance.PlayFireBallAudio();
            });
        }
        punchCardSystem.MoveCardToBottom(index);
        var usedPowerUp = appliedPowerUps[0];
        appliedPowerUps.RemoveAt(0);
        appliedPowerUps.Add(usedPowerUp);
        UpdatePowerUpIcons();
    }

    public List<AppliedPowerUp> GetAppliedPowerUps()
    {
        return appliedPowerUps;
    }

    public void SetAppliedPowerUps(PowerUpDatabase.PowerUpType powerUp)
    {
        //var existingPowerUp = appliedPowerUps.Find(p => p != null && p.powerUpType == powerUp);
        //if (existingPowerUp != null)
        //{
        //    existingPowerUp.stackCount++;
        //}
        //else
        var newPowerUp = new AppliedPowerUp
        {
            powerUpType = powerUp,
            stackCount = 1
        };
        var nullSlotIndex = appliedPowerUps.FindIndex(p => p == null || p.powerUpType == PowerUpDatabase.PowerUpType.RecoverHP);
        if (nullSlotIndex != -1)
        {
            appliedPowerUps.RemoveAt(nullSlotIndex);
            appliedPowerUps.Insert(0, newPowerUp);
        }
        else
        {
            appliedPowerUps.RemoveAt(appliedPowerUps.Count - 1);
            appliedPowerUps.Insert(0, newPowerUp);
        }

        UpdatePowerUpIcons();
    }

    private void UpdatePowerUpIcons()
    {
        punchCardSystem.SetPowerUpIcons(appliedPowerUps);

        for (int i = 0; i < appliedPowerUps.Count; i++)
        {
            //if (appliedPowerUps[i] != null && appliedPowerUps[i].powerUpType != PowerUpDatabase.PowerUpType.RecoverHP)
            //    continue;

            //punchCardSystem.SetPowerUpIcon(DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(appliedPowerUps[i].powerUpType).icon);
            //if (appliedPowerUps[i] != null && appliedPowerUps[i].powerUpType != PowerUpDatabase.PowerUpType.RecoverHP)
            //{
            //    punchCardSystem.SetFirstPowerUpIcon(DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(appliedPowerUps[i].powerUpType).icon);
            //    //powerUpIconImages[i].sprite = DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(appliedPowerUps[i].powerUpType).icon;
            //}
            //else
            //{
            //    punchCardSystem.SetFirstPowerUpIcon(DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(PowerUpDatabase.PowerUpType.NormalPunch).icon);
            //    //powerUpIconImages[i].sprite = DatabaseManager.Instance.powerUpDatabase.GetPowerUpData(PowerUpDatabase.PowerUpType.NormalPunch).icon;
            //}
        }
    }

    public void LevelComplete()
    {
        levelStarted = false;
    }

    public void LevelStart()
    {
        DOVirtual.DelayedCall(1f, () =>
        {
            levelStarted = true;
        });
    }

    public void LockLeftFireball()
    {
        isLeftFireballLocked = true;
    }
    public void LockRightFireball()
    {
        isRightFireballLocked = true;
    }
    public void LockMiddleFireball()
    {
        isMiddleFireballLocked = true;
    }
    public void UnlockLeftFireball()
    {
        isLeftFireballLocked = false;
    }
    public void UnlockRightFireball()
    {
        isRightFireballLocked = false;
    }
    public void UnlockMiddleFireball()
    {
        isMiddleFireballLocked = false;
    }
}

