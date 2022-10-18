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

    private AudioClip clickSFX;

    void Start() {
        this.ApplyColour(this.defaultColor);
    }

    public void ApplyDefault() {
        this.ApplyColour(this.defaultColor);
    }

    public void ApplyHover() {
        this.ApplyColour(this.hoverColor);
        this.clickSFX = Resources.Load<AudioClip>("Audio/SFX/UI/click");
        AudioController.main.PlayClip(this.clickSFX, Mixers.UI);
    }

    public void ApplyPressed() {
        this.ApplyColour(this.pressedColor);
        this.clickSFX = Resources.Load<AudioClip>("Audio/SFX/UI/select_action");
        AudioController.main.PlayClip(this.clickSFX, Mixers.UI);
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
