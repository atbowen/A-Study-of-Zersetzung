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

    private ID driversID;

    public override void Activate() {

        if (vehicleEntrance != null && vehicleEntrance.GetComponent<CarEntrance>() != null) {
            if (vehicleEntrance.GetComponent<CarEntrance>().canEnter) {
                CarEntrance entrance = vehicleEntrance.GetComponent<CarEntrance>();
                entrance.entering = true;

                if (driversID != null) {
                    Transform oper = driversID.transform.parent;

                    oper.transform.position = new Vector3(entrance.transform.position.x, vehicle.position.y - vehicleOriginHeightOffset, entrance.transform.position.z);
                    oper.transform.LookAt(new Vector3(entrance.PositionToFaceWhenEntering.position.x, oper.transform.position.y, entrance.PositionToFaceWhenEntering.position.z));
                    oper.transform.SetParent(vehicle);
                    oper.GetComponent<Rigidbody>().isKinematic = true;
                    oper.GetComponent<Collider>().enabled = false;
                }

                ParallelActions parActs = new ParallelActions();
                parActs.actions = new List<Action>();
                if (driversID != null) {
                    if (driversID.GetType() == typeof(IDCharacter)) {
                        IDCharacter IDChar = (IDCharacter)driversID;
                        foreach (Action act in IDChar.enterCarActions) { parActs.actions.Add(act); }
                    }
                }
                foreach (Action act in entranceActions) { parActs.actions.Add(act); }

                actCoord.TriggerParallelActions(parActs);

                bCam.OperateVehicle(this);
            }
        }
    }

    public override void DisplayID() {
        scanner.EnableInfoPanelWithID(this);
    }

    public void SetDriversID(ID ident) {
        driversID = ident;
    }

    public ID GetDriversID() {
        return driversID;
    }
}
