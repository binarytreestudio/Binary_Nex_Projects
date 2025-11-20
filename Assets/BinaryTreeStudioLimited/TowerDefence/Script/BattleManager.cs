using UnityEngine;
using Nex.Essentials;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

namespace TowerDefence
{
    public class BattleManager : Singleton<BattleManager>
    {
        [Serializable]
        private class SetupConfig
        {
            public GameObject setupPanel = null!;
            public TMP_Text setupInstruction = null!;
            public GameObject setupPrefab = null!;
            public float setupPrefabYPosition = 432;
            public float canvasWidth = 1920;
        }

        public enum LaneSetting
        {
            Straight = 0,
            SShape = 1,
        }

        [Header("Nex Setup")]
        [SerializeField] private bool skipNexSetup = false;
        [SerializeField] private SetupConfig setupConfig = null!;
        [SerializeField] private MdkController mdkController = null!;
        [SerializeField] private PlayAreaController playAreaController = null!;
        [SerializeField] private BodyPoseController bodyPoseController = null!;
        [SerializeField] private PlayAreaPreviewFrameProvider playAreaPreviewFrameProvider = null!;

        [Serializable]
        private class MenuScreenConfig
        {
            public GameObject menuPanel = null!;
            public Button singlePlayerButton = null!;
            public Button twoPlayerButton = null!;
            public Button threePlayerButton = null!;
            public Button fourPlayerButton = null!;
        }
        [Header("Player Config")]
        [SerializeField] private MenuScreenConfig menuScreenConfig = null!;
        [Serializable]
        private class PlayerConfig
        {
            public float[] singlePlayerPosition = { 0.5f };
            public float[] twoPlayerPositions = { 0.3f, 0.6f };
            public float[] threePlayerPositions = { 0.2f, 0.5f, 0.8f };
            public float[] fourPlayerPositions = { 0.1f, 0.4f, 0.7f, 0.9f };
        }
        [SerializeField] private PlayerConfig playerPositionsConfig;
        [SerializeField] private GameObject playerPrefab;

        [Header("Camera Config")]
        [SerializeField] private Camera mainCamera;
        [Serializable]
        private struct CameraConfigBaseOnPlayerCount
        {
            public int playerCount;
            public float fieldOfView;
            public Vector3 position;
            public Quaternion rotation;
        }
        [SerializeField] private List<CameraConfigBaseOnPlayerCount> cameraConfigs;

        [Header("Game Config")]
        [SerializeField] private GameObject lanePrefab;
        [SerializeField] private float restartGameDelay = 2f;
        [SerializeField] private float levelCompleteDelay = 2f;
        [SerializeField] private LaneSetting laneSetting = LaneSetting.Straight;
        public LaneSetting LaneType => laneSetting;

        private int playerCount = 1;
        private bool gameStarted = false;
        public bool GameStarted => gameStarted;
        private List<GameObject> spawnedLanes = new();

        public Action<int> OnGameStarted;

        void Start()
        {
            Jazz.GlobalOptions.shared.enableMultiWindowDVP = false;

            mdkController.StartRunning().Forget();

            Run(destroyCancellationToken).Forget();
        }

        private async UniTaskVoid Run(CancellationToken cancellationToken)
        {
            mdkController.DewarpLocked = false;
            mdkController.EnableConsistency = false;
            playAreaController.Locked = false;

            await RunMenu(cancellationToken);

            if (!skipNexSetup)
                await RunSetup(cancellationToken);

            mdkController.DewarpLocked = true;
            mdkController.EnableConsistency = true;
            playAreaController.Locked = true;
            RunGame();
        }

