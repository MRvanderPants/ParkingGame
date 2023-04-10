using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public static UIController main;
    public AudioClip MainMenuMusic;
    public AudioClip HighscoreMusic;

    private Transform goalPanel;
    private Transform targetPanel;
    private Transform mainMenuPanel;
    private Transform highscorePanel;
    private Transform settingsPanel;

    public GoalPanelUI GoalPanelUI {
        get => this.goalPanel.GetComponent<GoalPanelUI>();
    }

    void Awake() {
        UIController.main = this;
    }

    void Start() {
        SettingsUI.main.CreatePlayerPrefs();
        this.goalPanel = this.transform.Find("GoalPanel");
        this.targetPanel = this.transform.Find("TargetPanel");
        this.mainMenuPanel = this.transform.Find("MainMenuPanel");
        this.highscorePanel = this.transform.Find("HighScoresPanel");
        this.settingsPanel = this.transform.Find("SettingsPanel");
        this.ToggleHighscores(false);
        this.ToggleSettings(false);
        this.ToggleMainMenu(true);
    }

    public void ToggleMainMenu(bool state) {
        this.goalPanel.gameObject.SetActive(!state);
        this.targetPanel.gameObject.SetActive(!state);
        this.mainMenuPanel.gameObject.SetActive(state);
        if (state) {
            AudioController.main.PlayMusic(this.MainMenuMusic);
        } else {
            AudioController.main.StopMixer(Mixers.Music);
        }
    }

    public void ToggleHighscores(bool state) {
        this.highscorePanel.gameObject.SetActive(state);
        if(state) {
            AudioController.main.PlayMusic(this.HighscoreMusic);
            this.highscorePanel.GetComponent<HighscoreUI>().Activate();
        } else {
            AudioController.main.StopMixer(Mixers.Music);
        }
    }

    public void OpenSettings() {
        this.ToggleSettings(true);
    }

    public void ToggleSettings(bool state) {
        this.mainMenuPanel.gameObject.SetActive(!state);
        this.settingsPanel.gameObject.SetActive(state);
        if (state) {
            SettingsUI.main.Activate();
        }
        else {
            SettingsUI.main.Deactivate();
        }
    }
}
