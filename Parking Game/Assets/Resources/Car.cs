using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

    [HideInInspector] public Bounds bounds;

    public float movementSpeed = 2f;
    public float explosionVolume = 0.125f;
    public byte fadeOutSpeed = 5;
    public bool captured;
    public bool released = false;
    public bool isExtraCar = false;
    public TrafficRouteType routeType = TrafficRouteType.Default;

    private BoxCollider boxCollider;
    private Material material;
    private Vector3[] initialRoute;
    private Vector3[] route;
    private float startTime = -1f;
    private float journeyLength;
    private bool startPointRemoved = false;
    private bool stopMovement = false;
    private Action onDestroy;
    private GameObject star;
    private Rigidbody rb;
    private ParticleSystem particles;
    private AudioClip[] explosions;

    private void Start() {
        this.particles = this.GetComponent<ParticleSystem>();
        this.rb = this.GetComponent<Rigidbody>();
        this.rb.isKinematic = true;
        this.explosions = new AudioClip[4] {
            Resources.Load<AudioClip>("Audio/SFX/explosion1"),
            Resources.Load<AudioClip>("Audio/SFX/explosion2"),
            Resources.Load<AudioClip>("Audio/SFX/explosion3"),
            Resources.Load<AudioClip>("Audio/SFX/explosion4")
        };
    }

    private void Update() {
        if (!this.captured && this.startTime != -1f && !this.stopMovement) {
            this.HandleMovement();
        }
    }

    public void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            other.GetComponent<PlayerController>().AddCollider(this);
        }
    }

    public void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            other.GetComponent<PlayerController>().RemoveCollider(this);
        }
    }

    public void SetRoute(Vector3[] route, Action onDestroy, TrafficRouteType routeType = TrafficRouteType.Default) {
        this.InitRoute(route, onDestroy, routeType);
        this.StartNextNode();

        GoalData goalData = MissionController.main.CurrentGoalData;
        if (this.routeType == TrafficRouteType.Target && goalData != null) {
            this.material.color = goalData.targetColour;
            this.star.SetActive(true);
        }
    }

    public void Launch() {
        this.captured = false;
        this.released = true;
        this.stopMovement = true;
        this.rb.isKinematic = false;

        this.particles.Play();
        Vector3 vector = PlayerController.main.transform.up;
        vector.x += UnityEngine.Random.Range(-0.3f, 0.3f);
        vector.y += UnityEngine.Random.Range(-0.3f, 0.3f);
        Vector3 force2 = vector * 20f;
        force2.z = -5f;
        this.rb.AddForce(force2, ForceMode.Impulse);
        new TimedTrigger(2f, () => {
            this.onDestroy?.Invoke();
            Destroy(this.gameObject);
        });
    }

    public void Release() {
        this.captured = false;
        new TimedTrigger(0.5f, () => {
            this.released = true;
            if (this == null || this.gameObject == null) {
                return;
            }
            ModelFadeOut fader = this.gameObject.AddComponent<ModelFadeOut>();
            fader.Init(this.material, () => {
                this.onDestroy?.Invoke();
            });
        });
        this.StartNextNode();
    }

    public void Kill() {
        this.onDestroy?.Invoke();
        Destroy(this.gameObject);
    }

    public void PlayExplosionSFX() {
        int r = UnityEngine.Random.Range(0, this.explosions.Length - 1);
        AudioController.main.PlayClip(this.explosions[r], Mixers.SFX, this.explosionVolume);
    }

    private void InitRoute(Vector3[] route, Action onDestroy, TrafficRouteType routeType = TrafficRouteType.Default) {
        this.boxCollider = this.GetComponent<BoxCollider>();
        this.bounds = this.boxCollider.bounds;
        this.initialRoute = route;
        this.route = route;
        this.routeType = routeType;
        this.onDestroy = onDestroy;
        this.transform.position = this.route[0];
        this.material = this.transform.Find("Model").GetComponent<MeshRenderer>().material;
        this.star = this.transform.Find("Star").gameObject;
        this.star.SetActive(false);
    }

    private void StartNextNode() {
        if (this == null || this.gameObject == null || this.transform == null) { return; }
        this.journeyLength = Vector3.Distance(this.transform.position, this.route[0]);
        this.startTime = Time.time;
        this.transform.rotation = Quaternion.Euler(Vector3.zero);
        this.transform.LookAt(this.route[0], this.transform.forward);
    }

    private void HandleMovement() {
        float distCovered = (Time.time - this.startTime) * (movementSpeed * LevelController.main.SpeedMultiplier);
        float fractionOfJourney = distCovered / journeyLength;
        if (float.IsNaN(fractionOfJourney)) {
            fractionOfJourney = 0f;
        }
        this.transform.position = Vector3.Lerp(this.transform.position, this.route[0], fractionOfJourney);
        float distance = Vector3.Distance(this.transform.position, this.route[0]);
        if (distance < 0.25f) {
            this.transform.position = this.route[0];
            List<Vector3> newList = new List<Vector3>();
            for (int i = 1; i < this.route.Length; i++) {
                newList.Add(this.route[i]);
            }
            this.route = newList.ToArray();
            if (this.route.Length > 0) {
                this.StartNextNode();
            }
            else {
                if (this.routeType == TrafficRouteType.Target) {
                    List<Vector3> list = new List<Vector3>();
                    list.AddRange(this.initialRoute);
                    if (!this.startPointRemoved) {
                        list.RemoveAt(0);
                        this.startPointRemoved = true;
                    }
                    this.SetRoute(list.ToArray(), this.onDestroy, this.routeType);
                }
                else {
                    this.Kill();
                }
            }
        }
    }
}
