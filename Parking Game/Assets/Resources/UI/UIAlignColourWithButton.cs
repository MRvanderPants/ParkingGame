using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIAlignColourWithButton : MonoBehaviour {

    public Color32 defaultColor;
    public Image[] images;

    void Start() {
        this.ApplyColour(this.defaultColor);
    }

    public void ApplyColour(Color32 color) {
        for (int i = 0; i < this.images.Length; i++) {
            this.images[i].color = this.defaultColor;
        }
    }
}
