using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIModelRotation : MonoBehaviour {
    void Update() {
        this.transform.localEulerAngles = this.transform.localEulerAngles + new Vector3(0f, 0f, 0.2f);
    }
}
