using System;
using UnityEngine;

public enum TimeSignatures {
    Four_Four,
    Three_Four,
    Twelve_Eight
}

public enum Tempos {
    Regular,
    DoubleTime,
    HalfTime
}

public enum Ticks {
    Whole,
    Quarter,
    Eight
}

public class Metronome : MonoBehaviour {

    public readonly Observable OnTick = new Observable();
    public readonly Observable OnTickOnce = new Observable();

    // The index of the current tick in the bar
    private int tickIndex = 0;

    // The time-delay between ticks
    private float tickTime;

    // The last time a tick occured
    private float lastTickTime = -1f;

    // Makes sure the eight notes will be locked
    private bool eightTriggered = false;

    private int speedChange = -1;
    private int bpm;
    private float delay = 0.15f;

    public int Bpm { get { return this.bpm; } }

    public float Delay {
        get => this.delay;
        set => this.delay = value;
    }

    public float BeatsPerSecond {
        get => this.bpm / 60f;
    }

    public float SecondsPerBeat {
        get => 1f / this.BeatsPerSecond;
    }

    void FixedUpdate() {
        this.HandleTicks();
    }

    public void StartTicking(int bpm, TimeSignatures timeSignature, float audioDelay = 0.15f) {
        this.lastTickTime = Time.time;
        this.bpm = bpm;
        this.tickTime = BPM.BpmToTime(bpm);
        this.Signature = timeSignature;
        this.delay = audioDelay;
    }

    public void StopTicking() {
        this.lastTickTime = -1f;
    }

    public int Speed {
        get {
            switch(this.speedChange) {
                case -1: return 1;
                case 0: return 0;
                case 1: return 2;
                default: return -1;
            }
        }
        set {
            if ((value == 1 && this.speedChange == 0) || (value == 0 && this.speedChange == 1)) {
                this.speedChange = -1;
            }
            else {
                this.speedChange = value;
            }
        }
    }

    public float TickTime {
        get { return this.tickTime * 0.5f; }
    }

    public TimeSignatures Signature { get; private set; }

    public void QueueSpeedChange(bool speedUp) {
        if ((speedUp && this.speedChange == 0) || (!speedUp && this.speedChange == 1)) {
            this.speedChange = -1;
        } else {
            this.speedChange = (speedUp) ? 1 : 0;
        }
    }

    public Action Pause() {
        this.lastTickTime = -1f;
        return () => {
            this.Resume();
        };
    }

    private void Resume() {
        this.lastTickTime = Time.time;
    }

    private void HandleTicks() {
        if (this.lastTickTime != -1f) {
            if (Time.time > this.lastTickTime + tickTime) {
                this.TriggerQuarter();
            } else if (Time.time > this.lastTickTime + (tickTime * 0.5f) && !this.eightTriggered) {
                this.TriggerEight();
            }
        }
    }

    private void TriggerQuarter() {
        Ticks type = Ticks.Quarter;

        int index = this.tickIndex;
        if (index == 0) {
            type = Ticks.Whole;
            this.AdjustSpeed();
        }

        index++;
        int barLength = BPM.GetBarLength(this.Signature);
        if (index >= barLength) {
            index = 0;
        }

        this.ResetTick(index, type);
    }

    private void TriggerEight() {
        this.eightTriggered = true;
        this.TriggerCallback(Ticks.Eight);
    }

    private void TriggerCallback(Ticks type) {
        this.OnTick.Next(type);

        if (type == Ticks.Whole) {
            this.OnTickOnce.Next(type);
            this.OnTickOnce.Destroy();
        }
    }

    private void ResetTick(int index, Ticks type) {
        this.tickIndex = index;
        this.lastTickTime = Time.time;
        this.eightTriggered = false;
        this.TriggerCallback(type);
    }

    private void AdjustSpeed() {
        switch (this.speedChange) {
            case 0:
                this.tickTime = BPM.BpmToTime(this.bpm) * 1.5f;
                break;

            case 1:
                this.tickTime = BPM.BpmToTime(this.bpm) * 0.5f;
                break;

            default:
                this.tickTime = BPM.BpmToTime(this.bpm);
                break;
        }
    }
}
