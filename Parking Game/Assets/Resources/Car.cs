using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

    [HideInInspector] public Bounds bounds;

    public float movementSpeed = 2f;
    public bool captured;

    private BoxCollider boxCollider;
    private Vector3[] route;
    private float startTime = -1f;
    private float journeyLength;
    private Action onDestroy;

    private void Update() {
        if (!this.captured && this.startTime != -1f) {
            float distCovered = (Time.time - this.startTime) * movementSpeed;
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
                } else {
                    this.onDestroy?.Invoke();
                    Destroy(this.gameObject);
                }
            }
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

    public void SetRoute(Vector3[] route, Action onDestroy) {
        this.boxCollider = this.GetComponent<BoxCollider>();
        this.bounds = this.boxCollider.bounds;
        this.route = route;
        this.transform.position = this.route[0];
        this.StartNextNode();
    }

    public void Release() {
        this.captured = false;
        this.StartNextNode();
    }

    private void StartNextNode() {
        this.journeyLength = Vector3.Distance(this.transform.position, this.route[0]);
        this.startTime = Time.time;
        this.transform.rotation = Quaternion.Euler(Vector3.zero);
        this.transform.LookAt(this.route[0], this.transform.forward);
    }
}
