using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalPanelUI : MonoBehaviour {

    public Color32 defaultColor;
    public Color32 midColor;
    public Color32 lowColor;

    private RectTransform progressBar;
    private float animationSpeed = 0f;
    private Pulse pulse;
    private readonly List<Action<bool>> callbacks = new List<Action<bool>>();
    private TextMeshProUGUI scoreLabel;
    private TextMeshProUGUI levelLabel;

    void Update() {
        if (this.animationSpeed != 0f && this.progressBar.localScale.x > 0f) {
            Vector2 scale = this.progressBar.localScale;
            scale.x -= Time.deltaTime * this.animationSpeed * LevelController.main.SpeedMultiplier;
            this.progressBar.localScale = scale;
            if (this.progressBar.localScale.x > 0.5f) {
                this.progressBar.GetComponent<Image>().color = this.defaultColor;
            }

            if (this.progressBar.localScale.x <= 0.5f) {
                this.progressBar.GetComponent<Image>().color = this.midColor;
            }

            if (this.progressBar.localScale.x <= 0.25f) {
                this.progressBar.GetComponent<Image>().color = this.lowColor;
                this.pulse.enabled = true;
                this.pulse.animationSpeed = 1.0025f;
            }

            if (this.progressBar.localScale.x <= 0f) {
                for (int i = 0; i < this.callbacks.Count; i++) {
                    this.callbacks[i](false);
                }
                this.callbacks.Clear();
                this.pulse.enabled = false;
                this.progressBar.GetComponent<Image>().color = this.defaultColor;
            }
        }
    }

    public void StartTimer(GoalData goalData, Action<bool> callback) {
        Transform mainPanel = this.transform.Find("BackPanel").Find("Main Panel");
        this.progressBar = mainPanel.Find("ProgressPanel").Find("Fill").GetComponent<RectTransform>();
        var title = mainPanel.Find("Title").GetComponent<TextMeshProUGUI>();
        var icon = mainPanel.Find("Title").Find("Icon").GetComponent<Image>();
        var description = mainPanel.Find("DescriptionPanel").Find("Label").GetComponent<TextMeshProUGUI>();
        var settings = MissionController.main.GetMissionSettingsForType(goalData.goalType);

        title.text = settings.name;
        description.text = settings.description;
        progressBar.localScale = new Vector3(1f, 1f, 1f);
        icon.overrideSprite = settings.icon;
        this.animationSpeed = 1f / goalData.timeLimit;
        this.callbacks.Clear();
        this.callbacks.Add(callback);

        this.pulse = this.progressBar.transform.parent.GetComponent<Pulse>();
        this.pulse.enabled = false;
    }

    public void UpdateScore(int score) {
        this.scoreLabel = this.transform.Find("BackPanel").Find("Main Panel").Find("ScorePanel").Find("Label").GetComponent<TextMeshProUGUI>();
        this.scoreLabel.transform.parent.gameObject.AddComponent<EnlargeBounce>();
        this.scoreLabel.text = (score * 100).ToString();
    }

    public void UpdateLevel(int level) {
        this.levelLabel = this.transform.Find("BackPanel").Find("Main Panel").Find("LevelPanel").Find("Label").GetComponent<TextMeshProUGUI>();
        this.levelLabel.transform.parent.gameObject.AddComponent<EnlargeBounce>();
        this.levelLabel.text = level + " LVL";
    }
}
