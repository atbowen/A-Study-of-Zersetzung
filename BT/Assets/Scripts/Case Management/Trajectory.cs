using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Trajectory : MonoBehaviour
{
    public GameObject trajectoryLaser;

    public string labelStart, labelEnd;
    public Vector3 trajStart, trajEnd;

    public bool IsVisibleOnActivation() {
        return this.GetComponent<MeshRenderer>().enabled;
    }

    public void SetVisibleOnActivationOrNot(bool yesOrNo) {
        this.GetComponent<MeshRenderer>().enabled = yesOrNo;
    }

    public Vector3 GetVectorBetweenStartAndEndPoints() {
        return trajEnd - trajStart;
    }
}
