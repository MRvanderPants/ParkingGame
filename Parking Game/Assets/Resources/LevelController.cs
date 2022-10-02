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
}

public class LevelController : MonoBehaviour {

    private GoalPanelUI goalPanelUI;

    void Start() {
        this.goalPanelUI = this.transform.Find("GoalPanel").GetComponent<GoalPanelUI>();
        GameController.main.StartGame(this);
    }

    public void StartLevel(GoalData goalData) {
        this.goalPanelUI.StartTimer(goalData, (bool result) => {
            this.FinishLevel(result);
        });
    }

    private void FinishLevel(bool result) {
        Debug.Log("Ended in " + result);
    }
}
