using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController main;

    public GoalData[] minigames;

    void Awake() {
        GameController.main = this;
    }

    void Start() {
        
    }

    public void StartGame(LevelController controller) {
        this.StartLevel(this.minigames, controller);
    }

    private void StartLevel(GoalData[] minigames, LevelController controller) {
        controller.StartLevel(minigames[0]);
    }
}
