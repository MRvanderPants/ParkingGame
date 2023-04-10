using System;
using System.Collections.Generic;
using UnityEngine;

public class EnlargeBounce : MonoBehaviour {

    private readonly float lerpTime = 0.05f;
    private readonly float scaleDistance = 1.175f;
    private Vector3 initialScale;
    private bool bounced = false;

    private readonly List<Action> callbacks = new List<Action>();

    public void OnAnimationEnd(Action callback) {
        this.callbacks.Add(callback);
    }

    private Vector3 Scale {
        get => this.transform.localScale;
        set => this.transform.localScale = value;
    }

    private float ScalePercentage {
        get => scaleDistance - 1f;
    }

    private float BouncePoint {
        get => 1 + (this.ScalePercentage * 0.33f);
    }

    void Start() {
        this.initialScale = this.Scale;
        this.Scale = this.Scale * this.scaleDistance;
    }

    void FixedUpdate() {
        this.Scale = Vector3.Lerp(this.Scale, this.initialScale, this.lerpTime);

        if (!this.bounced && this.Scale.x <= this.initialScale.x * this.BouncePoint) {
            this.Scale = this.Scale * (this.BouncePoint + 0.1f);
            this.bounced = true;
        }

        if (Vector3.Distance(this.Scale, this.initialScale) <= 0.05) {
            this.transform.localScale = this.initialScale;
            for (int i = 0; i < this.callbacks.Count; i++) {
                this.callbacks[i]();
            }
            this.callbacks.Clear();
            this.Scale = this.initialScale;
            Destroy(this);
        }
    }
}
