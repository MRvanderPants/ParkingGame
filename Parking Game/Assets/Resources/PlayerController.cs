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
    public float minimalDriftingAngle = 0.33f;
    public float driftRotationLerp = 0.25f;

    [Header("Catching")]
    public float minimalCatchDistance = 0.5f;
    public float targetCatchTime = 0.5f;
    public float invalidCatchTime = 2f;
    public float catchSpeedMultiplier = 5f;
    public float missionStartCaptureDelay = 2f;

    [Header("SFX")]
    public AudioClip driftingStartSFX;
    public AudioClip[] collisionSoundSFX;
    public AudioClip[] collisionAdditionalSoundSFX;

    [Header("Hyper Mode")]
    public float hyperModeScale = 2.5f;
    public float hyperModeScaleSpeed = 1.025f;
    public float hyperModeImpactDuration = 0.33f;
    public float hyperModeBlinkDuration = 0.1f;
    public int hyperModeBlinkAmount = 20;

    [Header("Car Movement Mode")]
    public float carMovementTimeMultiplier = 2f;
    [Min(1)]public int carMovementChance = 2;

    [Header("Misc")]
    public float compassDistance = 1.5f;
    public float minimalInput = 0.5f;
    public float initialWaitTime = 3f;

    private Transform model;
    private float targetDriftAngle = 0f;
    private float currentDriftAngle = 0f;
    private Rigidbody rb;
    private Car capturedCar;
    private bool startupHolding = true;
    private bool isDrifting = false;
    private bool isCaptureDisabled = false;
    private bool sfxThrottle = false;
    private bool playingLongSfx = false;
    private ParticleSystem captureParticles;
    private GameObject wallHitParticlePrefab;
    private AudioSource driftingAudioSource;

    private readonly List<Car> colliders = new List<Car>();
    private readonly List<GameObject> compasses = new List<GameObject>();

    public bool Difting {
        get => this.isDrifting && this.currentDriftAngle > Mathf.Abs(this.driftingRotationIncrease * this.minimalDriftingAngle);
    }

    public bool CanMove {
        get => this.capturedCar == null || (
            this.capturedCar != null
            && !MissionController.main.GetCurrentMissionSettings().lockPlayerDuringCapture
        );
    }

    void Awake() {
        PlayerController.main = this;
    }

    void Start() {
        this.rb = this.GetComponent<Rigidbody>();
        this.model = this.transform.Find("Model");
        this.captureParticles = this.GetComponent<ParticleSystem>();
        this.wallHitParticlePrefab = Resources.Load<GameObject>("Particles/WallHitParticle");

        TimerUI.main.StartTimer(this.initialWaitTime, () => {
            this.startupHolding = false;
        });
    }

    void FixedUpdate() {
        this.HandleCompasses();
        if (!this.CanMove || this.startupHolding) {
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        this.isDrifting = Input.GetButton("Fire1") || Input.GetButton("Fire2") || Input.GetButton("Fire3");
        this.HandleDrifting();
        this.HandleHorizontalMovement(horizontal, vertical, this.isDrifting);
        this.HandleVerticalMovement(horizontal, vertical, this.isDrifting);
        this.CheckForCapture();
    }

    public void AddCollider(Car car) {
        this.colliders.Add(car);
    }

    public void RemoveCollider(Car car) {
        this.colliders.Remove(car);
    }

    public void SetHyperMode(bool active) {
        Vector3 newScale = Vector3.one * (active ? this.hyperModeScale : 1f);
        DynamicScale ds = this.gameObject.AddComponent<DynamicScale>();
        ds.Init(newScale, this.hyperModeScaleSpeed);

        if (active) {
            this.StartBlinker();
        }
    }

    public void DisableCaptureTemporarily(float delay = -1f) {
        this.isCaptureDisabled = true;
        float time = delay == -1f ? this.missionStartCaptureDelay : delay;
        new TimedTrigger(time, () => {
            this.isCaptureDisabled = false;
        });
    }

    private void HandleVerticalMovement(float horizontal, float vertical, bool drifting) {
        float speed = this.moveSpeed;
        if (horizontal != 0 && !drifting) {
            speed *= this.turnSpeedReduction;
        }

        if (vertical >= 0) {
            this.rb.velocity = this.transform.up * speed;
        } else if (vertical < -this.minimalInput) {
            this.rb.velocity = -this.transform.up * speed * this.backwardSpeedReduction;
        } else {
            this.rb.velocity = Vector3.zero;
        }
    }

    private void HandleHorizontalMovement(float horizontal, float vertical, bool drifting) {
        if (this.rb.velocity == Vector3.zero) {
            return;
        }

        // Basic rotation logic
        float rotSpeed = 0f;
        float driftingMultiplier = drifting ? 2f : 1f;
        if (horizontal < -this.minimalInput) {
            rotSpeed = rotationSpeed * driftingMultiplier;
        }
        else if (horizontal > this.minimalInput) {
            rotSpeed = -rotationSpeed * driftingMultiplier;
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
        bool carMoving = MissionController.main.CurrentGoalData.goalType == GoalType.CarMoving;
        if ((!carMoving && this.capturedCar != null) || this.transform == null || this.isCaptureDisabled) {
            return;
        }

        for (int i = 0; i < this.colliders.Count; i++) {
            Car car = this.colliders[i];
            if (car != null && !car.GetComponent<Car>().released && (!carMoving || this.capturedCar != car)) {
                Vector3 myPos = this.transform.position; myPos.z = 0f;
                Vector3 carPos = car.transform.position; carPos.z = 0f;
                float distance = Vector3.Distance(myPos, carPos);

                GoalType goalType = MissionController.main.CurrentGoalData.goalType;
                float catchDistance = goalType == GoalType.HyperMode
                    ? this.minimalCatchDistance * this.hyperModeScale
                    : this.minimalCatchDistance;

                if (distance < catchDistance) {
                    this.CaptureCar(car);
                }
            }
        }
    }

    private void CaptureCar(Car car) {
        BaseMissionSettings missionSettings = MissionController.main.GetCurrentMissionSettings();
        if (missionSettings.lockPlayerDuringCapture) {
            this.rb.velocity = Vector3.zero;
        }

        if (missionSettings.goalType != GoalType.CarMoving) {
            this.capturedCar = car;
            car.transform.position = this.transform.position;
        }
        car.captured = true;

        this.captureParticles.Play();
        CameraController.main.Shake(0.25f, 0.125f, 1f);
        this.ResolveCarCaptureByGoal(car);
    }

    private void ResolveCarCaptureByGoal(Car car) {
        GoalData goalData = MissionController.main.CurrentGoalData;
        switch (goalData.goalType) {

            // Mode to avoid cars for the duration of the mission
            case GoalType.Stealth:
                AudioController.main.PlayClip("wrongCapture");
                new TimedTrigger(1f, () => {
                    this.ReleaseCar();
                    LevelController.main.EndGame();
                });
                break;

            case GoalType.HyperMode:
                this.ResolveHyperCapture();
                break;

            case GoalType.CarMoving:
                this.ResolveCarMovingCapture(car);
                break;

            // Default mode to capture a specific car
            case GoalType.CaptureTarget:
            default:
                if (car.routeType == TrafficRouteType.Target) {
                    this.ResolveValidCapture();
                } else {
                    this.ResolveInvalidCapture();
                }
                break;
        }
    }

    private void ResolveHyperCapture() {
        AudioController.main.PlayClip("capture");
        LevelController.main.Score += LevelController.main.LevelIndex;
        this.capturedCar.PlayExplosionSFX();
        new TimedTrigger(this.hyperModeImpactDuration, () => {
            if (this.capturedCar != null) {
                this.capturedCar.Launch();
            }
            this.capturedCar = null;
        });
    }

    private void ResolveCarMovingCapture(Car car) {
        if (!car.released) {
            car.released = true;
            LevelController.main.Score -= LevelController.main.LevelIndex;
            car.PlayExplosionSFX();
            car.Launch();
            CameraController.main.Shake(0.25f, 0.125f, 1f);
            if (this.capturedCar) {
                this.capturedCar.GetComponent<ParticleSystem>().Stop();
            }
        }
    }

    private void ResolveValidCapture() {
        AudioController.main.PlayClip("capture");
        GoalData goalData = MissionController.main.CurrentGoalData;
        BaseMissionSettings baseMissionSettings = MissionController.main.GetMissionSettingsForType(GoalType.CarMoving);
        int levelIndex = LevelController.main.LevelIndex;
        if (levelIndex >= baseMissionSettings.minLevel) {
            int r = UnityEngine.Random.Range(0, 10);
            if (r < this.carMovementChance) {
                LevelController.main.EndMission();
                this.colliders.Remove(this.capturedCar);
                this.capturedCar.transform.parent = this.transform;
                this.capturedCar.transform.rotation = Quaternion.Euler(new Vector3(-90f, 0f, 180f));
                this.capturedCar.GetComponent<BoxCollider>().enabled = false;

                MissionController.main.InsertMission(new GoalData() {
                    goalType = GoalType.CarMoving,
                    timeLimit = goalData.timeLimit * this.carMovementTimeMultiplier,
                    targetPrefab = Resources.Load<GameObject>("Prefabs/TargetArea"),
                });
                return;
            }
        }

        TimerUI.main.StartTimer(this.targetCatchTime, () => {
            this.ReleaseCar();
            LevelController.main.EndMission();
        });
    }

    private void ResolveInvalidCapture() {
        LevelController.main.SpeedMultiplier = this.catchSpeedMultiplier;
        AudioController.main.PlayClip("wrongCapture");
        AudioSource fastForwardSource = AudioController.main.PlayClip("fastForward", Mixers.SFX, true);
        TimerUI.main.StartTimer(this.invalidCatchTime, () => {
            LevelController.main.SpeedMultiplier = 1f;
            AudioController.main.FadeOutClip(fastForwardSource);
            this.ReleaseCar();
        });
    }

    private void ReleaseCar() {
        this.capturedCar.Release();
        new TimedTrigger(this.targetCatchTime, () => {
            this.capturedCar = null;
        });
        this.DisableCaptureTemporarily(1f);
    }

    private void OnCollisionEnter(Collision collision) {
        switch (collision.collider.gameObject.tag) {
            case "environment":
                CameraController.main.Shake(0.25f, 0.125f, 2f);
                Vector3 contactPoint = collision.contacts[0].point;
                GameObject particle = Instantiate(this.wallHitParticlePrefab);
                particle.transform.position = contactPoint;
                this.HandleCollisionSFX();
                new TimedTrigger(1.5f, () => {
                    Destroy(particle);
                });
                break;

            case "pickup":
                Destroy(collision.collider.gameObject);
                AudioController.main.PlayClip("capture");
                UIController.main.GoalPanelUI.AddTime(0.3f);
                CameraController.main.Shake(0.25f, 0.125f, 1f);
                break;

            case "destructable":
                Transform target = collision.collider.transform.parent.Find("target");
                collision.collider.gameObject.GetComponent<BoxCollider>().enabled = false;
                this.LaunchDestructable(target);
                break;

            case "targetArea":
                AudioController.main.PlayClip("capture");
                LevelController.main.ClearTargetArea();
                this.capturedCar.Release();
                this.capturedCar = null;
                LevelController.main.EndMission();
                break;

            default: break;
        }
    }

    private void LaunchDestructable(Transform target) {
        int r = UnityEngine.Random.Range(0, 10);
        if (r < 3) {
            target.transform.parent.GetComponent<Destructable>().SpawnPickup();
        }
        target.GetComponent<ParticleSystem>().Play();
        Vector3 vector = PlayerController.main.transform.up;
        vector.x += UnityEngine.Random.Range(-0.3f, 0.3f);
        vector.y += UnityEngine.Random.Range(-0.3f, 0.3f);
        Vector3 force2 = vector * 20f;
        force2.z = -5f;
        Rigidbody rb = target.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(force2, ForceMode.Impulse);
        AudioController.main.PlayClip(this.collisionSoundSFX[0], Mixers.SFX, 0.2f);
        new TimedTrigger(2f, () => {
            if (target != null && target.gameObject != null) {
                Destroy(target.gameObject);
            }
        });
    }

    private void HandleDrifting() {
        this.HandleDriftingSFX();
    }

    private void HandleDriftingSFX() {
        if (this.isDrifting && this.driftingAudioSource == null) {
            this.driftingAudioSource = AudioController.main.PlayClip(this.driftingStartSFX, Mixers.SFX);
        } else if (!this.isDrifting && this.driftingAudioSource != null) {
            AudioController.main.FadeOutClip(this.driftingAudioSource);
            this.driftingAudioSource = null;
        }
    }

    private void HandleCollisionSFX() {
        if (this.sfxThrottle) {
            return;
        }
        this.sfxThrottle = true;
        new TimedTrigger(0.05f, () => {
            this.sfxThrottle = false;
        });

        int r0 = UnityEngine.Random.Range(0, this.collisionSoundSFX.Length);
        AudioController.main.PlayClip(this.collisionSoundSFX[r0], Mixers.SFX, 0.1f);

        if (!this.playingLongSfx) {
            int r1 = UnityEngine.Random.Range(0, this.collisionAdditionalSoundSFX.Length);
            int r2 = UnityEngine.Random.Range(0, 10);
            if (r2 > 7) {
                this.playingLongSfx = true;
                AudioController.main.PlayClip(this.collisionAdditionalSoundSFX[r1], Mixers.SFX, 0.2f);
                new TimedTrigger(2f, () => {
                    this.playingLongSfx = false;
                });
            }
        }
    }

    private void HandleCompasses() {
        Car[] targets = LevelController.main.TargetCars;
        List<Car> cars = new List<Car>();
        for (int i = 0; i < targets.Length; i++) {
            if (!targets[i].released) {
                cars.Add(targets[i]);
            }
        }

        GoalType currentGoalType = MissionController.main.CurrentGoalData.goalType;
        if (currentGoalType == GoalType.CarMoving && LevelController.main.targetArea) {
            if (this.compasses.Count > 0) {
                this.MoveCompasses();
                return;
            } else {
                this.CreateCompasses();
                return;
            }
        }

        if (cars.Count > 0 && cars.Count != this.compasses.Count) {
            this.RemoveAllCompasses();
            this.CreateCompasses(cars.ToArray());
        } else if (cars.Count > 0 && cars.Count == this.compasses.Count && currentGoalType != GoalType.CarMoving) {
            this.MoveCompasses(cars.ToArray());
        } else {
            this.RemoveAllCompasses();
        }
    }

    private void CreateCompasses(Car[] targets) {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Compass");
        for (int i = 0; i < targets.Length; i++) {
            GameObject compass = Instantiate(prefab, this.transform);
            this.compasses.Add(compass);
        }
    }

    private void CreateCompasses() {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Compass");
        GameObject compass = Instantiate(prefab, this.transform);
        this.compasses.Add(compass);
    }

    private void MoveCompasses(Car[] targets) {
        for (int i = 0; i < targets.Length; i++) {
            GameObject compass = this.compasses[i];
            Transform target = targets[i].transform;
            compass.transform.position = this.transform.position;
            float angle = this.GetAngle(compass.transform.position, target.transform.position) - 90f;
            Vector3 eul = compass.transform.eulerAngles;
            eul.z = angle;
            compass.transform.eulerAngles = eul;
            compass.transform.position += compass.transform.up * this.compassDistance;
        }
    }

    private void MoveCompasses() {
        GameObject compass = this.compasses[0];
        Transform target = LevelController.main.targetArea.transform;
        compass.transform.position = this.transform.position;
        float angle = this.GetAngle(compass.transform.position, target.transform.position) - 90f;
        Vector3 eul = compass.transform.eulerAngles;
        eul.z = angle;
        compass.transform.eulerAngles = eul;
        compass.transform.position += compass.transform.up * this.compassDistance;
    }

    private float GetAngle(Vector3 from, Vector3 to) {
        var dy = to.y - from.y;
        var dx = to.x - from.x;
        var theta = Mathf.Atan2(dy, dx);
        theta *= 180 / Mathf.PI;
        return theta;
    }

    private void RemoveAllCompasses() {
        for (int i = 0; i < this.compasses.Count; i++) {
            Destroy(this.compasses[i]);
        }
        this.compasses.Clear();
    }

    private void StartBlinker() {
        GoalData goalData = MissionController.main.CurrentGoalData;
        float stopTime = Time.time + goalData.timeLimit;
        new TimedTrigger(goalData.timeLimit * (1f - this.hyperModeBlinkDuration), () => {
            Blinker blinker = this.model.gameObject.AddComponent<Blinker>();
            blinker.Init(
                stopTime,
                this.hyperModeBlinkAmount,
                this.model.GetComponent<SpriteRenderer>()
            );
        });
    }
}
