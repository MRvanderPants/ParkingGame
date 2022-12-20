using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrafficRouteType {
    Default,
    Target,
}

[ExecuteInEditMode]
public class TrafficRoute : MonoBehaviour {

    public TrafficRouteType routeType = TrafficRouteType.Default;

    [Header("Spawn Settings")]
    public float minimalSpawnTime = 2f;
    public float randomSpawnMultiplier = 2f;
    public float hyperMultiplier = 2f;
    [Space]
    public Transform[] nodes;

    private LineRenderer lineRenderer;
    private Transform start;
    private Transform end;
    private GameObject carPrefab;
    private List<Vector3> positions;
    private LevelControlledTimedTrigger hyperModeTrigger;

    private readonly List<Car> cars = new List<Car>();

    public Car[] Cars {
        get => this.cars.ToArray();
    }

    void Start() {
        if (Application.isPlaying) {
            this.carPrefab = Resources.Load<GameObject>("Prefabs/Car");
            if (this.routeType != TrafficRouteType.Target) {
                new LevelControlledTimedTrigger(this.minimalSpawnTime, () => {
                    this.SpawnCar();
                });
            }

            LevelController.main.onMissionChange.Subscribe((object data) => {
                GoalData goalData = (GoalData)data;
                if (goalData.goalType == GoalType.HyperMode && this.routeType != TrafficRouteType.Target) {
                    this.hyperModeTrigger = new LevelControlledTimedTrigger(this.minimalSpawnTime * this.hyperMultiplier, () => {
                        this.SpawnCar(true);
                    }, true);
                } else if (this.hyperModeTrigger != null) {
                    this.hyperModeTrigger.Destroy();
                }
            });
        }
    }

    void Update() {
        this.FetchObjects();
        this.DrawLine();
    }

    public void SpawnCar(bool isExtraCar = false) {
        GameObject car = Instantiate(this.carPrefab);
        Car carCar = car.GetComponent<Car>();
        carCar.isExtraCar = isExtraCar;
        carCar.SetRoute(this.positions.ToArray(), () => {
            this.cars.Remove(carCar);
            if (this.routeType == TrafficRouteType.Target || carCar.isExtraCar) {
                return;
            }
            float r = UnityEngine.Random.Range(
                this.minimalSpawnTime,
                this.minimalSpawnTime * this.randomSpawnMultiplier
            );
            new LevelControlledTimedTrigger(r, () => {
                this.SpawnCar();
            });
        }, this.routeType);
        this.cars.Add(carCar);
    }

    public void RemoveAllCars() {
        for (int i = 0; i < this.cars.Count; i++) {
            this.cars[i].Release();
        }
        this.cars.Clear();
    }

    private void FetchObjects() {
        if (this.lineRenderer == null) {
            this.lineRenderer = this.GetComponent<LineRenderer>();
        }

        if (this.start == null) {
            this.start = this.transform.Find("Start");
        }

        if (this.end == null) {
            this.end = this.transform.Find("End");
        }
    }

    private void DrawLine() {
        if (Application.isPlaying) {
            this.lineRenderer.enabled = false;
            this.start.GetComponent<MeshRenderer>().enabled = false;
            this.end.GetComponent<MeshRenderer>().enabled = false;
            for (int i = 0; i < this.nodes.Length; i++) {
                this.nodes[i].GetComponent<MeshRenderer>().enabled = false;
            }
        } else {
            if (this.routeType == TrafficRouteType.Target) {
                this.lineRenderer.sharedMaterial.color = new Color32(255, 0, 0, 255);
            }
            this.lineRenderer.enabled = true;
            this.start.GetComponent<MeshRenderer>().enabled = true;
            this.end.GetComponent<MeshRenderer>().enabled = true;
        }

        this.lineRenderer.positionCount = this.nodes.Length + 2;
        this.positions = new List<Vector3>();

        this.positions.Add(this.start.position);
        for (int i = 0; i < this.nodes.Length; i++) {
            this.positions.Add(this.nodes[i].position);
            if (!Application.isPlaying) {
                this.nodes[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }
        if (this.routeType != TrafficRouteType.Target) {
            this.positions.Add(this.end.position);
        } else {
            this.positions.Add(this.nodes[0].position);
        }
        this.lineRenderer.SetPositions(this.positions.ToArray());
    }
}
