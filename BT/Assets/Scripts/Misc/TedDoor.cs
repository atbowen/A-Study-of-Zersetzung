using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TedDoor : MonoBehaviour {

    public float fearFactor;
    public Transform tedDoorToAnimate, tedDoorToAnimateDup, tedDoorTrigInside, tedDoorTrigOutside, wholeDoorSetup, wholeDoorSetupDup, roomEntrance;
    public Transform[] objectsToHideMR, objectsToHideSMR, objectsHavingCollidersToDisable,
                        objectsToHideMRDupDoor, objectsToHideSMRDupDoor, objectsHavingCollidersToDisableDupDoor;                //MR = mesh renderer, SMR = skinned mesh renderer
    public bool doorOpen;

    private BodyCam bCam;
    private Teddy ted;
    private TeddyRightEye rightEye;
    private CameraMaster camMaster;
    private Fear fearMeter;
    private AutoDoor tedDoorInside, tedDoorOutside;
    private Animator anim, animDup;
    private Collider doorCollider;
    private Vector3 wholeDoorTransportFromLocation, tedTransportFromLocation, rightEyeTransportFromLocation;
    private bool tedInsideOrOut;

    private StatusPopup statusWindow;

	// Use this for initialization
	void Start () {
        camMaster = FindObjectOfType<CameraMaster>();
        bCam = FindObjectOfType<BodyCam>();
        ted = FindObjectOfType<Teddy>();
        rightEye = FindObjectOfType<TeddyRightEye>();
        fearMeter = FindObjectOfType<Fear>();
        tedDoorInside = tedDoorTrigInside.GetComponent<AutoDoor>();
        tedDoorOutside = tedDoorTrigOutside.GetComponent<AutoDoor>();
        anim = tedDoorToAnimate.GetComponent<Animator>();
        animDup = tedDoorToAnimateDup.GetComponent<Animator>();
        doorCollider = tedDoorToAnimate.GetComponent<Collider>();
        statusWindow = FindObjectOfType<StatusPopup>();

        doorOpen = false;
        tedInsideOrOut = false;

        wholeDoorTransportFromLocation = wholeDoorSetup.position;
        tedTransportFromLocation = ted.transform.position;
        rightEyeTransportFromLocation = rightEye.transform.position;

	}
	
	// Update is called once per frame
	void Update () {
        

        if (camMaster.hallucinating || camMaster.atTedsRoom) {                                                                                                      // if we're in hallucination mode, enable the renderer and collider of this hallucination object
            ShowMeshesAndColliders(objectsToHideMR, objectsToHideSMR, objectsHavingCollidersToDisable);

            if (fearMeter.adrenaline < fearMeter.adrMax) {
                fearMeter.adrenaline += 1 / ((bCam.transform.position - this.transform.position).magnitude + 1) * fearFactor * Time.deltaTime;  // increase Ted's fear at a certain rate dependent upon proximity
            }

            if (!doorOpen) {
                if ((camMaster.atTedsRoom && (tedDoorInside.doorTriggeredToOpen || tedDoorOutside.doorTriggeredToOpen)) || 
                    (!camMaster.atTedsRoom && tedDoorOutside.doorTriggeredToOpen)) {

                    anim.SetTrigger("open");
                    animDup.SetTrigger("open");
                    doorOpen = true;
                    doorCollider.isTrigger = true;


                    if (!camMaster.atTedsRoom) {
                        Vector3 tedDistanceToDoor = ted.transform.position - wholeDoorSetup.position;
                        Vector3 rightEyeDistanceToDoor = rightEye.transform.position - wholeDoorSetup.position;

                        HideMeshesAndColliders(objectsToHideMRDupDoor, objectsToHideSMRDupDoor, objectsHavingCollidersToDisableDupDoor);
                        animDup.SetTrigger("open");

                        wholeDoorSetup.position = roomEntrance.position;
                        tedTransportFromLocation = ted.transform.position;
                        ted.transform.position = (wholeDoorSetup.position + tedDistanceToDoor);
                        //if (camMaster.rightEyeLodged) {
                        //    tedTransportFromLocation = ted.transform.position;
                        //    ted.transform.position = (wholeDoorSetup.position + tedDistanceToDoor);
                        //} else {
                        //    rightEyeTransportFromLocation = rightEye.transform.position;
                        //    rightEye.transform.position = (rightEyeDistanceToDoor + wholeDoorSetup.position);
                        //}

                        camMaster.atTedsRoom = true;
                        statusWindow.FlashStatusText("Entering  apartment...");
                    }
                }
            } else {
                if (tedDoorOutside.doorTriggeredToOpen && !tedDoorInside.doorTriggeredToOpen) {
                    tedInsideOrOut = false;
                }

                if (!tedDoorOutside.doorTriggeredToOpen && tedDoorInside.doorTriggeredToOpen) {
                    tedInsideOrOut = true;
                }

                if ((!tedDoorOutside.doorTriggeredToOpen && !tedDoorInside.doorTriggeredToOpen)) {
                    anim.SetTrigger("close");
                    animDup.SetTrigger("close");
                    doorOpen = false;
                    doorCollider.isTrigger = false;

                    if (!tedInsideOrOut) {
                        if (!camMaster.hallucinating) {
                            HideMeshesAndColliders(objectsToHideMR, objectsToHideSMR, objectsHavingCollidersToDisable);
                        }

                        Vector3 tedDistanceToDoor = ted.transform.position - wholeDoorSetup.position;
                        Vector3 rightEyeDistanceToDoor = rightEye.transform.position - wholeDoorSetup.position;

                        wholeDoorSetup.position = wholeDoorTransportFromLocation;
                        ted.transform.position = (wholeDoorSetup.position + tedDistanceToDoor);
                        //if (camMaster.rightEyeLodged) {
                        //    ted.transform.position = (wholeDoorSetup.position + tedDistanceToDoor);
                        //} else {
                        //    rightEye.transform.position = (rightEyeDistanceToDoor + wholeDoorSetup.position);
                        //}

                        ShowMeshesAndColliders(objectsToHideMRDupDoor, objectsToHideSMRDupDoor, objectsHavingCollidersToDisableDupDoor);
                        

                        camMaster.atTedsRoom = false;
                    }
                }
            }

        } else {                                                                                                                            // else make this object invisible and untouchable

            for (int i = 0; i < objectsToHideMR.Length; i++) {
                objectsToHideMR[i].GetComponent<MeshRenderer>().enabled = false;
            }
            for (int i = 0; i < objectsToHideSMR.Length; i++) {
                objectsToHideSMR[i].GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
            for (int i = 0; i < objectsHavingCollidersToDisable.Length; i++) {
                objectsHavingCollidersToDisable[i].GetComponent<Collider>().enabled = false;
            }
        }
	}

    void ShowMeshesAndColliders(Transform[] MRObjects, Transform [] SMRObjects, Transform [] colObjects) {
        for (int i = 0; i < objectsToHideMR.Length; i++) {
            MRObjects[i].GetComponent<MeshRenderer>().enabled = true;
        }
        for (int i = 0; i < objectsToHideSMR.Length; i++) {
            SMRObjects[i].GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
        for (int i = 0; i < objectsHavingCollidersToDisable.Length; i++) {
            colObjects[i].GetComponent<Collider>().enabled = true;
        }
    }

    void HideMeshesAndColliders(Transform[] MRObjects, Transform[] SMRObjects, Transform[] colObjects) {
        for (int i = 0; i < objectsToHideMR.Length; i++) {
            MRObjects[i].GetComponent<MeshRenderer>().enabled = false;
        }
        for (int i = 0; i < objectsToHideSMR.Length; i++) {
            SMRObjects[i].GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        for (int i = 0; i < objectsHavingCollidersToDisable.Length; i++) {
            colObjects[i].GetComponent<Collider>().enabled = false;
        }
    }
}
