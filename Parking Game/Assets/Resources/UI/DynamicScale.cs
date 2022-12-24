using UnityEngine;

public class DynamicScale : MonoBehaviour {

    private bool growing = false;
    private bool playedSFX = false;
    private Vector3 targetSize = Vector3.one;
    private float growSpeed = 1.125f;
    private AudioClip pop;
    private AudioClip stretch;
    private AudioClip stretchOut;

    void Update() {
        if (!growing) {
            return;
        }

        bool direction = this.transform.localScale.magnitude < this.targetSize.magnitude;
        if (direction) {
            // grow
            this.transform.localScale *= this.growSpeed;
            if (!this.playedSFX) {
                this.playedSFX = true;
                AudioController.main.PlayClip(this.stretch, Mixers.SFX, 0.66f);
            }
        } else {
            // shrink
            float negSpeed = 1 - (this.growSpeed - 1);
            this.transform.localScale *= negSpeed;
            if (!this.playedSFX) {
                this.playedSFX = true;
                AudioController.main.PlayClip(this.stretchOut, Mixers.SFX, 0.66f);
            }
        }

        float distance = Vector3.Distance(this.transform.localScale, this.targetSize);
        if (distance < 0.1f) {
            this.growing = false;
            AudioController.main.PlayClip(this.pop, Mixers.SFX, 0.66f);
            Destroy(this);
        }
    }

    public void Init(Vector3 targetSize, float speed) {
        this.pop = Resources.Load <AudioClip> ("Audio/SFX/pop");
        this.stretch = Resources.Load <AudioClip> ("Audio/SFX/stretch");
        this.stretchOut = Resources.Load <AudioClip> ("Audio/SFX/stretchOut");
        this.targetSize = targetSize;
        this.growSpeed = speed;
        this.growing = true;
    }
}
