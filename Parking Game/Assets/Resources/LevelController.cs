using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Actual dataset that should be used to generate a set of missions for a level.
 */
[System.Serializable]
public class LevelData {
    public int levelIndex;
    public int missionCount;
    public GoalType[] availableTypes;
}

public class LevelController : MonoBehaviour {

    public static LevelController main;

    public GameObject targetArea;
    public AudioClip LevelMusic;
    public AudioClip LevelMusicIntro;
    public readonly Observable onMissionChange = new Observable();

    private GoalPanelUI goalPanelUI;
    private TargetPanelUI targetPanelUI;
    private TrafficRoute[] targetRoutes;
    private PickupSpawner[] pickupSpawners;
    private LevelData levelData;
    private int score = 0;
    private int levelIndex = 0;
    private float speedMultiplier = 1f;
    private bool gameEnded = false;

    private List<TimedTrigger> spawnTriggers = new List<TimedTrigger>();

    #region Getters and setters
    public float SpeedMultiplier {
        get => this.speedMultiplier;
        set => this.speedMultiplier = value;
    }

    public int Score {
        get => this.score;
        set {
            this.score = value;
            UIController.main.GoalPanelUI.UpdateScore(this.score);
        }
    }

    public int LevelIndex {
        get => this.levelIndex;
        set {
            this.levelIndex = value;
            UIController.main.GoalPanelUI.UpdateLevel(this.levelIndex);
        }
    }

    public Car[] TargetCars {
        get {
            List<Car> targets = new List<Car>();
            for (int i = 0; i < this.targetRoutes.Length; i++) {
                targets.AddRange(this.targetRoutes[i].Cars);
            }
            return targets.ToArray();
        }
    }
    #endregion

    void Awake() {
        LevelController.main = this;
    }

    void Start() {
        this.goalPanelUI = this.transform.Find("GoalPanel").GetComponent<GoalPanelUI>();
        this.targetPanelUI = this.transform.Find("TargetPanel").GetComponent<TargetPanelUI>();
        this.FindAllTargetRoutes();
    }

    #region Game controlling methods
    // Called from UI
    public void StartGame() {
        this.gameEnded = false;
        this.Score = 0;
        this.levelIndex = 0;
        this.LevelIndex = 0;
        this.levelData = null;
        UIController.main.ToggleMainMenu(false);
        this.CreateInitialLevelData();
        this.CreatePlayer();
        this.StartLevel();
        this.pickupSpawners = GameObject.FindObjectsOfType<PickupSpawner>();
        AudioController.main.StopMixer(Mixers.Music);
        new TimedTrigger(2f, () => {
            AudioController.main.PlayMusic(this.LevelMusicIntro, this.LevelMusic, 0.3f);
        });
    }

    // Called from UI
    public void RestartGame() {
        UIController.main.TogglePause(false);
        Time.timeScale = 1;
        this.EndMission();
        this.EndGame();
    }

    // Called from UI
    public void Resume() {
        UIController.main.TogglePause(false);
        Time.timeScale = 1;
    }

    public void EndGame() {
        this.gameEnded = true;
        UIController.main.ToggleHighscores(true);
        for (int i = 0; i < this.targetRoutes.Length; i++) {
            this.targetRoutes[i].RemoveAllCars();
        }

        TrafficRoute[] routes = GameObject.FindObjectsOfType<TrafficRoute>();
        for (int i = 0; i < routes.Length; i++) {
            routes[i].StopExtraSpawning();
            routes[i].RemoveAllCars();
        }

        if (PlayerController.main.gameObject != null) {
            Destroy(PlayerController.main.gameObject);
        }

        // Reset all destructables
        Destructable[] destructables = GameObject.FindObjectsOfType<Destructable>();
        for (int j = 0; j < destructables.Length; j++) {
            destructables[j].Reset();
        }
    }

    public void QuitGame() {
        Application.Quit();
    }
    #endregion

    private void StartLevel() {
        this.LevelIndex = this.levelIndex + 1;
        this.levelData = MissionController.main.ResetLevelData(this.levelData, this.LevelIndex);
        this.StartMission();
    }

