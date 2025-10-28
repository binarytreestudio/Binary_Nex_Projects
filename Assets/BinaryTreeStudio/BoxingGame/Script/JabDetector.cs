#nullable enable

using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NodeIndex = Nex.Essentials.SimplePose.NodeIndex;
using System.Threading;
using Nex.Essentials;


public class JabDetector : MonoBehaviour
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

    [Tooltip("Minimum speed to consider a jab gesture in inches/second")]
    [SerializeField]
    private float jabSpeedThreshold = 60f; // Minimum speed to consider a jab gesture

    [Tooltip("Time window to detect the jab gesture in seconds")]
    [SerializeField]
    private float jabDetectionWindow = 0.2f; // Time window to detect the jab gesture

    [Tooltip("Cooldown time after a jab is detected in seconds")]
    [SerializeField]
    public float jabCooldown = 0.5f;

    [SerializeField]
    private float jabDistanceFromElbowThreshold = 10f; // in inches
    public event Action<Vector2>? OnJabDetected;

    private History<Vector2> handPositionHistory = null!; // Jab detection window in seconds


    private void Start()
    {
        handPositionHistory = new History<Vector2>(jabDetectionWindow);
        JabDetectionLoop(destroyCancellationToken).Forget();
    }

    private async UniTaskVoid JabDetectionLoop(CancellationToken cancellationToken)
    {
        while (isActiveAndEnabled)
        {
            // Detect jab per frame
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: cancellationToken);

            // Get the latest hand and elbow positions
            bodyPoseController.TryGetBodyPose(poseIndex, BodyPoseController.PoseFlavor.Raw, out var bodyPose);
            var handPosition = bodyPose[handedness == Handedness.Left ? NodeIndex.LeftWrist : NodeIndex.RightWrist];
            var elbowPosition = bodyPose[handedness == Handedness.Left ? NodeIndex.LeftElbow : NodeIndex.RightElbow];
            if (!handPosition.HasValue) continue; // No hand position available
            if (!elbowPosition.HasValue) continue; // No elbow position available

            // Store the hand position relative to the elbow
            var referencedHandPosition = handPosition.Value - elbowPosition.Value;
            handPositionHistory.Add(referencedHandPosition, Time.time);
            if (handPositionHistory.Count < 2) continue; // Not enough data points yet

            // Calculate the jab speed
            Vector2 oldVector = handPositionHistory.EarliestItem;
            Vector2 newVector = handPositionHistory.LatestItem;
            var deltaTime = handPositionHistory.LatestItem.timestamp - handPositionHistory.EarliestItem.timestamp;
            if (deltaTime <= 0.9f * jabDetectionWindow) continue; // Not enough time elapsed

            var jabSpeed = Vector2.Distance(oldVector, newVector) / deltaTime / bodyPose.pixelsPerInch;

            if (jabSpeed < jabSpeedThreshold) continue; // Not a fast enough jab

            if (referencedHandPosition.magnitude > jabDistanceFromElbowThreshold) continue; // Hand not close enough from elbow

            // Handle a valid jab gesture
            handPositionHistory.Clear();
            OnJabDetected?.Invoke(handPosition.Value);
            await UniTask.Delay(TimeSpan.FromSeconds(jabCooldown), cancellationToken: cancellationToken);
        }
    }
}

