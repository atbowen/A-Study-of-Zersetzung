using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IDCharacter : ID
{
    public Texture portrait;
    public Transform warrantMarker;
    public List<Transform> markers;

    public override void Activate() {
        if (myself.GetComponent<CyclopsAI>() != null && this.Arrestable) {
            CyclopsAI AI = myself.GetComponent<CyclopsAI>();
            AI.GotPinched();
            prisonController.CaptureSuspect(myself, Vector3.zero);
        }
    }
}
