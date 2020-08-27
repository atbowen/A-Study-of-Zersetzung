using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DocumentHandling : MonoBehaviour
{
    public bool buttonPressed, documentBeingHeld;
    public float maxDistanceToPickUp;

    private Rigidbody rigid;
    private Collider coll;
    private BodyCam bCam;
    private CameraMaster camMaster;
    private TeddyLeftEye leftEye;
    private TeddyRightEye rightEye;
    private HeadVision tedHeadVision;
    private Transform useRefPosition;

    // Start is called before the first frame update
    void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
        coll = this.GetComponent<Collider>();
        bCam = FindObjectOfType<BodyCam>();
        camMaster = FindObjectOfType<CameraMaster>();
        leftEye = FindObjectOfType<TeddyLeftEye>();
        rightEye = FindObjectOfType<TeddyRightEye>();
        tedHeadVision = FindObjectOfType<HeadVision>();

        useRefPosition = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (buttonPressed) {

            if (documentBeingHeld) {
                this.transform.Translate(0, 0, 30, Space.Self);
                this.transform.parent = null;

                rigid.useGravity = true;
                rigid.detectCollisions = true;

                rigid.AddForce(useRefPosition.forward * 100);

                buttonPressed = false;

                useRefPosition = null;
            } else {
                rigid.useGravity = false;
                rigid.detectCollisions = false;

                if (bCam.bodyControl && !camMaster.rightEyeLodged) {
                    useRefPosition = leftEye.transform.Find("DocumentHandler");
                } else if (bCam.bodyControl && camMaster.rightEyeLodged) {
                    useRefPosition = rightEye.transform.Find("DocHandlerBothEyesOffset");
                } else {
                    useRefPosition = rightEye.transform.Find("DocumentHandler");
                }
                
                this.transform.parent = useRefPosition.transform;
                this.transform.localRotation = Quaternion.identity;
                this.transform.Rotate(90, 180, 0, Space.Self);
                this.transform.position = useRefPosition.position;

                buttonPressed = false;
            }

            documentBeingHeld = !documentBeingHeld;
        }

        if (documentBeingHeld && useRefPosition != null) {
            //Vector3 smoothingVelocity = Vector3.zero;
            //this.transform.position = Vector3.SmoothDamp(this.transform.position, useRefPosition.transform.Find("DocumentHandler").transform.position, ref smoothingVelocity, 0.05f);
            this.transform.position = useRefPosition.position;
        }
    }
}
