using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDDocument : ID
{
    public bool needsToUnfold;
    public string foldingTriggerString, unfoldingTriggerString;
    public string tedPickUpAnimationString, tedTossAnimationString;
    public float tedPickUpAnimationTime, tedTossAnimationTime;

    public AudioClip crumplingSound, uncrumplingSound;

    private Transform useRefPosition;

    public override void DisplayID() {
        scanner.LookingAtNote(this);
    }

    public override void Activate() {
        //if (bCam.holdingDocument) {
        //this.transform.Translate(0, 0, 30, Space.Self);
        //this.transform.parent = null;

        //rigid.useGravity = true;
        //rigid.detectCollisions = true;

        //rigid.AddForce(useRefPosition.forward * 100);

        //useRefPosition = null;
        //}
        //else {
        //    rigid.useGravity = false;
        //    rigid.detectCollisions = false;

        //    if (bCam.bodyControl && !camMaster.rightEyeLodged) {
        //        useRefPosition = leftEye.transform.Find("DocumentHandler");
        //    }
        //    else if (bCam.bodyControl && camMaster.rightEyeLodged) {
        //        useRefPosition = rightEye.transform.Find("DocHandlerBothEyesOffset");
        //    }
        //    else {
        //        useRefPosition = rightEye.transform.Find("DocumentHandler");
        //    }

        //    this.transform.parent = useRefPosition.transform;
        //    this.transform.localRotation = Quaternion.identity;
        //    this.transform.Rotate(90, 180, 0, Space.Self);
        //    this.transform.position = useRefPosition.position;

        //    bCam.SetDocumentHeld(this);
        //}

        bCam.InitiateUseActionWithAnimationTrigger(tedPickUpAnimationString, tedPickUpAnimationTime, false);

        rigid.detectCollisions = false;
        rigid.useGravity = false;
        rigid.isKinematic = true;
        coll.enabled = false;

        if (bCam.bodyControl && !camMaster.rightEyeLodged) {
            useRefPosition = leftEye.transform.Find("DocumentHandler");
        }
        else if (bCam.bodyControl && camMaster.rightEyeLodged) {
            useRefPosition = rightEye.transform.Find("DocHandlerBothEyesOffset");
        }
        else {
            useRefPosition = rightEye.transform.Find("DocumentHandler");
        }

        this.transform.parent.parent = useRefPosition.transform;
        this.transform.parent.localRotation = Quaternion.identity;
        this.transform.parent.Rotate(90, 180, 0, Space.Self);

        this.transform.parent.position = useRefPosition.position + useRefPosition.TransformDirection(0, 0, 5);

        //bCam.holdingDocument = true;

        bCam.SetDocumentHeld(this);

        if (needsToUnfold) {
            anim.SetTrigger(unfoldingTriggerString);
            FindObjectOfType<MusicPlayer>().PlaySFX(uncrumplingSound);
        }

        scanner.HideReticleAndText(true);
    }

    public void DropDocument() {
        if (camMaster.rightEyeLodged) {
            //this.transform.parent.Translate(0, 0, 25, Space.Self);

            this.transform.parent.position = ted.transform.position + ted.transform.TransformDirection(0, -10, 40);
            this.transform.parent.parent = null;
        } else {
            this.transform.parent.Translate(0, 0, 20, Space.Self);
            this.transform.parent.parent = null;
        }

        bCam.InitiateUseActionWithAnimationTrigger(tedTossAnimationString, tedTossAnimationTime, false);

        anim.SetTrigger(foldingTriggerString);
        FindObjectOfType<MusicPlayer>().PlaySFX(crumplingSound);

        coll.enabled = true;
        rigid.isKinematic = false;
        rigid.useGravity = true;
        rigid.detectCollisions = true;

        rigid.AddForce(useRefPosition.forward * 100);        

        useRefPosition = null;

        bCam.SetDocumentHeld(null);

        scanner.HideReticleAndText(false);
    }
}
