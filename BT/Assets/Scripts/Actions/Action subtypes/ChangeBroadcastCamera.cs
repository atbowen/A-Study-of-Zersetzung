using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Change Broadcast Camera")]
public class ChangeBroadcastCamera : Action
{
    public string newCamera;

    private BroadcastManager broadcastController;

    public override void DoAction() {

        broadcastController = FindObjectOfType<BroadcastManager>();

        if (broadcastController.currentCamera != null) {
            broadcastController.currentCamera.enabled = false;
        }

        if (GameObject.Find(newCamera).GetComponent<Camera>() != null) {
            GameObject.Find(newCamera).GetComponent<Camera>().enabled = true;
        }
    }
}
