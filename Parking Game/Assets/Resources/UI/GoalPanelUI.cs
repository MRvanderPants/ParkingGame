using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalDescriptions {

    public static string GetTitleForType(GoalType type) {
        switch (type) {
            case GoalType.CaptureColour:
                return "Colour catcher";

            case GoalType.Stealth:
                return "Private parking";

            case GoalType.Ticket:
                return "Parking Police";

            case GoalType.CaptureTarget:
            default:
                return "Catch that car!";
        }
    }

    public static string GetDescriptionForType(GoalType type) {
        switch (type) {
            case GoalType.CaptureColour:
                return "Capture only cars of the specified colour.";

            case GoalType.Stealth:
                return "Avoid parking any cars and run out the clock.";

            case GoalType.Ticket:
                return "Find misparked cars and give them a ticket.";

            case GoalType.CaptureTarget:
            default:
                return "Find the highlighted car before the time runs out.";
        }
    }
}

public class GoalPanelUI : MonoBehaviour {

    public Color32 defaultColor;
    public Color32 midColor;
    public Color32 lowColor;

    private RectTransform progressBar;
    private float animationSpeed = 0f;
    private Pulse pulse;
    private readonly List<Action<bool>> callbacks = new List<Action<bool>>();

    void Update() {
        if (this.animationSpeed != 0f && this.progressBar.localScale.x > 0f) {
            Vector2 scale = this.progressBar.localScale;
            scale.x -= Time.deltaTime * this.animationSpeed;
            this.progressBar.localScale = scale;

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
        var description = mainPanel.Find("DescriptionPanel").Find("Label").GetComponent<TextMeshProUGUI>();

        title.text = GoalDescriptions.GetTitleForType(goalData.goalType);
        description.text = GoalDescriptions.GetDescriptionForType(goalData.goalType);
        progressBar.localScale = new Vector3(1f, 1f, 1f);
        this.animationSpeed = 1f / goalData.timeLimit;
        this.callbacks.Add(callback);

        this.pulse = this.progressBar.transform.parent.GetComponent<Pulse>();
        this.pulse.enabled = false;
    }
}
