using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedTriggerMono : MonoBehaviour {

    private float startTime = -1f;
    private float delayTime = -1f;
    private Action callback;
    private bool loop = false;

    void FixedUpdate() {
        if (this.callback != null && this.startTime != -1 && Time.time > this.startTime + this.delayTime) {
            this.callback();
            if (!this.loop) {
                Destroy(this);
            } else {
                this.startTime = Time.time;
            }
        }
    }

    public void Init(float delayTime, Action callback, bool loop) {
        this.delayTime = delayTime;
        this.startTime = Time.time;
        this.callback = callback;
        this.loop = loop;
    }

    public void Destroy() {
        Destroy(this);
    }
}

public class TimedTrigger {

    private TimedTriggerMono component;

    public TimedTrigger(float time, Action callback, bool loop = false) {
        this.component = Camera.main.gameObject.AddComponent<TimedTriggerMono>();
        this.component.Init(time, callback, loop);
    }

    public void Destroy() {
        this.component.Destroy();
    }
}
