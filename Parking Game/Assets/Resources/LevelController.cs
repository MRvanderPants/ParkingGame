using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GoalType {
    CaptureTarget,
    CaptureColour,
    Stealth,
    Ticket,
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

    private GoalPanelUI goalPanelUI;
    private TargetPanelUI targetPanelUI;

    private TrafficRoute[] targetRoutes;
    private PlayerController playerController;

    void Start() {
        this.goalPanelUI = this.transform.Find("GoalPanel").GetComponent<GoalPanelUI>();
        this.targetPanelUI = this.transform.Find("TargetPanel").GetComponent<TargetPanelUI>();
        this.FindAllTargetRoutes();
    }

    public void StartGame() {
        UIController.main.ToggleMainMenu(false);
        GameController.main.StartGame(this);
    }

    public void StartLevel(GoalData goalData) {
        this.CreatePlayer();
        new TimedTrigger(0.05f, () => {
            this.targetPanelUI.SetTarget(goalData);
            this.ActivateRandomTargetRoute();
            this.goalPanelUI.StartTimer(goalData, (bool result) => {
                this.FinishLevel(result);
            });
        });
    }

    public void FinishLevel(bool result) {
        Debug.Log("Ended in " + result);
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
}