    #region  Mission controlling methods
    private void StartMission() {
        new TimedTrigger(0.05f, () => {
            GoalData goalData = MissionController.main.CurrentGoalData;
            MissionStartUI.main.Display(goalData);
            //if (goalData.goalType != GoalType.Stealth && goalData.goalType != GoalType.HyperMode && goalData.goalType != GoalType.CarMoving) {
            //    this.targetPanelUI.SetTarget(goalData);
            //    this.ActivateRandomTargetRoute();
            //} else {
            //    this.targetPanelUI.Hide();
            //}

            switch(goalData.goalType) {
                case GoalType.HyperMode:
                    PlayerController.main.SetHyperMode(true);
                    this.targetPanelUI.Hide();
                    break;

                case GoalType.CarMoving:
                    this.CreateTargetArea(goalData);
                    this.targetPanelUI.Hide();
                    break;

                case GoalType.Stealth:
                    PlayerController.main.DisableCaptureTemporarily();
                    this.targetPanelUI.Hide();
                    break;

                default:
                    this.targetPanelUI.SetTarget(goalData);
                    this.ActivateRandomTargetRoute();
                    break;
            }

            BaseMissionSettings settings = MissionController.main.GetMissionSettingsForType(goalData.goalType);
            if (settings.overwriteMusic != null) {
                AudioController.main.PlayMusic(settings.overwriteMusic);
            }

            this.onMissionChange.Next(goalData);
            this.goalPanelUI.StartTimer(goalData, (bool result) => {
                if (this.gameEnded) { return; }

                if (goalData.goalType == GoalType.Stealth || goalData.goalType == GoalType.HyperMode) {
                    this.EndMission();
                } else {
                    this.EndGame();
                }
            });

            if (goalData.goalType != GoalType.Stealth && goalData.goalType != GoalType.HyperMode) {
                this.ClearSpawnTriggers();
                this.spawnTriggers = this.HandleSpawns(goalData);
            } else {
                for (int i = 0; i < this.pickupSpawners.Length; i++) {
                    this.pickupSpawners[i].Clear();
                }

                GameObject[] pickups = GameObject.FindGameObjectsWithTag("pickup");
                for (int j = 0; j < pickups.Length; j++) {
                    Destroy(pickups[j]);
                }
            }
        });
    }

    public void EndMission() {
        this.Score += this.levelIndex;
        GoalData goalData = MissionController.main.CurrentGoalData;
        if (goalData.goalType == GoalType.HyperMode) {
            PlayerController.main.SetHyperMode(false);
        }

        if (goalData.goalType == GoalType.CarMoving) {
            this.ClearTargetArea();
        }

        BaseMissionSettings settings = MissionController.main.GetMissionSettingsForType(goalData.goalType);
        if (settings.overwriteMusic != null) {
            AudioController.main.PlayMusic(this.LevelMusicIntro, this.LevelMusic);
        }

        bool hasEnded = MissionController.main.EndMission();
        if (hasEnded) {
            this.StartLevel();
        } else {
            this.StartMission();
        }
    }
    #endregion

    public void ClearTargetArea() {
        Destroy(this.targetArea);
        this.targetArea = null;
    }

    private void FindAllTargetRoutes() {
        TrafficRoute[] routes = GameObject.FindObjectsOfType<TrafficRoute>();
        List<TrafficRoute> routeList = new List<TrafficRoute>();
        for (int i = 0; i < routes.Length; i++) {
            if (routes[i].routeType == TrafficRouteType.Target) {
                routeList.Add(routes[i]);
            }
        }
        this.targetRoutes = routeList.ToArray();
    }

    private void ActivateRandomTargetRoute() {
        int r = UnityEngine.Random.Range(0, this.targetRoutes.Length);
        this.targetRoutes[r].SpawnCar();
    }

    private void CreatePlayer() {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/ParkingSpot");
        Instantiate(prefab);
    }

    private void CreateInitialLevelData() {
        this.levelData = new LevelData {
            levelIndex = this.levelIndex,
            missionCount = 1,
            availableTypes = new GoalType[1] {
                GoalType.CaptureTarget
            }
        };
    }

    private List<TimedTrigger> HandleSpawns(GoalData goalData) {
        return new List<TimedTrigger>() {
            new TimedTrigger(goalData.timeLimit * 0.33f, () => {
                this.SpawnPickups(0.3f);
            }),

            new TimedTrigger(goalData.timeLimit * 0.5f, () => {
                this.SpawnPickups(0.1f);
            }),

            new TimedTrigger(goalData.timeLimit * 0.75f, () => {
                this.SpawnPickups(0.2f);
            }),

            new TimedTrigger(goalData.timeLimit * 0.85f, () => {
                this.SpawnPickups(0.3f);
            }),
        };
    }

    private void ClearSpawnTriggers() {
        for (int i = 0; i < this.spawnTriggers.Count; i++) {
            this.spawnTriggers[i].Destroy();
        }
    }

    private void SpawnPickups(float percentage) {
        int total = Mathf.RoundToInt(this.pickupSpawners.Length * percentage);
        for (int i = 0; i < total; i++) {
            int r = UnityEngine.Random.Range(0, this.pickupSpawners.Length);
            PickupSpawner spawner = this.pickupSpawners[r];
            spawner.Spawn();
        }
    }

    private void CreateTargetArea(GoalData goalData) {

        // Collect all route-nodes within range
        List<Transform> routeNodes = new List<Transform>();
        for (int i = 0; i < this.targetRoutes.Length; i++) {
            var route = this.targetRoutes[i];
            for (int j = 0; j < route.nodes.Length; j++) {
                Transform node = route.nodes[j];
                float distance = Vector3.Distance(node.position, PlayerController.main.transform.position);
                if (distance > 5 && distance < 25f) {
                    routeNodes.Add(node);
                }
            }
        }

        // Decide on where to spawn the area
        int r = UnityEngine.Random.Range(0, routeNodes.Count);
        Transform obj = routeNodes[r];

        // Create the area
        GameObject area = Instantiate(goalData.targetPrefab);
        area.transform.position = obj.transform.position; // TODO Replace with random location
        this.targetArea = area;
    }
}
