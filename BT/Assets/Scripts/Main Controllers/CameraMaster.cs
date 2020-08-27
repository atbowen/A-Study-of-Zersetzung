using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMaster : MonoBehaviour {

    // Pause parameters
    public bool gamePaused;
    public RawImage pauseScreen;

    // Head, HUD, and camera stuff
    public RenderTexture staticView, loadingView, leftEyeRT, rightEyeRT, headdActualRT, headdRefRT;
    public RawImage mainOverlay, topRightInsert, bothEyesScreen, hallucinationScreen;                                        // mainOverlay image is for full screen projections; insert is for picture-in-a-picture
    public Texture[] hallucinationFrames;
    public RawImage hallucinatingTextBgd;
    public Text hallucinatingText;
    public float hallucinationFrameTime, hallucinationScreenAlphaDecrement;
    public Camera bodyCamera;
    public Transform headActual, headNoBob;
    public Transform headObjectHavingMesh;
    public enum ViewMode { Std, XRay, OffAir, Loading };                                              // Std = normal screen, XRay = wireframe, OffAir = static, Adjusting = loading screen
    public bool leftEyeLodged, rightEyeLodged;                                                          // Checks for eyes in place
    public bool rightEyeAbilitiesAvailable, reticleEnabled;

    public bool jammed, hallucinating;

    public bool toolSelectorOpen;

    public bool atTedsRoom;

    // Comms
    public bool commsEnabled;
    public InputField commsEnter;
    public Font englishFont, solarFont;
    public bool englishOrSolar;

    // Public use timers
    public bool leftEyeAvailable;
    public float keyTimer, keyTimerRef;
    public bool triggerPressed;

    // Look at all of this crap vvv  This game is going to be great.
    private BodyCam view;
    private Teddy ted;
    private TeddyHead tedHead;
    private TeddyLeftEye leftEye;
    private TeddyRightEye rightEye;
    private WorkDesk wkDesk;
    private Transform leftEyepatch, rightEyepatch;
    private Animator anim, animHead;
    private Rigidbody rigid, rightEyeRigid;
    private Transform headd;
    private RenderTexture headdRT;
    private CommsController commsControl;
    private ToolSelector toolSelect;
    private MusicPlayer musicBox;                                                                
    private Collider rightEyeColl;
    private RawImage reticleImg;
    private RawImage commsImg;
    private StatusPopup statusWindow;

    private enum ViewStates { Other, BodyRightEyeIn, BodyRightEyeOut, RightEye, RightEyeDislodging, RightEyeLodging }
    private ViewStates CamState;

    private float startStatic, staticTime, clearStatic, clearTime;                                      
    private Vector3 initialPosBody, initialPosLeft, initialPosRight;                                    
    private Quaternion returnRotLeft, returnRotRight, initialRotBody;                                   

    private int currentJamFrameIndex;
    private float jamFrameTimeIndex;

    private int currentHallucinationFrameIndex;
    private float hallucinationFrameTimeIndex;
    private string stringOfAlphas, hallucinatingNonRandomText;
    private bool hallucinatingUnscrambledText;

    private float reticlePressTime;
    private bool reticlePressed, reticleHidden;
    private float eyeDislodgeTimer, eyeDislodgeTimerRef;

    // Use this for initialization
    void Start () {
        
        // Ted stuff
        ted = FindObjectOfType<Teddy>();
        anim = ted.GetComponent<Animator>();
        rigid = ted.GetComponent<Rigidbody>();
        tedHead = FindObjectOfType<TeddyHead>();
        animHead = tedHead.GetComponent<Animator>();
        view = FindObjectOfType<BodyCam>();        
        rightEye = FindObjectOfType<TeddyRightEye>();
        rightEyeColl = rightEye.GetComponent<Collider>();
        rightEyeRigid = rightEye.GetComponent<Rigidbody>();
        leftEye = FindObjectOfType<TeddyLeftEye>();

        // Do I need these???
        headdRT = headdActualRT;
        headd = headActual;

        // HUD stuff
        wkDesk = FindObjectOfType<WorkDesk>();
        reticleImg = GameObject.Find("Reticle").GetComponent<RawImage>();
        commsImg = commsEnter.transform.parent.GetComponent<RawImage>();
        commsControl = FindObjectOfType<CommsController>();
        toolSelect = FindObjectOfType<ToolSelector>();
        statusWindow = FindObjectOfType<StatusPopup>();

        // Music player
        musicBox = FindObjectOfType<MusicPlayer>();        

        // Game not paused at start
        gamePaused = false;

        // Main overlay effects and picture-in-picture off at start
        mainOverlay.enabled = false;
        topRightInsert.enabled = false;

        // No right eye collision to start
        RightEyeCollisions(false);

        // Eyes lodged at start
        leftEyeLodged = true;                                                                           
        rightEyeLodged = true;

        // Jamming overlay (not jammed by default)
        jammed = false;                                                                                 
        currentJamFrameIndex = 0;                                                                       
        jamFrameTimeIndex = 0;                                                                          

        // Hallucination settings and text (not hallucinating by default)
        hallucinating = false;                                                                          
        hallucinatingUnscrambledText = false;
        currentHallucinationFrameIndex = 0;                                                             
        hallucinationFrameTimeIndex = 0;                                                                
        stringOfAlphas = "abcdefghijklmnopqrstuvwxyz~~~~~~////\\\\\\---------......";
        
        // Comms (off at start)
        commsEnabled = false;
        commsEnter.DeactivateInputField();
        commsEnter.GetComponent<Image>().enabled = false;

        // Misc settings
        reticleEnabled = true;
        atTedsRoom = false;
        englishOrSolar = true;
        leftEyeAvailable = false;
        toolSelect.toolSelectorOpen = false;
        toolSelectorOpen = false;

        // Keypress timers
        eyeDislodgeTimer = 0.2f;
        eyeDislodgeTimerRef = 0;
        reticlePressTime = 0;
        reticlePressed = false;
        //keyTimer = 0.1f;
        keyTimerRef = 0;

        // Establish reference positions and rotations
        initialPosBody = view.transform.localPosition;
        initialPosLeft = leftEye.transform.localPosition;
        initialPosRight = rightEye.transform.localPosition;
        initialRotBody = view.transform.localRotation;
        returnRotLeft = leftEye.transform.localRotation;
        returnRotRight = rightEye.transform.localRotation;

        // Set up body camera
        SwitchCam(view.transform, headd, initialPosBody, initialRotBody);
        // Necessary???
        initialRotBody = view.transform.localRotation;
        // Set state
        CamState = ViewStates.BodyRightEyeIn;
    }

    // Update is called once per frame
    void FixedUpdate() {

        // Every current possible state:
        // Controlling body with/without right eye, right eye lodging/dislodging, controlling right eye (and "other" state)
        switch (CamState) {
            case ViewStates.Other:
                break;
            case ViewStates.BodyRightEyeIn:
                if (!view.Using()) {
                    if (Input.GetButtonDown("X Button") && (Time.time - keyTimerRef > keyTimer) && !commsEnabled) {
                        view.InitiateUseActionWithAnimationTrigger("Take out eye", 1, false);
                        CamState = ViewStates.RightEyeDislodging;
                    }
                }
                break;
            case ViewStates.BodyRightEyeOut:
                if (!view.Using()) {
                    if (Input.GetButtonDown("X Button") && (Time.time - keyTimerRef > keyTimer)
                            && (headd.transform.position - rightEye.transform.position).magnitude < 20 && !commsEnabled) {
                        view.InitiateUseActionWithAnimationTrigger("Take out eye", 1, false);
                        CamState = ViewStates.RightEyeLodging;
                    }
                }
                break;
            case ViewStates.RightEye:
                break;
            case ViewStates.RightEyeDislodging:
                if (view.GetUseFreezeTimeRemaining() < 0.6f) {
                    DislodgeRightEye();
                    CamState = ViewStates.BodyRightEyeOut;
                }
                break;
            case ViewStates.RightEyeLodging:
                if (view.GetUseFreezeTimeRemaining() < 0.8f) {
                    LodgeRightEye();
                    CamState = ViewStates.BodyRightEyeIn;
                }
                break;
        }
        
        // Check for pause status
        // Game can be paused at any time
        WhenGameIsPaused();

        if (Input.GetButtonDown("Start") && (Time.time - keyTimerRef > keyTimer)) {                             // Toggle pause menu (SPRING)
            gamePaused = !gamePaused;
            keyTimerRef = Time.time;                                                                            //
            if (gamePaused) { wkDesk.CallDeskOpenSound(); }
            else            { wkDesk.CallDeskCloseSound(); }

        }                                                                                                       //

        //(((Input.GetAxis("Triggers") < -0.1) || Input.GetKeyDown(KeyCode.Mouse1)) && Time.time - keyTimerRef > keyTimer && !triggerPressed)
        //((Input.GetButton("Square Button")) && Time.time - keyTimerRef > keyTimer)

        // When game isn't paused
        if (!gamePaused) {
            
            // Toggle comms
            if ((Input.GetButton("Square Button")) && Time.time - keyTimerRef > keyTimer && !triggerPressed) {
                commsControl.textActivated = !commsControl.textActivated;
                commsEnabled = !commsEnabled;
                if (commsEnabled) { view.SwitchAnimationStateToIdle(); }
                keyTimerRef = Time.time;                                                                                       
            }

            // When comms aren't up
            if (!commsEnabled) {

                if ((rightEye.rightEyeActive || (rightEyeLodged && view.bodyControl)) && Input.GetButtonDown("Triangle Button") && !gamePaused && !commsEnabled
                            && (Time.time - reticlePressTime > 0.5)) {                                                                          //      Light toggle (C)
                    rightEye.rightLight.enabled = !rightEye.rightLight.enabled;
                    reticlePressTime = Time.time;
                }

                if (view.bodyControl && view.bodyCamActive && ((Input.GetButton("Left Bumper")) || Input.GetKey(KeyCode.Mouse1))) {
                    toolSelectorOpen = true;
                    toolSelect.toolSelectorOpen = true;

                    statusWindow.FlashStatusText("Tool  Selector  open.");
                } else {
                    toolSelectorOpen = false;
                    toolSelect.toolSelectorOpen = false;
                }
                
                // Check if hallucinating
                WhatToDoIfHallucinating();

                if (((Time.time - reticlePressTime > 0.5) && ((Input.GetAxis("D-Pad Left Right") > 0)) ||       // Toggle hallucinations on and off
                        ((Time.time - reticlePressTime > 0.1) && Input.GetButtonDown("Hallucinations")))) {     //                                                     

                    hallucinating = !hallucinating;                                                             //
                    hallucinationFrameTimeIndex = Time.time;                                                    //
                    reticlePressTime = Time.time;                                                               //
                }                                                                                               //

                // Check if right eye abilities are available
                CheckIfRightEyePowersAreAvailable();

                if (!reticleHidden && ((Time.time - reticlePressTime > 0.5) && ((Input.GetAxis("D-Pad Left Right") < 0)) ||       // Toggle reticle on and off (if right eye powers are available)
                    ((Time.time - reticlePressTime > 0.1) && Input.GetButtonDown("Reticle")))) {                //
                    ToggleReticle();                                                                            //
                    reticlePressTime = Time.time;                                                               //
                }                                                                                               //

                // If both eyes are in
                if (leftEyeLodged && rightEyeLodged) {
                    topRightInsert.enabled = false;                                                             // No picture in picture                          
                    bothEyesScreen.enabled = true;                                                              // Screen overlay for both eyes in is enabled

                    view.bodyCamActive = true;                                                                  // Main camera is displaying from Ted's perspective
                    view.bodyControl = true;                                                                    // Player can move Ted

                    // Dislodge right eye
                    //if (Input.GetButtonDown("X Button") && (Time.time - keyTimerRef > keyTimer)) {
                    //    view.InitiateUseActionWithAnimationTrigger("Take out eye", 1);
                    //}   
                // If the eyes are not both lodged
                } else {
                    // If the player is controlling the right eye
                    if (rightEye.rightEyeActive) {
                        bothEyesScreen.enabled = false;                                                         // Both eyes screen overlay effect is disabled
                        view.bodyCamActive = false;                                                             // Main camera not displaying Ted's perspective

                        // Toggle Ted tracking or 6DOF
                        if (Input.GetButtonDown("Left Bumper") && (Time.time - keyTimerRef > keyTimer)) {       
                            ToggleTedTrackOr6DOF();                                                             
                            keyTimerRef = Time.time;                                                            
                        }                                                                                       

                        // Toggle right eye TPP
                        if (Input.GetButtonDown("Right Bumper") && (Time.time - keyTimerRef > keyTimer)) {                                                                                                                         //                            
                            ToggleRightEyeTPP();                                                                
                            keyTimerRef = Time.time;                                                            
                        }                                                                                       

                        // Switch perspective
                        if (Input.GetButtonDown("Circle Button") && (Time.time - keyTimerRef > keyTimer)) {


                            SwitchCam(view.transform, headd, initialPosBody,
                                leftEye.transform.rotation * Quaternion.AngleAxis(0, Vector3.up));                 // Make main camera child of left eye (current rotation)

                            //if (leftEyeLodged) {                                                              // ****** Applicable if left eye functionality is implemented
                            //    view.bodyControl = true;                                                      //
                            //    view.bodyCamActive = true;                                                    //
                            //} else {                                                                          //
                            //    view.bodyControl = false;                                                     //
                            //    leftEye.leftEyeActive = true;                                                 //
                            //}                                                                                 //

                            topRightInsert.texture = rightEyeRT;                                                // Picture in picture is now right eye's perspective
                            rightEye.rightEyeActive = false;                                                    // Right eye not in control
                            view.bodyCamActive = true;                                                          // Make main camera display Ted's perspective
                            view.bodyControl = true;                                                            // Player controls Ted

                            keyTimerRef = Time.time;
                        }
                    // If right eye is out but not active, and left eye is in place
                    } else if (!rightEye.rightEyeActive && leftEyeLodged) {
                        

                        if (Input.GetButtonDown("Circle Button") && (Time.time - keyTimerRef > keyTimer)) {
                            if (rightEye.rightEyeLock) {
                                view.SwitchAnimationStateToIdle();
                                view.bodyControl = true;
                            } else {
                                view.bodyControl = false;
                            }
                            rightEye.rightEyeActive = true;
                            
                            topRightInsert.texture = leftEyeRT;                                                     // Picture in picture shows left eye perspective
                            view.GetComponent<Camera>().nearClipPlane = 0.3f;                                       // Clipping in 
                            bothEyesScreen.enabled = false;                                                         // Both eyes screen overlay effect disabled

                            SwitchCam(view.transform, rightEye.transform,
                                        Vector3.zero, Quaternion.identity);

                            keyTimerRef = Time.time;

                        }
                    }

                    if (Input.GetButtonDown("X Button") && (Time.time - keyTimerRef > keyTimer)) {
                        leftEye.leftEyeActive = false;

                        if (rightEyeLodged) {

                            rightEyeLodged = false;
                            rightEye.transform.parent = null;
                            rightEye.transform.Translate(0, 0, 5f, Space.Self);

                            SwitchCam(view.transform, rightEye.transform,
                                      Vector3.zero, Quaternion.identity);                   // Make body camera child of right eye                     

                            rightEye.transform.localRotation = returnRotRight;
                            leftEye.transform.localRotation = returnRotLeft;
                            view.transform.localRotation = initialRotBody;
                            view.curRotX = 0;

                            RightEyeCollisions(true);
                            view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                            view.bodyControl = false;                                       // Disable body controls

                            rightEye.rightEyeActive = true;                                 // Enable right eye controls
                            rightEye.rightEyeLock = false;

                            topRightInsert.texture = headdRT;

                        } else if (!rightEyeLodged) {

                            //if ((headd.transform.position - rightEye.transform.position).magnitude < 20) {

                            //    RightEyeCollisions(false);
                            //    rightEyeLodged = true;

                            //    if (leftEyeLodged) {
                            //        SwitchCam(view.transform, headd, initialPosBody, initialRotBody);
                            //        view.bodyCamActive = true;
                            //    }

                            //    view.bodyControl = true;
                            //    rightEye.rightEyeActive = false;

                            //    rightEye.transform.parent = null;
                            //    rightEye.transform.parent = headd.transform;
                            //    rightEye.transform.localPosition = initialPosRight;
                            //    rightEye.transform.localRotation = returnRotRight;

                            //    view.curRotX = 0;

                            //    if (leftEyeLodged) {
                            //        leftEye.transform.localRotation = returnRotLeft;
                            //    }
                            //}
                        }

                        keyTimerRef = Time.time;
                    }
                }
            // If comms are up
            } else {
                if (Input.GetKeyDown(KeyCode.RightAlt)) {                                                   // Toggle comms language
                    if (englishOrSolar) { commsEnter.textComponent.font = solarFont; }                      //
                    else                { commsEnter.textComponent.font = englishFont; }                    //
                                                                                                            //
                    commsEnter.textComponent.text = "";                                                     //
                    englishOrSolar = !englishOrSolar;                                                       //
                }
            }
        }

        if (Mathf.Abs(Input.GetAxis("Triggers")) > 0.1 || Input.GetButton("Square Button")) {
            triggerPressed = true;
        } else { triggerPressed = false; }
    }

    //**
    //***
    //FUNCTIONS
    //***
    //**

    public void SwitchMode(Camera cam, ViewMode mode) {         // Switch camera modes (overlays, x-ray stuff, etc.)
        // X-Ray mode (not implemented)
        if (mode == ViewMode.XRay) {
            //EnableXRayView();                                 
        // Off-air or static mode
        } else if (mode == ViewMode.OffAir) {                   
            cam.targetTexture = staticView;                     // Show static effect
        // Loading mode
        } else if (mode == ViewMode.Loading) {                  
            cam.targetTexture = loadingView;                    // Show loading screen
        // Normal view mode
        } else if (mode == ViewMode.Std) { 
            mainOverlay.enabled = false;
        }
    }

    public void OverrideReticle(bool yOrN) {
        if (yOrN) {
            reticleHidden = true;
            reticleEnabled = false;
        } else {
            reticleHidden = false;
            reticleEnabled = true;
        }
    }
    
    // Necessary effects when game is or is not paused
    private void WhenGameIsPaused() {
        // If paused...
        if (gamePaused) {
            rigid.constraints = RigidbodyConstraints.FreezeAll;             // Enable rigidbody constraints (just the position ones, really)
            anim.speed = 0;                                                 // Freeze Ted's animations
            animHead.speed = 0;                                             //
            pauseScreen.enabled = true;                                     // Enable pause screen
            wkDesk.deskEnabled = true;                                      // Open SPRING
            wkDesk.openDesk = true;                                         //
        } else {
            rigid.constraints &= ~RigidbodyConstraints.FreezePosition;      // Disable position constraints
            pauseScreen.enabled = false;                                    // Disable pause screen
            wkDesk.deskEnabled = false;                                     // Close SPRING
            wkDesk.closeDesk = true;                                        //
        }
    }

    // Dislodge right eye
    private void DislodgeRightEye() {
        view.SwitchAnimationStateToIdle();                                                      // Revert to Ted's idle animation

        topRightInsert.enabled = true;                                                          // Enable picture in picture, Ted's perspective
        topRightInsert.texture = headdRT;                                                       //

        view.bodyCamActive = false;                                                             // Main camera is not displaying from Ted's perspective
        view.bodyControl = false;                                                               // Player cannot move Ted
        leftEye.transform.localRotation = returnRotLeft;                                        // Return left eye to initial rotation

        rightEye.rightEyeActive = true;                                                         // Player is controlling right eye
        rightEye.rightEyeLock = false;                                                          // Right eye is not locked to location
        rightEyeLodged = false;                                                                 // Right eye is no longer in place
        rightEye.transform.parent = null;                                                       // Right eye is unparented
        rightEye.transform.Translate(0, 0, 10f, Space.Self);                                    // Right eye moves forward, out of socket
        SwitchCam(view.transform, rightEye.transform,                                           // Make main camera child of right eye, initialize pos and rot
                      Vector3.zero, Quaternion.identity);                                       //
        view.curRotX = 0;                                                                       // Zero calculation of main camera's verticle rotation
        RightEyeCollisions(true);                                                               // Enable right eye collider                        
        keyTimerRef = Time.time;
    }

    // Lodge right eye
    private void LodgeRightEye() {
        
        RightEyeCollisions(false);
        rightEyeLodged = true;

        if (leftEyeLodged) {
            SwitchCam(view.transform, headd, initialPosBody, initialRotBody);
            view.bodyCamActive = true;
        }

        view.bodyControl = true;
        rightEye.rightEyeActive = false;

        rightEye.transform.parent = null;
        rightEye.transform.parent = headd.transform;
        rightEye.transform.localPosition = initialPosRight;
        rightEye.transform.localRotation = returnRotRight;

        view.curRotX = 0;

        if (leftEyeLodged) {
            leftEye.transform.localRotation = returnRotLeft;
        }       
    }

    // Switch perspective by making main camera a child of target object; initialize orientation
    public void SwitchCam(Transform cam, Transform target, Vector3 finalPos, Quaternion finalRot) {
        cam.parent = null;                                                                              // De-parent
        cam.parent = target;                                                                            // Make child of target

        cam.localPosition = finalPos;                                                                   // Initialize position
        cam.localRotation = finalRot;                                                                   // Initialize rotation
    }

    // Toggle between controlling right eye, and locking it in place and controlling Ted
    private void ToggleRightEyeTPP() {
        if (rightEye.rightEyeLock) {
            if (!reticleHidden) {
                reticleEnabled = true;
                reticleImg.enabled = true;
            }
            view.bodyControl = false;

            view.SwitchAnimationStateToIdle();
        } else if (!rightEye.rightEyeLock) {
            if (!reticleHidden) {
                reticleEnabled = false;
                reticleImg.enabled = false;
            }
            view.bodyControl = true;
        }

        rightEye.rightEyeLock = !rightEye.rightEyeLock;
        rightEye.tedTrack = false;
    }


    private void WhatToDoIfHallucinating() {                                                                                    // If hallucinating, show camera effect
        if (hallucinating) {                                                                                                    //
            hallucinationScreen.enabled = true;                                                                                 //
            hallucinatingText.enabled = true;
            hallucinatingTextBgd.enabled = true;
            
            hallucinationScreen.texture = hallucinationFrames[currentHallucinationFrameIndex];                                  //
                                                                                                                                //
            if ((Time.time - hallucinationFrameTimeIndex) > hallucinationFrameTime) {                                                              //
                currentHallucinationFrameIndex = (currentHallucinationFrameIndex + 1) % hallucinationFrames.Length;             //
                hallucinationFrameTimeIndex = Time.time;                                                                        //
                Color imageColor = hallucinationScreen.color;
                imageColor.a = hallucinationScreen.color.a - (hallucinationScreenAlphaDecrement * hallucinationScreen.color.a);
                if (imageColor.a > 0.2) {
                    hallucinationScreen.color = imageColor;
                }
                if (hallucinatingUnscrambledText) { hallucinatingText.text = hallucinatingNonRandomText; } 
                else { hallucinatingText.text = GenerateHallucinationText(); }
            }                                                                                                                   //
                                                                                                                                //
        } else {                                                                                                                //
            hallucinationScreen.enabled = false;                                                                                //
            hallucinatingText.enabled = false;
            hallucinatingTextBgd.enabled = false;

            Color newColor = hallucinationScreen.color;
            newColor.a = 1;
            hallucinationScreen.color = newColor;
        }                                                                                                                       //
    }                                                                                                                           //

    private string GenerateHallucinationText() {
        char c1 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];
        char c2 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];
        char c3 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];
        char c4 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];
        char c5 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];
        char c6 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];
        char c7 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];
        char c8 = stringOfAlphas[Random.Range(0, stringOfAlphas.Length - 1)];

        char[] combinedChars = new char[8];
        combinedChars[0] = c1;
        combinedChars[1] = c2;
        combinedChars[2] = c3;
        combinedChars[3] = c4;
        combinedChars[4] = c5;
        combinedChars[5] = c6;
        combinedChars[6] = c7;
        combinedChars[7] = c8;
        return new string(combinedChars);
    }

    public void SetHallucinationTextToStringInsteadOfRandom(string hallucinationTextString) {
        hallucinatingUnscrambledText = true;
        hallucinatingNonRandomText = hallucinationTextString;
    }

    public void SetHallucinatingTextToRandom() {
        hallucinatingUnscrambledText = false;
    }

    private void CheckIfRightEyePowersAreAvailable() {
        if (rightEye.rightEyeActive || (rightEyeLodged && view.bodyCamActive)) {
            rightEyeAbilitiesAvailable = true;
        } else {
            rightEyeAbilitiesAvailable = false;
        }
    }

    private void ToggleReticle() {
        if (rightEyeAbilitiesAvailable) {
            reticleEnabled = !reticleEnabled;
            if (reticleEnabled) {
                reticleImg.enabled = true;
            } else reticleImg.enabled = false;
        } else {
            reticleEnabled = false;
            reticleImg.enabled = false;
        }
    }

    private void RightEyeCollisions(bool collisionsTorF) {
        rightEyeColl.enabled = collisionsTorF;
        rightEyeRigid.isKinematic = !collisionsTorF;
    }

    private void ToggleTedTrackOr6DOF() {
        if (rightEye.rightEyeLock)  { rightEye.tedTrack = !rightEye.tedTrack; } 
        else                        { rightEye.sixDOF = !rightEye.sixDOF; }
    }
}
