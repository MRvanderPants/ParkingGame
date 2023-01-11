using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinker : MonoBehaviour {

    private float startTime = -1f;
    private float stopTime = -1f;
    private float animationDuration = -1f;
    private float animationFrame = 1f;
    private int animationState = 0;
    private int lastStateTrigger = 0;
    private SpriteRenderer spriteRenderer;

    public void Init(float stopTime, int blinkAmount, SpriteRenderer spriteRenderer) {
        this.stopTime = stopTime;
        this.spriteRenderer = spriteRenderer;
        this.animationDuration = stopTime - Time.time;
        this.animationFrame = this.animationDuration / (float)blinkAmount;
        this.startTime = Time.time;
        this.animationState = 0;
        this.lastStateTrigger = 0;
    }

    void Update() {
        if (this.startTime != -1f) {

            if (Time.time > this.startTime + (this.animationState * this.animationFrame)) {
                if (this.lastStateTrigger != this.animationState) {
                    this.lastStateTrigger++;
                    Color32 col = this.spriteRenderer.material.color;
                    if (this.animationState % 2 == 0) {
                        col.a = 127;
                    }
                    else {
                        col.a = 255;
                    }
                    this.spriteRenderer.material.color = col;
                }
                this.animationState++;
            }

            if (Time.time > this.stopTime) {
                Color32 col = this.spriteRenderer.material.color;
                col.a = 255;
                this.spriteRenderer.material.color = col;
                this.startTime = -1f;
                Destroy(this);
            }
        }
    }
}
