using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDDocument : ID
{
    public bool documentBeingHeld;
    public float maxDistanceToPickUp;

    private Transform useRefPosition;

    public override void Activate() {
        if (bCam.holdingDocument) {
            this.transform.Translate(0, 0, 30, Space.Self);
            this.transform.parent = null;

            rigid.useGravity = true;
            rigid.detectCollisions = true;

            rigid.AddForce(useRefPosition.forward * 100);

            useRefPosition = null;
        }
        else {
            rigid.useGravity = false;
            rigid.detectCollisions = false;

            if (bCam.bodyControl && !camMaster.rightEyeLodged) {
                useRefPosition = leftEye.transform.Find("DocumentHandler");
            }
            else if (bCam.bodyControl && camMaster.rightEyeLodged) {
                useRefPosition = rightEye.transform.Find("DocHandlerBothEyesOffset");
            }
            else {
                useRefPosition = rightEye.transform.Find("DocumentHandler");
            }

            this.transform.parent = useRefPosition.transform;
            this.transform.localRotation = Quaternion.identity;
            this.transform.Rotate(90, 180, 0, Space.Self);
            this.transform.position = useRefPosition.position;

            bCam.SetDocumentHeld(this);
        }

        bCam.holdingDocument = !bCam.holdingDocument;
    }
}
