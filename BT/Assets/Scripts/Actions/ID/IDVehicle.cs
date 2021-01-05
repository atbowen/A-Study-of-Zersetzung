using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDVehicle : ID
{
    public Transform vehicle, vehicleEntrance;
    public List<Transform> frontWheels, backWheels;

    public float vehicleOriginHeightOffset;

    public List<Action> entranceActions, exitActions;
    [SerializeField]
    public SetObjectProperties driverReorientationAfterExit;

    public bool transformDoOneEightyAfterExit;
    
    public CarControls carController;

    private IDCharacter driversID;

    public override void Activate() {

        if (vehicleEntrance != null && vehicleEntrance.GetComponent<CarEntrance>() != null && driversID != null) {

            // If the driver's collider or one of their additional colliders (necessary to interact with cloth, where applicable) is the collider currently colliding with the vehicle entrance collider,
            // let them enter
            if (driversID.transform.GetComponent<Collider>() == vehicleEntrance.GetComponent<CarEntrance>().GetCurrentCollider()
                || driversID.additionalColliders.Contains(vehicleEntrance.GetComponent<CarEntrance>().GetCurrentCollider())) {

                CarEntrance entrance = vehicleEntrance.GetComponent<CarEntrance>();
                entrance.entering = true;

                if (driversID != null) {
                    Transform oper = driversID.transform;

                    oper.transform.position = new Vector3(entrance.transform.position.x, vehicle.position.y - vehicleOriginHeightOffset, entrance.transform.position.z);
                    oper.transform.LookAt(new Vector3(entrance.PositionToFaceWhenEntering.position.x, oper.transform.position.y, entrance.PositionToFaceWhenEntering.position.z));
                    oper.transform.SetParent(vehicle);
                    oper.GetComponent<Rigidbody>().isKinematic = true;
                    oper.GetComponent<Collider>().enabled = false;
                    if (driversID.additionalColliders.Count > 0) {
                        foreach (Collider col in driversID.additionalColliders) { col.enabled = false; }
                    }
                }

                ParallelActions parActs = new ParallelActions();
                parActs.actions = new List<Action>();
                
                foreach (Action act in driversID.enterCarActions) { parActs.actions.Add(act); }

                foreach (Action act in entranceActions) { parActs.actions.Add(act); }

                actCoord.TriggerParallelActions(parActs);

                bCam.OperateVehicle(this);
            }
        }
    }

    public override void DisplayID() {
        scanner.EnableInfoPanelWithID(this);
    }

    public override void DisplayID(IDCharacter charID) {
        
    }

    public void SetDriversID(IDCharacter ident) {
        driversID = ident;
    }

    public ID GetDriversID() {
        return driversID;
    }
}
