using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BodyCam : MonoBehaviour {

    public Transform headActual;
    public Vector3 headInitialPos;
    public Quaternion headInitialRot;
    public float lookSensitivity, vertLookLimiter;                                                                                   // Mouse motion speed factor
    public float walkSpeed, runSpeed, sneakSpeed, sneakRunSpeed, usingSpeed;                                                         // Movement speed factors
    public float idleSpeedAnim, walkSpeedAnim, runSpeedAnim, sneakSpeedAnim, sneakRunSpeedAnim;
    public float movementSpeedMultiplier, sidewaysRunSlowdownFactor, sidewaysStepSlowdownFactor, backwardsMovingSlowdownFactor, sidestepAnimationSpeedupFactor,
                    moveSpeedSlopeFactor, animSpeedSlopeFactor;
    
    public float animSpeed;
    public float jumpHeight;                                                                                        // How are we handling jumping?  Avoid it altogether??
    public float zoomFactor, scrollWheelSensitivity;
    public float defaultZoom;
    public bool bodyCamActive, bodyControl;                                                                         // Both eyes lodged --> activate bodycam; if bodycontrol, we are controlling the body
    public float curRotX;                                                                                           // "Manually" track the camera's vert rot, since initial 0 deg - 1 deg = -359 deg
    public Text FOVtext;
    public bool dead;
    public float health, numberOfAttacks;
    public float usingFreezeTime, usingFreezeTimerRef;
    public bool holdingDocument;
    public bool triggerPressed;
    public float steeringHandsChangeFrameFactor, steeringHandsSpecialTaskChangeFrameFactor;

    [SerializeField]
    public ParallelActions enterCarActions, exitCarActions, actionsAfterUsing;

    // Components
    private Teddy ted;
    private Transform headd, useRefPosition;
    private TeddyHead tedHead;
    private Animator anim, animHead;
    private Rigidbody rigid;
    private Collider tedCollider;
    private SkinnedMeshRenderer meshRend;
    private TeddyRightEye rightEye;
    private TeddyLeftEye leftEye;
    private GameObject leftEyeCam, rightEyeCam;                                           // Head reference needed for when we figure out how to animate head during animations...
    private CameraMaster camControl;                                                                               // Camera Control script reference needed for accessing EyeLodged switches
    private Camera cam;
    private IDDocument docHeld;
    private IDCharacter IDChar;
    private CommsController commsControl;
    private ToolSelector toolSelect;
    private InfoScan scanner;
    private WorkDesk wkDesk;
    private StatusPopup statusWindow;
    private ActionSceneCoordinator actionCoord;
    private PrisonManager prisonController;
    private Inventory inv;
    private MusicPlayer musicBox;
    private Crown tedFunds;

    private Disposal trashcan;

    public enum MovementModes { OnFoot, Driving, Flying, Inspecting};
    private MovementModes movementScheme;

    public bool runMode, sneakMode;                                                                                           // running vs. walking toggle
    private bool isGrounded;                                                                                        // Will only be needed if there's jumping and falling
    private bool isMovingForward, isMovingBackwards, isRightStepping, isLeftStepping;                                                                                         // Switch used for controlling when walk animation starts/ends
    private float moveSpeed;                                                                                        // Move speed   
    private Vector3 initialPos, curPos, tempPos;                                  // For frame to frame position tracking, for determining when to start/end walk anim
    private float curSelectorPosX, curSelectorPosY, curSelectorStickX, curSelectorStickY;
    private float vertLookLimLo = -75;                                                                              // Vertical look limit, low
    private float vertLookLimHi =  75;                                                                              // Vertical look limit, high
    private float sneakToggleTime, curSneakToggleTime, sneakAnimationWeight, sidewaysRunSlowdown, sidewaysStepSlowdown, backwardsMovingSlowdown, sidestepAnimationSpeedup;
    private float defaultUsingFreezeTime;
    private bool isUsing, useFreeze, isInVehicle;

    private IDVehicle currentVehicleID;
    private CarControls currentCarControls;
    private List<Transform> frontWheels = new List<Transform>();
    private List<Transform> backWheels = new List<Transform>();
    private Transform currentSteeringWheel;
    private float curSteeringHandsPosX;     // This is always Ted's driving hand position/steering animation frame from the previous frame, to use as a reference for gradually changing the animation frame
    private bool enteringVehicle, exitingVehicle;
    private Quaternion tedHeadDefaultRotationInVehicle;
    private string interiorStopUsingAnimationTrigger = "Stop using";
    private string previousDrivingAnimationTrigger = "Stop using";
    private string currentLeftArmDrivingAnimationState, currentRightArmDrivingAnimationState;
    private bool leftArmIsPerformingSpecialDrivingTask, rightArmIsPerformingSpecialDrivingTask;
    private float currentDrivingArmSpecialTaskAnimationNormalizedTime;
    private bool spinAfterExitingVehicle;
    private float exitSpinFactor = 780;
    private float currentCumulativeSpin;
    private float cumulativeSpinOffset = -390;
    private float spinTimeRef;

    // Use this for initialization
    void Start () {
        Cursor.visible = false;                                                                                     // Hide the damn cursor
        //this.transform.localRotation = Quaternion.identity;

        camControl  = FindObjectOfType<CameraMaster>();
        cam = this.GetComponent<Camera>();

        ted                 = FindObjectOfType<Teddy>();
        anim                = ted.GetComponent<Animator>();
        rigid               = ted.GetComponent<Rigidbody>();
        tedCollider         = ted.GetComponent<Collider>();
        meshRend            = ted.transform.Find("test_Ted").GetComponent<SkinnedMeshRenderer>();
        tedHead             = FindObjectOfType<TeddyHead>();
        animHead            = tedHead.GetComponent<Animator>();

        IDChar              = ted.transform.Find("ID").GetComponent<IDCharacter>();

        leftEye     = FindObjectOfType<TeddyLeftEye>();
        rightEye    = FindObjectOfType <TeddyRightEye>();

        commsControl = FindObjectOfType<CommsController>();
        toolSelect = FindObjectOfType<ToolSelector>();
        scanner = FindObjectOfType<InfoScan>();
        wkDesk = FindObjectOfType<WorkDesk>();
        statusWindow = FindObjectOfType<StatusPopup>();
        actionCoord = FindObjectOfType<ActionSceneCoordinator>();
        prisonController = FindObjectOfType<PrisonManager>();

        inv = ted.transform.Find("Inventory").GetComponent<Inventory>();
        musicBox = FindObjectOfType<MusicPlayer>();
        tedFunds = FindObjectOfType<Crown>();

        trashcan = FindObjectOfType<Disposal>();

        headd = headActual;

        bodyControl = true;                                                                                         // Start with body control switched on
        bodyCamActive = true;                                                                                       // Start with view through body camera

        runMode = false;                                                                                            // Start at walking pace
        sneakMode = false;
        isGrounded = false;                                                                                         // Not grounded at start???
        isMovingForward = false;                                                                                          // Start not walking
        isMovingBackwards = false;
        isRightStepping = false;
        isLeftStepping = false;
        
        sneakToggleTime = 0.5f;
        curSneakToggleTime = 0;
        sneakAnimationWeight = 0;

        sidewaysRunSlowdown = 1;
        sidewaysStepSlowdown = 1;
        sidestepAnimationSpeedup = 1;
        backwardsMovingSlowdown = 1;

        initialPos = this.transform.localPosition;                                                                  // Establish initial position (why???)

        curRotX = 0;                                                                                                // Vertical rotation monitoring variable initialized to 0
        curPos = ted.transform.position;                                                                            // Set initial current position
        tempPos = curPos - new Vector3(curPos.x, curPos.y, curPos.z - 0.02f);                                       // Set initial (hypothetical) previous-frame position to 0

        health = 100;
        dead = false;
        numberOfAttacks = 0;

        useRefPosition = null;
        usingFreezeTimerRef = 0;
        isUsing = false;
        useFreeze = false;
        defaultUsingFreezeTime = usingFreezeTime;

        docHeld = null;
        holdingDocument = false;

        curSelectorPosX = 0f;
        curSelectorPosY = 0f;
        //curSelectorStickX = 0f;
        //curSelectorStickY = 0f;

        curSteeringHandsPosX = 0;  // Zero is the clamp value of the neutral steering position

        movementScheme = MovementModes.OnFoot;

        enteringVehicle = false;
        exitingVehicle = false;
        currentLeftArmDrivingAnimationState = "Left Arm Driving.Steering";
        currentRightArmDrivingAnimationState = "Right Arm Driving.Steering";
        leftArmIsPerformingSpecialDrivingTask = false;
        rightArmIsPerformingSpecialDrivingTask = false;
        spinAfterExitingVehicle = false;
        currentCumulativeSpin = cumulativeSpinOffset;

        headInitialPos = tedHead.transform.localPosition;
        headInitialRot = tedHead.transform.localRotation;

        IDChar.RecordHeadInitialTransform();
    }

    // Update is called once per frame
    void FixedUpdate() {

        if (health <= 0 && !dead) {
            
            anim.SetTrigger("Dead");
            animHead.SetTrigger("Dead");
            animSpeed = idleSpeedAnim;
            dead = true;
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) && !camControl.gamePaused) {
            if (dead) {
                anim.SetTrigger("Revived");
                animHead.SetTrigger("Revived");
                dead = false;
                health = 100;
            }
        }

        if (camControl.commsEnabled == false) {

            //// This prevents overlapping actions
            //if (isUsing) {
            //    if (Time.time - usingFreezeTimerRef > usingFreezeTime) {
            //        isUsing = false;
            //        useFreeze = false;
            //        anim.SetBool("Using", false);
            //        anim.SetLayerWeight(3, 0);              // Set weight of Using layer to zero
            //                                                //anim.SetBool("Entering Car", false);

            //        if (actionsAfterUsing.actions.Count > 0) {
            //            actionCoord.TriggerParallelActions(actionsAfterUsing);
            //        }
            //    }
            //}

            

            // Determine what a right trigger press does when player is controlling a living Ted who is not mid-action
            if ((bodyControl && !useFreeze && !dead)) { //|| (rightEye.rightEyeActive && !useFreeze)) {

                if (((Input.GetAxis("Triggers") > 0.1) || Input.GetKeyDown(KeyCode.Mouse0)) &&
                        Time.time - camControl.keyTimerRef > camControl.keyTimer && !triggerPressed) {
                   
                    // If tool wheel open, trigger press selects highlighted tool
                    if (toolSelect.toolSelectorOpen) {
                        toolSelect.EnableTool();
                    } else {             // IF TRIGGER PRESS AND TOOL SELECTOR WHEEL NOT OPEN /////////

                        // If holding document, drop it <--- MIGHT BE UNNECESSARY
                        //if (docHeld != null && docHeld.documentBeingHeld) {
                        //    docHeld.buttonPressed = true;
                        //}
                        
                        // Without this check, a single trigger press can drop the document, then snatch it up during the same frame
                        bool docDroppedThisFrame = false;

                        if (IsHoldingDocument()) {
                            docHeld.DropDocument();
                            camControl.keyTimerRef = Time.time;

                            docDroppedThisFrame = true;
                        }

                        // Set useRefPosition variable for origin of raycast--use left eye if controlling body with right eye out, otherwise use this camera location
                        if (bodyControl && !camControl.rightEyeLodged) {
                            useRefPosition = leftEye.transform;
                        } else {
                            useRefPosition = this.transform;
                        }

                        //useRefPosition.localPosition = Vector3.zero;              // <-- Not used???
                        //useRefPosition.localRotation = Quaternion.identity;

                        // Determines use action based on what we're looking at
                        RaycastHit hit;
                        if (!IsHoldingDocument() && Physics.Raycast(useRefPosition.position, useRefPosition.TransformDirection(Vector3.forward), 
                            out hit, rightEye.maxIDRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {

                            ID targetedID = null;

                            if (hit.collider.GetComponent<PointerToID>()) {
                                targetedID = hit.collider.GetComponent<PointerToID>().desiredID;
                            }
                            else if (hit.collider.transform.Find("ID") != null) {
                                targetedID = hit.collider.transform.Find("ID").GetComponent<ID>();
                            }

                            if (targetedID != null &&
                                (hit.collider.ClosestPoint(useRefPosition.position) - useRefPosition.position).magnitude < targetedID.maxDistanceToActivate) {        

                                if (!docHeld && !docDroppedThisFrame) {
                                    // Activate the object holding the ID! (pick up IDItem, use IDInteractable, etc.)
                                    if (targetedID.GetType() == typeof(IDVehicle)) {
                                        IDVehicle vehicleID = (IDVehicle)targetedID;
                                        vehicleID.SetDriversID(ted.transform.Find("ID").GetComponent<ID>());

                                        targetedID.Activate();
                                        targetedID.AddToEvidencePool();
                                    }
                                    else
                                    {
                                        targetedID.Activate();
                                        targetedID.AddToEvidencePool();
                                    }
                                }
                            }
                        }

                        camControl.keyTimerRef = Time.time;                     // resets timer on CameraMaster
                    }

                }

                // Set triggerPressed to true if either trigger is pressed beyond threshold
                if (Mathf.Abs(Input.GetAxis("Triggers")) > 0.1) {
                    triggerPressed = true;
                } else { triggerPressed = false; }
            }


            if (Input.GetAxis("D-Pad Up Down") < 0 && !camControl.gamePaused) {
                if (cam.fieldOfView < 100) {
                    cam.fieldOfView += zoomFactor;
                }
            } else if (Input.GetAxis("D-Pad Up Down") > 0 && !camControl.gamePaused) {
                if (cam.fieldOfView > 5) {
                    cam.fieldOfView -= zoomFactor;
                }
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0 && !camControl.gamePaused) {
                if (cam.fieldOfView < 100) {
                    cam.fieldOfView += scrollWheelSensitivity;
                }
            } else if (Input.GetAxis("Mouse ScrollWheel") > 0 && !camControl.gamePaused) {
                if (cam.fieldOfView > 5) {
                    cam.fieldOfView -= scrollWheelSensitivity;
                }
            }

            FOVtext.text = ((int)cam.fieldOfView).ToString();

            if (bodyControl && !useFreeze && !dead) {                                                                                          // If we're controlling the body...

                switch (movementScheme) {

                    case MovementModes.OnFoot:

                        //if (driverTedMeshRend.enabled) {
                        //    driverTedMeshRend.enabled = false;
                        //    ted.transform.Find("ID").GetComponent<IDCharacter>().head.SetParent(ted.transform.Find("ID").GetComponent<IDCharacter>().parentBoneOfHead);
                        //    tedHead.transform.localPosition = headInitialPos;
                        //    tedHead.transform.localRotation = headInitialRot;
                        //}

                        meshRend.enabled = false;
                        ted.transform.Find("ID").GetComponent<IDCharacter>().head.SetParent(null);

                        // This prevents overlapping actions
                        if (isUsing) {
                            if (Time.time - usingFreezeTimerRef > usingFreezeTime) {
                                isUsing = false;
                                useFreeze = false;
                                anim.SetBool("Using", false);
                                anim.SetLayerWeight(3, 0);              // Set weight of Using layer to zero
                                                                        //anim.SetBool("Entering Car", false);

                                if (actionsAfterUsing.actions.Count > 0) {
                                    actionCoord.TriggerParallelActions(actionsAfterUsing);
                                }
                                actionsAfterUsing.actions.Clear();
                            }
                        }

                        ted.transform.Find("ID").GetComponent<IDCharacter>().head.SetParent(ted.transform.Find("ID").GetComponent<IDCharacter>().parentBoneOfHead);
                        tedHead.transform.localPosition = headInitialPos;
                        tedHead.transform.localRotation = headInitialRot;
                        meshRend.enabled = true;

                        if (bodyControl && !useFreeze && !dead) {

                            if (Input.GetButton("Left Stick Press") && !camControl.gamePaused) { runMode = true; }                                        //      We can toggle running vs. walking
                            else { runMode = false; }

                            if (!runMode && Input.GetButtonDown("Right Stick Press") && !camControl.gamePaused && (Time.time - curSneakToggleTime > sneakToggleTime)) {
                                sneakMode = !sneakMode;
                                curSneakToggleTime = Time.time;
                            }

                            curPos = ted.transform.position;                                                                        //      Every frame, reset curPos to Ted's body's current position
                            Vector2 curPosNoVertical = new Vector2(curPos.x, curPos.z);
                            Vector2 tempPosNoVertical = new Vector2(tempPos.x, tempPos.z);
                            //float distTrav = (curPos - tempPos).magnitude;                                                          //      Distance traveled between frames = current position - position in last frame
                            float distTrav = (curPosNoVertical - tempPosNoVertical).magnitude;
                            tempPos = curPos;                                                                                       //      Retain current position for next frame's calculation

                            if (isLeftStepping || isRightStepping) {
                                sidewaysRunSlowdown = sidewaysRunSlowdownFactor;
                                sidewaysStepSlowdown = sidewaysStepSlowdownFactor;
                                sidestepAnimationSpeedup = sidestepAnimationSpeedupFactor;
                            }
                            else {
                                sidewaysRunSlowdown = 1;
                                sidewaysStepSlowdown = 1;
                                sidestepAnimationSpeedup = 1;
                            }

                            if (isMovingBackwards) { backwardsMovingSlowdown = backwardsMovingSlowdownFactor; }
                            else { backwardsMovingSlowdown = 1; }

                            Vector3 slopeAdj = GetDirectionOfFloor();
                            float moveXAdj = 1 - (slopeAdj.x * moveSpeedSlopeFactor);
                            float moveZAdj = 1 - (slopeAdj.z * moveSpeedSlopeFactor);

                            float animSpeedUnadj;

                            if (distTrav > 0.01 && !dead) {
                                if (runMode) {                                                                                          //      Set run speed or walk speed
                                    anim.SetLayerWeight(1, 1);
                                    anim.SetLayerWeight(3, 0);
                                    if (isUsing) {
                                        anim.SetLayerWeight(6, 1);
                                        anim.SetLayerWeight(4, 0);
                                    }
                                    else {
                                        anim.SetLayerWeight(4, 1);
                                        anim.SetLayerWeight(6, 0);
                                    }
                                    anim.SetLayerWeight(0, 0);
                                    animHead.SetLayerWeight(1, 1);

                                    if (sneakMode) {
                                        moveSpeed = sneakRunSpeed * movementSpeedMultiplier * sidewaysRunSlowdown * backwardsMovingSlowdown;
                                        if (distTrav > 0.05) { animSpeedUnadj = sneakRunSpeedAnim * movementSpeedMultiplier * sidewaysStepSlowdown * backwardsMovingSlowdown * distTrav; }
                                        else { animSpeedUnadj = sneakRunSpeedAnim * movementSpeedMultiplier; }
                                    }
                                    else {
                                        moveSpeed = runSpeed * movementSpeedMultiplier * sidewaysRunSlowdown * backwardsMovingSlowdown;
                                        if (distTrav > 0.05) { animSpeedUnadj = runSpeedAnim * movementSpeedMultiplier * sidewaysRunSlowdown * backwardsMovingSlowdown * distTrav; }
                                        else { animSpeedUnadj = runSpeedAnim * movementSpeedMultiplier * sidewaysRunSlowdown; }
                                    }
                                }
                                else {
                                    anim.SetLayerWeight(1, 0);
                                    if (isUsing) {
                                        anim.SetLayerWeight(6, 1);
                                        anim.SetLayerWeight(3, 0);
                                    }
                                    else {
                                        anim.SetLayerWeight(3, 1);
                                        anim.SetLayerWeight(6, 0);
                                    }
                                    anim.SetLayerWeight(4, 0);
                                    animHead.SetLayerWeight(1, 0);

                                    if (sneakMode) {
                                        moveSpeed = sneakSpeed * movementSpeedMultiplier * sidewaysStepSlowdown * backwardsMovingSlowdown;
                                        if (distTrav > 0.05) { animSpeedUnadj = sneakSpeedAnim * movementSpeedMultiplier * sidestepAnimationSpeedup * backwardsMovingSlowdown * distTrav; }
                                        else { animSpeedUnadj = sneakSpeedAnim * movementSpeedMultiplier * sidestepAnimationSpeedup; }
                                    }
                                    else {
                                        moveSpeed = walkSpeed * movementSpeedMultiplier * sidewaysStepSlowdown * backwardsMovingSlowdown;
                                        if (distTrav > 0.05) { animSpeedUnadj = walkSpeedAnim * movementSpeedMultiplier * sidestepAnimationSpeedup * backwardsMovingSlowdown * distTrav; }
                                        else { animSpeedUnadj = walkSpeedAnim * movementSpeedMultiplier * sidestepAnimationSpeedup; }
                                    }
                                }
                            }
                            else {
                                animSpeedUnadj = idleSpeedAnim;
                            }

                            if (isUsing) { anim.speed = usingSpeed; }
                            else { anim.speed = animSpeedUnadj + (animSpeedSlopeFactor * slopeAdj.magnitude); }

                            animHead.speed = anim.speed;

                            if (sneakMode) {
                                if (sneakAnimationWeight < 0.96) {
                                    sneakAnimationWeight += Time.deltaTime * 3;
                                }
                                anim.SetLayerWeight(2, sneakAnimationWeight);
                                anim.SetLayerWeight(0, 0);
                                anim.SetLayerWeight(1, 0);
                                anim.SetLayerWeight(3, 0);
                                anim.SetLayerWeight(4, 0);

                                if (isUsing) { anim.SetLayerWeight(6, 1); }
                                else { anim.SetLayerWeight(5, sneakAnimationWeight); }
                                animHead.SetLayerWeight(2, sneakAnimationWeight);
                            }
                            else {
                                if (sneakAnimationWeight > 0.04) {
                                    sneakAnimationWeight -= Time.deltaTime * 3;
                                }
                                anim.SetLayerWeight(2, sneakAnimationWeight);

                                if (isUsing) { anim.SetLayerWeight(6, 1); }
                                else { anim.SetLayerWeight(5, sneakAnimationWeight); }
                                animHead.SetLayerWeight(2, sneakAnimationWeight);
                            }

                            if (!camControl.gamePaused) {
                                float rotX = 0;
                                float rotY = 0;

                                float stickX = 0;
                                float stickY = 0;

                                if (toolSelect.toolSelectorOpen) {
                                    rotX = -Input.GetAxis("Mouse Y") * 100;                                                                 //      Get vertical look input from mouse (inverted by default)
                                    rotY = Input.GetAxis("Mouse X") * 100;
                                    stickX = Input.GetAxis("Right Joystick Vertical") * 100;
                                    stickY = Input.GetAxis("Right Joystick Horizontal") * 100;

                                    //if ((Mathf.Abs(rotX) > 5 || Mathf.Abs(rotY) > 5) && (((rotX - curSelectorPosY) > 10) || ((rotY - curSelectorPosX) > 10))) {
                                    //    toolSelect.IconPointer(rotY, -rotX);
                                    //}
                                    //else if ((Mathf.Abs(stickX) > 0.5 || Mathf.Abs(stickY) > 0.5) && (((stickX - curSelectorStickY) > 0.2) || ((stickY - curSelectorStickX) > 0.2))) {
                                    //    toolSelect.IconPointer(stickY, stickX);
                                    //}

                                    if (Mathf.Abs(rotX) < 10) { rotX = 0; }
                                    if (Mathf.Abs(rotY) < 10) { rotY = 0; }

                                    toolSelect.IconPointer(rotY + stickY, -rotX + stickX);

                                    //curSelectorPosX = rotY;
                                    //curSelectorPosY = rotX;
                                    //curSelectorStickX = stickY;
                                    //curSelectorStickY = stickX;

                                }
                                else {
                                    if (rightEye.rightEyeActive || leftEye.leftEyeActive) {
                                        rotX = (Input.GetAxis("Right Joystick Vertical") - Input.GetAxis("Mouse Y")) * 100;                                                                 //      Get vertical look input from mouse (inverted by default)
                                        rotY = (Input.GetAxis("Right Joystick Horizontal") + Input.GetAxis("Mouse X")) * 100;                                                                  //      Get horizontal look input from mouse
                                    }
                                    else {
                                        rotX = (Input.GetAxis("Right Joystick Vertical") - Input.GetAxis("Mouse Y")) * cam.fieldOfView;
                                        rotY = (Input.GetAxis("Right Joystick Horizontal") + Input.GetAxis("Mouse X")) * cam.fieldOfView;
                                    }


                                    if ((curRotX < vertLookLimHi && curRotX > vertLookLimLo) || (curRotX >= vertLookLimHi && rotX < 0)      //      If vertical look rotation is between limits, or outside limits and moving in...
                                                                                                || (curRotX <= vertLookLimLo && rotX >= 0)) {

                                        if (bodyCamActive) {                                                                                //          If we're looking through the body camera...
                                            this.transform.Rotate(rotX * lookSensitivity, 0, 0);                                            //              Rotate the body camera vertically
                                        }
                                        else {
                                            headd.transform.Rotate(rotX * lookSensitivity, 0, 0);
                                        }

                                        curRotX += rotX * lookSensitivity;                                                                  //          Keep track of current rotation value by manually adding current vert rot

                                        if (camControl.leftEyeLodged) {                                                                     //          If the left eye is in...
                                            leftEye.transform.Rotate(rotX * lookSensitivity * vertLookLimiter, 0, 0);                                  //              Rotate the left eye vertically with the sensitivity factor reduced
                                        }
                                        if (camControl.rightEyeLodged) {                                                                    //          If the right eye is in...
                                            rightEye.transform.Rotate(rotX * lookSensitivity * vertLookLimiter, 0, 0);                                 //              Do same as with left eye^^^
                                        }
                                    }

                                    ted.transform.Rotate(0, rotY * lookSensitivity, 0);                                                     //      Rotate whole body horizontally by sensitivity factor
                                }

                                float moveX = Input.GetAxisRaw("Left Joystick Horizontal") + Input.GetAxis("Horizontal");                                                              //      Get lateral translational movement input (x-axis) 
                                float moveZ = Input.GetAxisRaw("Left Joystick Vertical") + Input.GetAxis("Vertical");                                                                //      Get forward translational movement input (z-axis)
                                                                                                                                                                                     //Vector3 slopeAdj = GetDirectionOfFloor();
                                                                                                                                                                                     //float moveXAdj = 1 - slopeAdj.x;
                                                                                                                                                                                     //float moveZAdj = 1 - slopeAdj.z;

                                //ted.transform.Translate(moveX * moveSpeed * Time.deltaTime, 0, moveZ * moveSpeed * Time.deltaTime, Space.Self);       // Move
                                Vector3 newPosition = ted.transform.position +
                                            ted.transform.TransformDirection(moveX * moveXAdj, 0, moveZ * moveZAdj) * moveSpeed * Time.deltaTime;
                                rigid.MovePosition(newPosition);

                                if (distTrav > 0.05) {                                                                     //      If distance traveled is greater than threshold and Ted isn't already walking...
                                    if ((Mathf.Abs(moveX) > Mathf.Abs(moveZ))) {
                                        isMovingForward = false;
                                        isMovingBackwards = false;
                                        if (moveX > 0 && !isRightStepping) {
                                            SetAnimationStates("Step Right");
                                        }
                                        else if (moveX < 0 && !isLeftStepping) {
                                            SetAnimationStates("Step Left");
                                        }
                                    }
                                    else {
                                        if (Mathf.Abs(moveZ) >= Mathf.Abs(moveX)) {
                                            if (moveZ > 0 && !isMovingForward) {
                                                SetAnimationStates("Move Forward");
                                            }
                                            else if (moveZ < 0 && !isMovingBackwards) {
                                                SetAnimationStates("Move Backwards");
                                            }
                                        }
                                    }
                                }
                                else if (distTrav <= 0.02) {                                                                           //      If distance traveled is below the threshold...
                                    SwitchAnimationStateToIdle();
                                }
                            }
                        }
                        break;
                    case (MovementModes.Driving):

                        //meshRend.enabled = false;
                        //driverTedMeshRend.enabled = true;

                        if (isUsing && Time.time - usingFreezeTimerRef > usingFreezeTime) {

                            if (enteringVehicle) {
                                tedHeadDefaultRotationInVehicle = tedHead.transform.localRotation;

                                currentCarControls.isDriving = true;
                                currentCarControls.EnableInteriorInteractables();
                                enteringVehicle = false;
                                isUsing = false;
                            }

                            if (exitingVehicle) {

                                currentCarControls.isDriving = false;
                                currentCarControls.DisableInteriorInteractables();
                                currentVehicleID.vehicle.GetComponent<Rigidbody>().velocity = Vector3.zero;
                                exitingVehicle = false;

                                
                                //if (actionsAfterUsing.actions.Count > 0) {
                                //    actionCoord.TriggerParallelActions(actionsAfterUsing);
                                //}
                                //actionsAfterUsing.actions.Clear();

                                movementScheme = MovementModes.OnFoot;
                                isInVehicle = false;

                                //usingFreezeTimerRef = Time.time;
                                //usingFreezeTime = 1;



                                //spinAfterExitingVehicle = true;
                                //currentCumulativeSpin = cumulativeSpinOffset;

                                //spinTimeRef = Time.time;

                                //anim.SetBool("Spin 180 after exiting car", true);
                                // ****NOTE: We do not set isUsing to false, since we want isUsing = true to remain into the MovementModes.OnFoot case, so the afterUsingActions, etc. can occur
                                // N/A ^^^
                            }

                            //if (spinAfterExitingVehicle) {
                            //    if (currentCumulativeSpin < 180) {
                            //        if (currentCumulativeSpin >= 0) {
                            //            ted.transform.Rotate(new Vector3(0, 3 * exitSpinFactor * Time.deltaTime, 0), Space.Self);
                            //        }
                            //        currentCumulativeSpin += 3 * exitSpinFactor * Time.deltaTime;
                            //    }
                            //    else {
                            //        Debug.Log(Time.time - spinTimeRef);
                            //        anim.SetBool("Spin 180 after exiting car", false);
                            //        spinAfterExitingVehicle = false;
                            //        movementScheme = MovementModes.OnFoot;
                            //        isInVehicle = false;
                            //        currentCumulativeSpin = 0;
                            //    }
                            //}
                        }

                        if (!isUsing && !camControl.gamePaused && bodyControl) {

                            //anim.SetLayerWeight(7, 1);
                            anim.SetLayerWeight(8, 1);
                            anim.SetLayerWeight(9, 1);

                            float rotX = 0;
                            float rotY = 0;

                            if (rightEye.rightEyeActive || leftEye.leftEyeActive) {
                                rotX = (Input.GetAxis("Right Joystick Vertical") - Input.GetAxis("Mouse Y")) * 100;                                                                 //      Get vertical look input from mouse (inverted by default)
                                rotY = (Input.GetAxis("Right Joystick Horizontal") + Input.GetAxis("Mouse X")) * 100;                                                                  //      Get horizontal look input from mouse
                            }
                            else {
                                rotX = (Input.GetAxis("Right Joystick Vertical") - Input.GetAxis("Mouse Y")) * cam.fieldOfView;
                                rotY = (Input.GetAxis("Right Joystick Horizontal") + Input.GetAxis("Mouse X")) * cam.fieldOfView;
                            }

                            float steeringHandsPosX = Input.GetAxis("Left Joystick Horizontal");

                            float frameDelta = (steeringHandsPosX - curSteeringHandsPosX);
                            float newFrame = curSteeringHandsPosX + (frameDelta * steeringHandsChangeFrameFactor);

                            if (leftArmIsPerformingSpecialDrivingTask) {
                                if (currentDrivingArmSpecialTaskAnimationNormalizedTime < 1) {                                    
                                    currentDrivingArmSpecialTaskAnimationNormalizedTime += steeringHandsSpecialTaskChangeFrameFactor * Time.deltaTime;
                                    Debug.Log(currentDrivingArmSpecialTaskAnimationNormalizedTime);
                                }

                                anim.Play(currentLeftArmDrivingAnimationState, 8, currentDrivingArmSpecialTaskAnimationNormalizedTime);
                            }
                            else {
                                anim.Play(currentLeftArmDrivingAnimationState, 8, (newFrame + 1) / 2);      // currentLeftArmDrivingAnimationState SHOULD be "Left Arm Driving.Steering"
                            }

                            if (rightArmIsPerformingSpecialDrivingTask) {
                                if (currentDrivingArmSpecialTaskAnimationNormalizedTime < 1) {                                    
                                    currentDrivingArmSpecialTaskAnimationNormalizedTime += steeringHandsSpecialTaskChangeFrameFactor * Time.deltaTime;
                                }

                                anim.Play(currentRightArmDrivingAnimationState, 9, currentDrivingArmSpecialTaskAnimationNormalizedTime);
                            }
                            else {
                                anim.Play(currentRightArmDrivingAnimationState, 9, (newFrame + 1) / 2);     // currentRightArmDrivingAnimationState SHOULD be "Right Arm Driving.Steering"
                            }                            

                            float angle;
                            if (newFrame >= 0) { angle = newFrame * 300; }
                            else { angle = (newFrame * 300) + 360; }

                            currentSteeringWheel.localEulerAngles = new Vector3(200, 0, angle);

                            curSteeringHandsPosX = newFrame;

                            if ((curRotX < vertLookLimHi && curRotX > vertLookLimLo) || (curRotX >= vertLookLimHi && rotX < 0)      //      If vertical look rotation is between limits, or outside limits and moving in...
                                                                                        || (curRotX <= vertLookLimLo && rotX >= 0)) {

                                if (bodyCamActive) {                                                                                //          If we're looking through the body camera...
                                    this.transform.Rotate(rotX * lookSensitivity, 0, 0);                                            //              Rotate the body camera vertically
                                }
                                else {
                                    headd.transform.Rotate(rotX * lookSensitivity, 0, 0);
                                }

                                curRotX += rotX * lookSensitivity;                                                                  //          Keep track of current rotation value by manually adding current vert rot

                                if (camControl.leftEyeLodged) {                                                                     //          If the left eye is in...
                                    leftEye.transform.Rotate(rotX * lookSensitivity * vertLookLimiter, 0, 0);                                  //              Rotate the left eye vertically with the sensitivity factor reduced
                                }
                                if (camControl.rightEyeLodged) {                                                                    //          If the right eye is in...
                                    rightEye.transform.Rotate(rotX * lookSensitivity * vertLookLimiter, 0, 0);                                 //              Do same as with left eye^^^
                                }
                            }

                            tedHead.transform.Rotate(0, rotY * lookSensitivity, 0);
                        }
                        break;
                    case (MovementModes.Flying):
                        break;
                    case (MovementModes.Inspecting):
                        break;
                }

            }
        }
    }

    public bool Using() {
        return isUsing;
    }

    public bool Freezing() {
        return useFreeze;
    }

    // For other scripts to determine whether Ted is inside a vehicle--e.g., Tool Selection should not be enabled when he is in a vehicle
    public bool InVehicle() {
        return isInVehicle;
    }

    public void StopUsing() {
        isUsing = false;
        usingFreezeTimerRef = Time.time;
    }

    public float GetUseFreezeTimeRemaining() {
        if (isUsing) { return (usingFreezeTime - (Time.time - usingFreezeTimerRef));
        } else return 0;
    }

    public float GetPercentageOfFreezeTimeRemaining() {
        if (isUsing) { return (GetUseFreezeTimeRemaining() / usingFreezeTime); }
        else return 0;
    }

    public bool IsHoldingDocument() {
        return (docHeld != null);
    }

    public void SetDocumentHeld(IDDocument doc) {
        docHeld = doc;
    }

    // Get normal vector from floor underneath Ted for determining move speed and anim speed adjustments
    private Vector3 GetDirectionOfFloor() {
        RaycastHit hit;
        if (Physics.Raycast(ted.transform.position, -ted.transform.up, out hit, 100)) {
            return new Vector3(hit.normal.x, 0, hit.normal.z);
        } else return Vector3.zero;
    }

    public void ChangeMovementMode(MovementModes mode) {
        movementScheme = mode;
    }

    // Starts "using" action which triggers an animation
    public void InitiateUseActionWithAnimationTrigger(string triggerName, float actionTime, bool freeze) {
        //anim.SetLayerWeight(3, 1);          // Sets weight of additive layer "Using" to max
        anim.SetTrigger(triggerName);
        anim.SetBool("Using", true);
        usingFreezeTime = actionTime;
        usingFreezeTimerRef = Time.time;
        isUsing = true;
        useFreeze = freeze;
        if (useFreeze) {
            anim.speed = 1;
            animHead.speed = 1;
        }
    }

    public void SwitchAnimationStateToIdle() {
        anim.SetBool("ForwardMoving", false);
        animHead.SetBool("ForwardMoving", false);
        anim.SetBool("LeftStepping", false);
        animHead.SetBool("LeftStepping", false);
        anim.SetBool("RightStepping", false);
        animHead.SetBool("RightStepping", false);
        anim.SetBool("BackwardMoving", false);
        animHead.SetBool("BackwardMoving", false);
        anim.SetBool("Driving", false);
        anim.SetBool("Using", false);
        isMovingForward = false;
        isMovingBackwards = false;
        isRightStepping = false;
        isLeftStepping = false;
    }

    public void SetAnimationStates(string animName) {
        switch (animName) {
            case ("Move Forward"):
                anim.SetBool("ForwardMoving", true);
                animHead.SetBool("ForwardMoving", true);
                anim.SetBool("LeftStepping", false);
                animHead.SetBool("LeftStepping", false);
                anim.SetBool("RightStepping", false);
                animHead.SetBool("RightStepping", false);
                anim.SetBool("BackwardMoving", false);
                animHead.SetBool("BackwardMoving", false);
                isLeftStepping = false;
                isRightStepping = false;
                isMovingBackwards = false;
                break;
            case ("Move Backwards"):
                anim.SetBool("ForwardMoving", false);
                animHead.SetBool("ForwardMoving", false);
                anim.SetBool("LeftStepping", false);
                animHead.SetBool("LeftStepping", false);
                anim.SetBool("RightStepping", false);
                animHead.SetBool("RightStepping", false);
                anim.SetBool("BackwardMoving", true);
                animHead.SetBool("BackwardMoving", true);
                isLeftStepping = false;
                isRightStepping = false;
                isMovingBackwards = true;
                break;
            case ("Step Right"):
                anim.SetBool("ForwardMoving", false);
                animHead.SetBool("ForwardMoving", false);
                anim.SetBool("LeftStepping", false);
                animHead.SetBool("LeftStepping", false);
                anim.SetBool("RightStepping", true);
                animHead.SetBool("RightStepping", true);
                anim.SetBool("BackwardMoving", false);
                animHead.SetBool("BackwardMoving", false);
                isRightStepping = true;
                isLeftStepping = false;
                isMovingBackwards = false;
                break;
            case ("Step Left"):
                anim.SetBool("ForwardMoving", false);
                animHead.SetBool("ForwardMoving", false);
                anim.SetBool("LeftStepping", true);
                animHead.SetBool("LeftStepping", true);
                anim.SetBool("RightStepping", false);
                animHead.SetBool("RightStepping", false);
                anim.SetBool("BackwardMoving", false);
                animHead.SetBool("BackwardMoving", false);
                isRightStepping = false;
                isLeftStepping = true;
                isMovingBackwards = false;
                break;
        }
    }

    public float GetFOV() {
        return cam.fieldOfView;
    }

    public void OperateVehicle(IDVehicle ident) {
        movementScheme = MovementModes.Driving;
        currentVehicleID = ident;
        currentSteeringWheel = currentVehicleID.vehicle.GetComponent<CarControls>().steeringWheel;
        if (currentVehicleID.vehicle != null) {
            if (currentVehicleID.vehicle.GetComponent<CarControls>() != null) {
                currentCarControls = currentVehicleID.vehicle.GetComponent<CarControls>();
            }
        }

        foreach (Transform fWheel in ident.frontWheels) {
            frontWheels.Add(fWheel);
        }
        foreach (Transform bWheel in ident.backWheels) {
            backWheels.Add(bWheel);
        }

        float longestEntranceActionTime = 0;

        foreach (Action act in ident.entranceActions) {
            if (act.Duration > longestEntranceActionTime) { longestEntranceActionTime = act.Duration; }
        }

        enteringVehicle = true;
        isUsing = true;
        isInVehicle = true;
        usingFreezeTime = longestEntranceActionTime;
        usingFreezeTimerRef = Time.time;
    }

    public void LeaveVehicle(IDVehicle ident) {
        anim.speed = 1;
        tedHead.transform.localRotation = tedHeadDefaultRotationInVehicle;

        currentVehicleID.vehicle.GetComponent<CarControls>().ResetSteeringWheel();

        if (ident.driverReorientationAfterExit != null) {
            actionsAfterUsing.actions.Clear();
            actionsAfterUsing.actions.Add(ident.driverReorientationAfterExit);
        }

        float longestEntranceActionTime = 0;

        foreach (Action act in ident.exitActions) {
            if (act.Duration > longestEntranceActionTime) { longestEntranceActionTime = act.Duration; }
        }

        exitingVehicle = true;
        isUsing = true;

        //anim.SetLayerWeight(7, 0);
        anim.SetLayerWeight(8, 0);
        anim.SetLayerWeight(9, 0);

        usingFreezeTime = longestEntranceActionTime;
        usingFreezeTimerRef = Time.time;
    }

    public void TriggerVehicleInteriorAnimation(string trigger) {
        if (trigger != previousDrivingAnimationTrigger) {
            if (trigger != "" && !isUsing) {
                anim.SetTrigger(trigger);
                switch (trigger) {
                    case ("Open ZXR interior handle"):
                        currentLeftArmDrivingAnimationState = "Left Arm Driving.Exit car";
                        currentRightArmDrivingAnimationState = "Right Arm Driving.Steering";

                        leftArmIsPerformingSpecialDrivingTask = true;
                        rightArmIsPerformingSpecialDrivingTask = false;
                        break;
                }

                currentDrivingArmSpecialTaskAnimationNormalizedTime = 0;
            }
            else {
                anim.SetTrigger(interiorStopUsingAnimationTrigger);
                currentLeftArmDrivingAnimationState = "Left Arm Driving.Steering";
                currentRightArmDrivingAnimationState = "Right Arm Driving.Steering";

                leftArmIsPerformingSpecialDrivingTask = false;
                rightArmIsPerformingSpecialDrivingTask = false;
            }
        }

        previousDrivingAnimationTrigger = trigger;
    }
}
