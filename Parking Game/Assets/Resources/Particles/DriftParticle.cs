using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftParticle : MonoBehaviour {
    public float lifetime = 2f;
    void Start() {
        new TimedTrigger(lifetime, () => {
            if (this == null) {
                return;
            }
            this.FadeOut(() => { });
        });
    }

    public void FadeOut(Action onDestroy) {
        ModelFadeOut fader = this.gameObject.AddComponent<ModelFadeOut>();
        fader.Init(this.GetComponent<MeshRenderer>().material, onDestroy);
    }
}
