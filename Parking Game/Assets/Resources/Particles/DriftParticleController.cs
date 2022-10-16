using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftParticleController : MonoBehaviour {

    public int maxParticles = 10;
    public float spawnDelay = 0.05f;

    private GameObject prefab;
    private PlayerController playerController;
    private float lastTime = -1f;
    private bool colourBlack = false;

    private readonly List<GameObject> particles = new List<GameObject>();

    void Start() {
        this.prefab = Resources.Load<GameObject>("Particles/ParkingSpotEffect");
        this.playerController = this.GetComponent<PlayerController>();
    }

    void Update() {
        if (this.playerController.Difting && (this.lastTime == -1f || Time.time > this.lastTime + this.spawnDelay)) {
            this.CreateElement();
            this.lastTime = Time.time;
        }
    }

    private void CreateElement() {
        GameObject obj = Instantiate(this.prefab);
        Transform model = this.transform.Find("Model").transform;
        obj.transform.position = model.transform.position;
        obj.transform.rotation = model.transform.rotation;
        obj.transform.localScale = model.transform.localScale;

        if (this.colourBlack) {
            obj.GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 0, 255);
        }
        this.colourBlack = !this.colourBlack;
        this.particles.Add(obj);

        if (this.particles.Count > this.maxParticles) {
            int diff = this.particles.Count - this.maxParticles;
            for (int i = 0; i < diff; i++) {
                if (this.particles[0].gameObject != null) {
                    this.particles[0].GetComponent<DriftParticle>().FadeOut(() => { });
                }
                this.particles.RemoveAt(0);
            }
        }
    }
}
