using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedTriggerMono : MonoBehaviour {

    private float startTime = -1f;
    private float delayTime = -1f;
    private Action callback;
    private bool loop = false;
    private bool relative = false;

    void FixedUpdate() {
        float delay = this.relative ? LevelController.main.SpeedMultiplier * this.delayTime : this.delayTime;
        if (this.callback != null && this.startTime != -1 && Time.time > this.startTime + delay) {
            this.callback();
            if (!this.loop) {
                Destroy(this);
            } else {
                this.startTime = Time.time;
            }
        }
    }

    public void Init(float delayTime, Action callback, bool loop, bool relative = false) {
        this.delayTime = delayTime;
        this.startTime = Time.time;
        this.relative = relative;
        this.callback = callback;
        this.loop = loop;
    }

    public void Destroy() {
        Destroy(this);
    }
}

public class TimedTrigger {

    private readonly TimedTriggerMono component;

    public TimedTrigger(float time, Action callback, bool loop = false) {
        this.component = Camera.main.gameObject.AddComponent<TimedTriggerMono>();
        this.component.Init(time, callback, loop);
    }

    public void Destroy() {
        this.component.Destroy();
    }
}

public class LevelControlledTimedTrigger {

    private readonly TimedTriggerMono component;

    public LevelControlledTimedTrigger(float time, Action callback, bool loop = false) {
        this.component = Camera.main.gameObject.AddComponent<TimedTriggerMono>();
        this.component.Init(time, callback, loop, true);
    }

    public void Destroy() {
        this.component.Destroy();
    }
}
