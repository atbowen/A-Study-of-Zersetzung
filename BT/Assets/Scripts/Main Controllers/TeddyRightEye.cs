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
    public Texture tedTrackIcon, eyeLockedIcon, eyeUnlockedIcon, sixDOFIcon, levelTiltIcon;

    private Teddy ted;                                                                                              // Ted reference needed for lock mode
    private TeddyHead tedHead;
    private GameObject cam;                                                                                         // Is this eye camera reference needed??                        !!!
    private BodyCam bCam;
    private Camera bCamCamera;
    private TeddyLeftEye leftEye;  
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
    private Transform previousObject;
    private bool targetInfoCleared;

    private bool rightEyeAbilitiesAvailable;

    private ID scanObject, previousTargetedID;
    private IDCharacter hostID;
    private Ghost targetedGhost;
    private TextAndSpeech targetedSpeech;
    private NPCIntelligence targetedAI;
    private AudioSource targetedAudio;

    // Use this for initialization
    void Start () {

        ted = FindObjectOfType<Teddy>();
        tedHead = FindObjectOfType<TeddyHead>();
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

        lockIndicator.enabled = true;
        sixDOFIndicator.enabled = true;

        scanStart = 0;

        storedSpeed = rightEyeSpeed;

        scanObject = null;
        previousTargetedID = null;
        hostID = ted.transform.GetComponent<IDCharacter>();
        targetedGhost = null;
        targetedSpeech = null;
        targetedAI = null;
        targetedAudio = null;

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

        if (rightEyeLock) {
            if (tedTrack) {
                this.transform.LookAt(tedHead.transform.position);
                lockIndicator.texture = tedTrackIcon;
                sixDOFIndicator.texture = levelTiltIcon;
            }
            else {
                lockIndicator.texture = eyeLockedIcon;
            }
        }
        else {
            lockIndicator.texture = eyeUnlockedIcon;
            if (sixDOF) {
                sixDOFIndicator.texture = sixDOFIcon;
            }
            else {
                sixDOFIndicator.texture = levelTiltIcon;
            }
        }

        //if (rightEyeLock)   {lockIndicator.enabled = true;} 
        //else                {lockIndicator.enabled = false;}

        if (camMaster.reticleEnabled || !camMaster.reticleEnabled) {
            

            //lockIndicator.enabled = true;

            RaycastHit hit;
            //if (camMaster.rightEyeLodged) {
            //    scanFrom = this.transform.TransformPoint(0, 0, 15);
            //}
            //if (bCam.bodyControl && !camMaster.rightEyeLodged) {
            //    scanFrom = leftEye.transform;
            //} else {
            //    scanFrom = bCam.transform;
            //}

            scanFrom = bCam.transform;

            //if (bCam.IsHoldingDocument()) { scanner.HideReticleAndText(true); }

            if (rightEyeAbilitiesAvailable && !bCam.IsHoldingDocument() && !bCam.Using() && Physics.Raycast(scanFrom.position, scanFrom.TransformDirection(Vector3.forward), 
                                                            out hit, maxIDRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {

                targetInfoCleared = false;

                // Set important AI components of target, if present
                Transform targetTran;

                if (hit.collider.GetComponent<PointerToID>()) {
                    scanObject = hit.collider.GetComponent<PointerToID>().desiredID;
                    targetTran = hit.collider.GetComponent<PointerToID>().desiredID.transform;

                    if (scanObject.GetType() == typeof(IDInteractable)) {
                        IDInteractable IDThing = (IDInteractable)scanObject;

                        if (hit.collider.GetComponent<PointerToID>().makeDesiredActivateThisCollider) {

                            IDThing.currentSwitchCollider = hit.collider;

                            if (!IDThing.switchColliders.Contains(hit.collider)) { IDThing.switchColliders.Add(hit.collider); }

                            foreach (Collider col in hit.collider.GetComponent<PointerToID>().otherCollidersToActivate) {
                                if (!IDThing.switchColliders.Contains(col)) { IDThing.switchColliders.Add(col); }
                            }

                            IDThing.switchColliderWithPointerColliders = true;
                        }
                    }
                }
                else {

                    targetTran = hit.collider.transform;
                    if (targetTran.GetComponent<ID>() != null) {
                        scanObject = targetTran.GetComponent<ID>();

                        scanObject.ClearSwitchColliders();
                    }
                    else {
                        scanObject = null;
                    }

                }

                if (targetTran.GetComponent<Ghost>() != null)           { targetedGhost = targetTran.GetComponent<Ghost>(); }
                else                                                    { targetedGhost = null; }
                if (targetTran.GetComponent<TextAndSpeech>() != null)   { targetedSpeech = targetTran.GetComponent<TextAndSpeech>(); }
                else                                                    { targetedSpeech = null; }
                if (targetTran.GetComponent<NPCIntelligence>() != null) { targetedAI = targetTran.GetComponent<NPCIntelligence>(); }
                else                                                    { targetedAI = null; }
                if (targetTran.GetComponent<AudioSource>() != null)     { targetedAudio = targetTran.GetComponent<AudioSource>(); }
                else                                                    { targetedAudio = null; }

                // Activating comms with a Waterman rapidly increases its alert level
                if (hit.collider.transform.GetComponent<CyclopsAI>() != null && commsControl.textActive && hit.collider.transform.GetComponent<CyclopsAI>().alertLevel < 100) {
                    hit.collider.transform.GetComponent<CyclopsAI>().alertLevel += Time.deltaTime * hit.collider.transform.GetComponent<CyclopsAI>().alertMultiplier;
                }

                if (scanObject != null) {

                    //if (!toolSelect.toolSelectorOpen) {
                        
                    //}
                    if (previousTargetedID == scanObject) {

                        if (scanObject.GetType() == typeof(IDCharacter)) {
                            IDCharacter idChar = (IDCharacter)scanObject;
                            foreach (Transform mark in idChar.markers) {

                                if (scanObject.hacked && camMaster.reticleEnabled) { mark.GetComponent<MeshRenderer>().enabled = true; }
                                else { mark.GetComponent<MeshRenderer>().enabled = false; }

                            }
                        }
                    }

                    if (scanObject.GetType() == typeof(IDInteractable)) {
                        IDInteractable interactableTarget = (IDInteractable)scanObject;

                        bCam.TriggerVehicleInteriorAnimation(interactableTarget.interiorVehicleAnimationTrigger);
                    }
                    else {
                        bCam.TriggerVehicleInteriorAnimation("");
                    }

                    // Display info panel for ID holder (based on ID type)
                    // Vaultable objects need the scanning entity passed into the DisplayID method--the vault command is only displayed when in range to vault
                    if (scanObject.GetType() == typeof(IDVaultObject) && camMaster.rightEyeLodged && bCam.bodyControl) {
                        IDVaultObject vaultID = (IDVaultObject)scanObject;

                        vaultID.DisplayID(hostID);
                    }
                    else {
                        scanObject.DisplayID();
                    }

                    previousObject = targetTran;
                    previousTargetedID = scanObject;

                    //if (Input.GetButton("Square Button") && !camMaster.gamePaused) {
                    //    if (!scanObject.KnowDescription) {
                    //        if ((((Time.time - scanStart) * 40) > hit.collider.bounds.size.magnitude)) {
                    //            scanObject.KnowDescription = true;
                    //        }
                    //    }
                    //}
                } else {

                    if (previousTargetedID != null) { previousTargetedID.ClearSwitchColliders(); }
                    
                    scanObject = null;

                    //if (!toolSelect.toolSelectorOpen) {
                    //    scanner.DisplayTextUsingModeColor(scanner.displayModeText);
                    //}

                    scanStart = Time.time;
                    scanner.DisableInfoPanel();

                    if (previousTargetedID != null && previousTargetedID.GetType() == typeof(IDCharacter)) {
                        IDCharacter idChar = (IDCharacter)previousTargetedID;
                        foreach (Transform mark in idChar.markers) { mark.GetComponent<MeshRenderer>().enabled = false; }
                    }

                    bCam.TriggerVehicleInteriorAnimation("");
                }

            } else {

                if (previousTargetedID != null && previousTargetedID.GetType() == typeof(IDInteractable)) {
                    IDInteractable IDThing = (IDInteractable)previousTargetedID;

                    IDThing.switchColliders.Clear();
                    IDThing.switchColliderWithPointerColliders = false;
                }

                scanObject = null;
                targetedGhost = null;
                targetedSpeech = null;
                targetedAI = null;
                targetedAudio = null;

                // If tool selector isn't up, set the reticle display text to display mode color
                //if (!toolSelect.toolSelectorOpen) {
                //    scanner.DisplayTextUsingModeColor(scanner.displayModeText);
                //}

                // Clear the info box info
                if (!bCam.IsHoldingDocument()) {
                    scanner.DisableInfoPanel();
                }
                scanStart = Time.time;

                // If the previous target was a character, clear its markers
                if (!targetInfoCleared && previousTargetedID != null && previousTargetedID.GetType() == typeof(IDCharacter)) {
                    IDCharacter idChar = (IDCharacter)previousTargetedID;
                    foreach (Transform mark in idChar.markers) { mark.GetComponent<MeshRenderer>().enabled = false; }
                    targetInfoCleared = true;
                }

                bCam.TriggerVehicleInteriorAnimation("");
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
                    //sixDOFIndicator.enabled = true;

                } else {                                                                                            //          If not in 6dof mode...
                    this.transform.Rotate(rotX * rightEyeLookSens, 0, 0);                                           //              Rotation, vertical, why separate??              !!!
                    this.transform.Rotate(0, rotY * rightEyeLookSens, 0, Space.World);                              //              Rotation, horizontal, why separate??            !!!

                    this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x,
                                                                  this.transform.localEulerAngles.y,
                                                                  0);                                           //          If not in 6dof mode, zero loc z-rot
                    //sixDOFIndicator.enabled = false;
                }
            } //else {
                //if (tedTrack) { 
                //    this.transform.LookAt(ted.transform.position + new Vector3(0, 55, 0));                          //          Eye rotation follows Ted (origin + 55 y)
                //}
            //}                                                                                                       //      If view is locked...
        }
    }

    public void RightEyeAbilitiesAreGo(bool yesOrNo) {
        rightEyeAbilitiesAvailable = yesOrNo;
    }

    public ID RightEyeTargetID() {
        return scanObject;
    }

    public Ghost RightEyeTargetGhost() {
        return targetedGhost;
    }

    public TextAndSpeech RightEyeTargetSpeech() {
        return targetedSpeech;
    }

    public NPCIntelligence RightEyeTargetAI() {
        return targetedAI;
    }

    public AudioSource RightEyeTargetAudio() {
        return targetedAudio;
    }
}
