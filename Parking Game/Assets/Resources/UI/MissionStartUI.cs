using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionStartUI : MonoBehaviour {

    public static MissionStartUI main;

    private GameObject backPanel;
    private RectTransform rect;
    private TextMeshProUGUI label;
    private Image icon;

    void Awake() {
        MissionStartUI.main = this;
    }

    void Start() {
        this.backPanel = this.transform.Find("BackPanel").gameObject;
        this.rect = this.transform.Find("Panel").GetComponent<RectTransform>();
        this.label = this.rect.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        this.icon = this.rect.transform.Find("Icon").GetComponent<Image>();
        this.rect.gameObject.SetActive(false);
        this.backPanel.SetActive(false);
    }

    public void Display(GoalData goalData) {
        this.rect.gameObject.SetActive(true);
        this.backPanel.SetActive(true);
        var settings = MissionController.main.GetMissionSettingsForType(goalData.goalType);
        this.icon.overrideSprite = settings.icon;
        this.label.text = settings.name;

        new TimedTrigger(2.5f, () => {
            this.backPanel.SetActive(false);
            this.rect.gameObject.SetActive(false);
        });
    }
}
