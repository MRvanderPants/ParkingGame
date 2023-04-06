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

    public void EndGame() {
        this.gameEnded = true;
        UIController.main.ToggleHighscores(true);
        for (int i = 0; i < this.targetRoutes.Length; i++) {
            this.targetRoutes[i].RemoveAllCars();
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
            if (goalData.goalType != GoalType.Stealth && goalData.goalType != GoalType.HyperMode) {
                this.targetPanelUI.SetTarget(goalData);
                this.ActivateRandomTargetRoute();
            } else {
                this.targetPanelUI.Hide();
            }

            if (goalData.goalType == GoalType.HyperMode) {
                PlayerController.main.SetHyperMode(true);
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
                this.HandleSpawns(goalData);
            } else {
                for (int i = 0; i < this.pickupSpawners.Length; i++) {
                    this.pickupSpawners[i].Clear();
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

    private void HandleSpawns(GoalData goalData) {
        new TimedTrigger(goalData.timeLimit * 0.33f, () => {
            this.SpawnPickups(0.3f);
        });

        new TimedTrigger(goalData.timeLimit * 0.5f, () => {
            this.SpawnPickups(0.1f);
        });

        new TimedTrigger(goalData.timeLimit * 0.75f, () => {
            this.SpawnPickups(0.2f);
        });

        new TimedTrigger(goalData.timeLimit * 0.85f, () => {
            this.SpawnPickups(0.3f);
        });
    }

    private void SpawnPickups(float percentage) {
        int total = Mathf.RoundToInt(this.pickupSpawners.Length * percentage);
        for (int i = 0; i < total; i++) {
            int r = UnityEngine.Random.Range(0, this.pickupSpawners.Length);
            PickupSpawner spawner = this.pickupSpawners[r];
            spawner.Spawn();
        }
    }
}
