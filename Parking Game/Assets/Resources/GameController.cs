using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController main;

    public GoalData[] minigames;

    private LevelController controller;
    private readonly List<GoalData> minigameSelection = new List<GoalData>();

    public bool Ready {
        get => this.minigameSelection.Count > 0;
    }

    public GoalData CurrentGoal {
        get => this.minigameSelection.Count > 0 ? this.minigameSelection[0] : null;
    }

    void Awake() {
        GameController.main = this;
    }

    public void StartGame(LevelController controller) {
        this.controller = controller;
        this.StartLevel(this.minigames);
    }

    public void Next() {
        this.minigameSelection.RemoveAt(0);
        if (this.minigameSelection.Count <= 0) {
            this.controller.FinishLevel(true);
            return;
        }
        controller.StartLevel(this.minigameSelection[0]);
    }

    private void StartLevel(GoalData[] minigames) {
        this.minigameSelection.Clear();
        this.minigameSelection.AddRange(minigames);
        this.controller.StartLevel(minigames[0]);
    }
}
