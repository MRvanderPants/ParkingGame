using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GoalType {
    CaptureTarget,
    CaptureColour,
    Stealth,
    Ticket,
    CarMoving,
};

[System.Serializable]
public class GoalData {
    public GoalType goalType;
    public int value;
    public float timeLimit;
    public GameObject targetPrefab;
    public Color32 targetColour;
}

public class LevelController : MonoBehaviour {

    public static LevelController main;

    private GoalData goalData;
    private GoalPanelUI goalPanelUI;
    private TargetPanelUI targetPanelUI;
    private TrafficRoute[] targetRoutes;
    private int score = 0;

    public GoalData CurrentGoalData {
        get => this.goalData;
    }

    public int Score {
        get => this.score;
        set {
            this.score = value;
            UIController.main.GoalPanelUI.UpdateScore(this.score);
        }
    }

    void Awake() {
        LevelController.main = this;
    }

    void Start() {
        this.goalPanelUI = this.transform.Find("GoalPanel").GetComponent<GoalPanelUI>();
        this.targetPanelUI = this.transform.Find("TargetPanel").GetComponent<TargetPanelUI>();
        this.goalData = this.GenerateGoalData();
        this.FindAllTargetRoutes();
    }

    public void StartGame() {
        this.Score = 0;
        UIController.main.ToggleMainMenu(false);
        this.CreatePlayer();
        GameController.main.StartGame(this);
    }

    public void EndMission() {
        this.Score++;
        this.StartLevel();
    }

    public void StartLevel() {
        this.goalData = this.GenerateGoalData();
        new TimedTrigger(0.05f, () => {
            this.targetPanelUI.SetTarget(this.goalData);
            this.ActivateRandomTargetRoute();
            this.goalPanelUI.StartTimer(this.goalData, (bool result) => {
                this.FinishLevel(result);
            });
        });
    }

    public void FinishLevel(bool result) {
        Debug.Log("Ended in " + result);
        UIController.main.ToggleMainMenu(true);
        for (int i = 0; i < this.targetRoutes.Length; i++) {
            this.targetRoutes[i].RemoveAllCars();
        }
        Destroy(PlayerController.main.gameObject);
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

    private GoalData GenerateGoalData() {
        return new GoalData() {
            goalType = this.GenerateGoalType(),
            value = 0,
            timeLimit = UnityEngine.Random.Range(20, 30), // TODO replace
            targetPrefab = Resources.Load<GameObject>("Prefabs/Car"),
            targetColour = this.GenerateColor(),
        };
    }

    private GoalType GenerateGoalType() {
        int r = UnityEngine.Random.Range(0, 4);
        switch (r) {
            case 0: return GoalType.CaptureColour;
            case 1: return GoalType.Stealth;
            case 2: return GoalType.Ticket;
            case 3:
            default: return GoalType.CaptureTarget;
        }
    }

    private Color32 GenerateColor() {
        byte r = (byte)UnityEngine.Random.Range(0, 255);
        byte g = (byte)UnityEngine.Random.Range(0, 255);
        byte b = (byte)UnityEngine.Random.Range(0, 255);
        return new Color32(r, g, b, 255);
    }
}
