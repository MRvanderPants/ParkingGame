using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour {
    public Image fade;
    void Start() {
        this.fade.color = new Color32(0, 0, 0, 0);
        new TimedTrigger(4f, () => {
            float count = 0f;
            new TimedTrigger(0.01f, () => {
                count += 20;
                this.fade.color = new Color32(0, 0, 0, (byte)count);
                if (count >= 255f) {
                    this.fade.color = new Color32(0, 0, 0, 255);
                    UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                }
            }, true);
        });
    }
}
