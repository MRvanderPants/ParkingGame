using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrafficRouteType {
    Default,
    Target,
}

[ExecuteInEditMode]
public class TrafficRoute : MonoBehaviour {

    public float minimalSpawnTime = 2f;
    public Transform[] nodes;
    public TrafficRouteType routeType = TrafficRouteType.Default;

    private LineRenderer lineRenderer;
    private Transform start;
    private Transform end;
    private GameObject carPrefab;
    private List<Vector3> positions;

    void Start() {
        if (Application.isPlaying) {
            this.carPrefab = Resources.Load<GameObject>("Prefabs/Car");
            if (this.routeType != TrafficRouteType.Target) {
                new TimedTrigger(this.minimalSpawnTime, () => {
                    this.SpawnCar();
                });
            }
        }
    }

    void Update() {
        this.FetchObjects();
        this.DrawLine();
    }

    public void SpawnCar() {
        GameObject car = Instantiate(this.carPrefab);
        if (this.positions.Count <= 0) {
            return;
        }
        car.GetComponent<Car>().SetRoute(this.positions.ToArray(), () => {
            if (this.routeType == TrafficRouteType.Target) {
                return;
            }
            float r = UnityEngine.Random.Range(this.minimalSpawnTime, this.minimalSpawnTime * 2f);
            new TimedTrigger(r, () => {
                this.SpawnCar();
            });
        }, this.routeType);
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
