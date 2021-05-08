using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMaster : MonoBehaviour {

    // Pause parameters
    public bool gamePaused;
    public RawImage pauseScreen;

    // Important references
    [SerializeField]
    private Camera bodyCamera, headCam, rightEyeCam;
    public Transform head, headVision;

    // Head, HUD, and camera stuff
    public RenderTexture staticView, loadingView, leftEyeRT, rightEyeRT, headCamRT;
    public RawImage mainOverlay, topRightInsert, insertFrame, insertReticle, insertFlasher, hallucinationScreen;   // mainOverlay image is main cam; insert is picture-in-a-picture
    public Text FOVText;
    public RawImage hallucinatingTextBgd;
    public List<RawImage> gardenOverlays;
    public List<Camera> gardenOverlayCameras;
    public RawImage vawnTripOverlay;
    public Camera vawnTripCamera;

    public float zoomFactor, scrollWheelSensitivity;
    public Color FOVMainColor, FOVPIPColor;
    public RawImage PIPFOVConnector;
    public List<Texture> PIPFOVConnectorFrames, PIPFOVDisconnectorFrames;
    private bool connectingPIPToFOV, disconnectingPIPFromFOV;
    [SerializeField]
    private float PIPFOVConnectorFrameChangeTime;
    private int PIPFOVConnectorFrameIndex;
    private float PIPFOVConnectorFrameChangeTimeRef;

    // RawImages, etc. whose alphas need to be adjusted for the Garden overlays
    [SerializeField]
    public List<ImageAndAlpha> imagesWithDependentAlpha;
    public float alphaScalarForGardenOverlays;    

    // Hallucination and vawn trip parameters
    public Text hallucinatingText;
    public float hallucinationFrameTime, hallucinationScreenAlphaDecrement;
    public float gardenOverlayFOVShiftMinTime, gardenOverlayFOVShiftMaxTime, gardenOverlayFOVShiftMin, gardenOverlayFOVShiftMax, gardenaCamFOVProximityThreshold, gardenCamFOVShiftRate;
    public float vawnTripRotRandMin, vawnTripRotRandMax, vawnTripRotTimeRandMin, vawnTripRotTimeRandMax, vawnTripRotRate,
                vawnTripFOVStretchRandMin, vawnTripFOVStretchRandMax, vawnTripFOVStretchTimeRandMin, vawnTripFOVStretchTimeRandMax, vawnTripFOVStretchRate,
                vawnTripSecondPulseTime, vawnTripThirdPulseTime;
    public AudioClip heartbeatSound;
    [SerializeField, Range(0.0f, 100.0f)]
    private float vawnTripStandardDosageAmount, vawnPoisonReductionPerHeartBeat;
    
    // Viewing modes--probably not needed
    public enum ViewMode { Std, XRay, OffAir, Loading };                                              // Std = normal screen, XRay = wireframe, OffAir = static, Adjusting = loading screen

    // Important states
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
    public float keyTimer, stardaterWaitTimeBetweenButtonPresses, keyTimerRef;
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
    // Now it's 12.12.2020!  We're going to get the mechanics done by end of year, demo by end of January 2021.  IT'S GOING TO HAPPEN.
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
    private DatingScreen dateScreen;
    private ToolSelector toolSelect;
    private MusicPlayer musicBox;                                                                
    private Collider rightEyeColl;
    private RawImage reticleImg;
    private RawImage commsImg;
    private StatusPopup statusWindow;
    private SapPoisonLevel sapMonitor;

    private Camera activeCam;

    private AudioSource rightEyeAudio;

    private enum ViewStates { Other, BodyRightEyeIn, BodyRightEyeOut, RightEye, RightEyeDislodging, RightEyeLodging }
    private ViewStates CamState;
    
    // Initial positions and rotations
    private Vector3 initialPosBCam, initialPosLeft, initialPosRight;
    private Quaternion returnRotLeft, returnRotRight, initialRotBCam, initialRotHeadVision;

    // Frequency jammed! (not used)
    private int currentJamFrameIndex;
    private float jamFrameTimeIndex;

    // Picture in picture flashing light parameters
    [Range(0.0f, 1.0f), SerializeField]
    private float insertFlasherMinAlpha, insertFlasherMaxAlpha;
    [SerializeField]
    private float insertFlasherIntensityChangeFactor;
    private bool insertFlasherAlphaIsIncreasing;

    // Reticle enabling/disabling
    private float reticlePressTime;
    private bool reticleReleased, reticleHidden;

    // Eye lodging/dislodging graphics
    private bool showingEyeGridlines;
    private float eyeGridlinesStartTimeRef;
    private int currentEyeGridlineFrame;

    // Garden overlay stretching and framing
    private float gardenOverlayFOVShiftRefTime, gardenOverlayFOVShiftRandomTime;
    private int currentHallucinationFrameIndex;
    private float hallucinationFrameTimeIndex;
    private string stringOfAlphas, hallucinatingNonRandomText;
    private bool hallucinatingUnscrambledText;

    // Vawn trip overlay
    private float vawnTripRandRot, vawnTripRotRefTime, vawnTripRotRandTime,
                vawnTripRandFOVStretch, vawnTripFOVStretchRefTime, vawnTripFOVStretchRandTime, vawnTripCurrentTargetFOV;
    private bool vawnTripIsStretchNeg, vawnTripIsStretchPos, vawnTripNegRotFirst, vawnTripNegFOVStretchFirst;
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
        headdRT = headCamRT;
        headd = headVision;

        // HUD stuff
        wkDesk = FindObjectOfType<WorkDesk>();
        reticleImg = GameObject.Find("Reticle").GetComponent<RawImage>();
        commsImg = commsEnter.transform.parent.GetComponent<RawImage>();
        commsControl = FindObjectOfType<CommsController>();
        dateScreen = FindObjectOfType<DatingScreen>();
        toolSelect = FindObjectOfType<ToolSelector>();
        statusWindow = FindObjectOfType<StatusPopup>();
        sapMonitor = FindObjectOfType<SapPoisonLevel>();

        // Music player
        musicBox = FindObjectOfType<MusicPlayer>();

        // The active camera is the one whose field of view can be increased/decreased
        activeCam = bodyCamera;

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

        insertFlasherAlphaIsIncreasing = false;

        // Jamming overlay (not jammed by default)
        jammed = false;                                                                                 
        currentJamFrameIndex = 0;                                                                       
        jamFrameTimeIndex = 0;

        // Better hallucination/Garden effect than below
        showGardenOverlays = false;
        gardenOverlayFOVShiftRefTime = Time.time;
        gardenOverlayFOVShiftRandomTime = Random.Range(gardenOverlayFOVShiftMinTime, gardenOverlayFOVShiftMaxTime);

        // This was intended to adjust the transparency of HUD elements when the Garden overlays are enabled/disabled
        // Original alphas of all of the elements are stored in the imagesWithDependentAlpha list, to be reset to when the overlays are disabled
        //if (imagesWithDependentAlpha.Count > 0) {
        //    foreach (ImageAndAlpha img in imagesWithDependentAlpha) {
        //        img.IntializeOriginalAlpha();
        //        Debug.Log(img.image.color.a + ", " + img.GetOriginalAlpha());
        //    }
        //}

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

        // HUD element effects
        connectingPIPToFOV = false;
        disconnectingPIPFromFOV = false;
        PIPFOVConnector.enabled = false;
        PIPFOVConnectorFrameIndex = 0;
        
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

        // Disable the comms special menu at start
        commsSpecialOptionsMenuOpened = false;

        // Establish reference positions and rotations
        initialPosBCam = view.transform.localPosition;
        initialPosLeft = leftEye.transform.localPosition;
        initialPosRight = rightEye.transform.localPosition;
        initialRotBCam = view.transform.localRotation;
        returnRotLeft = leftEye.transform.localRotation;
        returnRotRight = rightEye.transform.localRotation;

        initialRotHeadVision = headVision.localRotation;

        // Set up body camera
        SwitchCam(view.transform, headd, initialPosBCam, initialRotBCam);
        // Necessary???
        initialRotBCam = view.transform.localRotation;
        // Set state
        CamState = ViewStates.BodyRightEyeIn;

        // For now, let's make right eye abilities always available
        rightEye.RightEyeAbilitiesAreGo(true);
    }

    // Update is called once per frame
    void FixedUpdate() {

        // Picture in picture graphical effects
        PictureInPictureActivationCheck();

        // Sets active camera and displays the current camera's FOV
        FOVSyncer();


        

        // Every current possible state:
        // Controlling body with/without right eye, right eye lodging/dislodging, controlling right eye (and "other" state)
        if (!gamePaused) {
            switch (CamState) {
                case ViewStates.Other:
                    break;
                case ViewStates.BodyRightEyeIn:

                    //rightEye.RightEyeAbilitiesAreGo(true);

                    // No picture-in-picture, bodyCam is the operating camera, and Ted's body is being controlled
                    topRightInsert.enabled = false;
                    view.bodyCamActive = true;
                    view.bodyControl = true;

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
                        if (Input.GetButtonDown("X Button") && (Time.time - view.usingFreezeTimerRef > view.usingFreezeTime) && !commsEnabled) {

                            view.InitiateUseActionWithAnimationTrigger("Take out eye", 1, false);
                            CamState = ViewStates.RightEyeDislodging;

                            headCam.fieldOfView = bodyCamera.fieldOfView;
                        }
                    }
                    break;
                case ViewStates.BodyRightEyeOut:

                    //if (rightEye.rightEyeActive)    { rightEye.RightEyeAbilitiesAreGo(true); }
                    //else                            { rightEye.RightEyeAbilitiesAreGo(false); }

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
                                //rightEyeGridlinesBgd.enabled = false;
                            }
                        }
                    }

                    if (!view.Using()) {
                        if (Input.GetButtonDown("X Button") && (Time.time - view.usingFreezeTimerRef > view.usingFreezeTime)
                                && (headd.transform.position - rightEye.transform.position).magnitude < 20 && !commsEnabled) {
                            showingEyeGridlines = false;
                            rightEyeGridlinesBgd.enabled = false;

                            view.InitiateUseActionWithAnimationTrigger("Take out eye", 1, false);
                            CamState = ViewStates.RightEyeLodging;

                            if (PIPFOVConnector.enabled) {
                                disconnectingPIPFromFOV = true;
                                PIPFOVConnectorFrameIndex = 0;
                                PIPFOVConnectorFrameChangeTimeRef = Time.time;
                            }

                            bodyCamera.fieldOfView = headCam.fieldOfView;
                        }
                    }
                    break;
                case ViewStates.RightEye:
                    break;
                case ViewStates.RightEyeDislodging:
                    // Pop the eye out a little earlier than when the "take out eye" animation ends
                    if (view.GetUseFreezeTimeRemaining() < 0.6f) {
                        DislodgeRightEye();
                        CamState = ViewStates.BodyRightEyeOut;
                    }
                    break;
                case ViewStates.RightEyeLodging:
                    // Pop the eye in a little earlier than when the "take out eye" (but actually lodging) animation ends
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
            if (gamePaused) {
                wkDesk.ShowWorkDesk(true);
                //wkDesk.CallDeskOpenSound();
                // If pulling up the pause/Work Desk screen system and it goes straight to the Desk-Status screen, restart the status text effects
                //FindObjectOfType<DeskScreen>().InitializeStatusTexts();

                bodyCamera.enabled = false;
                rightEyeCam.enabled = false;
            }
            else            { //wkDesk.CallDeskCloseSound(); 
                wkDesk.ShowWorkDesk(false);

                bodyCamera.enabled = true;
                rightEyeCam.enabled = true;
            }

        }                                                                                                       //

        //(((Input.GetAxis("Triggers") < -0.1) || Input.GetKeyDown(KeyCode.Mouse1)) && Time.time - keyTimerRef > keyTimer && !triggerPressed)
        //((Input.GetButton("Square Button")) && Time.time - keyTimerRef > keyTimer)

        // When game isn't paused
        if (!gamePaused) {



            // Toggle comms
            if ((Input.GetButtonDown("Square Button")) && Time.time - keyTimerRef > keyTimer && !triggerPressed) {
                if (!commsEnabled) {
                    view.SwitchAnimationStateToIdle();
                    commsControl.OpenCommsWindow();
                    commsEnabled = true;
                }
                else {
                    commsControl.textActivated = false;
                    commsControl.ClearCommsWindow();
                    commsEnabled = false;
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

                //if ((rightEye.rightEyeActive || (rightEyeLodged && view.bodyControl)) && Input.GetButtonDown("Triangle Button") &&
                //            (Time.time - reticlePressTime > 0.5)) {                                                                          //      Light toggle (C)
                //    if (rightEye.rightLight.enabled)    { rightEyeAudio.PlayOneShot(rightEyeLightSwitchOn); }
                //    else                                { rightEyeAudio.PlayOneShot(rightEyeLightSwitchOff); }
                //    rightEye.rightLight.enabled = !rightEye.rightLight.enabled;
                //    reticlePressTime = Time.time;
                //}

                // Opening the Tool Selector
                if (view.bodyControl && view.bodyCamActive && !view.InVehicle() && ((Input.GetButton("Left Bumper")) || Input.GetKey(KeyCode.Mouse1))) {
                    toolSelectorOpen = true;
                    toolSelect.toolSelectorOpen = true;
                } else {
                    toolSelectorOpen = false;
                    toolSelect.toolSelectorOpen = false;
                }
                
                // If controlling the body and looking from the body's POV, right bumper takes a dose of vawn sap
                if (view.bodyControl && view.bodyCamActive && Input.GetButtonDown("Right Bumper") && Time.time - keyTimerRef > keyTimer) {
                    //hallucinating = !hallucinating;

                    sapMonitor.TakeHit(vawnTripStandardDosageAmount);

                    keyTimerRef = Time.time;
                    InitializeVawnTripStretchVariables();
                }

                
                // Vawn tripping
                if (((Time.time - reticlePressTime > 0.5) && ((Input.GetAxis("D-Pad Left Right") > 0)) ||       // Toggle hallucinations on and off
                        ((Time.time - reticlePressTime > 0.1) && Input.GetButtonDown("Hallucinations")))) {     //                                                     

                    //hallucinating = !hallucinating;                                                             //
                    hallucinationFrameTimeIndex = Time.time;                                                    //
                    reticlePressTime = Time.time;                                                               //

                    showGardenOverlays = !showGardenOverlays;
                }                                                                                               //

                // Check if right eye abilities are available
                CheckIfRightEyePowersAreAvailable();

                // Toggle reticle
                if (!reticleHidden && !view.IsHoldingDocument() && ((Time.time - reticlePressTime > 0.5) && ((Input.GetAxis("D-Pad Left Right") < 0)) ||       // Toggle reticle on and off (if right eye powers are available)
                    ((Time.time - reticlePressTime > 0.1) && Input.GetButtonDown("Reticle")))) {                //
                    ToggleReticle();                                                                            //
                    reticlePressTime = Time.time;                                                               //
                }                                                                                               //

                if (Input.GetAxis("D-Pad Up Down") < 0) {
                    if (activeCam.fieldOfView < 100) {
                        activeCam.fieldOfView += zoomFactor;
                    }
                }
                else if (Input.GetAxis("D-Pad Up Down") > 0) {
                    if (activeCam.fieldOfView > 5) {
                        activeCam.fieldOfView -= zoomFactor;
                    }
                }

                if (Input.GetAxis("Mouse ScrollWheel") < 0) {
                    if (activeCam.fieldOfView < 100) {
                        activeCam.fieldOfView += scrollWheelSensitivity;
                    }
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
                    if (activeCam.fieldOfView > 5) {
                        activeCam.fieldOfView -= scrollWheelSensitivity;
                    }
                }

                FOVText.text = (105 - (int)activeCam.fieldOfView).ToString();


                // EYE LOGIC ////////////////////////////


                // If both eyes are in
                if (leftEyeLodged && rightEyeLodged) {
                    //topRightInsert.enabled = false;                                                             // No picture in picture

                    //view.bodyCamActive = true;                                                                  // Main camera is displaying from Ted's perspective
                    //view.bodyControl = true;                                                                    // Player can move Ted

                // If the eyes are not both lodged
                } else {
                    // If the player is controlling the right eye
                    if (rightEye.rightEyeActive)  {
                        view.bodyCamActive = false;                                                             // Main camera not displaying Ted's perspective
                        rightEyeGridlinesBgd.enabled = true;

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


                            SwitchCam(view.transform, headd, initialPosBCam,
                                initialRotBCam);                 // Make main camera child of left eye (current rotation)

                            //if (leftEyeLodged) {                                                              // ****** Applicable if left eye functionality is implemented
                            //    view.bodyControl = true;                                                      //
                            //    view.bodyCamActive = true;                                                    //
                            //} else {                                                                          //
                            //    view.bodyControl = false;                                                     //
                            //    leftEye.leftEyeActive = true;                                                 //
                            //}                                                                                 //

                            topRightInsert.texture = rightEyeRT;                                                // Picture in picture is now right eye's perspective
                            rightEyeCam.fieldOfView = bodyCamera.fieldOfView;
                            bodyCamera.fieldOfView = headCam.fieldOfView;

                            rightEye.rightEyeActive = false;                                                    // Right eye not in control
                            view.bodyCamActive = true;                                                          // Make main camera display Ted's perspective
                            view.bodyControl = true;                                                            // Player controls Ted

                            view.SwitchAnimationStateToIdle();
                            keyTimerRef = Time.time;
                        }
                    // If right eye is out but not active, and left eye is in place
                    } else if (!rightEye.rightEyeActive && leftEyeLodged) {

                        rightEyeGridlinesBgd.enabled = false;

                        if (Input.GetButtonDown("Circle Button") && (Time.time - keyTimerRef > keyTimer)) {
                            if (rightEye.rightEyeLock) {
                                
                                view.bodyControl = true;
                            } else {
                                view.bodyControl = false;
                                view.SwitchAnimationStateToIdle();
                            }
                            rightEye.rightEyeActive = true;
                            
                            topRightInsert.texture = headCamRT;                                                 // Picture in picture shows left eye perspective
                            headCam.fieldOfView = bodyCamera.fieldOfView;
                            bodyCamera.fieldOfView = rightEyeCam.fieldOfView;

                            view.GetComponent<Camera>().nearClipPlane = 0.3f;                                       // Clipping in

                            SwitchCam(view.transform, rightEye.transform,
                                        Vector3.zero, Quaternion.identity);

                            keyTimerRef = Time.time;

                        }
                    }

                    if (Input.GetButtonDown("X Button") && (Time.time - keyTimerRef > keyTimer)) {
                        leftEye.leftEyeActive = false;

                        if (rightEyeLodged) {

                            //rightEyeLodged = false;
                            //rightEye.transform.parent = null;
                            //rightEye.transform.Translate(0, 0, 5f, Space.Self);

                            //SwitchCam(view.transform, rightEye.transform,
                            //          Vector3.zero, Quaternion.identity);                   // Make body camera child of right eye                     

                            //rightEye.transform.localRotation = returnRotRight;
                            //leftEye.transform.localRotation = returnRotLeft;
                            //view.transform.localRotation = initialRotBody;
                            //view.curRotX = 0;

                            //RightEyeCollisions(true);
                            //view.bodyCamActive = false;                                     // Disable body camera between-eyes view
                            //view.bodyControl = false;                                       // Disable body controls

                            //rightEye.rightEyeActive = true;                                 // Enable right eye controls
                            //rightEye.rightEyeLock = false;

                            //topRightInsert.texture = headdRT;

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

                if (dateScreen.StardaterIsActive()) {

                    if ((Time.time - keyTimerRef > stardaterWaitTimeBetweenButtonPresses) && !dateScreen.TedIsInChat()) {

                        if (Input.GetButtonDown("X Button")) {
                            dateScreen.PressX();
                            keyTimerRef = Time.time;
                        }
                        if (Input.GetButtonDown("Circle Button")) {
                            dateScreen.PressCircle();
                            keyTimerRef = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                            dateScreen.PressLeftLS();
                            keyTimerRef = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                            dateScreen.PressRightLS();
                            keyTimerRef = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                            dateScreen.PressDownLS();
                            keyTimerRef = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                            dateScreen.PressUpLS();
                            keyTimerRef = Time.time;
                        }
                    }

                    if (dateScreen.TedIsInChat()) {

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
                else {

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
            //wkDesk.deskEnabled = true;                                      // Open SPRING
        } else {
            rigid.constraints &= ~RigidbodyConstraints.FreezePosition;      // Disable position constraints
            pauseScreen.enabled = false;                                    // Disable pause screen
            //wkDesk.deskEnabled = false;                                     // Close SPRING
        }
    }

    // Dislodge right eye
    private void DislodgeRightEye() {
        view.SwitchAnimationStateToIdle();                                                      // Revert to Ted's idle animation

        topRightInsert.enabled = true;                                                          // Enable picture in picture, Ted's perspective
        topRightInsert.texture = headCamRT;                                                       //

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
            SwitchCam(view.transform, headd, initialPosBCam, initialRotBCam);
            view.bodyCamActive = true;
        }

        view.bodyControl = true;
        rightEye.rightEyeActive = false;

        rightEye.tedTrack = false;

        rightEye.transform.parent = null;
        rightEye.transform.parent = head.transform;
        rightEye.transform.localPosition = initialPosRight;
        rightEye.transform.localRotation = returnRotRight;

        view.curRotX = 0;
        headVision.localRotation = initialRotHeadVision;

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
        cam.SetParent(target);                                                                            // Make child of target

        cam.localPosition = finalPos;                                                                   // Initialize position
        cam.localRotation = finalRot;                                                                   // Initialize rotation
    }

    // Toggle between controlling right eye, and locking it in place and controlling Ted
    // It is assumed that rightEye.rightEyeActive = true, but the statement is repeated for clarity
    private void ToggleRightEyeTPP() {

        rightEye.rightEyeActive = true;

        if (rightEye.rightEyeLock) {
            if (!reticleHidden) {
                reticleEnabled = true;
                reticleImg.enabled = true;
            }
            view.bodyControl = false;

            // Initiate animation of HUD disconnection of PIP window from the FOV display
            disconnectingPIPFromFOV = true;
            PIPFOVConnectorFrameIndex = 0;
            PIPFOVConnectorFrameChangeTimeRef = Time.time;

            view.SwitchAnimationStateToIdle();
        } else if (!rightEye.rightEyeLock) {
            if (!reticleHidden) {
                reticleEnabled = false;
                reticleImg.enabled = false;
            }
            view.bodyControl = true;

            // Initiate animation of HUD connection between PIP window and the FOV display
            connectingPIPToFOV = true;
            PIPFOVConnectorFrameIndex = 0;
            PIPFOVConnectorFrameChangeTimeRef = Time.time;
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

                            cam.fieldOfView = activeCam.fieldOfView + randShift;
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

                //if (imagesWithDependentAlpha.Count > 0) {
                //    foreach (ImageAndAlpha img in imagesWithDependentAlpha) {
                //        img.SetAlphaToOriginal();
                //    }
                //}
            }

            foreach (Camera cam in gardenOverlayCameras) {
                if (cam.fieldOfView < (bodyCamera.fieldOfView - gardenaCamFOVProximityThreshold)) {
                    cam.fieldOfView += gardenCamFOVShiftRate * Time.deltaTime;
                } else if ( cam.fieldOfView > (bodyCamera.fieldOfView + gardenaCamFOVProximityThreshold)) {
                    cam.fieldOfView -= gardenCamFOVShiftRate * Time.deltaTime;
                }
            }
        }

        if (sapMonitor.IsSapToxicityLevelHighEnoughToInduceHallucination()) {              // if (view.bodyControl && view.bodyCamActive && hallucinating) {
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

                        // After each heartbeat, reduce the poison level
                        sapMonitor.Detox(vawnPoisonReductionPerHeartBeat);

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

        hallucinationScreen.enabled = false;
        hallucinatingText.enabled = false;
        hallucinatingTextBgd.enabled = false;
    }

    private void InitializeVawnTripStretchVariables() {

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

    public Camera GetActiveCamera() {
        return activeCam;
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


    // HUD GRAPHICAL EFFECTS

    // Activate/deactivate picture-in-picture HUD elements
    private void PictureInPictureActivationCheck() {
        if (topRightInsert.enabled) {
            insertFrame.enabled = true;
            insertFlasher.enabled = true;
            insertReticle.enabled = true;

            if (insertFlasherAlphaIsIncreasing) {
                if (insertFlasher.color.a < insertFlasherMaxAlpha) {
                    Color newColor = new Color(insertFlasher.color.r, insertFlasher.color.g, insertFlasher.color.b, insertFlasher.color.a + Time.deltaTime * insertFlasherIntensityChangeFactor);
                    insertFlasher.color = newColor;
                }
                else {
                    insertFlasherAlphaIsIncreasing = false;
                }
            }
            else {
                if (insertFlasher.color.a > insertFlasherMinAlpha) {
                    Color newColor = new Color(insertFlasher.color.r, insertFlasher.color.g, insertFlasher.color.b, insertFlasher.color.a - Time.deltaTime * insertFlasherIntensityChangeFactor);
                    insertFlasher.color = newColor;
                }
                else {
                    insertFlasherAlphaIsIncreasing = true;
                }
            }
        }
        else {
            insertFrame.enabled = false;
            insertFlasher.enabled = false;
            insertReticle.enabled = false;
        }
    }

    // Determine active camera and display current FOV
    private void FOVSyncer() {
        if (rightEye.rightEyeActive && view.bodyControl) {
            activeCam = headCam;
            FOVText.color = FOVPIPColor;
            PIPFOVConnector.enabled = true;
        }
        else {
            activeCam = bodyCamera;
            FOVText.color = FOVMainColor;
        }

        if (connectingPIPToFOV) {

            PIPFOVConnector.texture = PIPFOVConnectorFrames[PIPFOVConnectorFrameIndex];

            if (Time.time - PIPFOVConnectorFrameChangeTimeRef > PIPFOVConnectorFrameChangeTime) {
                if (PIPFOVConnectorFrameIndex < PIPFOVConnectorFrames.Count - 1) {
                    PIPFOVConnectorFrameIndex++;
                    PIPFOVConnectorFrameChangeTimeRef = Time.time;
                }
                else {
                    connectingPIPToFOV = false;
                }
            }
        }

        if (disconnectingPIPFromFOV) {

            PIPFOVConnector.texture = PIPFOVDisconnectorFrames[PIPFOVConnectorFrameIndex];

            if (Time.time - PIPFOVConnectorFrameChangeTimeRef > PIPFOVConnectorFrameChangeTime) {
                if (PIPFOVConnectorFrameIndex < PIPFOVDisconnectorFrames.Count - 1) {
                    PIPFOVConnectorFrameIndex++;
                    PIPFOVConnectorFrameChangeTimeRef = Time.time;
                }
                else {
                    disconnectingPIPFromFOV = false;
                    PIPFOVConnector.enabled = false;
                }
            }

        }
    }


    // Class for list of HUD elements that need alpha adjustments when switching the Garden overlays on/off
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
