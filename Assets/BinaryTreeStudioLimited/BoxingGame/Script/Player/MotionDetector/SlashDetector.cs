#nullable enable

using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NodeIndex = Nex.Essentials.SimplePose.NodeIndex;
using System.Threading;
using Nex.Essentials;

public class SlashDetector : MonoBehaviour
{
    private enum Handedness
    {
        Left,
        Right
    }

    [Tooltip("Player index (0 for Player 1, 1 for Player 2, etc.)")]
    [SerializeField]
    private int poseIndex;

    [SerializeField] private Handedness handedness = Handedness.Left;
    [SerializeField] private BodyPoseController bodyPoseController = null!;

    [Tooltip("Minimum speed to consider a slash gesture in inches/second")]
    [SerializeField]
    private float slashSpeedThreshold = 60f; // Minimum speed to consider a slash gesture

    [Tooltip("Time window to detect the slash gesture in seconds")]
    [SerializeField]
    private float slashDetectionWindow = 0.2f; // Time window to detect the slash gesture

    [Tooltip("Cooldown time after a slash is detected in seconds")]
    [SerializeField]
    public float slashCooldown = 0.5f;

    [Tooltip("Identify a slash only if the hand starts within chestDistanceThreshold inches from chest")]
    [SerializeField]
    private bool requireTriggerFromChest = false;

    [Tooltip("If requireTriggerFromChest is true, this is the max distance from chest in inches to consider the slash valid.")]
    [SerializeField]
    private float chestDistanceThreshold = 10f;

    public event Action<Vector2>? OnSlashDetected;

    private History<Vector2> handPositionHistory = null!; // Slash detection window in seconds


    private void OnEnable()
    {
        handPositionHistory = new History<Vector2>(slashDetectionWindow);
        SlashDetectionLoop(destroyCancellationToken).Forget();
    }

    private async UniTaskVoid SlashDetectionLoop(CancellationToken cancellationToken)
    {
        while (isActiveAndEnabled)
        {
            // Detect slash per frame
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: cancellationToken);

            // Get the latest hand and chest positions
            bodyPoseController.TryGetBodyPose(poseIndex, BodyPoseController.PoseFlavor.Raw, out var bodyPose);
            var handPosition = bodyPose[handedness == Handedness.Left ? NodeIndex.LeftWrist : NodeIndex.RightWrist];
            var chestPosition = bodyPose[NodeIndex.Chest];
            if (!handPosition.HasValue) continue; // No hand position available
            if (!chestPosition.HasValue) continue; // No chest position available

            // Store the hand position relative to the chest
            var referencedHandPosition = handPosition.Value - chestPosition.Value;
            handPositionHistory.Add(referencedHandPosition, Time.time);
            if (handPositionHistory.Count < 2) continue; // Not enough data points yet

            // Calculate the slash speed
            Vector2 oldVector = handPositionHistory.EarliestItem;
            Vector2 newVector = handPositionHistory.LatestItem;
            var deltaTime = handPositionHistory.LatestItem.timestamp - handPositionHistory.EarliestItem.timestamp;
            if (deltaTime <= 0.9f * slashDetectionWindow) continue; // Not enough time elapsed

            var slashSpeed = Vector2.Distance(oldVector, newVector) / deltaTime / bodyPose.pixelsPerInch;

            if (slashSpeed < slashSpeedThreshold) continue; // Not a fast enough slash

            if (requireTriggerFromChest && oldVector.magnitude / bodyPose.pixelsPerInch > chestDistanceThreshold)
                continue;   // Not a slash starting from chest area

            // Handle a valid slash gesture
            handPositionHistory.Clear();
            OnSlashDetected?.Invoke(newVector - oldVector);
            await UniTask.Delay(TimeSpan.FromSeconds(slashCooldown), cancellationToken: cancellationToken);
        }
    }

    public void Init(int index)
    {
        poseIndex = index;

        bodyPoseController = FindFirstObjectByType<BodyPoseController>();
    }
}
