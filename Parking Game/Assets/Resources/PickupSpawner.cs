using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour {

    private GameObject timePickupPrefab;
    private GameObject spawnee;
    private bool active = false;

    void Start() {
        this.timePickupPrefab = Resources.Load<GameObject>("Prefabs/TimeExtender");
    }

    public void Spawn(float duration = 7.5f) {
        if (this.active) {
            return;
        }
        this.active = true;
        this.spawnee = Instantiate(this.timePickupPrefab);
        this.spawnee.transform.position = this.transform.position;
        new TimedTrigger(duration, () => {
            this.Clear();
        });
    }

    public void Clear() {
        this.active = false;
        if (this.spawnee != null) {
            Destroy(this.spawnee);
        }
    }
}