        /// Choose the number of players and update the PlayAreaController
        private async UniTask RunMenu(CancellationToken cancellationToken)
        {
            menuScreenConfig.menuPanel.SetActive(true);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var onePlayerButton = menuScreenConfig.singlePlayerButton.OnClickAsync(linkedCts.Token);
            var twoPlayerButton = menuScreenConfig.twoPlayerButton.OnClickAsync(linkedCts.Token);
            var threePlayerButton = menuScreenConfig.threePlayerButton.OnClickAsync(linkedCts.Token);
            var fourPlayerButton = menuScreenConfig.fourPlayerButton.OnClickAsync(linkedCts.Token);

            var task = await UniTask.WhenAny(onePlayerButton, twoPlayerButton, threePlayerButton, fourPlayerButton);

            // One task finished, cancel others
            linkedCts.Cancel();

            // Set the number of players based on the button/key input
            switch (task)
            {
                case 0:
                    playerCount = 1;
                    playAreaController.PlayerPositions = playerPositionsConfig.singlePlayerPosition;
                    break;

                case 1:
                    playerCount = 2;
                    playAreaController.PlayerPositions = playerPositionsConfig.twoPlayerPositions;
                    break;
                case 2:
                    playerCount = 3;
                    playAreaController.PlayerPositions = playerPositionsConfig.threePlayerPositions;
                    break;
                case 3:
                    playerCount = 4;
                    playAreaController.PlayerPositions = playerPositionsConfig.fourPlayerPositions;
                    break;
            }

            menuScreenConfig.menuPanel.SetActive(false);
        }

        private async UniTask RunSetup(CancellationToken cancellationToken)
        {
            // Now we enter setup state.
            // Switch the instructions depending on the number of players
            setupConfig.setupInstruction.text = playerCount switch
            {
                1 => "Stand in the middle and raise your hand to start.",
                _ => "Stand side by side and raise your hand to start."
            };

            setupConfig.setupPanel.SetActive(true);

            // For each player, we want to figure out if they are holding up one of their hands or not.
            // And which hand they are holding up.
            // If both are raised, default to right hand.
            var playerSetupDetectors = new OnePlayerSetupDetector[playerCount];
            for (var playerIndex = 0; playerIndex < playerCount; ++playerIndex)
            {
                var playerPosition = playAreaController.PlayerPositions[playerIndex];
                var framePosition = new Vector2(playerPosition * setupConfig.canvasWidth,
                    setupConfig.setupPrefabYPosition);

                // Instantiate the player setup detector prefab inside the setup panel
                var obj = Instantiate(setupConfig.setupPrefab, setupConfig.setupPanel.transform, false);

                // Initialize setup controller, pass controllers and params dynamically
                var setupController = obj.GetComponent<OnePlayerSetupDetector>();
                setupController.Initialize(playAreaController, bodyPoseController, playerIndex, framePosition);
                playerSetupDetectors[playerIndex] = setupController;

                // Initialize preview frame provider
                var provider = obj.GetComponent<PlayAreaMaskedPreviewFrameProvider>();
                provider.Initialize(playAreaPreviewFrameProvider, playerPosition);
            }

            await UniTask.WhenAll(playerSetupDetectors.Select(detector =>
                detector.WaitUntilIsReady(cancellationToken)));

            setupConfig.setupPanel.SetActive(false);
        }

