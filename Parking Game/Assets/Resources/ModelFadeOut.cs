using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelFadeOut : MonoBehaviour {

    public byte fadeOutSpeed = 1;

    private Material material;
    private bool fadingOut = false;
    private Action onDestroy;

    // Update is called once per frame
    void Update() {
        if (this.fadingOut) {
            this.HandleFadeOut();
        }
    }

    public void Init(Material material, Action onDestroy) {
        this.material = material;
        this.fadingOut = true;
        this.onDestroy = onDestroy;
    }

    private void HandleFadeOut() {
        Color32 col = this.material.color;
        float speed = this.fadeOutSpeed * LevelController.main.SpeedMultiplier;
        col.a -= (byte)speed;
        this.material.color = col;

        if (col.a <= 0) {
            this.onDestroy();
            Destroy(this.gameObject);
        }
    }
}
