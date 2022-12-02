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
            UIController.main.GoalPanelUI.UpdateLevel(this.score);
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
        this.Score = 0;
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
        UIController.main.ToggleHighscores(true);
        for (int i = 0; i < this.targetRoutes.Length; i++) {
            this.targetRoutes[i].RemoveAllCars();
        }
        Destroy(PlayerController.main.gameObject);
    }
    #endregion

    #region Level controlling methods
    private void StartLevel() {
        this.LevelIndex = this.levelIndex + 1;
        this.levelData = MissionController.main.UpdateLevelData(this.levelData, this.levelIndex);
        MissionController.main.GenerateMissions(this.levelData);
        this.StartMission();
    }

    private void EndLevel() {
        this.Score++;
        this.StartLevel();
    }
    #endregion

    #region  Mission controlling methods
    private void StartMission() {
        new TimedTrigger(0.05f, () => {
            this.targetPanelUI.SetTarget(MissionController.main.CurrentGoalData);
            this.ActivateRandomTargetRoute();
            this.goalPanelUI.StartTimer(MissionController.main.CurrentGoalData, (bool result) => {
                this.EndGame(result);
            });
        });
    }

    public void EndMission() {
        bool hasEnded = MissionController.main.EndMission();
        if (hasEnded) {
            this.EndLevel();
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
