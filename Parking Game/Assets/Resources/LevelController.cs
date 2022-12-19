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

    private GoalPanelUI goalPanelUI;
    private TargetPanelUI targetPanelUI;
    private TrafficRoute[] targetRoutes;
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
        this.LevelIndex = 0;
        UIController.main.ToggleMainMenu(false);
        this.CreateInitialLevelData();
        this.CreatePlayer();
        this.StartLevel();
        AudioController.main.StopMixer(Mixers.Music);
        new TimedTrigger(2f, () => {
            AudioController.main.PlayMusic(this.LevelMusicIntro, this.LevelMusic, 0.3f);
        });
    }

    public void EndGame(bool result) {
        this.gameEnded = true;
        UIController.main.ToggleHighscores(true);
        for (int i = 0; i < this.targetRoutes.Length; i++) {
            this.targetRoutes[i].RemoveAllCars();
        }
        if (PlayerController.main.gameObject != null) {
            Destroy(PlayerController.main.gameObject);
        }
    }
    #endregion

    private void StartLevel() {
        this.LevelIndex = this.levelIndex + 1;
        this.levelData = MissionController.main.UpdateLevelData(this.levelData, this.levelIndex);
        MissionController.main.GenerateMissions(this.levelData);
        this.StartMission();
    }

    #region  Mission controlling methods
    private void StartMission() {
        new TimedTrigger(0.05f, () => {
            GoalData goalData = MissionController.main.CurrentGoalData;
            if (goalData.goalType != GoalType.Stealth) {
                this.targetPanelUI.SetTarget(goalData);
                this.ActivateRandomTargetRoute();
            } else {
                this.targetPanelUI.Hide();
            }
            this.goalPanelUI.StartTimer(goalData, (bool result) => {
                if (this.gameEnded) { return; }

                if (goalData.goalType == GoalType.Stealth) {
                    this.EndMission();
                } else {
                    this.EndGame(result);
                }
            });
        });
    }

    public void EndMission() {
        this.Score += this.levelIndex;
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
}
