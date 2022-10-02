using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour {

    public bool direction = true;
    public float maxSize = 1.5f;
    public float animationSpeed = 1.05f;

    private RectTransform rect;
    private Vector3 startScale;

    void Start() {
        this.rect = this.GetComponent<RectTransform>();
        this.startScale = this.transform.localScale;
    }

    void Update() {
        if (direction) {
            this.rect.localScale *= this.animationSpeed;
            if (this.rect.localScale.x > this.maxSize) {
                this.direction = !this.direction;
            }
        } else {
            this.rect.localScale *= 1 - (this.animationSpeed - 1);
            if (this.rect.localScale.x < this.startScale.x) {
                this.direction = !this.direction;
            }
        }
    }
}
