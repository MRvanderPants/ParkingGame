using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("Forward movement stats")]
    public float moveSpeed = 0.01f;
    public float turnSpeedReduction = 0.5f;
    public float backwardSpeedReduction = 0.5f;

    [Header("Rotation stats")]
    public float rotationSpeed = 0.2f;
    public float driftingRotationIncrease = 45f;
    public float driftRotationLerp = 0.25f;

    [Header("Misc")]
    public float minimalInput = 0.5f;

    private Transform model;
    private float targetDriftAngle = 0f;
    private float currentDriftAngle = 0f;

    void Start() {
        this.model = this.transform.Find("Model");
    }

    void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool drifting = Input.GetButton("Fire1");
        this.HandleHorizontalMovement(horizontal, vertical, drifting);
        this.HandleVerticalMovement(horizontal, vertical, drifting);
    }

    private void HandleVerticalMovement(float horizontal, float vertical, bool drifting) {
        float speed = this.moveSpeed;
        if (horizontal != 0 && !drifting) {
            speed *= this.turnSpeedReduction;
        }
        Vector3 pos = this.transform.position;
        Vector3 movement = this.transform.up *= speed;
        if (vertical > this.minimalInput) {
            pos += movement;
        }
        else if (vertical < -this.minimalInput) {
            pos -= movement * this.backwardSpeedReduction;
        }
        this.transform.position = pos;
    }

    private void HandleHorizontalMovement(float horizontal, float vertical, bool drifting) {
        // Basic rotation logic
        float rotSpeed = 0f;
        if (horizontal < -this.minimalInput) {
            rotSpeed = rotationSpeed;
        }
        else if (horizontal > this.minimalInput) {
            rotSpeed = -rotationSpeed;
        }

        // Angle to model to make it look like it is drifting + lerping
        if (rotSpeed != 0) {
            this.transform.Rotate(0f, 0f, rotSpeed, Space.World);
            if (drifting && vertical != 0) {
                if (this.targetDriftAngle == 0f) {
                    this.targetDriftAngle = this.driftingRotationIncrease;
                    this.currentDriftAngle = 1f;
                } else if (this.currentDriftAngle < this.driftingRotationIncrease) {
                    this.currentDriftAngle *= this.driftRotationLerp;
                }
                Vector3 euler = this.transform.rotation.eulerAngles;
                this.model.transform.localRotation = Quaternion.Euler(euler.x, euler.y, horizontal * -this.currentDriftAngle);
            } else {

                if (this.targetDriftAngle != 0f) {
                    this.targetDriftAngle = 0f;
                    this.transform.Rotate(0f, 0f, horizontal * -this.currentDriftAngle, Space.World);
                    this.model.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                //if (this.targetDriftAngle != 0f) {
                //    this.targetDriftAngle = 0f;
                //} else if (this.currentDriftAngle > 0f) {
                //    this.currentDriftAngle *= 2 - this.driftRotationLerp;
                //}
                //Vector3 euler = this.transform.rotation.eulerAngles;
                //this.model.transform.localRotation = Quaternion.Euler(euler.x, euler.y, horizontal * -this.currentDriftAngle);
            }
        }
    }
}
