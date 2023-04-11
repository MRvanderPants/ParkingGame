using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public static CameraController main;

    public float minimumRenderDistance = 30f;
    public float viewportCutoff = 0.4f;

    private Camera mainCamera;
    private Transform wrapper;
    private float shake = 0f;
    private float shakeAmount = 0.5f;
    private float decreaseFactor = 1.25f;

    void Awake() {
        CameraController.main = this;
    }

    void Start() {
        this.wrapper = this.transform.parent;
        this.mainCamera = this.GetComponent<Camera>();
    }

    void Update() {
        if (PlayerController.main == null) {
            return;
        }
        this.HandleMovement();
        this.HandleShake();
    }

    public void Shake(float duration, float amount = 0.5f, float decreaseFactor = 1.25f) {
        if (PlayerPrefs.GetInt("screen_shake_on") != 1) {
            return;
        }
        this.shakeAmount = amount;
        this.decreaseFactor = decreaseFactor;
        this.shake = duration;
    }

    private void HandleVisibility() {
        GameObject[] models = GameObject.FindGameObjectsWithTag("environment");
        for (int i = 0; i < models.Length; i++) {
            Transform modelTransform = models[i].transform;
            var dist = Vector3.Distance(modelTransform.position, this.mainCamera.transform.position);
            if (dist < this.minimumRenderDistance) {
                this.ToggleRenderObject(modelTransform);
            }
        }
    }

    private void ToggleRenderObject(Transform obj) {
        Vector3 viewPos = this.mainCamera.WorldToViewportPoint(obj.position);
        float min = 0 - this.viewportCutoff;
        float max = 1 + this.viewportCutoff;
        bool isEnabled = viewPos.x >= min && viewPos.x <= max && viewPos.y >= min && viewPos.y <= max && viewPos.z > 0;
        obj.GetComponent<Renderer>().enabled = isEnabled;
    }

    private void HandleMovement() {
        Vector3 playerPos = PlayerController.main.transform.position;
        playerPos.z = this.wrapper.position.z;
        var journeyLength = Vector3.Distance(this.wrapper.position, playerPos);
        float fractionOfJourney = Time.time / journeyLength;
        if (float.IsNaN(fractionOfJourney)) {
            fractionOfJourney = 0f;
        }
        this.wrapper.position = Vector3.Lerp(this.wrapper.position, playerPos, fractionOfJourney);
        if (journeyLength > 0.1f) {
            this.HandleVisibility();
        }
    }

    private void HandleShake() {
        if (this.shake > 0f) {
            this.mainCamera.transform.localPosition = Random.insideUnitSphere * shakeAmount;
            shake -= Time.deltaTime * decreaseFactor;
        }
        else {
            this.shake = 0f;
        }
    }
}
