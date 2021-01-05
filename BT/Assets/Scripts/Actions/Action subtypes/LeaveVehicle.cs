using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Leave Vehicle")]
public class LeaveVehicle : Action {

    public bool isTed;

    public Quaternion headOrientation;

    public override void DoAction() {
        Transform actor = GameObject.Find(actorName).transform;

        if (actor.GetComponent<IDVehicle>()) {
            IDVehicle ident = actor.GetComponent<IDVehicle>();
            CarControls carControl = ident.carController;

            if (ident.vehicle.GetComponent<Rigidbody>().velocity.magnitude < carControl.maxVelocityMagnitudeToExit) {
                ParallelActions parActs = new ParallelActions();
                parActs.actions = new List<Action>();
                if (ident.GetDriversID() != null) {
                    if (ident.GetDriversID().GetType() == typeof(IDCharacter)) {
                        IDCharacter IDChar = (IDCharacter)ident.GetDriversID();
                        foreach (Action act in IDChar.exitCarActions) { parActs.actions.Add(act); }
                    }
                }
                foreach (Action act in ident.exitActions) { parActs.actions.Add(act); }

                //foreach (Action act in parActs.actions) { act.DoAction(); }

                FindObjectOfType<ActionSceneCoordinator>().TriggerParallelActions(parActs);

                if (isTed) {
                    if (headOrientation != null) { FindObjectOfType<TeddyHead>().transform.localRotation = headOrientation; }
                    else { FindObjectOfType<TeddyHead>().transform.localRotation = Quaternion.identity; }
                }

                // For now this only works with Ted
                FindObjectOfType<BodyCam>().LeaveVehicle(ident);

                carControl.DisableInteriorInteractables();
            }
            else {
                carControl.HighSpeedExitWarning();
            }
        }        
    }
}
