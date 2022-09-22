using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float minimumRenderDistance = 30f;
    public float viewportCutoff = 0.4f;

    private PlayerController playerController;
    private Camera mainCamera;

    void Start() {
        this.playerController = PlayerController.main;
        this.mainCamera = this.GetComponent<Camera>();
    }

    void Update() {
        Vector3 playerPos = this.playerController.transform.position;
        playerPos.z = this.transform.position.z;
        var journeyLength = Vector3.Distance(this.transform.position, playerPos);
        float fractionOfJourney = Time.time / journeyLength;
        if (float.IsNaN(fractionOfJourney)) {
            fractionOfJourney = 0f;
        }
        this.transform.position = Vector3.Lerp(this.transform.position, playerPos, fractionOfJourney);
        if (journeyLength > 0.1f) {
            this.HandleVisibility();
        }
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
}
