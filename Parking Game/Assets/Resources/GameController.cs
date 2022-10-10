using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController main;
    public GoalData[] minigames;
    private LevelController controller;

    public bool Ready {
        get => this.controller.CurrentGoalData != null;
    }

    void Awake() {
        GameController.main = this;
    }

    public void StartGame(LevelController controller) {
        this.controller = controller;
        this.controller.StartLevel();
    }

    public void Next() {
        this.controller.StartLevel();
    }
}
