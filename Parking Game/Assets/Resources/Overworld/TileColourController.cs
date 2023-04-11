using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileColourController : MonoBehaviour {

    public static TileColourController main;

    public Color32[] colours;

    private Metronome metronome;
    private TilemapRenderer tilemapRenderer;
    private int colourIndex = 0;

    void Awake() {
        TileColourController.main = this;
    }

    public void Init(int bpm = 140) {
        return;
        this.tilemapRenderer = this.gameObject.GetComponent<TilemapRenderer>();
        this.metronome = this.gameObject.AddComponent<Metronome>();
        this.metronome.StopTicking();
        this.metronome.StartTicking(bpm, TimeSignatures.Four_Four);
        this.metronome.OnTick.Subscribe((data) => {
            Ticks tick = (Ticks)data;
            this.OnTick(tick);
        });
    }

    private void OnTick(Ticks tick) {
        if (tick == Ticks.Quarter || tick == Ticks.Whole) {
            this.colourIndex++;
            if (this.colourIndex > this.colours.Length - 1) {
                this.colourIndex = 0;
            }
            this.tilemapRenderer.material.color = this.colours[this.colourIndex];
        }
    }
}