        void RunGame()
        {
            //spawnedLanes.Insert(0, Instantiate(lanePrefab, Vector3.zero, Quaternion.identity));
            //// Instantiate lanes based on player count
            //for (int i = 0; i < playerCount; i++)
            //{
            //    spawnedLanes.Insert(i + 1, Instantiate(lanePrefab, Vector3.zero, Quaternion.identity));
            //}
            //spawnedLanes.Insert(playerCount + 1, Instantiate(lanePrefab, Vector3.zero, Quaternion.identity));

            int totalLanes = playerCount + 2;
            float spacing = 2f;
            float startX = -spacing * Mathf.Floor((totalLanes - 1) / 2f);

            GameObject lane;
            switch (LaneType)
            {
                case LaneSetting.Straight:
                    // Instantiate and position all lanes
                    for (int i = 0; i < totalLanes; i++)
                    {
                        lane = Instantiate(lanePrefab, Vector3.zero, Quaternion.Euler(0, 180, 0));
                        float xPos = startX + i * spacing;
                        lane.transform.position = new Vector3(xPos, 0, 10);
                        spawnedLanes.Add(lane);
                    }

                    // Configure camera based on player count
                    var cameraConfig = cameraConfigs.Find(config => config.playerCount == playerCount);
                    mainCamera.fieldOfView = cameraConfig.fieldOfView;
                    mainCamera.transform.position = cameraConfig.position;
                    mainCamera.transform.rotation = cameraConfig.rotation;

                    break;
                case LaneSetting.SShape:
                    //First lane
                    lane = Instantiate(lanePrefab, Vector3.zero, Quaternion.Euler(0, 90, 0));
                    lane.transform.localScale = new Vector3(1, 1, 0.5f);
                    lane.transform.position = new Vector3(0, 0, 20);
                    spawnedLanes.Add(lane);

                    //Second lane
                    lane = Instantiate(lanePrefab, spawnedLanes[0].transform.Find("Destination").position, Quaternion.Euler(0, 180, 0));
                    lane.transform.localScale = new Vector3(1, 1, 0.25f);
                    Transform startPosition = lane.transform.Find("Start");
                    Vector3 difference = spawnedLanes[0].transform.Find("Destination").position - startPosition.position;
                    lane.transform.position += difference;
                    spawnedLanes.Add(lane);

                    //Third lane
                    lane = Instantiate(lanePrefab, spawnedLanes[1].transform.Find("Destination").position, Quaternion.Euler(0, 270, 0));
                    lane.transform.localScale = new Vector3(1, 1, 0.5f);
                    startPosition = lane.transform.Find("Start");
                    difference = spawnedLanes[1].transform.Find("Destination").position - startPosition.position;
                    lane.transform.position += difference;
                    spawnedLanes.Add(lane);

                    //Fourth lane
                    lane = Instantiate(lanePrefab, spawnedLanes[2].transform.Find("Destination").position, Quaternion.Euler(0, 180, 0));
                    lane.transform.localScale = new Vector3(1, 1, 0.25f);
                    startPosition = lane.transform.Find("Start");
                    difference = spawnedLanes[2].transform.Find("Destination").position - startPosition.position;
                    lane.transform.position += difference;
                    spawnedLanes.Add(lane);

                    //Fifth lane
                    lane = Instantiate(lanePrefab, spawnedLanes[3].transform.Find("Destination").position, Quaternion.Euler(0, 90, 0));
                    lane.transform.localScale = new Vector3(1, 1, 0.5f);
                    startPosition = lane.transform.Find("Start");
                    difference = spawnedLanes[3].transform.Find("Destination").position - startPosition.position;
                    lane.transform.position += difference;
                    spawnedLanes.Add(lane);

                    mainCamera.transform.position = spawnedLanes[spawnedLanes.Count - 1].transform.position + new Vector3(0, 3f, -3f);

                    break;
            }

            float laneLength = 10;
            float secment = laneLength / (playerCount + 1);
            // Instantiate players
            for (int i = 0; i < playerCount; i++)
            {
                var playerObj = Instantiate(playerPrefab);
                switch (LaneType)
                {
                    case LaneSetting.Straight:
                        playerObj.transform.position = new Vector3(spawnedLanes[i + 1].transform.position.x, 0, 0);
                        break;
                    case LaneSetting.SShape:
                        playerObj.transform.position = new Vector3(spawnedLanes[spawnedLanes.Count - 1].transform.position.x - 5 + secment * (i + 1), 0, spawnedLanes[spawnedLanes.Count - 1].transform.position.z - 1f);
                        break;
                }
                var playerController = playerObj.GetComponent<PlayerController>();
                PlayerManager.Instance.RegisterPlayerController(playerController);
            }

            EnemyManager.Instance.SetLanes(spawnedLanes);
            gameStarted = true;
            OnGameStarted?.Invoke(playerCount);
            AudioManager.Instance.PlayGameStartAudio();
            GameplayHUDController.Instance.SetLevelText(1);
        }

        public void GameOver()
        {
            UIManager.Instance.ShowGameOverPanel();
            AudioManager.Instance.PlayGameOverAudio();
            DOVirtual.DelayedCall(restartGameDelay, () =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            });
        }

        public void LevelComplete()
        {
            //UIManager.Instance.ShowLevelCompletePanel();
            AudioManager.Instance.PlayGameWinAudio();
            UIManager.Instance.ShowPowerUpPanel();
            PlayerManager.Instance.LevelComplete();
            //DOVirtual.DelayedCall(levelCompleteDelay, () =>
            //{
            //    EnemyManager.Instance.StartNextLevel();
            //    AudioManager.Instance?.PlayGameStartAudio();
            //});
        }
    }
}
