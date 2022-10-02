using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour {

    public static TimerUI main;

    private Canvas canvas;
    private Image progressBar;
    private float animationSpeed = 0f;
    private readonly List<Action> callbacks = new List<Action>();

    void Awake() {
        TimerUI.main = this;
    }

    void Start() {
        this.progressBar = this.GetComponent<Image>();
        this.canvas = this.transform.parent.parent.parent.parent.GetComponent<Canvas>();
        this.canvas.enabled = false;
    }

    void Update() {
        if (this.animationSpeed != 0f && this.progressBar.fillAmount > 0f) {
            this.progressBar.fillAmount -= Time.deltaTime * this.animationSpeed;

            if (this.progressBar.fillAmount <= 0f) {
                for (int i = 0; i < this.callbacks.Count; i++) {
                    this.callbacks[i]();
                }
                this.callbacks.Clear();
                this.canvas.enabled = false;
            }
        }
    }

    public void StartTimer(float duration, Action callback) {
        this.canvas.enabled = true;
        this.progressBar.fillAmount = 1f;
        this.animationSpeed = 1f / duration;
        this.callbacks.Add(callback);
    }
}
