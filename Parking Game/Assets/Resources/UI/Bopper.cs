using UnityEngine;

public class Bopper : MonoBehaviour {

    public bool moveVertically = false;
    public float distance = 5f;
    public float speed = 0.1f;

    private bool direction = false;
    private bool started = false;
    private float start = 0f;

    void Start() {
        this.direction = false;
        this.started = true;
        this.start = (moveVertically) ? this.transform.localPosition.y : this.transform.localPosition.x;
    }

    void FixedUpdate() {
        if (started) {

            Vector3 pos = this.transform.localPosition;
            if (this.moveVertically) {
                if (this.transform.localPosition.y >= this.start + this.distance || this.transform.localPosition.y < this.start) {
                    this.direction = !this.direction;
                }

                if(this.direction) {
                    pos.y -= speed;
                } else {
                    pos.y += speed;
                }
            }

            if (!this.moveVertically) {
                if (this.transform.localPosition.x >= this.start + this.distance || this.transform.localPosition.x < this.start) {
                    this.direction = !this.direction;
                }

                if (this.direction) {
                    pos.x += speed;
                }
                else {
                    pos.x -= speed;
                }
            }
            this.transform.localPosition = pos;
        }
    }
}
