using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeddyRightEye : MonoBehaviour {

    public Light rightLight;
    public bool rightEyeActive, rightEyeLock, tedTrack;
    public float rightEyeSpeed;                                                                                     // Factor controlling eye movement speed, all directions
    public float rightEyeLookSens;                                                                                  // Factor controlling eye rotation speed
    public float tiltSpeed, depthSpeed;                                                                                         // Factor controlling eye tilt speed
    public bool sixDOF;                                                                                             // Toggle for classic six-degrees-of-freedom controls
    public float maxIDRangeFactor, maxIDRange;                                                                      // From how far away can the eye ID targets? See calc in FixedUpdate
    public RawImage rightEyeScreen, rightEyeScreenPIP, lockIndicator, sixDOFIndicator;
    public Texture lockIndicatorOn, lockIndicatorOff;

    private Teddy ted;                                                                                              // Ted reference needed for lock mode
    //private Dave dave;
    private GameObject cam;                                                                                         // Is this eye camera reference needed??                        !!!
    private BodyCam bCam;
    private Camera bCamCamera;
    private TeddyLeftEye leftEye;
    private ID scanObject;
    private InfoScan scanner;
    private CameraMaster camMaster;
    private Text scanText;
    private Rigidbody rigid;
    private Collider coll;
    private Transform scanFrom;
    private CommsController commsControl;
    private ToolSelector toolSelect;
    private float storedSpeed;
    private float scanStart;
    private string emptyTxt;
    private string previousTargetedIDObjectName;
    private Transform previousObject;
    private bool targetInfoCleared;
    

    // Use this for initialization
    void Start () {

        ted = FindObjectOfType<Teddy>();
        //dave = FindObjectOfType<Dave>();
        camMaster = FindObjectOfType<CameraMaster>();                                                                                                                
        cam = GameObject.Find("RightEyeCam");
        bCam = FindObjectOfType<BodyCam>();
        bCamCamera = FindObjectOfType<BodyCam>().GetComponent<Camera>();
        leftEye = FindObjectOfType<TeddyLeftEye>();
        scanner = FindObjectOfType<InfoScan>();
        scanText = scanner.GetComponent<Text>();
        rigid = this.GetComponent<Rigidbody>();
        coll = this.GetComponent<Collider>();
        commsControl = FindObjectOfType<CommsController>();
        toolSelect = FindObjectOfType<ToolSelector>();

        rightEyeActive = false;                                                                                     // Right eye starts inactive
        rightEyeLock = false;                                                                                        // Right eye starts locked (really??)
        tedTrack = false;

        //rigid.velocity = Vector3.zero;
        //rigid.angularVelocity = Vector3.zero;

        sixDOF = false;                                                                                             // Non-6dof on start
        rightLight.enabled = false;                                                                                 // Eye light off on start

        lockIndicator.enabled = false;
        sixDOFIndicator.enabled = false;

        emptyTxt = null;

        scanStart = 0;

        storedSpeed = rightEyeSpeed;

        scanObject = null;
        previousTargetedIDObjectName = "";
        targetInfoCleared = false;

        scanFrom = null;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        maxIDRange = maxIDRangeFactor * (100 / bCamCamera.fieldOfView);

        if (camMaster.gamePaused) {
            rigid.constraints = RigidbodyConstraints.FreezeAll;
        } else {
            rigid.constraints &= ~RigidbodyConstraints.FreezePosition;
        }


        if (tedTrack) {
            this.transform.LookAt(ted.transform.position + new Vector3(0, 55, 0));
            lockIndicator.texture = lockIndicatorOn;
            sixDOFIndicator.enabled = false;
        } else {
            lockIndicator.texture = lockIndicatorOff;
        }

        if (rightEyeLock)   {lockIndicator.enabled = true;} 
        else                {lockIndicator.enabled = false;}

        if (camMaster.reticleEnabled) {
            scanText.enabled = true;
            

            //lockIndicator.enabled = true;

            RaycastHit hit;
            //if (camMaster.rightEyeLodged) {
            //    scanFrom = this.transform.TransformPoint(0, 0, 15);
            //}
            if (bCam.bodyControl && !camMaster.rightEyeLodged) {
                scanFrom = leftEye.transform;
            } else {
                scanFrom = bCam.transform;
            }


            if (Physics.Raycast(scanFrom.position, scanFrom.TransformDirection(Vector3.forward), out hit, maxIDRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {

                targetInfoCleared = false;

                if (hit.collider.transform.Find("Ghost") != null) {
                    camMaster.SetHallucinationTextToStringInsteadOfRandom(hit.collider.transform.Find("Ghost").GetComponent<Ghost>().hallucinationText);
                } else { camMaster.SetHallucinatingTextToRandom(); }

                if (hit.collider.transform.Find("TextAndSpeech") != null) {
                    commsControl.startingPrompt = hit.collider.transform.Find("TextAndSpeech").GetComponent<TextAndSpeech>().openingTextLine;
                    commsControl.transmissionSource = hit.collider.transform;
                    if (hit.collider.transform.GetComponent<CyclopsAI>() != null && commsControl.textActive && hit.collider.transform.GetComponent<CyclopsAI>().alertLevel < 100) {
                        hit.collider.transform.GetComponent<CyclopsAI>().alertLevel += Time.deltaTime * hit.collider.transform.GetComponent<CyclopsAI>().alertMultiplier;
                    }
                } else {
                    commsControl.startingPrompt = null;
                    commsControl.transmissionSource = null;
                }

                if (hit.collider.transform.Find("ID") != null) {

                    scanObject = hit.collider.transform.Find("ID").GetComponent<ID>();
                    if (!toolSelect.toolSelectorOpen) {
                        if (scanObject.KnowName)    { scanner.DisplayTextUsingTargetingColor(scanObject.ObjName);} 
                        else                        { scanner.DisplayTextUsingTargetingColor(scanObject.UnknownField); }
                    }
                    if (previousTargetedIDObjectName == scanObject.ObjName) {

                        if (scanObject.IDType == ID.IdentificationType.Character) {
                            IDCharacter idChar = (IDCharacter)scanObject;
                            foreach (Transform mark in idChar.markers) {

                                if (scanObject.hacked && camMaster.reticleEnabled) { mark.GetComponent<MeshRenderer>().enabled = true; }
                                else { mark.GetComponent<MeshRenderer>().enabled = false; }

                            }
                        }
                    }

                    previousObject = hit.collider.transform;
                    previousTargetedIDObjectName = scanObject.ObjName;



                    //scanner.DisplayBlurb(scanObject.ObjName, scanObject.ObjActualName, scanObject.ObjAvatarName, scanObject.ObjDescription, scanObject.UnknownField,
                    //    scanObject.ObjStatus, scanObject.KnowName, scanObject.KnowActName, scanObject.KnowAvatarName, scanObject.KnowDescription, scanObject.KnowStatus);
                    scanner.EnableInfoPanelWithID(scanObject);

                    if (Input.GetButton("Square Button") && !camMaster.gamePaused) {
                        if (!scanObject.KnowDescription) {
                            if ((((Time.time - scanStart) * 40) > hit.collider.bounds.size.magnitude)) {
                                scanObject.KnowDescription = true;
                            }
                        }
                    }
                } else {
                    if (!toolSelect.toolSelectorOpen) {
                        scanner.DisplayTextUsingModeColor(scanner.displayModeText);
                    }

                    scanStart = Time.time;
                    //scanner.DisplayBlurb(emptyTxt, emptyTxt, emptyTxt, emptyTxt, emptyTxt, emptyTxt, false, false, false, false, false);
                    scanner.DisableInfoPanel();

                    if (scanObject != null && scanObject.IDType == ID.IdentificationType.Character) {
                        IDCharacter idChar = (IDCharacter)scanObject;
                        foreach (Transform mark in idChar.markers) { mark.GetComponent<MeshRenderer>().enabled = false; }
                    }
                }

            } else {
                // Clear comms starting prompt and resume hallucination text scramble
                commsControl.startingPrompt = null;
                camMaster.SetHallucinatingTextToRandom();

                // If tool selector isn't up, set the reticle display text to display mode color
                if (!toolSelect.toolSelectorOpen) {
                    scanner.DisplayTextUsingModeColor(scanner.displayModeText);
                }

                // Clear the info box info
                //scanner.DisplayBlurb(emptyTxt, emptyTxt, emptyTxt, emptyTxt, emptyTxt, emptyTxt, false, false, false, false, false);
                scanner.DisableInfoPanel();
                scanStart = Time.time;

                // If the previous target was a character, clear its markers
                if (!targetInfoCleared && scanObject != null && scanObject.IDType == ID.IdentificationType.Character) {
                    IDCharacter idChar = (IDCharacter)scanObject;
                    foreach (Transform mark in idChar.markers) { mark.GetComponent<MeshRenderer>().enabled = false; }
                    targetInfoCleared = true;
                }
            }
        } else {
            scanText.enabled = false;
            //scanner.DisplayBlurb("", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, false, false, false, false);                              // keeps Infoscan box from remaining when switching control away from right eye
            scanner.DisableInfoPanel();

            //lockIndicator.enabled = false;
            sixDOFIndicator.enabled = false;

            rightEyeScreen.enabled = false;
            if (!camMaster.rightEyeLodged) {
                rightEyeScreenPIP.enabled = true;
            } else { rightEyeScreenPIP.enabled = false; }
        }

        if (rightEyeActive) {                                                                                       // If the right eye is active...
            rightEyeScreen.enabled = true;
            rightEyeScreenPIP.enabled = false;

            if (!rightEyeLock && camMaster.commsEnabled == false && !camMaster.gamePaused) {                                                                                    //      If view isn't locked...

                float moveX = Input.GetAxis("Left Joystick Horizontal") + Input.GetAxis("Horizontal");                                                          //          Side movement (A-D), loc x
                float moveY = 0;
                float moveZ = 0;

                if (sixDOF) {
                    if (Input.GetButton("Left Stick Press")) {
                        moveY = Input.GetAxis("Left Joystick Vertical") + Input.GetAxis("Vertical");
                        moveZ = 0;                                                            //          Forward-back movement (W-S), loc z
                    } else {
                        moveY = 0;
                        moveZ = Input.GetAxis("Left Joystick Vertical") + Input.GetAxis("Vertical");
                    }
                } else {
                    moveZ = Input.GetAxis("Left Joystick Vertical") + Input.GetAxis("Vertical");
                }
                this.transform.Translate(moveX * rightEyeSpeed * Time.fixedDeltaTime,
                                         moveY * depthSpeed * Time.fixedDeltaTime,
                                         moveZ * rightEyeSpeed * Time.fixedDeltaTime, Space.Self);                       //          Translation, * rightEyeSpeed factor

                

                float rotX = (Input.GetAxis("Right Joystick Vertical") - Input.GetAxis("Mouse Y")) * bCamCamera.fieldOfView;                                                             //          Vert rotation, loc x, default inverted
                float rotY = (Input.GetAxis("Right Joystick Horizontal") + Input.GetAxis("Mouse X")) * bCamCamera.fieldOfView;                                                              //          Horiz rotation, loc y
                float rotZ = -Input.GetAxis("Triggers") + Input.GetAxis("Tilt");                                                                //          Tilt, loc z (inverted), for 6dof mode

               

                if (sixDOF) {                                                                                       //          If in 6dof mode...
                    this.transform.Rotate(rotX * rightEyeLookSens,
                                          rotY * rightEyeLookSens,
                                          rotZ * tiltSpeed);                                                        //              Rotation, * sens and tilt factors
                    sixDOFIndicator.enabled = true;

                } else {                                                                                            //          If not in 6dof mode...
                    this.transform.Rotate(rotX * rightEyeLookSens, 0, 0);                                           //              Rotation, vertical, why separate??              !!!
                    this.transform.Rotate(0, rotY * rightEyeLookSens, 0, Space.World);                              //              Rotation, horizontal, why separate??            !!!

                    this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x,
                                                                  this.transform.localEulerAngles.y,
                                                                  0);                                           //          If not in 6dof mode, zero loc z-rot
                    sixDOFIndicator.enabled = false;
                }
            } //else {
                //if (tedTrack) { 
                //    this.transform.LookAt(ted.transform.position + new Vector3(0, 55, 0));                          //          Eye rotation follows Ted (origin + 55 y)
                //}
            //}                                                                                                       //      If view is locked...
        }
    }
}
