using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour {

    public RenderTexture staticView, viewAdjust, leftEyeRT, rightEyeRT;
    public RawImage mainOverlay, topRightInsert;                                                        // mainOverlay image is for full screen projections; insert is for picture-in-a-picture
    public Camera bodyCamera;
    public enum ViewMode { Std, XRay, OffAir, Adjusting };                                              // Std = normal screen, XRay = wireframe, OffAir = static, Adjusting = loading screen
    public bool jammed;                                                                                 // When will transmissions from eye cameras to interface be jammed??
    public float jamDuration;                                                                           // Inverse multiplier for jamming time--shorter the distance from jam source, longer the jam
    public bool leftEyeLodged, rightEyeLodged, leftEyeTransitioning, rightEyeTransitioning;             // Eye "lodging" switches determine view and control scenarios

    private BodyCam view;                                                                               // Between eyes camera is used when both eyes are lodged
    private Teddy ted;
    private TeddyLeftEye leftEye;
    private TeddyRightEye rightEye;
    private Animator anim;
    private Rigidbody rigid, leftEyeRigid, rightEyeRigid;
    private GameObject headd;
    private Collider leftEyeColl, rightEyeColl;
    
    private bool jamView, clearView;
    private float startStatic, staticTime, clearStatic, clearTime;                                      // Variables for creating random static when transmissions from an eye is jammed
    private Vector3 initialPosBody, initialPosLeft, initialPosRight;                                    // For resetting position
    private Quaternion returnRotLeft, returnRotRight, initialRotBody;                                   // For resetting rotation

    void Start() {
        view = FindObjectOfType<BodyCam>();
        ted = FindObjectOfType<Teddy>();
        anim = ted.GetComponent<Animator>();
        rigid = ted.GetComponent<Rigidbody>();

        leftEye = FindObjectOfType<TeddyLeftEye>();
        rightEye = FindObjectOfType<TeddyRightEye>();
        leftEyeColl = leftEye.GetComponent<Collider>();
        rightEyeColl = rightEye.GetComponent<Collider>();
        leftEyeRigid = leftEye.GetComponent<Rigidbody>();
        rightEyeRigid = rightEye.GetComponent<Rigidbody>();
        headd = GameObject.Find("head");

        mainOverlay.enabled    = false;                                                                 // mainOverlay is used for special screens, like loading, static, and jamming; off at start
        topRightInsert.enabled = false;                                                                 // Picture within picture is off at start

        leftEyeColl.enabled = false;
        rightEyeColl.enabled = false;
        leftEyeRigid.isKinematic = true;
        rightEyeRigid.isKinematic = true;

        view.bodyCamActive = true;

        leftEyeLodged = true;                                                                           // Eyes lodged at start
        rightEyeLodged = true;                                                                          //

        leftEyeTransitioning = false;                                                                   // Eyes not in process of lodging/dislodging at start
        rightEyeTransitioning = false;                                                                  //

        jammed = false;                                                                                 // No jamming at start
        jamView = false;                                                                                //
        clearView = true;                                                                               //

        initialPosBody = view.transform.localPosition;                                                  // Set initial positions for body and eyes
        initialPosLeft = leftEye.transform.localPosition;                                               //
        initialPosRight = rightEye.transform.localPosition;                                             //

        initialRotBody = view.transform.localRotation;                                                  // Set initial rotations for body and eyes
        returnRotLeft = leftEye.transform.localRotation;                                                //
        returnRotRight = rightEye.transform.localRotation;                                              //

        Debug.Log(view.transform.localEulerAngles);                                                // Troubleshoot eye rotation--what is the initial orientation, for reference?


    }

    void Update() {



        if ((leftEye.leftEyeLock && leftEye.leftEyeActive) || (rightEye.rightEyeLock && rightEye.rightEyeActive)) {                                             
            view.bodyControl = true;
            view.bodyCamActive = false;                                                                   
        } else if ((!leftEye.leftEyeLock && leftEye.leftEyeActive) || (!rightEye.rightEyeLock && rightEye.rightEyeActive)) {
            view.bodyControl = false;
        }

        if (leftEye.tedTrack && !leftEye.leftEyeActive) {
            leftEye.transform.LookAt(ted.transform.position + new Vector3(0, 55, 0));
        } else if (rightEye.tedTrack && !rightEye.rightEyeActive) {
            rightEye.transform.LookAt(ted.transform.position + new Vector3(0, 55, 0));
        }

        if (leftEyeLodged && rightEyeLodged) {                                                          // Close picture within picture if both eyes are in
            topRightInsert.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.J)) {                                                              // Toggle jamming state -- TEMPORARY TESTING FUNCTION
            jammed = !jammed;
            jamView = true;                                                  
        }                                                                                               // ... toggle done

        if (jammed) {                                                                                   // JAMMED SCREEN LOGIC -- need to implement proximity jam duration factor (jamDuration)         !!
            if (jamView) {                                                                              // Ultimate intent is to have random flashes of jam screen that become longer
                jamView = false;                                                                        // and more frequent as sensor (Teddy, eye, etc.) gets closer to jamming signal
                clearView = false;
                SwitchMode(bodyCamera, ViewMode.Adjusting);                                             // Once jammed is set to true, view changes to static/adjusting animated image
                startStatic = Random.Range(1, 3);                                                       // Establishes duration of jam burst
                staticTime = Time.time;                                                                 // Time of jam burst start
                clearStatic = Random.Range(1, 3);                                                       // Establishes duration of clear interval following jam (yes, on the same frame)
                clearTime = Time.time + startStatic;                                                    // Time of clear interval start = time after jam burst ends
            } else {                                                            
                if (!clearView && (Time.time - staticTime > startStatic)) {                             // When jam burst time has passed...
                    SwitchMode(bodyCamera, ViewMode.Std);                                               //      set view to normal
                    clearView = true;                                                                   //      and set clearView to true to bypass this code (so normal view doesn't keep loading**).
                } else if (Time.time - clearTime > clearStatic) {                                       // When the clear interval time has passed (following the jam burst)...
                    jamView = true;                                                                     //      flip jamView "mode" to true to open up the jamView block and restart jam/clear cycle.
                }                                                                                                      
            }                                                                   
        } else {                                                                                        // **Well, so the normal view is set every frame jammed is false, but it works, so whatever
            SwitchMode(bodyCamera, ViewMode.Std);                               
            jamView = true;                                                    
        }                                                                                               // JAMMED SCREEN LOGIC DONE

        if (Input.GetKeyDown(KeyCode.Tab)) {                                                            // If we press TAB...
            Debug.Log(view.bodyCamActive + " " + view.bodyControl + " " + leftEye.leftEyeActive + " " + rightEye.rightEyeActive + " " + leftEye.leftEyeLock + " " + rightEye.rightEyeLock);

            if (!leftEyeLodged || !rightEyeLodged) {                                                    //      If at least one of the eyes is out...
                if (leftEye.leftEyeActive) {                                                            //          And if we're either controlling the left eye, or it's in and the body cam is on??? 
                    leftEye.leftEyeActive = false;                                                      //              Deactivate the left eye
                    SwitchCam(view.gameObject, rightEye.gameObject,
                              Vector3.zero + new Vector3(0, 0, 0.57f), Quaternion.identity);                    

                    if (rightEyeLodged) {
                        view.bodyControl = true;
                    } else {
                        view.bodyControl = false;
                        rightEye.rightEyeActive = true;
                    }
                    topRightInsert.texture = leftEyeRT;
                } else if (rightEye.rightEyeActive) {
                    rightEye.rightEyeActive = false;
                    SwitchCam(view.gameObject, leftEye.gameObject,
                              Vector3.zero + new Vector3(0, 0, 0.57f), Quaternion.identity);

                    if (leftEyeLodged) {
                        view.bodyControl = true;
                    } else {
                        view.bodyControl = false;
                        leftEye.leftEyeActive = true;
                    }
                    topRightInsert.texture = rightEyeRT;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {                                 // Camera insert logic  
            Debug.Log(leftEye.transform.localRotation); 
                                                                               
            if (!topRightInsert.enabled) {                                      
                topRightInsert.enabled = true;
            }

            rightEye.rightEyeActive = false;

            if (leftEyeLodged) {
                anim.SetTrigger("DislodgeLeftEye");
                returnRotLeft = leftEye.transform.localRotation;                // Store left eye rotation just prior to dislodging
                returnRotRight = rightEye.transform.localRotation;
                Debug.Log(returnRotLeft);
                leftEyeLodged = false;
                leftEye.transform.parent = null;
                leftEye.transform.Translate(0, 0, 5f, Space.Self);

                SwitchCam(view.gameObject, leftEye.gameObject, 
                          Vector3.zero, Quaternion.identity);                   // Make body camera child of left eye

                leftEyeColl.enabled = true;                                     // Switch on left eye collider after dislodging
                leftEyeRigid.isKinematic = false;
                view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                view.bodyControl = false;                                       // Disable body controls

                leftEye.leftEyeActive = true;                                   // Enable left eye controls
                leftEye.leftEyeLock = false;

                topRightInsert.texture = rightEyeRT;                            // If left eye pops out, main view is left eye cam and insert is right eye cam

                anim.SetTrigger("LeftEyeDislodged");

            } else if (!leftEyeLodged) {
                if ((headd.transform.position - leftEye.transform.position).magnitude < 20) {
                    leftEyeColl.enabled = false;
                    leftEyeRigid.isKinematic = true;

                    anim.SetTrigger("DislodgeLeftEye");
                    leftEyeLodged = true;

                    if (rightEyeLodged) {                                                               // If lodging left eye, and right eye is lodged, turn on body cam
                        SwitchCam(view.gameObject, headd.gameObject, initialPosBody, initialRotBody);                        
                        view.bodyCamActive = true;                        
                    }

                    view.bodyControl = true;
                    leftEye.leftEyeActive = false;

                    leftEye.transform.parent = headd.transform;
                    leftEye.transform.localPosition = initialPosLeft;
                    leftEye.transform.localRotation = returnRotLeft;

                    view.curRotX = 0;

                    anim.SetTrigger("LeftEyeDislodged");

                    if (rightEyeLodged) {
                        rightEye.transform.localRotation = returnRotRight;
                    }
                }
            }

            Debug.Log(view.transform.rotation);
        }


        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            if (!topRightInsert.enabled) {
                topRightInsert.enabled = true;
            }

            leftEye.leftEyeActive = false;

            if (rightEyeLodged) {
                anim.SetTrigger("DislodgeRightEye");
                returnRotRight = rightEye.transform.localRotation;
                returnRotLeft = leftEye.transform.localRotation;
                rightEyeLodged = false;
                rightEye.transform.parent = null;
                rightEye.transform.Translate(0, 0, 5f, Space.Self);

                SwitchCam(view.gameObject, rightEye.gameObject, 
                          Vector3.zero, Quaternion.identity);                   // Make body camera child of right eye                     

                rightEyeColl.enabled = true;                                    // Switch on right eye collider after dislodging
                rightEyeRigid.isKinematic = false;
                view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                view.bodyControl = false;                                       // Disable body controls

                rightEye.rightEyeActive = true;                                 // Enable right eye controls
                rightEye.rightEyeLock = false;

                topRightInsert.texture = leftEyeRT;                             // If right eye pops out, main view is right eye cam and insert is left eye cam

                anim.SetTrigger("RightEyeDislodged");
            } else if (!rightEyeLodged) {
                if ((headd.transform.position - rightEye.transform.position).magnitude < 20) {
                    rightEyeColl.enabled = true;
                    rightEyeRigid.isKinematic = false;

                    anim.SetTrigger("DislodgeRightEye");
                    rightEyeLodged = true;

                    if (leftEyeLodged) {
                        SwitchCam(view.gameObject, headd.gameObject, initialPosBody, initialRotBody);
                        view.bodyCamActive = true;                        
                    }

                    view.bodyControl = true;
                    rightEye.rightEyeActive = false;

                    rightEye.transform.parent = headd.transform;
                    rightEye.transform.localPosition = initialPosRight;
                    rightEye.transform.localRotation = returnRotRight;

                    view.curRotX = 0;

                    anim.SetTrigger("RightEyeDislodged");

                    if (leftEyeLodged) {
                        leftEye.transform.localRotation = returnRotLeft;
                    }
                }
            }
        }

    }                                                                                                   // ... Camera insert logic done

    public void SwitchMode(Camera cam, ViewMode mode) {                                                 // Switch camera modes (overlays, x-ray stuff, etc.)
        if (mode == ViewMode.XRay) {
            //EnableXRayView();
        } else if (mode == ViewMode.OffAir) {
            cam.targetTexture = staticView;
        } else if (mode == ViewMode.Adjusting) {
            mainOverlay.enabled = true;
        } else if (mode == ViewMode.Std) {
            mainOverlay.enabled = false;
        }
    }

    public void SwitchCam(GameObject cam, GameObject target, Vector3 finalPos, Quaternion finalRot) {   // Parent target to camera object, set camera's position and rotation
        cam.transform.parent = null;
        cam.transform.parent = target.transform;
        cam.transform.localPosition = finalPos;
        cam.transform.localRotation = finalRot;
    }
}
