using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIAlignColourWithButton : MonoBehaviour {

    public Color32 defaultColor;
    public Color32 hoverColor;
    public Color32 pressedColor;
    public Color32 disabledColor;
    public Image[] images;

    void Start() {
        this.ApplyColour(this.defaultColor);
    }

    public void ApplyDefault() {
        this.ApplyColour(this.defaultColor);
    }

    public void ApplyHover() {
        this.ApplyColour(this.hoverColor);
        AudioController.main.PlayClip("click", Mixers.UI);
    }

    public void ApplyPressed() {
        this.ApplyColour(this.pressedColor);
        AudioController.main.PlayClip("select_action", Mixers.UI);
    }

    public void ApplyDisabled() {
        this.ApplyColour(this.disabledColor);
    }

    public void ApplyColour(Color color) {
        for (int i = 0; i < this.images.Length; i++) {
            this.images[i].color = color;
        }
    }
}
