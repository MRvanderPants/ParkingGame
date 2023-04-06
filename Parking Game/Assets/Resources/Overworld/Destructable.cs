using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour {

    public GameObject prefab;
    public GameObject pickupPrefab;

    public void SpawnPickup() {
        new TimedTrigger(1f, () => {
            var spawnee = Instantiate(this.pickupPrefab);
            spawnee.transform.position = this.transform.position;

            new TimedTrigger(3f, () => {
                if (spawnee == null) {
                    return;
                }
                Blinker blinker = spawnee.AddComponent<Blinker>();
                blinker.Init(
                    Time.time + 2f,
                    PlayerController.main.hyperModeBlinkAmount,
                    spawnee.GetComponent<SpriteRenderer>()
                );
            });

            new TimedTrigger(5f, () => {
                if (spawnee == null) {
                    return;
                }
                Destroy(spawnee);
            });
        });
    }

    public void Reset() {
        if (this.prefab != null) {
            GameObject obj = Instantiate(this.prefab);
            obj.transform.position = this.transform.position;
            Destroy(this.gameObject);
        }
    }
}
