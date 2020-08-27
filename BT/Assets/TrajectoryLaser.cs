using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLaser : MonoBehaviour
{
    public Vector3 originalStartPoint, originalEndPoint;

    public bool IsVisibleOnActivation() {
        return this.GetComponent<MeshRenderer>().enabled;
    }

    public void SetVisibleOnActivationOrNot(bool yesOrNo) {
        this.GetComponent<MeshRenderer>().enabled = yesOrNo;
    }
}
