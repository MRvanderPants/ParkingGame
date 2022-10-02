using System;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimation : MonoBehaviour {

    public float distance = -1f;
    public float startingValue = -1f;
    public bool direction = true;
    public object target = null;

    private float animationSpeed = -1f;
    private bool started = false;
    private Action callback;

    public float Speed {
        get => (850f / (float)Screen.width) * this.animationSpeed;
    }
    
    public void Init(float speed, float distance, bool direction = true, Action callback = null) {
        this.animationSpeed = speed;
        this.direction = direction;
        this.callback = callback;
        this.started = true;
        this.distance = distance;
        this.target = this.GetTarget();
        this.startingValue = this.StartingValue();
        this.UpdateUI();
    }

    // Update is called once per frame
    void Update() {
        if (this.started) {
            this.distance += this.direction ? this.Speed : - this.Speed;
            this.UpdateUI();

            if (CheckLimit(this.distance)) {
                if (this.callback != null) {
                    this.callback();
                }
                Destroy(this);
            }
        }
    }

    public virtual float StartingValue() { return -1f; }
    public virtual object GetTarget() { return null; }
    public virtual void UpdateUI() { }
    public virtual bool CheckLimit(float distance) { return true; }
}
