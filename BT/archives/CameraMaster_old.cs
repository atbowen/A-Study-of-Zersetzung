using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMaster : MonoBehaviour {

    public bool gamePaused;
    public RawImage pauseScreen;

    public RenderTexture staticView, viewAdjust, leftEyeRT, rightEyeRT, headdActualRT, headdRefRT;
    public RawImage mainOverlay, topRightInsert, bothEyesScreen, hallucinationScreen;                                        // mainOverlay image is for full screen projections; insert is for picture-in-a-picture
    public Texture[] hallucinationFrames;
    public Camera bodyCamera;
    public Transform headActual, headNoBob;
    public Transform headObjectHavingMesh;
    public enum ViewMode { Std, XRay, OffAir, Adjusting };                                              // Std = normal screen, XRay = wireframe, OffAir = static, Adjusting = loading screen
    public bool jammed;                                                                                 // When will transmissions from eye cameras to interface be jammed??
    public float jamDuration;                                                                           // Inverse multiplier for jamming time--shorter the distance from jam source, longer the jam
    public float hallucinationFrameTimeLength;
    public bool leftEyeLodged, rightEyeLodged, leftEyeTransitioning, rightEyeTransitioning;             // Eye "lodging" switches determine view and control scenarios
    public bool rightEyeAbilitiesAvailable, reticleEnabled;
    public bool hallucinating, atTedsRoom;
    public bool commsEnabled;
    public InputField commsEnter;
    public Font englishFont, solarFont;
    public bool englishOrSolar;
    public bool leftEyeAvailable;
    public bool headbob;
    public Vector3 cameraFollowVelocity;
    public float camSmoothTime;

    private BodyCam view;                                                                               // Between eyes camera is used when both eyes are lodged
    private Teddy ted;
    private TeddyHead tedHead;
    private TeddyLeftEye leftEye;
    private TeddyRightEye rightEye;
    private WorkDesk wkDesk;
    private Transform leftEyepatch, rightEyepatch;
    private Animator anim, animHead;
    private Rigidbody rigid, leftEyeRigid, rightEyeRigid;
    private Transform headd;
    private RenderTexture headdRT;
    private Transform camTarget;
                                                                
    private Collider leftEyeColl, rightEyeColl;
    private RawImage reticleImg;
    private RawImage commsImg;

    private bool jamView, clearView;
    private float startStatic, staticTime, clearStatic, clearTime;                                      // Variables for creating random static when transmissions from an eye is jammed
    private Vector3 initialPosBody, initialPosLeft, initialPosRight;                                    // For resetting position
    private Quaternion returnRotLeft, returnRotRight, initialRotBody;                                   // For resetting rotation
    private float reticlePressTime, hallucinationFrameTimeIndex;
    private bool reticlePressed;
    private int currentHallucinationFrameIndex;
    private float eyeDislodgeTimer, eyeDislodgeTimerRef;

    // Use this for initialization
    void Start () {
        view = FindObjectOfType<BodyCam>();
        ted = FindObjectOfType<Teddy>();
        anim = ted.GetComponent<Animator>();
        rigid = ted.GetComponent<Rigidbody>();
        tedHead = FindObjectOfType<TeddyHead>();
        animHead = tedHead.GetComponent<Animator>();
        wkDesk = FindObjectOfType<WorkDesk>();

        commsImg = commsEnter.transform.parent.GetComponent<RawImage>();

        leftEye = FindObjectOfType<TeddyLeftEye>();
        rightEye = FindObjectOfType<TeddyRightEye>();
        //leftEyeColl = leftEye.GetComponent<Collider>();
        rightEyeColl = rightEye.GetComponent<Collider>();
        //leftEyeRigid = leftEye.GetComponent<Rigidbody>();
        rightEyeRigid = rightEye.GetComponent<Rigidbody>();
        //headd = GameObject.Find("tedhead");
        reticleImg = GameObject.Find("Reticle").GetComponent<RawImage>();

        leftEyepatch = leftEye.transform.Find("lefteyepatch");
        rightEyepatch = rightEye.transform.Find("righteyepatch");

        gamePaused = false;

        mainOverlay.enabled = false;                                                                 // mainOverlay is used for special screens, like loading, static, and jamming; off at start
        topRightInsert.enabled = false;                                                                 // Picture within picture is off at start

        //leftEyeColl.enabled = false;
        rightEyeColl.enabled = false;
        //leftEyeRigid.isKinematic = true;
        rightEyeRigid.isKinematic = true;

        view.bodyCamActive = true;

        leftEyeLodged = true;                                                                           // Eyes lodged at start
        rightEyeLodged = true;                                                                          //

        leftEyeTransitioning = false;                                                                   // Eyes not in process of lodging/dislodging at start
        rightEyeTransitioning = false;                                                                  //

        jammed = false;                                                                                 // No jamming at start
        jamView = false;                                                                                //
        clearView = true;                                                                              //
        currentHallucinationFrameIndex = 0;
        hallucinationFrameTimeIndex = 0;

        reticleEnabled = true;
        hallucinating = false;
        atTedsRoom = false;
        commsEnabled = false;
        commsEnter.enabled = false;
        commsEnter.DeactivateInputField();
        commsEnter.GetComponent<Image>().enabled = false;

        englishOrSolar = true;

        leftEyeAvailable = false;

        reticlePressTime = 0;
        reticlePressed = false;

        if (headbob) {
            headdRT = headdActualRT;
            headd = headActual;
        } else {
            headdRT = headdRefRT;
            headd = headNoBob;
        }

        //camTarget = null;                                                                             //smoothdamp attempt

        

        initialPosBody = view.transform.localPosition;                                                  // Set initial positions for body and eyes
        initialPosLeft = leftEye.transform.localPosition;                                               //
        initialPosRight = rightEye.transform.localPosition;                                             //

        initialRotBody = view.transform.localRotation;                                                  // Set initial rotations for body and eyes
        returnRotLeft = leftEye.transform.localRotation;                                                //
        returnRotRight = rightEye.transform.localRotation;

        SwitchCam(view.gameObject, headd, initialPosBody, initialRotBody);

        initialRotBody = view.transform.localRotation;

        eyeDislodgeTimer = 0.2f;
        eyeDislodgeTimerRef = 0;
    }

    // Update is called once per frame
    void FixedUpdate() {

        //view.transform.position = Vector3.SmoothDamp(view.transform.position, camTarget.position, ref cameraFollowVelocity, camSmoothTime);   //smoothdamp attempt
        //view.transform.rotation = Quaternion.LookRotation(camTarget.forward);

        if (Input.GetButtonDown("Start")) {
            gamePaused = !gamePaused;
        }

        if (gamePaused) {
            rigid.constraints = RigidbodyConstraints.FreezeAll;
            anim.speed = 0;
            animHead.speed = 0;
            pauseScreen.enabled = true;
            wkDesk.deskEnabled = true;
            wkDesk.openDesk = true;
        } else {
            rigid.constraints &= ~RigidbodyConstraints.FreezePosition;
            pauseScreen.enabled = false;
            wkDesk.deskEnabled = false;
            wkDesk.closeDesk = true;
        }

        //if (!headbob && view.bodyControl) {
        //    headObjectHavingMesh.GetComponent<SkinnedMeshRenderer>().enabled = false;
        //    leftEye.GetComponent<MeshRenderer>().enabled = false;
        //    leftEyepatch.GetComponent<MeshRenderer>().enabled = false;
        //    rightEye.GetComponent<MeshRenderer>().enabled = false;
        //    rightEyepatch.GetComponent<MeshRenderer>().enabled = false;
        //} else {
        //    headObjectHavingMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
        //    leftEye.GetComponent<MeshRenderer>().enabled = true;
        //    leftEyepatch.GetComponent<MeshRenderer>().enabled = true;
        //    rightEye.GetComponent<MeshRenderer>().enabled = true;
        //    rightEyepatch.GetComponent<MeshRenderer>().enabled = true;
        //}

        if (((Input.GetAxis("Triggers") < 0) || Input.GetKeyDown(KeyCode.Mouse1)) && Time.time - reticlePressTime > 0.5 && !gamePaused) {
            commsEnter.enabled = !commsEnter.enabled;

            reticlePressTime = Time.time;

            if (commsEnabled) {
                commsEnter.DeactivateInputField();
                //commsEnter.enabled = false;
                commsEnter.GetComponent<Image>().enabled = false;
                commsEnabled = false;
                commsImg.enabled = !commsImg.enabled;
                commsImg.transform.Find("CommsController").GetComponent<Text>().enabled = !commsImg.transform.Find("CommsController").GetComponent<Text>().enabled;
            } else {
                commsEnter.ActivateInputField();
                //commsEnter.enabled = true;
                commsEnter.GetComponent<Image>().enabled = true;
                commsEnabled = true;
                commsImg.enabled = true;
                commsImg.transform.Find("CommsController").GetComponent<Text>().enabled = true;
            }
        }

        //if (Input.GetKeyDown(KeyCode.M) && !commsEnabled && !gamePaused) {
        //    commsImg.enabled = !commsImg.enabled;
        //    commsImg.transform.Find("CommsController").GetComponent<Text>().enabled = !commsImg.transform.Find("CommsController").GetComponent<Text>().enabled;
        //}

        if (hallucinating) {
            hallucinationScreen.enabled = true;

            hallucinationScreen.texture = hallucinationFrames[currentHallucinationFrameIndex];

            if ((Time.time - hallucinationFrameTimeIndex) > hallucinationFrameTimeLength) {
                currentHallucinationFrameIndex = (currentHallucinationFrameIndex + 1) % hallucinationFrames.Length;
                hallucinationFrameTimeIndex = Time.time;
            }

        } else {
            hallucinationScreen.enabled = false;
        }

        if (!commsEnabled) {                                                                                // Controls only work when not using comms>>

            if (rightEye.rightEyeActive || (rightEyeLodged && view.bodyCamActive)) {
                rightEyeAbilitiesAvailable = true;
            } else { rightEyeAbilitiesAvailable = false; }

            if (rightEyeAbilitiesAvailable) {
                if (((Time.time - reticlePressTime > 0.5) && (Input.GetAxis("D-Pad Left Right") < 0) || Input.GetButtonDown("Reticle")) & !gamePaused) {                                                         // Toggle reticle...
                    reticleEnabled = !reticleEnabled;                                                           //
                    if (reticleEnabled) {                                                                       //
                        reticleImg.enabled = true;                                                              //
                    } else reticleImg.enabled = false;                                                          //

                    reticlePressTime = Time.time;
                }                                                                                              // ...>>
            } else {
                reticleEnabled = false;
                reticleImg.enabled = false;
                //reticlePressTime = 0;
            }

            if (((Time.time - reticlePressTime) > 0.5 && (Input.GetAxis("D-Pad Left Right") > 0) || Input.GetButtonDown("Hallucinations")) & !gamePaused) {                                                          // Toggles ability to see hallucinations...
                hallucinating = !hallucinating;                                                             // ...>>
                hallucinationFrameTimeIndex = Time.time;                                                    // Start timer for changing hallucination overlay sprites
                reticlePressTime = Time.time;
            }

            if (leftEyeLodged && rightEyeLodged) {                                                          // If eyes both lodged...
                topRightInsert.enabled = false;                                                             // no picture-in-picture

                bothEyesScreen.enabled = true;

                view.bodyCamActive = true;                                                                  // main camera at forehead
                view.bodyControl = true;                                                                    // can control body

                //if (Input.GetKeyDown(KeyCode.Alpha1) && leftEyeAvailable && !gamePaused) {                                                     // CONTROL -- left eye dislodge
                //    if (!topRightInsert.enabled) {                                                          // show picture-in-picture
                //        topRightInsert.enabled = true;
                //    }

                //    anim.SetTrigger("DislodgeLeftEye");                             // trigger dislodge left eye animation
                //    returnRotLeft = leftEye.transform.localRotation;                // store left eye rotation just prior to dislodging
                //    returnRotRight = rightEye.transform.localRotation;              // store right eye rotation
                //    leftEyeLodged = false;
                //    leftEye.transform.parent = null;                                // unparent left eye
                //    leftEye.transform.Translate(0, 0, 5f, Space.Self);              // move left eye 5 units in front of self
                //    SwitchCam(view.gameObject, leftEye.transform,
                //                  Vector3.zero, Quaternion.identity);               // Make body camera child of left eye

                //    //leftEyeColl.enabled = true;                                   // Switch on left eye collider after dislodging
                //    //leftEyeRigid.isKinematic = false;
                //    view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                //    view.bodyControl = false;                                       // Disable body controls

                //    leftEye.leftEyeActive = true;                                   // Enable left eye controls
                //    leftEye.leftEyeLock = false;

                //    //topRightInsert.texture = rightEyeRT;                            // If left eye pops out, main view is left eye cam and insert is right eye cam
                //    topRightInsert.texture = headdRT;

                //    anim.SetTrigger("LeftEyeDislodged");
                //}

                if (Input.GetButtonDown("X Button") && !gamePaused && (Time.time - eyeDislodgeTimerRef > eyeDislodgeTimer)) {

                    //Debug.Log(rightEyeLodged + ", " + rightEye.rightEyeActive + ", " + rightEye.rightEyeLock);

                    view.SwitchAnimationStateToIdle();

                    eyeDislodgeTimerRef = Time.time;

                    if (!topRightInsert.enabled) {                                                          // Enable camera insert if eye pops out
                        topRightInsert.enabled = true;
                    }

                    //anim.SetTrigger("DislodgeRightEye");
                    //returnRotRight = rightEye.transform.localRotation;                // Store left eye rotation just prior to dislodging
                    //returnRotLeft = leftEye.transform.localRotation;
                    //rightEye.transform.localRotation = returnRotRight;                // ???
                    leftEye.transform.localRotation = returnRotLeft;
                    view.transform.localRotation = initialRotBody;
                    view.curRotX = 0;

                    rightEyeLodged = false;
                    rightEye.transform.parent = null;
                    rightEye.transform.Translate(0, 0, 10f, Space.Self);
                    SwitchCam(view.gameObject, rightEye.transform,
                                  Vector3.zero, Quaternion.identity);                   // Make body camera child of left eye

                    rightEyeColl.enabled = true;                                     // Switch on left eye collider after dislodging
                    rightEyeRigid.isKinematic = false;
                    view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                    view.bodyControl = false;                                       // Disable body controls

                    rightEye.rightEyeActive = true;                                   // Enable left eye controls
                    rightEye.rightEyeLock = false;

                    //topRightInsert.texture = leftEyeRT;                             // If left eye pops out, main view is left eye cam and insert is right eye cam
                    topRightInsert.texture = headdRT;

                    //anim.SetTrigger("RightEyeDislodged");
                }                                                                   // End If -- eyes both lodged...>>
            } else {
                //if (leftEye.leftEyeActive) {

                //    bothEyesScreen.enabled = false;

                //    if (Input.GetButtonDown("Left Bumper") && !gamePaused) {
                //        if (leftEye.leftEyeLock) { leftEye.tedTrack = !leftEye.tedTrack; } else { leftEye.sixDOF = !leftEye.sixDOF; }
                //    }

                //    if (Input.GetButtonDown("Right Bumper") && !gamePaused) {
                //        leftEye.leftEyeLock = !leftEye.leftEyeLock;
                //        //leftEye.sixDOF = false;
                //        rightEye.tedTrack = false;
                //    }

                //    view.bodyCamActive = false;
                //    if (leftEye.leftEyeLock) { view.bodyControl = true; } else { view.bodyControl = false; }

                //    if (Input.GetButtonDown("Circle Button") && !gamePaused) {
                //        leftEye.leftEyeActive = false;
                //        SwitchCam(view.gameObject, rightEye.transform,
                //                    Vector3.zero + new Vector3(0, 0, 0.57f), Quaternion.identity);

                //        if (rightEyeLodged) {
                //            view.bodyControl = true;
                //        } else {
                //            view.bodyControl = false;
                //            rightEye.rightEyeActive = true;
                //        }
                //        topRightInsert.texture = leftEyeRT;
                //    }
                //} else 
                //if (!leftEye.leftEyeActive && rightEyeLodged) {

                //    bothEyesScreen.enabled = false;

                //    if (Input.GetButtonDown("Circle Button") && !gamePaused) {
                //        SwitchCam(view.gameObject, leftEye.transform,
                //                    Vector3.zero + new Vector3(0, 0, 0.57f), Quaternion.identity);
                //        view.bodyControl = false;
                //        leftEye.leftEyeActive = true;

                //        //anim.SetBool("IsWalk", false);
                //        //animHead.SetBool("IsWalk", false);

                //        //topRightInsert.texture = rightEyeRT;
                //        topRightInsert.texture = headdRT;
                //    }
                //} else 
                if (rightEye.rightEyeActive) {

                    bothEyesScreen.enabled = false;

                    if (Input.GetButtonDown("Left Bumper") && !gamePaused) {
                        if (rightEye.rightEyeLock) { rightEye.tedTrack = !rightEye.tedTrack; } else { rightEye.sixDOF = !rightEye.sixDOF; }
                    }

                    if (Input.GetButtonDown("Right Bumper") && !gamePaused) {

                        if (rightEye.rightEyeLock) {
                            reticleEnabled = true;
                            reticleImg.enabled = true;

                            view.SwitchAnimationStateToIdle();
                        } else if (!rightEye.rightEyeLock) {
                            reticleEnabled = false;
                            reticleImg.enabled = false;
                        }

                        rightEye.rightEyeLock = !rightEye.rightEyeLock;
                        //rightEye.sixDOF = false;
                        rightEye.tedTrack = false;

                    }

                    view.bodyCamActive = false;
                    if (rightEye.rightEyeLock) {
                        view.bodyControl = true;
                    } else {
                        view.bodyControl = false;
                    }

                    if (Input.GetButtonDown("Circle Button") && !gamePaused) {
                        rightEye.rightEyeActive = false;

                        if (headbob || view.dead) {
                            //SwitchCam(view.gameObject, leftEye.gameObject,
                            //            Vector3.zero + new Vector3(0, 0, 2f), Quaternion.identity);
                            SwitchCam(view.gameObject, headd, initialPosBody, leftEye.transform.localRotation * Quaternion.Euler(0, 0, -90));
                        } else {
                            SwitchCam(view.gameObject, headd, initialPosBody, leftEye.transform.localRotation * Quaternion.Euler(0, 0, -90));
                        }

                        if (leftEyeLodged) {
                            view.bodyControl = true;
                            view.bodyCamActive = true;
                            //view.GetComponent<Camera>().nearClipPlane = 3f;
                        } else {
                            view.bodyControl = false;
                            leftEye.leftEyeActive = true;
                        }
                        topRightInsert.texture = rightEyeRT;
                    }
                } else if (!rightEye.rightEyeActive && leftEyeLodged) {

                    bothEyesScreen.enabled = false;

                    if (Input.GetButtonDown("Circle Button") && !gamePaused) {
                        SwitchCam(view.gameObject, rightEye.transform,
                                    Vector3.zero + new Vector3(0, 0, 0.57f), Quaternion.identity);
                        view.bodyControl = false;
                        rightEye.rightEyeActive = true;

                        view.SwitchAnimationStateToIdle();

                        view.GetComponent<Camera>().nearClipPlane = 0.3f;

                        topRightInsert.texture = leftEyeRT;
                    }
                }

                //if (Input.GetKeyDown(KeyCode.Alpha1) && leftEyeAvailable && !gamePaused) {                                 // Camera insert logic  

                //    rightEye.rightEyeActive = false;

                //    if (leftEyeLodged) {
                //        anim.SetTrigger("DislodgeLeftEye");
                //        returnRotLeft = leftEye.transform.localRotation;                // Store left eye rotation just prior to dislodging
                //        returnRotRight = rightEye.transform.localRotation;
                //        leftEyeLodged = false;
                //        leftEye.transform.parent = null;
                //        leftEye.transform.Translate(0, 0, 5f, Space.Self);

                //        SwitchCam(view.gameObject, leftEye.transform,
                //                  Vector3.zero, Quaternion.identity);                   // Make body camera child of left eye

                //        //leftEyeColl.enabled = true;                                     // Switch on left eye collider after dislodging
                //        //leftEyeRigid.isKinematic = false;
                //        view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                //        view.bodyControl = false;                                       // Disable body controls

                //        leftEye.leftEyeActive = true;                                   // Enable left eye controls
                //        leftEye.leftEyeLock = false;

                //        //topRightInsert.texture = rightEyeRT;                            // If left eye pops out, main view is left eye cam and insert is right eye cam
                //        topRightInsert.texture = headdRT;

                //        anim.SetTrigger("LeftEyeDislodged");

                //    } else if (!leftEyeLodged) {
                //        if ((headd.transform.position - leftEye.transform.position).magnitude < 20) {
                //            //leftEyeColl.enabled = false;
                //            //leftEyeRigid.isKinematic = true;

                //            anim.SetTrigger("DislodgeLeftEye");
                //            leftEyeLodged = true;

                //            if (rightEyeLodged) {                                                               // If lodging left eye, and right eye is lodged, turn on body cam
                //                SwitchCam(view.gameObject, headd, initialPosBody, initialRotBody);
                //                view.bodyCamActive = true;
                //            }

                //            view.bodyControl = true;
                //            leftEye.leftEyeActive = false;

                //            leftEye.transform.parent = null;
                //            leftEye.transform.parent = headd.transform;
                //            leftEye.transform.localPosition = initialPosLeft;
                //            leftEye.transform.localRotation = returnRotLeft;

                //            view.curRotX = 0;

                //            anim.SetTrigger("LeftEyeDislodged");

                //            if (rightEyeLodged) {
                //                rightEye.transform.localRotation = returnRotRight;
                //            }
                //        }
                //    }
                //}

                if (Input.GetButtonDown("X Button") && !gamePaused && (Time.time - eyeDislodgeTimerRef > eyeDislodgeTimer)) {

                    //Debug.Log(rightEyeLodged + ", " + rightEye.rightEyeActive + ", " + rightEye.rightEyeLock);

                    leftEye.leftEyeActive = false;

                    eyeDislodgeTimerRef = Time.time;

                    if (rightEyeLodged) {
                        //anim.SetTrigger("DislodgeRightEye");
                        //returnRotRight = rightEye.transform.localRotation;
                        //returnRotLeft = leftEye.transform.localRotation;
                      

                        rightEyeLodged = false;
                        rightEye.transform.parent = null;
                        rightEye.transform.Translate(0, 0, 5f, Space.Self);

                        SwitchCam(view.gameObject, rightEye.transform,
                                  Vector3.zero, Quaternion.identity);                   // Make body camera child of right eye                     

                        rightEye.transform.localRotation = returnRotRight;
                        leftEye.transform.localRotation = returnRotLeft;
                        view.transform.localRotation = initialRotBody;
                        view.curRotX = 0;

                        rightEyeColl.enabled = true;                                    // Switch on right eye collider after dislodging
                        rightEyeRigid.isKinematic = false;
                        view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                        view.bodyControl = false;                                       // Disable body controls

                        rightEye.rightEyeActive = true;                                 // Enable right eye controls
                        rightEye.rightEyeLock = false;

                        //topRightInsert.texture = leftEyeRT;                             // If right eye pops out, main view is right eye cam and insert is left eye cam
                        topRightInsert.texture = headdRT;

                        //anim.SetTrigger("RightEyeDislodged");

                    } else if (!rightEyeLodged) {
                        
                        //Debug.Log("dist between right eye and head: " + (headd.transform.position - rightEye.transform.position).magnitude);

                        if ((headd.transform.position - rightEye.transform.position).magnitude < 20) {
                            rightEyeColl.enabled = false;
                            rightEyeRigid.isKinematic = true;

                            //anim.SetTrigger("DislodgeRightEye");
                            rightEyeLodged = true;

                            if (leftEyeLodged) {
                                SwitchCam(view.gameObject, headd, initialPosBody, initialRotBody);
                                view.bodyCamActive = true;
                            }

                            view.bodyControl = true;
                            rightEye.rightEyeActive = false;

                            rightEye.transform.parent = null;
                            rightEye.transform.parent = headd.transform;
                            rightEye.transform.localPosition = initialPosRight;
                            rightEye.transform.localRotation = returnRotRight;

                            view.curRotX = 0;

                            //anim.SetTrigger("RightEyeDislodged");

                            if (leftEyeLodged) {
                                leftEye.transform.localRotation = returnRotLeft;
                            }
                        }
                    }
                }
            }

        } else {
            if (Input.GetKeyDown(KeyCode.RightAlt) && !gamePaused) {
                if (englishOrSolar) { commsEnter.textComponent.font = solarFont;}
                else                { commsEnter.textComponent.font = englishFont;}

                commsEnter.textComponent.text = "";
                englishOrSolar = !englishOrSolar;
            }
        }
    }

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

    public void SwitchCam(GameObject cam, Transform target, Vector3 finalPos, Quaternion finalRot) {   // Parent target to camera object, set camera's position and rotation
        cam.transform.parent = null;
        cam.transform.parent = target;

        //cam.transform.position = target.position;                     //smoothdamp smooth camera procedure attempt
        //camTarget = target;

        cam.transform.localPosition = finalPos;
        cam.transform.localRotation = finalRot;

        //cam.transform.rotation = Quaternion.LookRotation(camTarget.up);   //smoothdamp
    }
}
