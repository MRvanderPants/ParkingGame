using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public static PlayerController main;

    [Header("Forward movement stats")]
    public float moveSpeed = 1f;
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
    private Rigidbody rb;
    private BoxCollider boxCollider;
    private Car capturedCar;

    private readonly List<Car> colliders = new List<Car>();

    void Awake() {
        PlayerController.main = this;
    }

    void Start() {
        this.rb = this.GetComponent<Rigidbody>();
        this.boxCollider = this.GetComponent<BoxCollider>();
        this.model = this.transform.Find("Model");
    }

    void Update() {
        if (this.capturedCar != null) {
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool drifting = Input.GetButton("Fire1");
        this.HandleHorizontalMovement(horizontal, vertical, drifting);
        this.HandleVerticalMovement(horizontal, vertical, drifting);
        this.CheckForCapture();
    }

    public void AddCollider(Car car) {
        this.colliders.Add(car);
    }

    public void RemoveCollider(Car car) {
        this.colliders.Remove(car);
    }

    private void HandleVerticalMovement(float horizontal, float vertical, bool drifting) {
        float speed = this.moveSpeed;
        if (horizontal != 0 && !drifting) {
            speed *= this.turnSpeedReduction;
        }

        if (vertical > this.minimalInput) {
            this.rb.velocity = this.transform.up * speed;
        }
        else if (vertical < -this.minimalInput) {
            this.rb.velocity = -this.transform.up * speed * this.backwardSpeedReduction;
        } else {
            this.rb.velocity = Vector3.zero;
        }
    }

    private void HandleHorizontalMovement(float horizontal, float vertical, bool drifting) {
        if (vertical == 0 || this.rb.velocity == Vector3.zero) {
            return;
        }

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
            if (drifting && vertical != 0 && horizontal != 0) {
                if (this.targetDriftAngle == 0f) {
                    this.targetDriftAngle = this.driftingRotationIncrease;
                    this.currentDriftAngle = 1f;
                } else if (this.currentDriftAngle < this.driftingRotationIncrease) {
                    this.currentDriftAngle *= this.driftRotationLerp;
                }
                Vector3 euler = this.transform.rotation.eulerAngles;
                this.model.transform.localRotation = Quaternion.Euler(euler.x, euler.y, horizontal * -this.currentDriftAngle);
            } else if (!drifting) {

                // When drifting stops align the car to the proper rotation
                if (this.targetDriftAngle != 0f) {
                    this.targetDriftAngle = 0f;
                    this.transform.Rotate(0f, 0f, horizontal * -this.currentDriftAngle, Space.World);
                    this.model.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
            }
        } else if (drifting && this.targetDriftAngle != 0f) {
            this.targetDriftAngle = 0f;
            this.transform.Rotate(0f, 0f, horizontal * -this.currentDriftAngle, Space.World);
            this.model.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    private void CheckForCapture() {
        if (this.capturedCar != null || this.transform == null) {
            return;
        }

        for (int i = 0; i < this.colliders.Count; i++) {
            Car car = this.colliders[i];
            if (car == null) { return; }
            Vector3 myPos = this.transform.position; myPos.z = 0f;
            Vector3 carPos = car.transform.position; carPos.z = 0f;
            float distance = Vector3.Distance(myPos, carPos);
            if (distance < 0.5f) {
                this.CaptureCar(car);
            }
        }
    }

    private void CaptureCar(Car car) {
        this.rb.velocity = Vector3.zero;
        this.capturedCar = car;
        car.captured = true;
        car.transform.position = this.transform.position;
        Transform model = car.transform.Find("Model");
        //car.transform.rotation = this.transform.rotation;

        if (car.routeType == TrafficRouteType.Target) {
            Debug.Log("Captured target car");
            TimerUI.main.StartTimer(0.5f, () => {
                this.ReleaseCar();
                GameController.main.Next();
            });
        } else {
            TimerUI.main.StartTimer(5f, () => {
                this.ReleaseCar();
            });
        }
    }

    private void ReleaseCar() {
        this.capturedCar.Release();
        new TimedTrigger(0.5f, () => {
            this.capturedCar = null;
        });
    }
}
