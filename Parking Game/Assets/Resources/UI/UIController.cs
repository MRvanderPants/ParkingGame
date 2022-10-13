using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public static UIController main;

    private Transform goalPanel;
    private Transform targetPanel;
    private Transform mainMenuPanel;

    public GoalPanelUI GoalPanelUI {
        get => this.goalPanel.GetComponent<GoalPanelUI>();
    }

    void Awake() {
        UIController.main = this;
    }

    void Start() {
        this.goalPanel = this.transform.Find("GoalPanel");
        this.targetPanel = this.transform.Find("TargetPanel");
        this.mainMenuPanel = this.transform.Find("MainMenuPanel");
        this.ToggleMainMenu(true);
    }

    public void ToggleMainMenu(bool state) {
        this.goalPanel.gameObject.SetActive(!state);
        this.targetPanel.gameObject.SetActive(!state);
        this.mainMenuPanel.gameObject.SetActive(state);
    }
}
