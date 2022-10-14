using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

    [HideInInspector] public Bounds bounds;

    public float movementSpeed = 2f;
    public byte fadeOutSpeed = 5;
    public bool captured;
    public bool released = false;
    public TrafficRouteType routeType = TrafficRouteType.Default;

    private BoxCollider boxCollider;
    private Material material;
    private Vector3[] initialRoute;
    private Vector3[] route;
    private float startTime = -1f;
    private float journeyLength;
    private bool startPointRemoved = false;
    private Action onDestroy;

    private void Update() {
        if (!this.captured && this.startTime != -1f) {
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
        this.boxCollider = this.GetComponent<BoxCollider>();
        this.bounds = this.boxCollider.bounds;
        this.initialRoute = route;
        this.route = route;
        this.routeType = routeType;
        this.onDestroy = onDestroy;
        this.transform.position = this.route[0];
        this.material = this.transform.Find("Model").GetComponent<MeshRenderer>().material;
        this.StartNextNode();

        GoalData goalData = LevelController.main.CurrentGoalData;
        if (this.routeType == TrafficRouteType.Target && goalData != null) {
            this.material.color = goalData.targetColour;
        }
    }

    public void Release() {
        this.captured = false;
        new TimedTrigger(0.5f, () => {
            this.released = true;
            if (!this.gameObject) {
                this.onDestroy?.Invoke();
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

    private void StartNextNode() {
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
