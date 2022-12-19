using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingSpawner : MonoBehaviour {

    private GameObject carPrefab;

    void Start() {
        this.carPrefab = Resources.Load<GameObject>("Prefabs/Car");
    }

    void Update() {
        
    }
}
