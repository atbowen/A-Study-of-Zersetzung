using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDVehicle : ID
{
    public override void Activate() {
        if (vehicleEntrance != null && vehicleEntrance.GetComponent<CarEntrance>() != null) {
            if (vehicleEntrance.GetComponent<CarEntrance>().canEnter) {
                CarEntrance entrance = vehicleEntrance.GetComponent<CarEntrance>();
                entrance.entering = true;
                ted.transform.position = new Vector3(entrance.transform.position.x, 0, entrance.transform.position.z);
                ted.transform.LookAt(new Vector3(entrance.PositionToFaceWhenEntering.position.x, ted.transform.position.y, entrance.PositionToFaceWhenEntering.position.z));
                ted.transform.parent = myself;

                actCoord.TriggerParallelActions(bCam.enterCarActions);
                bCam.ChangeMovementMode(BodyCam.MovementModes.Driving);
            }
        }
    }
}
