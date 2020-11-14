using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMaster : MonoBehaviour {

    // Pause parameters
    public bool gamePaused;
    public RawImage pauseScreen;

    // Head, HUD, and camera stuff
    public RenderTexture staticView, loadingView, leftEyeRT, rightEyeRT, headdActualRT, headdRefRT, blackAndWhiteRT;
    public RawImage mainOverlay, topRightInsert, bothEyesScreen, hallucinationScreen;                                        // mainOverlay image is for full screen projections; insert is for picture-in-a-picture
    public Texture[] hallucinationFrames;
    public RawImage hallucinatingTextBgd;
    public List<RawImage> gardenOverlays;
    public List<Camera> gardenOverlayCameras;
    public RawImage vawnTripOverlay;
    public Camera vawnTripCamera;

    // RawImages, etc. whose alphas need to be adjusted for the Garden overlays
    [SerializeField]
    public List<ImageAndAlpha> imagesWithDependentAlpha;
    public float alphaScalarForGardenOverlays;

    public Text hallucinatingText;
    public float hallucinationFrameTime, hallucinationScreenAlphaDecrement;
    public float gardenOverlayFOVShiftMinTime, gardenOverlayFOVShiftMaxTime, gardenOverlayFOVShiftMin, gardenOverlayFOVShiftMax, gardenaCamFOVProximityThreshold, gardenCamFOVShiftRate;
    public float vawnTripRotRandMin, vawnTripRotRandMax, vawnTripRotTimeRandMin, vawnTripRotTimeRandMax, vawnTripRotRate,
                vawnTripFOVStretchRandMin, vawnTripFOVStretchRandMax, vawnTripFOVStretchTimeRandMin, vawnTripFOVStretchTimeRandMax, vawnTripFOVStretchRate,
                vawnTripSecondPulseTime, vawnTripThirdPulseTime;
    public AudioClip heartbeatSound;
    public Camera bodyCamera;
    public Transform headActual, headNoBob;
    public Transform headObjectHavingMesh;
    public enum ViewMode { Std, XRay, OffAir, Loading };                                              // Std = normal screen, XRay = wireframe, OffAir = static, Adjusting = loading screen
    public bool leftEyeLodged, rightEyeLodged;                                                          // Checks for eyes in place
    public bool rightEyeAbilitiesAvailable, reticleEnabled;

    public bool jammed, hallucinating, showGardenOverlays;

    public bool toolSelectorOpen;

    public bool atTedsRoom;

    // Comms
    public bool commsEnabled;
    public InputField commsEnter;
    public Font englishFont, solarFont;
    public bool englishOrSolar;

    // Public use timers
    public bool leftEyeAvailable;
    public float commsWindowSpecialOptionsButtonHoldTime, commsWindowButtonHoldRefTime;
    public float keyTimer, keyTimerRef;
    public bool triggerPressed;

    // Eye effects
    public AudioClip rightEyeDislodgeSound, rightEyeLodgeSound;
    public RawImage rightEyeGridlinesBgd;
    public List<Texture> eyeDislodgeGridlineFrames;
    public float eyeGridlinesFrameSpeed;
    public int numberOfShutterFramesBeforeGridlines;

    // Eye light effects
    public AudioClip rightEyeLightSwitchOn, rightEyeLightSwitchOff;

    // Look at all of this crap vvv  This game is going to be great. :) Love this comment.  It's 10.4.2020, game's been in dev for 5 years and still loving it.  We're gonna finish this!
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

    private AudioSource rightEyeAudio;

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
    private bool reticleReleased, reticleHidden;

    private bool showingEyeGridlines;
    private float eyeGridlinesStartTimeRef;
    private int currentEyeGridlineFrame;

    private float gardenOverlayFOVShiftRefTime, gardenOverlayFOVShiftRandomTime;

    private float vawnTripRandRot, vawnTripRotRefTime, vawnTripRotRandTime,
                vawnTripRandFOVStretch, vawnTripFOVStretchRefTime, vawnTripFOVStretchRandTime, vawnTripCurrentTargetFOV;
    private bool vawnTripIsRotNeg, vawnTripIsRotPos, vawnTripIsStretchNeg, vawnTripIsStretchPos, vawnTripNegRotFirst, vawnTripNegFOVStretchFirst;
    private float[] vawnTripPulseTimes = new float[3];
    private int currentvawnTripPulseNumber;

    private bool commsSpecialOptionsMenuOpened;

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

        rightEyeAudio = rightEye.GetComponent<AudioSource>();

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

        // Hallucination bgd and eye gridlines off at start
        hallucinationScreen.enabled = false;
        rightEyeGridlinesBgd.enabled = false;

        // No right eye collision to start
        RightEyeCollisions(false);

        // Eyes lodged at start
        leftEyeLodged = true;                                                                           
        rightEyeLodged = true;

        // Jamming overlay (not jammed by default)
        jammed = false;                                                                                 
        currentJamFrameIndex = 0;                                                                       
        jamFrameTimeIndex = 0;

        // Better hallucination/Garden effect than below
        showGardenOverlays = false;
        gardenOverlayFOVShiftRefTime = Time.time;
        gardenOverlayFOVShiftRandomTime = Random.Range(gardenOverlayFOVShiftMinTime, gardenOverlayFOVShiftMaxTime);

        if (imagesWithDependentAlpha.Count > 0) {
            foreach (ImageAndAlpha img in imagesWithDependentAlpha) {
                img.IntializeOriginalAlpha();
                Debug.Log(img.image.color.a + ", " + img.GetOriginalAlpha());
            }
        }

        // Hallucination settings and text (not hallucinating by default)
        hallucinating = false;
        vawnTripOverlay.enabled = false;
        if (vawnTripFOVStretchTimeRandMin != 0 && vawnTripFOVStretchTimeRandMax != 0 && vawnTripSecondPulseTime != 0 && vawnTripThirdPulseTime != 0) {
            vawnTripPulseTimes[0] = Random.Range(vawnTripFOVStretchTimeRandMin, vawnTripFOVStretchTimeRandMax);
            vawnTripPulseTimes[1] = vawnTripSecondPulseTime;
            vawnTripPulseTimes[2] = vawnTripThirdPulseTime;
        }

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
        reticlePressTime = 0;
        reticleReleased = false;
        eyeGridlinesStartTimeRef = 0;
        showingEyeGridlines = false;
        currentEyeGridlineFrame = 0;

        //keyTimer = 0.1f;
        keyTimerRef = 0;

        commsSpecialOptionsMenuOpened = false;

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
        if (!gamePaused) {
            switch (CamState) {
                case ViewStates.Other:
                    break;
                case ViewStates.BodyRightEyeIn:
                    if (showingEyeGridlines) {
                        if (Time.time - eyeGridlinesStartTimeRef > eyeGridlinesFrameSpeed) {
                            if (currentEyeGridlineFrame < numberOfShutterFramesBeforeGridlines - 2) {
                                currentEyeGridlineFrame++;
                                rightEyeGridlinesBgd.texture = eyeDislodgeGridlineFrames[currentEyeGridlineFrame];

                                eyeGridlinesStartTimeRef = Time.time;
                            }
                            else {
                                showingEyeGridlines = false;
                                rightEyeGridlinesBgd.enabled = false;
                            }
                        }
                    }

                    if (!view.Using()) {
                        if (Input.GetButtonDown("X Button") && (Time.time - keyTimerRef > keyTimer) && !commsEnabled) {
                            view.InitiateUseActionWithAnimationTrigger("Take out eye", 1, false);
                            CamState = ViewStates.RightEyeDislodging;
                        }
                    }
                    break;
                case ViewStates.BodyRightEyeOut:
                    if (showingEyeGridlines) {
                        if (Time.time - eyeGridlinesStartTimeRef > eyeGridlinesFrameSpeed) {
                            if (currentEyeGridlineFrame < eyeDislodgeGridlineFrames.Count - 1) {
                                currentEyeGridlineFrame++;
                                rightEyeGridlinesBgd.texture = eyeDislodgeGridlineFrames[currentEyeGridlineFrame];
                                Color newColor = rightEyeGridlinesBgd.color;
                                newColor.a = rightEyeGridlinesBgd.color.a - (hallucinationScreenAlphaDecrement * 0.5f * rightEyeGridlinesBgd.color.a);
                                if (rightEyeGridlinesBgd.color.a > 0.2f && currentEyeGridlineFrame > numberOfShutterFramesBeforeGridlines - 1) {
                                    rightEyeGridlinesBgd.color = newColor;
                                }

                                eyeGridlinesStartTimeRef = Time.time;
                            }
                            else {
                                showingEyeGridlines = false;
                                rightEyeGridlinesBgd.enabled = false;
                            }
                        }
                    }

                    if (!view.Using()) {
                        if (Input.GetButtonDown("X Button") && (Time.time - keyTimerRef > keyTimer)
                                && (headd.transform.position - rightEye.transform.position).magnitude < 20 && !commsEnabled) {
                            showingEyeGridlines = false;
                            rightEyeGridlinesBgd.enabled = false;

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
            if ((Input.GetButtonDown("Square Button")) && Time.time - keyTimerRef > keyTimer && !triggerPressed) {
                commsEnabled = !commsEnabled;
                if (commsEnabled) {
                    view.SwitchAnimationStateToIdle();
                    commsControl.OpenCommsWindow();
                }
                else {
                    commsControl.textActivated = false;
                    commsControl.ClearCommsWindow();
                }

                commsWindowButtonHoldRefTime = Time.time;
                keyTimerRef = Time.time;                                                                                       
            }

            if (Input.GetButton("Square Button") && (Time.time - commsWindowButtonHoldRefTime > commsWindowSpecialOptionsButtonHoldTime) && commsEnabled) {
                if (!commsSpecialOptionsMenuOpened) {
                    commsControl.OpenSpecialOptionsMenu();
                    commsSpecialOptionsMenuOpened = true;
                } else {
                    if ((Input.GetAxis("D-Pad Left Right") > 0) && Time.time - keyTimerRef > keyTimer) {
                        commsControl.HighlightNextSpecialOption();
                        keyTimerRef = Time.time;
                    }
                    if ((Input.GetAxis("D-Pad Left Right") < 0) && Time.time - keyTimerRef > keyTimer) {
                        commsControl.HighlightPreviousSpecialOption();
                        keyTimerRef = Time.time;
                    }

                    if (Input.GetAxis("Triggers") > 0.1 && (Time.time - keyTimerRef > keyTimer)) {
                        commsControl.SelectSpecialOption();
                        keyTimerRef = Time.time;
                    }
                }
            }

            if (Input.GetButtonUp("Square Button") && commsEnabled) {
                if (!commsControl.textActivated) {

                    if (Time.time - commsWindowButtonHoldRefTime > commsWindowSpecialOptionsButtonHoldTime) {
                        //commsControl.OpenSpecialOptionsMenu();
                        commsControl.ClearCommsWindow();
                        commsEnabled = false;
                    }
                    else {
                        commsControl.ClearCommsWindow();
                        commsControl.textActivated = true;
                    }
                }
                commsSpecialOptionsMenuOpened = false;
                keyTimerRef = Time.time;
            }

            // Check if hallucinating
            WhatToDoIfHallucinating();

            // When comms aren't up
            if (!commsEnabled) {

                if ((rightEye.rightEyeActive || (rightEyeLodged && view.bodyControl)) && Input.GetButtonDown("Triangle Button") &&
                            (Time.time - reticlePressTime > 0.5)) {                                                                          //      Light toggle (C)
                    if (rightEye.rightLight.enabled)    { rightEyeAudio.PlayOneShot(rightEyeLightSwitchOn); }
                    else                                { rightEyeAudio.PlayOneShot(rightEyeLightSwitchOff); }
                    rightEye.rightLight.enabled = !rightEye.rightLight.enabled;
                    reticlePressTime = Time.time;
                }

                if (view.bodyControl && view.bodyCamActive && !view.InVehicle() && ((Input.GetButton("Left Bumper")) || Input.GetKey(KeyCode.Mouse1))) {
                    toolSelectorOpen = true;
                    toolSelect.toolSelectorOpen = true;

                    //statusWindow.FlashStatusText("Tool  Selector  open.");
                } else {
                    toolSelectorOpen = false;
                    toolSelect.toolSelectorOpen = false;
                }
                
                if (view.bodyControl && view.bodyCamActive && Input.GetButtonDown("Right Bumper") && Time.time - keyTimerRef > keyTimer) {
                    hallucinating = !hallucinating;
                    keyTimerRef = Time.time;

                    InitializeVawnTripRotVariables();
                    InitializeVawnTripStretchVariables();
                }

                

                if (((Time.time - reticlePressTime > 0.5) && ((Input.GetAxis("D-Pad Left Right") > 0)) ||       // Toggle hallucinations on and off
                        ((Time.time - reticlePressTime > 0.1) && Input.GetButtonDown("Hallucinations")))) {     //                                                     

                    //hallucinating = !hallucinating;                                                             //
                    hallucinationFrameTimeIndex = Time.time;                                                    //
                    reticlePressTime = Time.time;                                                               //

                    showGardenOverlays = !showGardenOverlays;
                }                                                                                               //

                // Check if right eye abilities are available
                CheckIfRightEyePowersAreAvailable();

                if (!reticleHidden && !view.IsHoldingDocument() && ((Time.time - reticlePressTime > 0.5) && ((Input.GetAxis("D-Pad Left Right") < 0)) ||       // Toggle reticle on and off (if right eye powers are available)
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

                if (commsControl.ReadyForSelection()) {
                    // Move response selector up, can hold button to scroll
                    if (((Time.time - reticlePressTime > 0.5) || reticleReleased) && ((Input.GetAxis("D-Pad Up Down") > 0)) ||
                            (((Time.time - reticlePressTime > 0.3) || reticleReleased) && (Input.GetAxis("Vertical") > 0))) {
                        reticleReleased = false;
                        commsControl.MoveResponseSelectorBarUp();
                        reticlePressTime = Time.time;
                    }

                    // Move response selector down, can hold button to scroll
                    if (((Time.time - reticlePressTime > 0.5) || reticleReleased) && ((Input.GetAxis("D-Pad Up Down") < 0)) ||
                            (((Time.time - reticlePressTime > 0.3) || reticleReleased) && (Input.GetAxis("Vertical") < 0))) {
                        reticleReleased = false;
                        commsControl.MoveResponseSelectorBarDown();
                        reticlePressTime = Time.time;
                    }

                    // If the axis values are low enough, the reticle button is released and can be pressed again
                    if (Mathf.Abs(Input.GetAxis("D-Pad Up Down")) < 0.001f && Mathf.Abs(Input.GetAxis("Vertical")) < 0.001f) {
                        reticleReleased = true;
                    }

                    if (Time.time - keyTimerRef > keyTimer && Input.GetButtonDown("X Button")) {
                        commsControl.SelectResponse();
                        keyTimerRef = Time.time;
                    }
                }

                // Show full Prompt text before it's fully unravelled by hitting the button
                if (commsControl.CanSkipUnravelling() && Time.time - keyTimerRef > keyTimer && Input.GetButtonDown("X Button")) {
                    commsControl.SkipUnravelling();
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

        // Play the cool dislodging sound
        rightEyeAudio.PlayOneShot(rightEyeDislodgeSound);

        // Initiate shutter and eye gridline frames
        showingEyeGridlines = true;
        currentEyeGridlineFrame = 0;
        eyeGridlinesStartTimeRef = Time.time;
        Color newColor = rightEyeGridlinesBgd.color;
        newColor.a = 1;
        rightEyeGridlinesBgd.color = newColor;
        rightEyeGridlinesBgd.texture = eyeDislodgeGridlineFrames[currentEyeGridlineFrame];
        rightEyeGridlinesBgd.enabled = true;
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

        // Play the cool lodging sound
        rightEyeAudio.PlayOneShot(rightEyeLodgeSound);

        // Initiate shutter frames
        showingEyeGridlines = true;
        currentEyeGridlineFrame = 0;
        eyeGridlinesStartTimeRef = Time.time;
        Color newColor = rightEyeGridlinesBgd.color;
        newColor.a = 1;
        rightEyeGridlinesBgd.color = newColor;
        rightEyeGridlinesBgd.texture = eyeDislodgeGridlineFrames[currentEyeGridlineFrame];
        rightEyeGridlinesBgd.enabled = true;
    }

    // Switch perspective by making main camera a child of target object; initialize orientation
    public void SwitchCam(Transform cam, Transform target, Vector3 finalPos, Quaternion finalRot) {
        //cam.parent.SetParent(null, false);                                                                              // De-parent
        cam.SetParent(target, false);                                                                            // Make child of target

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



        //if (rightEye.RightEyeTargetGhost() != null) { SetHallucinationTextToStringInsteadOfRandom(rightEye.RightEyeTargetGhost().hallucinationText); }
        //else                                        { SetHallucinatingTextToRandom(); }

        if (gardenOverlays.Count > 0) {
            if (showGardenOverlays) {

                //if (imagesWithDependentAlpha.Count > 0) {
                //    foreach (ImageAndAlpha img in imagesWithDependentAlpha) {
                //        img.ScaleOriginalAlpha(alphaScalarForGardenOverlays);
                //    }
                //}

                if (gardenOverlayCameras.Count > 0) {
                    if (Time.time - gardenOverlayFOVShiftRefTime > gardenOverlayFOVShiftRandomTime) {
                        foreach (Camera cam in gardenOverlayCameras) {
                            float randShift = Random.Range(gardenOverlayFOVShiftMin, gardenOverlayFOVShiftMax);
                            if (Random.Range(0, 2) == 0) {
                                randShift = randShift * -1;
                            }

                            cam.fieldOfView = bodyCamera.fieldOfView + randShift;
                            gardenOverlayFOVShiftRandomTime = Random.Range(gardenOverlayFOVShiftMinTime, gardenOverlayFOVShiftMaxTime);
                            gardenOverlayFOVShiftRefTime = Time.time;
                        }
                    }
                }

                foreach (RawImage img in gardenOverlays) {
                    img.enabled = true;
                }               
            }
            else {
                foreach (RawImage img in gardenOverlays) {
                    img.enabled = false;
                }

                if (imagesWithDependentAlpha.Count > 0) {
                    foreach (ImageAndAlpha img in imagesWithDependentAlpha) {
                        img.SetAlphaToOriginal();
                    }
                }
            }

            foreach (Camera cam in gardenOverlayCameras) {
                if (cam.fieldOfView < (bodyCamera.fieldOfView - gardenaCamFOVProximityThreshold)) {
                    cam.fieldOfView += gardenCamFOVShiftRate * Time.deltaTime;
                } else if ( cam.fieldOfView > (bodyCamera.fieldOfView + gardenaCamFOVProximityThreshold)) {
                    cam.fieldOfView -= gardenCamFOVShiftRate * Time.deltaTime;
                }
            }
        }

        if (view.bodyControl && view.bodyCamActive && hallucinating) {
            mainOverlay.enabled = false;
            vawnTripOverlay.enabled = true;

            //if (Time.time - vawnTripRotRefTime > vawnTripRotRandTime) {
            //    if (vawnTripNegRotFirst) {
            //        if (vawnTripIsRotNeg) {
            //            if (vawnTripCamera.transform.localEulerAngles.z > -vawnTripRandRot) { vawnTripCamera.transform.Rotate(0, 0, -vawnTripRotRate * Time.deltaTime); }
            //            else                                                                { vawnTripIsRotNeg = false; }
            //        }
            //        else {
            //            if (vawnTripCamera.transform.localEulerAngles.z < vawnTripRandRot) { vawnTripCamera.transform.Rotate(0, 0, vawnTripRotRate * Time.deltaTime); }
            //            else                                                                { InitializeVawnTripRotVariables(); }
            //        }
            //    }
            //    else {
            //        if (!vawnTripIsRotNeg) {
            //            if (vawnTripCamera.transform.localEulerAngles.z < vawnTripRandRot) { vawnTripCamera.transform.Rotate(0, 0, -vawnTripRotRate * Time.deltaTime); }
            //            else                                                                { vawnTripIsRotNeg = true; }
            //        }
            //        else {
            //            if (vawnTripCamera.transform.localEulerAngles.z > -vawnTripRandRot) { vawnTripCamera.transform.Rotate(0, 0, -vawnTripRotRate * Time.deltaTime); }
            //            else                                                                { InitializeVawnTripRotVariables(); }
            //        }
            //    }
            //}

            if (Time.time - vawnTripFOVStretchRefTime > vawnTripFOVStretchRandTime) {
                if (!vawnTripIsStretchNeg) {
                    if (vawnTripCamera.fieldOfView < (vawnTripCurrentTargetFOV)) {
                        vawnTripCamera.fieldOfView += vawnTripFOVStretchRate * Time.deltaTime;
                    }
                    else {
                        vawnTripIsStretchNeg = true;
                        vawnTripCurrentTargetFOV = vawnTripCamera.fieldOfView - vawnTripRandFOVStretch;
                        musicBox.PlaySFX(heartbeatSound);
                    }
                }
                else {
                    if (vawnTripCamera.fieldOfView > vawnTripCurrentTargetFOV) {
                        vawnTripCamera.fieldOfView -= vawnTripFOVStretchRate * Time.deltaTime;
                    }
                    else {

                        Debug.Log(currentvawnTripPulseNumber + ", " + vawnTripFOVStretchRandTime);

                        switch (currentvawnTripPulseNumber) {
                            case 0:
                                vawnTripRandFOVStretch = Random.Range(vawnTripFOVStretchRandMin, vawnTripFOVStretchRandMax);
                                vawnTripCurrentTargetFOV = vawnTripCamera.fieldOfView + vawnTripRandFOVStretch;
                                vawnTripFOVStretchRandTime = vawnTripSecondPulseTime;
                                currentvawnTripPulseNumber++;
                                break;
                            case 1:
                                vawnTripRandFOVStretch = Random.Range(vawnTripFOVStretchRandMin, vawnTripFOVStretchRandMax);
                                vawnTripCurrentTargetFOV = vawnTripCamera.fieldOfView + vawnTripRandFOVStretch;
                                vawnTripFOVStretchRandTime = vawnTripThirdPulseTime;
                                currentvawnTripPulseNumber++;
                                break;
                            case 2:
                                InitializeVawnTripStretchVariables();
                                break;
                        }

                        vawnTripIsStretchNeg = false;
                        vawnTripFOVStretchRefTime = Time.time;
                    }
                }
            }
        }
        else {
            mainOverlay.enabled = true;
            vawnTripOverlay.enabled = false;
        }

        hallucinationScreen.enabled = false;                                                                                //
        hallucinatingText.enabled = false;
        hallucinatingTextBgd.enabled = false;

        //if (hallucinating) {                                                                                                    //
        //    hallucinationScreen.enabled = true;                                                                                 //
        //    hallucinatingText.enabled = true;
        //    hallucinatingTextBgd.enabled = true;

        //    hallucinationScreen.texture = hallucinationFrames[currentHallucinationFrameIndex];                                  //
        //                                                                                                                        //
        //    if ((Time.time - hallucinationFrameTimeIndex) > hallucinationFrameTime) {                                           //
        //        currentHallucinationFrameIndex = (currentHallucinationFrameIndex + 1) % hallucinationFrames.Length;             //
        //        hallucinationFrameTimeIndex = Time.time;                                                                        //
        //        Color imageColor = hallucinationScreen.color;
        //        imageColor.a = hallucinationScreen.color.a - (hallucinationScreenAlphaDecrement * hallucinationScreen.color.a);
        //        if (imageColor.a > 0.2) {
        //            hallucinationScreen.color = imageColor;
        //        }
        //        if (hallucinatingUnscrambledText) { hallucinatingText.text = hallucinatingNonRandomText; } 
        //        else { hallucinatingText.text = GenerateHallucinationText(); }
        //    }                                                                                                                   //
        //                                                                                                                        //
        //} else {                                                                                                                //
        //    hallucinationScreen.enabled = false;                                                                                //
        //    hallucinatingText.enabled = false;
        //    hallucinatingTextBgd.enabled = false;

        //    Color newColor = hallucinationScreen.color;
        //    newColor.a = 1;
        //    hallucinationScreen.color = newColor;
        //}                                                                                                                       //
    }

    private void InitializeVawnTripRotVariables() {
        if (Random.Range(0, 2) == 0) {
            vawnTripNegRotFirst = true;
            vawnTripIsRotNeg = true;
        }
        else {
            vawnTripNegRotFirst = false;
            vawnTripIsRotNeg = false;
        }

        vawnTripRandRot = Random.Range(vawnTripRotRandMin, vawnTripRotRandMax);
        vawnTripRotRandTime = Random.Range(vawnTripRotTimeRandMin, vawnTripRotTimeRandMax);
        vawnTripRotRefTime = Time.time;
    }

    private void InitializeVawnTripStretchVariables() {
        //if (Random.Range(0, 2) == 0) {
        //    vawnTripNegFOVStretchFirst = true;
        //    vawnTripIsStretchNeg = true;
        //}
        //else {
        //    vawnTripNegFOVStretchFirst = false;
        //    vawnTripIsStretchNeg = false;
        //}

        vawnTripIsStretchNeg = false;
        
        vawnTripRandFOVStretch = Random.Range(vawnTripFOVStretchRandMin, vawnTripFOVStretchRandMax);        
        vawnTripFOVStretchRandTime = Random.Range(vawnTripFOVStretchTimeRandMin, vawnTripFOVStretchTimeRandMax);
        vawnTripPulseTimes[0] = vawnTripFOVStretchRandTime;
        currentvawnTripPulseNumber = 0;
        vawnTripCurrentTargetFOV = vawnTripCamera.fieldOfView + vawnTripRandFOVStretch;
        vawnTripFOVStretchRefTime = Time.time;
    }

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

    // Allows other scripts to reset the keyTimer to avoid double presses
    // This prevents a double press of the X button after choosing an exit line in comms, which would cause the eye to dislodge, for example
    public void ResetKeyTimer() {
        keyTimerRef = Time.time;
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

    [System.Serializable]
    public class ImageAndAlpha {
        public RawImage image;

        [SerializeField]
        private float originalAlpha;     // value is 0 to 1

        public float GetCurrentAlpha() {
            return image.color.a;
        }

        public float GetOriginalAlpha() {
            return originalAlpha;
        }

        public void ScaleOriginalAlpha(float alpha) {
            image.color = new Color(image.color.r, image.color.g, image.color.b, originalAlpha * alpha);
        }

        public void SetAlphaToOriginal() {
            image.color = new Color(image.color.r, image.color.g, image.color.b, originalAlpha);
        }

        public void IntializeOriginalAlpha() {
            if (originalAlpha == 0) {
                originalAlpha = image.color.a;
            }
        }
    }
}
