using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalePulse : MonoBehaviour {

    public bool direction = true;
    public float maxSize = 1.5f;
    public float animationSpeed = 1.05f;

    private Vector3 startScale;

    void Start() {
        this.startScale = this.transform.localScale;
    }

    void Update() {
        if (direction) {
            this.transform.localScale *= this.animationSpeed;
            if (this.transform.localScale.x > this.maxSize) {
                this.direction = !this.direction;
            }
        } else {
            this.transform.localScale *= 1 - (this.animationSpeed - 1);
            if (this.transform.localScale.x < this.startScale.x) {
                this.direction = !this.direction;
            }
        }
    }
}
