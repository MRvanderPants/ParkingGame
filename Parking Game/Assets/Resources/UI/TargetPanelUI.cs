using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetPanelUI : MonoBehaviour {

    public void SetTarget(GoalData goalData) {
        Transform model = this.transform.Find("Model");
        GameObject newModel = Instantiate(goalData.targetPrefab);
        Transform obj = newModel.transform.Find("Model");
        obj.SetParent(this.transform);
        obj.gameObject.layer = model.gameObject.layer;
        obj.position = model.position;
        obj.rotation = model.rotation;
        obj.localScale = model.localScale;
        obj.gameObject.AddComponent<UIModelRotation>();
        obj.GetComponent<MeshRenderer>().material.color = goalData.targetColour;
        Destroy(model.gameObject);
        Destroy(newModel);
    }
}
