using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {

    public static SettingsUI main;

    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider uiVolumeSlider;
    public TextMeshProUGUI shakeLabel;

    public Transform generalPage;
    public Transform audioPage;
    public Transform screenShakeIcon;

    public int screenShakeOn;

    void Awake() {
        SettingsUI.main = this;
    }

    void Start() {
        this.InitPrefs();
        this.InitListeners();
    }

    public void CreatePlayerPrefs() {
        if(!PlayerPrefs.HasKey("screen_shake_on")) {
            PlayerPrefs.SetFloat("volume_music", 1f);
            PlayerPrefs.SetFloat("volume_sfx", 1f);
            PlayerPrefs.SetFloat("volume_ui", 1f);
            PlayerPrefs.SetInt("screen_shake_on", 1);
        }
    }

    public void Activate(bool generalPage = true) {
        this.InitPrefs();
        if (generalPage) {
            this.generalPage.gameObject.SetActive(true);
            this.audioPage.gameObject.SetActive(false);
        } else {
            this.generalPage.gameObject.SetActive(false);
            this.audioPage.gameObject.SetActive(true);

        }
    }

    /**
     *  Called from UI 
     */
    public void Close() {
        this.Deactivate();
        UIController.main.ToggleSettings(false);
    }

    public void Deactivate() {
        this.generalPage.gameObject.SetActive(false);
        this.audioPage.gameObject.SetActive(false);
    }

    public void ToggleScreenShake() {
        this.screenShakeOn = this.screenShakeOn == 1 ? 0 : 1;
        PlayerPrefs.SetInt("screen_shake_on", this.screenShakeOn);
        this.UpdateScreenshake();
    }

    public void ResetHighscores() {
        HighscoreUI.main.ResetHighscores();
    }

    private void UpdateScreenshake() {
        this.shakeLabel.text = this.screenShakeOn == 1 ? "On" : "Off";
        this.screenShakeIcon.gameObject.SetActive(this.screenShakeOn == 1);
    }

    private void InitPrefs() {
        this.musicVolumeSlider.value = PlayerPrefs.GetFloat("volume_music");
        this.sfxVolumeSlider.value = PlayerPrefs.GetFloat("volume_sfx");
        this.uiVolumeSlider.value = PlayerPrefs.GetFloat("volume_ui");
        this.screenShakeOn = PlayerPrefs.GetInt("screen_shake_on");
        this.UpdateScreenshake();
    }

    private void InitListeners() {
        this.musicVolumeSlider.onValueChanged.AddListener((value) => {
            PlayerPrefs.SetFloat("volume_music", value);
            AudioController.main.UpdateVolumeLevels();
        });

        this.sfxVolumeSlider.onValueChanged.AddListener((value) => {
            PlayerPrefs.SetFloat("volume_sfx", value);
            AudioController.main.UpdateVolumeLevels();
        });

        this.uiVolumeSlider.onValueChanged.AddListener((value) => {
            PlayerPrefs.SetFloat("volume_ui", value);
            AudioController.main.UpdateVolumeLevels();
        });
    }
}
