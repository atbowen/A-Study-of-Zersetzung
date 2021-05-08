using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkDesk : MonoBehaviour
{
    public bool deskEnabled;
    public RawImage[] workDeskScreens;

    public RawImage workDeskHeader, workDeskCameraImage, upArrowSelector, downArrowSelector, leftArrowSelector, rightArrowSelector, workDeskFunctionTitleBgd,
                    windowObservationBgd;
    public Text workDeskFunctionTitle, windowObservationText;

    public Camera workDeskCamera, rightWindowReflectionCamera, leftWindowReflectionCamera;
    public RenderTexture workDeskCamRT;
    public Transform startupCamPos, fileCabinetCamPos, fileCabinetInspectionCamPos, pharmBookCamPos, pharmBookInspectionCamPos,
                        windowCamPos, windowInspectionCamPos, deskMatCamPos, deskMatInspectionCamPos,
                        messengerCamPos, messengerInspectionCamPos, storageBoxCamPos, fuseBoxCamPos, fuseBoxInspectionCamPos,
                        leftWindowReflectionCamPos, rightWindowReflectionCamPos, leftWindowReflectionInspectionCamPos, rightWindowReflectionInspectionCamPos,
                        cabinetDrawer, pharmBook, storageLid, fuseBoxCover;
    public string fileCabinetFunctionTitle, pharmaBookFunctionTitle, windowFunctionTitle, deskMatFunctionTitle, messagerFunctionTitle, fuseBoxFunctionTitle, storageBoxFunctionTitle;
    public string cabinetDrawerOpenTrigger, cabinetDrawerCloseTrigger,
                    pharmBookOpenTrigger, pharmBookCloseTrigger,
                    storageLidOpenTrigger, storageLidCloseTrigger,
                    fuseBoxCoverOpenTrigger, fuseBoxCoverCloseTrigger;
    public List<Texture> workDeskStartupTransitionFrames, workDeskShutdownTransitionFrames, workDeskStartupHeaderFrames, workDeskShutdownHeaderFrames;
    public float workDeskStartupTransitionTime, workDeskShutdownTransitionTime, stopCamSmoothingAtThisDistance, camSmoothingRate;
    private bool justArrivedAtCurCamPos, atCurCamPos, movingWindowReflectionCams, leftWindowReflectionCamAtPos, rightWindowReflectionCamAtPos;

    private Transform curCamPos, prevCamPos;

    private bool workDeskIsTransitioning;
    private int curWorkDeskTransitionFrameIndex;
    private float workDeskTransitionRefTime;

    public string openDeskSound, closeDeskSound, changeItemSound, cantChangeItemSound, useItemSound, switchTabSound;

    private bool curFunctionIsActive, delayCurFunctionDeactivation;
    private float delayCurFunctionDeactivationRefTime;

    // Project Review folder animation control parameters
    public Transform projectFolder;
    public List<Transform> projectFolderPositions;
    [SerializeField]
    private float projectFolderPullDelay, folderAnimationDelay;
    private float projectFolderPullDelayRefTime, folderAnimationDelayRefTime;
    private int curProjectFolderPositionsIndex;
    private Animator projectFolderAnim;
    [SerializeField]
    private string projectFolderTriggerStringToOpen, projectFolderTriggerStringToClose;
    [SerializeField]
    private float projectFolderSmoothRate;
    private bool projectFolderIsMovingToInspectionPos, projectFolderIsInFinalPosition, folderIsAnimating;

    // Status function parameters
    public Text eyeStatusTxt, eyeStatusHeaderTxt, hostStatusTxt, hostStatusHeaderTxt;
    [SerializeField]
    private Transform hostModel, eyeModel, eyeModelCamPos;
    private Transform eyeModelParent;
    [SerializeField]
    private string hostModelApproachTriggerString;
    private Vector3 eyeModelInitialLocalPos;
    private Quaternion eyeModelInitialLocalRot;
    [SerializeField]
    private float eyeModelSmoothMoveRate, eyeModelSmoothMoveTolerance, eyeModelDislodgeTimeDelay;
    private float eyeModelDislodgeDelayRefTime;
    [SerializeField]
    private string hostNameTitleSpecies, hostHeightWeightBloodType, hostHealthSapLevelConnectionStrength, hostExtraEffects,
                    eyeNameVersionConnection, eyeExtraEffects;
    [SerializeField]
    private string hostNameHeader, hostHeightHeader, hostHealthHeader, hostEffectsHeader,
                    eyeNameHeader, eyeEffectsHeader;
    [SerializeField]
    private Color statusTextHeaderColor, statusTextResultsColor;
    [SerializeField]
    private float hostNameDisplayDelay, hostHeightDisplayDelay, hostHealthDisplayDelay, hostEffectsDisplayDelay,
                    eyeNameDisplayDelay, eyeEffectsDisplayDelay;
    [SerializeField]
    private string statusTextRandomCharString;
    [SerializeField]
    private float charUnravelTimeMin, charUnravelTimeMax;
    [SerializeField, Range(0.0f, 1.0f)]
    private float chanceToDisplayRandomChar, chanceToResetString;
    private string curHostNameString, curHostHeightString, curHostHealthString, curHostEffectsString,
                    curEyeNameString, curEyeEffectsString;
    private bool justDisplayedRandomHostNameChar, justDisplayedRandomHostHeightChar, justDisplayedRandomHostHealthChar, justDisplayedRandomHostEffectsChar,
                    justDisplayedRandomEyeNameChar, justDisplayedRandomEyeEffectsChar;
    private float hostNameCharUnravelTime, hostHeightCharUnravelTime, hostHealthCharUnravelTime, hostEffectsCharUnravelTime,
                    eyeNameCharUnravelTime, eyeEffectsCharUnravelTime;
    private float hostNameUnravelFrameRefTime, hostHeightUnravelFrameRefTime, hostHealthUnravelFrameRefTime, hostEffectsUnravelFrameRefTime,
                    eyeNameUnravelFrameRefTime, eyeEffectsUnravelFrameRefTime;
    private int hostNameCharIndex, hostHeightCharIndex, hostHealthCharIndex, hostEffectsCharIndex,
                    eyeNameCharIndex, eyeEffectsCharIndex;
    [SerializeField, TextArea]
    private string windowObservation;

    public List<GameObject> earthPedestrians;
    public List<Transform> pedestrianSpawnPointsSetOne, pedestrianSpawnPointsSetTwo,
                            pedestrianDisposalPointsSetOne, pedestrianDisposalPointsSetTwo;

    private List<Transform> currentPedestrians = new List<Transform>();

    [SerializeField]
    private int pedestrianNumberMin, pedestrianNumberMax;
    [SerializeField]
    private float pedestrianWalkSpeed, timeBetweenSpawns;
    private float spawnRefTime;

    private Teddy ted;
    private DeskInventory deskInv;
    private ProjectReview projReview;
    private SystemScreen sysScreen;
    private DigestScreen compendiumScreen;
    private ToolsScreen toolScreen;    
    private MailScreen mailManager;
    private NewsTicker tickerFeed;

    private DeskScreen deskScreen;
        
    private MusicPlayer musicBox;

    private enum WorkDeskMode { Selecting, Projects, Digest, Stats, Tools, Messages, Inventory, System }
    private WorkDeskMode WorkDeskState;

    private enum DeskMode { Inactive, Desk, Tools, Mail, System};
    private DeskMode deskState;
    private const int numOfDeskModes = 4;

    private int currentItemIndex;
    private float itemSelectionTimer;
    private const float itemSelectionDelay = 0.3f;

    private bool joystickCentered;

    // Start is called before the first frame update
    void Start()
    {
        projReview = FindObjectOfType<ProjectReview>();
        deskInv = FindObjectOfType<DeskInventory>();

        ted = FindObjectOfType<Teddy>();
        mailManager = FindObjectOfType<MailScreen>();
        tickerFeed = FindObjectOfType<NewsTicker>();
        deskScreen = FindObjectOfType<DeskScreen>();
        toolScreen = FindObjectOfType<ToolsScreen>();
        sysScreen = FindObjectOfType<SystemScreen>();
        compendiumScreen = FindObjectOfType<DigestScreen>();
        musicBox = FindObjectOfType<MusicPlayer>();
        projectFolderAnim = projectFolder.GetComponent<Animator>();

        HideAllSpringScreens();
        ShowWorkDesk(false);
        toolScreen.showTools = false;
        //sysScreen.showOptions = false;
        //deskScreen.DeactivateDeskScreen();

        workDeskCamera.enabled = false;
        rightWindowReflectionCamera.enabled = false;
        leftWindowReflectionCamera.enabled = false;
        movingWindowReflectionCams = false;
        leftWindowReflectionCamAtPos = true;
        rightWindowReflectionCamAtPos = true;

        workDeskHeader.enabled = false;
        workDeskCameraImage.enabled = false;
        workDeskFunctionTitleBgd.enabled = false;
        upArrowSelector.enabled = false;
        downArrowSelector.enabled = false;
        leftArrowSelector.enabled = false;
        rightArrowSelector.enabled = false;

        workDeskFunctionTitle.enabled = false;

        workDeskIsTransitioning = false;
        justArrivedAtCurCamPos = false;
        atCurCamPos = false;

        curCamPos = deskMatCamPos;
        //cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerOpenTrigger);

        curProjectFolderPositionsIndex = 0;
        projectFolderIsInFinalPosition = true;

        hostStatusTxt.supportRichText = true;
        hostStatusHeaderTxt.supportRichText = true;
        eyeStatusTxt.supportRichText = true;
        eyeStatusHeaderTxt.supportRichText = true;

        eyeModelParent = eyeModel.parent;
        eyeModelInitialLocalPos = eyeModel.localPosition;
        eyeModelInitialLocalRot = eyeModel.localRotation;

        curFunctionIsActive = false;

        deskEnabled = false;
        deskState = DeskMode.Inactive;

        //WorkDeskState = WorkDeskMode.Selecting;
        WorkDeskState = WorkDeskMode.Tools;

        currentItemIndex = 0;
        itemSelectionTimer = 0;

        joystickCentered = true;
    }

    // Update is called once per frame
    void Update()
    {
        //if (openDesk) {
        //    musicBox.PlaySFX(openDeskSound);
        //    openDesk = false;
        //}

        //if (closeDesk) {
        //    musicBox.PlaySFX(closeDeskSound);
        //    closeDesk = false;
        //}

        if (deskEnabled) {

            ManageStatusScreenPedestrians();
            DestroyMarkedSpawnedPedestrians();

            if (workDeskIsTransitioning) {

                if (Time.time - workDeskTransitionRefTime > workDeskStartupTransitionTime) {
                    if ((curWorkDeskTransitionFrameIndex < workDeskStartupTransitionFrames.Count - 1) && (curWorkDeskTransitionFrameIndex < workDeskStartupHeaderFrames.Count - 1)) {

                        curWorkDeskTransitionFrameIndex++;
                        workDeskCameraImage.texture = workDeskStartupTransitionFrames[curWorkDeskTransitionFrameIndex];
                        workDeskHeader.texture = workDeskStartupHeaderFrames[curWorkDeskTransitionFrameIndex];

                        workDeskTransitionRefTime = Time.time;
                    }
                    else {
                        workDeskCameraImage.texture = workDeskCamRT;

                        rightWindowReflectionCamera.enabled = true;
                        leftWindowReflectionCamera.enabled = true;

                        workDeskFunctionTitle.enabled = true;
                        workDeskFunctionTitleBgd.enabled = true;

                        workDeskIsTransitioning = false;
                    }
                }

            }
            else {

                tickerFeed.showTicker = true;

                //// This is important for triggering things just when the camera arrives at the destination (but not repeatedly afterward)
                //if (curCamPos != prevCamPos) {
                //    atCurCamPos = false;
                //    justArrivedAtCurCamPos = false;
                //}

                MoveWorkDeskCameraIntoPosition();

                // Only allow movement from station to station if no function is currently engaged
                if (!curFunctionIsActive) {

                    ShowWorkDeskFunctionSelectorArrows(curCamPos);

                    if ((Time.time - itemSelectionTimer) > itemSelectionDelay) {
                        if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                            if (curCamPos == fileCabinetCamPos) {
                                curCamPos = pharmBookCamPos;
                                //cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerCloseTrigger);
                                //pharmBook.GetComponent<Animator>().SetTrigger(pharmBookOpenTrigger);
                                WorkDeskState = WorkDeskMode.Digest;
                            }
                            else if (curCamPos == pharmBookCamPos) {
                                curCamPos = deskMatCamPos;
                                //pharmBook.GetComponent<Animator>().SetTrigger(pharmBookCloseTrigger);
                                WorkDeskState = WorkDeskMode.Tools;
                            }
                            else if (curCamPos == windowCamPos) {
                                curCamPos = messengerCamPos;
                                ShowStatusWindows(false);
                                eyeModel.SetParent(eyeModelParent);
                                eyeModel.localPosition = eyeModelInitialLocalPos;
                                eyeModel.localRotation = eyeModelInitialLocalRot;
                                WorkDeskState = WorkDeskMode.Messages;
                            }
                            else if (curCamPos == deskMatCamPos) {
                                curCamPos = messengerCamPos;
                                WorkDeskState = WorkDeskMode.Messages;
                            }
                            else if (curCamPos == messengerCamPos) {
                                curCamPos = storageBoxCamPos;
                                //storageLid.GetComponent<Animator>().SetTrigger(storageLidOpenTrigger);
                                WorkDeskState = WorkDeskMode.Inventory;
                            }
                            else if (curCamPos == storageBoxCamPos) {

                            }
                            else if (curCamPos == fuseBoxCamPos) {
                                curCamPos = storageBoxCamPos;
                                //fuseBoxCover.GetComponent<Animator>().SetTrigger(fuseBoxCoverCloseTrigger);
                                //storageLid.GetComponent<Animator>().SetTrigger(storageLidOpenTrigger);
                                WorkDeskState = WorkDeskMode.Inventory;
                            }

                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {

                            if (curCamPos == fileCabinetCamPos) {

                            }
                            else if (curCamPos == pharmBookCamPos) {
                                curCamPos = fileCabinetCamPos;
                                //cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerOpenTrigger);
                                //pharmBook.GetComponent<Animator>().SetTrigger(pharmBookCloseTrigger);
                                WorkDeskState = WorkDeskMode.Projects;
                            }
                            else if (curCamPos == windowCamPos) {
                                curCamPos = fileCabinetCamPos;
                                //cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerOpenTrigger);
                                ShowStatusWindows(false);
                                eyeModel.SetParent(eyeModelParent);
                                eyeModel.localPosition = eyeModelInitialLocalPos;
                                eyeModel.localRotation = eyeModelInitialLocalRot;
                                WorkDeskState = WorkDeskMode.Projects;
                            }
                            else if (curCamPos == deskMatCamPos) {
                                curCamPos = pharmBookCamPos;
                                //pharmBook.GetComponent<Animator>().SetTrigger(pharmBookOpenTrigger);
                                WorkDeskState = WorkDeskMode.Digest;
                            }
                            else if (curCamPos == messengerCamPos) {
                                curCamPos = deskMatCamPos;
                                WorkDeskState = WorkDeskMode.Tools;
                            }
                            else if (curCamPos == storageBoxCamPos) {
                                curCamPos = messengerCamPos;
                                //storageLid.GetComponent<Animator>().SetTrigger(storageLidCloseTrigger);
                                WorkDeskState = WorkDeskMode.Messages;
                            }
                            else if (curCamPos == fuseBoxCamPos) {
                                curCamPos = messengerCamPos;
                                //fuseBoxCover.GetComponent<Animator>().SetTrigger(fuseBoxCoverCloseTrigger);
                                WorkDeskState = WorkDeskMode.Messages;
                            }

                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") > 0.1) {

                            if (curCamPos == fileCabinetCamPos) {
                                curCamPos = windowCamPos;
                                //cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerCloseTrigger);
                                WorkDeskState = WorkDeskMode.Stats;
                                hostModel.GetComponent<Animator>().SetTrigger(hostModelApproachTriggerString);
                                eyeModelDislodgeDelayRefTime = Time.time;
                            }
                            else if (curCamPos == pharmBookCamPos) {
                                curCamPos = windowCamPos;
                                //pharmBook.GetComponent<Animator>().SetTrigger(pharmBookCloseTrigger);
                                WorkDeskState = WorkDeskMode.Stats;
                                hostModel.GetComponent<Animator>().SetTrigger(hostModelApproachTriggerString);
                                eyeModelDislodgeDelayRefTime = Time.time;
                            }
                            else if (curCamPos == windowCamPos) {

                            }
                            else if (curCamPos == deskMatCamPos) {
                                curCamPos = windowCamPos;
                                WorkDeskState = WorkDeskMode.Stats;
                                hostModel.GetComponent<Animator>().SetTrigger(hostModelApproachTriggerString);
                                eyeModelDislodgeDelayRefTime = Time.time;
                            }
                            else if (curCamPos == messengerCamPos) {
                                curCamPos = fuseBoxCamPos;
                                //fuseBoxCover.GetComponent<Animator>().SetTrigger(fuseBoxCoverOpenTrigger);
                                WorkDeskState = WorkDeskMode.System;
                            }
                            else if (curCamPos == storageBoxCamPos) {
                                curCamPos = fuseBoxCamPos;
                                //storageLid.GetComponent<Animator>().SetTrigger(storageLidCloseTrigger);
                                //fuseBoxCover.GetComponent<Animator>().SetTrigger(fuseBoxCoverOpenTrigger);
                                WorkDeskState = WorkDeskMode.System;
                            }
                            else if (curCamPos == fuseBoxCamPos) {

                            }

                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") < -0.1) {

                            if (curCamPos == fileCabinetCamPos) {

                            }
                            else if (curCamPos == pharmBookCamPos) {

                            }
                            else if (curCamPos == windowCamPos) {
                                curCamPos = pharmBookCamPos;
                                //pharmBook.GetComponent<Animator>().SetTrigger(pharmBookOpenTrigger);
                                ShowStatusWindows(false);
                                eyeModel.SetParent(eyeModelParent);
                                eyeModel.localPosition = eyeModelInitialLocalPos;
                                eyeModel.localRotation = eyeModelInitialLocalRot;
                                WorkDeskState = WorkDeskMode.Digest;
                            }
                            else if (curCamPos == deskMatCamPos) {

                            }
                            else if (curCamPos == messengerCamPos) {

                            }
                            else if (curCamPos == storageBoxCamPos) {

                            }
                            else if (curCamPos == fuseBoxCamPos) {
                                curCamPos = messengerCamPos;
                                //fuseBoxCover.GetComponent<Animator>().SetTrigger(fuseBoxCoverCloseTrigger);
                                WorkDeskState = WorkDeskMode.Messages;
                            }

                            itemSelectionTimer = Time.time;
                        }
                    }
                }

                switch (WorkDeskState) {
                    case WorkDeskMode.Selecting:

                        break;
                    case WorkDeskMode.Projects:

                        if (Time.time - projectFolderPullDelayRefTime > projectFolderPullDelay && projectFolderIsMovingToInspectionPos) {

                            if (!projectFolderIsInFinalPosition) {

                                MoveProjectFileFolder();
                            }
                            else {
                                if (projReview.IsInactive()) {

                                    if (!folderIsAnimating) {

                                        projectFolderAnim.SetTrigger(projectFolderTriggerStringToOpen);
                                        folderIsAnimating = true;
                                        folderAnimationDelayRefTime = Time.time;
                                    }
                                    else {

                                        if (Time.time - folderAnimationDelayRefTime > folderAnimationDelay) {

                                            projReview.MakeActive(true);
                                            folderIsAnimating = false;
                                        }
                                    }
                                }
                            }
                        }

                        if (delayCurFunctionDeactivation && projReview.IsInactive()) {
                            if (!projectFolderIsInFinalPosition) {

                                if (!folderIsAnimating) {

                                    projectFolderAnim.SetTrigger(projectFolderTriggerStringToClose);
                                    folderIsAnimating = true;
                                    folderAnimationDelayRefTime = Time.time;

                                }
                                else {

                                    if (Time.time - folderAnimationDelayRefTime > folderAnimationDelay) {

                                        MoveProjectFileFolder();

                                    }

                                }
                            }
                            else {

                                folderIsAnimating = false;
                                curFunctionIsActive = false;
                                curCamPos = fileCabinetCamPos;
                                cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerCloseTrigger);
                                ShowWorkDeskFunctionSelectorArrows(fileCabinetCamPos);
                                itemSelectionTimer = Time.time;

                                delayCurFunctionDeactivation = false;
                            }
                        }

                        if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                            if (Input.GetButtonDown("X Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (!projReview.IsActive() && !curFunctionIsActive) {
                                    curFunctionIsActive = true;
                                    curCamPos = fileCabinetInspectionCamPos;
                                    cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerOpenTrigger);
                                    projectFolderIsInFinalPosition = false;
                                    curProjectFolderPositionsIndex = 1;     // Skip the first position, because after the drawer finishes opening, the folder WILL be in the first position
                                    projectFolderIsMovingToInspectionPos = true;
                                    projectFolderPullDelayRefTime = Time.time;
                                    HideWorkDeskFunctionSelectorArrows();
                                    itemSelectionTimer = Time.time;
                                }
                            }

                            if (Input.GetButtonDown("Circle Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (projReview.IsActive()) {
                                    projReview.MakeActive(false);
                                    delayCurFunctionDeactivation = true;
                                    projectFolderIsInFinalPosition = false;
                                    curProjectFolderPositionsIndex = projectFolderPositions.Count - 1;
                                    projectFolderIsMovingToInspectionPos = false;
                                }
                            }

                            if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                                projReview.PressUpLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                                projReview.PressDownLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Vertical") > 0.1) {
                                projReview.PressUpRS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Vertical") < -0.1) {
                                projReview.PressDownRS();
                                itemSelectionTimer = Time.time;
                            }
                        }

                        break;
                    case WorkDeskMode.Digest:

                        // When returning from inspecting the Digest, wait for the Digest Screen to reach the inactive state before moving out of the function
                        if (delayCurFunctionDeactivation && compendiumScreen.IsInInactiveState()) {

                            curFunctionIsActive = false;
                            curCamPos = pharmBookCamPos;
                            pharmBook.GetComponent<Animator>().SetTrigger(pharmBookCloseTrigger);
                            ShowWorkDeskFunctionSelectorArrows(pharmBookCamPos);
                            itemSelectionTimer = Time.time;

                            delayCurFunctionDeactivation = false;
                        }

                        if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                            if (Input.GetButtonDown("X Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (!compendiumScreen.IsActive()) {
                                    compendiumScreen.ShowDigestWindows(true);
                                    curFunctionIsActive = true;
                                    curCamPos = pharmBookInspectionCamPos;
                                    pharmBook.GetComponent<Animator>().SetTrigger(pharmBookOpenTrigger);
                                    HideWorkDeskFunctionSelectorArrows();
                                    itemSelectionTimer = Time.time;
                                }
                            }

                            if (Input.GetButtonDown("Circle Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (compendiumScreen.IsActive()) {
                                    compendiumScreen.ShowDigestWindows(false);
                                    delayCurFunctionDeactivation = true;
                                }
                            }

                            if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                                compendiumScreen.PressRightLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                                compendiumScreen.PressLeftLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                                compendiumScreen.PressUpLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                                compendiumScreen.PressDownLS();
                                itemSelectionTimer = Time.time;
                            }
                        }

                        break;
                    case WorkDeskMode.Stats:

                        // Yikes, but whatever

                        if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                            if (Input.GetButtonDown("X Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (curCamPos == windowCamPos) {
                                    ShowStatusWindows(false);
                                    curFunctionIsActive = true;
                                    curCamPos = windowInspectionCamPos;
                                    movingWindowReflectionCams = true;
                                    leftWindowReflectionCamAtPos = false;
                                    rightWindowReflectionCamAtPos = false;
                                    HideWorkDeskFunctionSelectorArrows();
                                    itemSelectionTimer = Time.time;
                                }
                            }

                            if (Input.GetButtonDown("Circle Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (curCamPos == windowInspectionCamPos) {
                                    curCamPos = windowCamPos;
                                    curFunctionIsActive = false;
                                    movingWindowReflectionCams = true;
                                    leftWindowReflectionCamAtPos = false;
                                    rightWindowReflectionCamAtPos = false;
                                    windowObservationBgd.enabled = false;
                                    windowObservationText.enabled = false;
                                    ShowWorkDeskFunctionSelectorArrows(windowCamPos);
                                }
                            }
                        }

                        if (movingWindowReflectionCams) {
                            if (!leftWindowReflectionCamAtPos && !rightWindowReflectionCamAtPos) {
                                if (curCamPos == windowCamPos) {
                                    MoveReflectionCamsIntoInspectionOrientation(leftWindowReflectionCamPos, rightWindowReflectionCamPos);
                                }
                                else if (curCamPos == windowInspectionCamPos) {
                                    MoveReflectionCamsIntoInspectionOrientation(leftWindowReflectionInspectionCamPos, rightWindowReflectionInspectionCamPos);
                                }
                            }
                            else {
                                if (curCamPos == windowInspectionCamPos) {

                                    windowObservationBgd.enabled = true;
                                    windowObservationText.text = windowObservation;
                                    windowObservationText.enabled = true;
                                }

                                movingWindowReflectionCams = false;
                            }
                        }

                        string headerColorHex = ColorUtility.ToHtmlStringRGBA(statusTextHeaderColor);
                        string resultsColorHex = ColorUtility.ToHtmlStringRGBA(statusTextResultsColor);

                        if (justArrivedAtCurCamPos && curCamPos == windowCamPos) {
                            ShowStatusWindows(true);
                            InitializeStatusTexts();

                            hostStatusTxt.text = "<color=#" + headerColorHex + ">" + hostNameHeader + "</color>\n\n\n" +
                                                    "<color=#" + headerColorHex + ">" + hostHeightHeader + "</color>\n\n\n" +
                                                    "<color=#" + headerColorHex + ">" + hostHealthHeader + "</color>\n\n\n" +
                                                    "<color=#" + headerColorHex + ">" + hostEffectsHeader + "</color>";
                            eyeStatusTxt.text = "<color=#" + headerColorHex + ">" + eyeNameHeader + "</color>\n\n\n" +
                                                    "<color=#" + headerColorHex + ">" + eyeEffectsHeader + "</color>\n";

                            justArrivedAtCurCamPos = false;
                        }

                        if (Time.time - eyeModelDislodgeDelayRefTime > eyeModelDislodgeTimeDelay) {

                            if (eyeModel.parent != null) { eyeModel.SetParent(null); }
                            SmoothMoveEyeModelToCamPosition();

                        }

                        if (atCurCamPos && curCamPos == windowCamPos) {                

                            hostStatusTxt.text = "<color=#" + headerColorHex + ">" + hostNameHeader + "</color>\n<color=#" + resultsColorHex + ">" + curHostNameString + "</color>\n\n" +
                                                    "<color=#" + headerColorHex + ">" + hostHeightHeader + "</color>\n<color=#" + resultsColorHex + ">" + curHostHeightString + "</color>\n\n" +
                                                    "<color=#" + headerColorHex + ">" + hostHealthHeader + "</color>\n<color=#" + resultsColorHex + ">" + curHostHealthString + "</color>\n\n" +
                                                    "<color=#" + headerColorHex + ">" + hostEffectsHeader + "</color>\n<color=#" + resultsColorHex + ">" + curHostEffectsString + "</color>";
                            eyeStatusTxt.text = "<color=#" + headerColorHex + ">" + eyeNameHeader + "</color>\n<color=#" + resultsColorHex + ">" + curEyeNameString + "</color>\n\n" +
                                                    "<color=#" + headerColorHex + ">" + eyeEffectsHeader + "</color>\n<color=#" + resultsColorHex + ">" + curEyeEffectsString + "</color>";

                            if ((Time.time - hostNameUnravelFrameRefTime > hostNameCharUnravelTime) && (hostNameCharIndex < hostNameTitleSpecies.ToCharArray().Length)) {

                                if (justDisplayedRandomHostNameChar) { curHostNameString = curHostNameString.Remove(curHostNameString.Length - 1, 1); }

                                bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                                if (displayRandomChar) {
                                    curHostNameString = curHostNameString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length)];
                                    justDisplayedRandomHostNameChar = true;
                                }
                                else {
                                    curHostNameString = curHostNameString + hostNameTitleSpecies.ToCharArray()[hostNameCharIndex];
                                    justDisplayedRandomHostNameChar = false;
                                    hostNameCharIndex++;
                                }

                                hostNameUnravelFrameRefTime = Time.time;
                            }
                            if ((Time.time - hostHeightUnravelFrameRefTime > hostHeightCharUnravelTime) && (hostHeightCharIndex < hostHeightWeightBloodType.ToCharArray().Length)) {

                                if (justDisplayedRandomHostHeightChar) { curHostHeightString = curHostHeightString.Remove(curHostHeightString.Length - 1, 1); }

                                bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                                if (displayRandomChar) {
                                    curHostHeightString = curHostHeightString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length)];
                                    justDisplayedRandomHostHeightChar = true;
                                }
                                else {
                                    curHostHeightString = curHostHeightString + hostHeightWeightBloodType.ToCharArray()[hostHeightCharIndex];
                                    justDisplayedRandomHostHeightChar = false;
                                    hostHeightCharIndex++;
                                }

                                hostHeightUnravelFrameRefTime = Time.time;
                            }
                            if ((Time.time - hostHealthUnravelFrameRefTime > hostHealthCharUnravelTime) && (hostHealthCharIndex < hostHealthSapLevelConnectionStrength.ToCharArray().Length)) {

                                if (justDisplayedRandomHostHealthChar) { curHostHealthString = curHostHealthString.Remove(curHostHealthString.Length - 1, 1); }

                                bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                                if (displayRandomChar) {
                                    curHostHealthString = curHostHealthString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)];
                                    justDisplayedRandomHostHealthChar = true;
                                }
                                else {
                                    curHostHealthString = curHostHealthString + hostHealthSapLevelConnectionStrength.ToCharArray()[hostHealthCharIndex];
                                    justDisplayedRandomHostHealthChar = false;
                                    hostHealthCharIndex++;
                                }

                                hostHealthUnravelFrameRefTime = Time.time;
                            }

                            if ((Time.time - hostEffectsUnravelFrameRefTime > hostEffectsCharUnravelTime) && (hostEffectsCharIndex < hostExtraEffects.ToCharArray().Length)) {

                                if (justDisplayedRandomHostEffectsChar) { curHostEffectsString = curHostEffectsString.Remove(curHostEffectsString.Length - 1, 1); }

                                bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                                if (displayRandomChar) {
                                    curHostEffectsString = curHostEffectsString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)];
                                    justDisplayedRandomHostEffectsChar = true;
                                }
                                else {
                                    curHostEffectsString = curHostEffectsString + hostExtraEffects.ToCharArray()[hostEffectsCharIndex];
                                    justDisplayedRandomHostEffectsChar = false;
                                    hostEffectsCharIndex++;
                                }

                                hostEffectsUnravelFrameRefTime = Time.time;
                            }
                            if ((Time.time - eyeNameUnravelFrameRefTime > eyeNameCharUnravelTime) && (eyeNameCharIndex > -1)) {

                                if (justDisplayedRandomEyeNameChar) { curEyeNameString = curEyeNameString.Remove(0, 1); }

                                bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                                if (displayRandomChar) {
                                    curEyeNameString = statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)] + curEyeNameString;
                                    justDisplayedRandomEyeNameChar = true;
                                }
                                else {
                                    curEyeNameString = eyeNameVersionConnection.ToCharArray()[eyeNameCharIndex] + curEyeNameString;
                                    justDisplayedRandomEyeNameChar = false;
                                    eyeNameCharIndex--;
                                }

                                eyeNameUnravelFrameRefTime = Time.time;
                            }
                            if ((Time.time - eyeEffectsUnravelFrameRefTime > eyeEffectsCharUnravelTime) && (eyeEffectsCharIndex > -1)) {

                                if (justDisplayedRandomEyeEffectsChar) { curEyeEffectsString = curEyeEffectsString.Remove(0, 1); }

                                bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                                if (displayRandomChar) {
                                    curEyeEffectsString = statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)] + curEyeEffectsString;
                                    justDisplayedRandomEyeEffectsChar = true;
                                }
                                else {
                                    curEyeEffectsString = eyeExtraEffects.ToCharArray()[eyeEffectsCharIndex] + curEyeEffectsString;
                                    justDisplayedRandomEyeEffectsChar = false;
                                    eyeEffectsCharIndex--;
                                }

                                eyeEffectsUnravelFrameRefTime = Time.time;
                            }
                        }

                        break;
                    case WorkDeskMode.Tools:

                        if (delayCurFunctionDeactivation && toolScreen.IsInactive()) {
                            curFunctionIsActive = false;
                            curCamPos = deskMatCamPos;
                            ShowWorkDeskFunctionSelectorArrows(deskMatCamPos);
                            itemSelectionTimer = Time.time;

                            delayCurFunctionDeactivation = false;
                        }

                        if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                            if (Input.GetButtonDown("X Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (toolScreen.IsInactive()) {
                                    toolScreen.MakeActive(true);
                                    curFunctionIsActive = true;
                                    curCamPos = deskMatInspectionCamPos;
                                    HideWorkDeskFunctionSelectorArrows();
                                    itemSelectionTimer = Time.time;
                                }
                                else {
                                    toolScreen.PressX();
                                }
                            }

                            if (Input.GetButtonDown("Circle Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (toolScreen.IsSelecting()) {
                                    toolScreen.MakeActive(false);
                                    delayCurFunctionDeactivation = true;
                                    delayCurFunctionDeactivationRefTime = Time.time;
                                }
                                else {
                                    toolScreen.PressCircle();
                                }
                            }

                            if (toolScreen.CanHoldRightStick()) {
                                if (Input.GetAxis("Right Joystick Horizontal") > 0.1) {
                                    toolScreen.PressRightRS(Input.GetAxis("Right Joystick Horizontal"));
                                }
                                if (Input.GetAxis("Right Joystick Horizontal") < -0.1) {
                                    toolScreen.PressLeftRS(Input.GetAxis("Right Joystick Horizontal"));
                                }
                                if (Input.GetAxis("Right Joystick Vertical") > 0.1) {
                                    toolScreen.PressUpRS(Input.GetAxis("Right Joystick Vertical"));
                                }
                                if (Input.GetAxis("Right Joystick Vertical") < -0.1) {
                                    toolScreen.PressDownRS(Input.GetAxis("Right Joystick Vertical"));
                                }
                                if (Mathf.Abs(Input.GetAxis("Right Joystick Horizontal")) < 0.1) {
                                    toolScreen.ReleaseHorizontalRS();
                                }
                                if (Mathf.Abs(Input.GetAxis("Right Joystick Vertical")) < 0.1) {
                                    toolScreen.ReleaseVerticalRS();
                                }
                            }
                            else {
                                if (Input.GetAxis("Right Joystick Horizontal") > 0.1) {
                                    toolScreen.PressRightRS(0);
                                    itemSelectionTimer = Time.time;
                                }
                                if (Input.GetAxis("Right Joystick Horizontal") < -0.1) {
                                    toolScreen.PressLeftRS(0);
                                    itemSelectionTimer = Time.time;
                                }
                            }

                            if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                                toolScreen.PressRightLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                                toolScreen.PressLeftLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                                toolScreen.PressUpLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                                toolScreen.PressDownLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("D-Pad Up Down") > 0.1) {
                                toolScreen.PressUpArrow();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("D-Pad Up Down") < -0.1) {
                                toolScreen.PressDownArrow();
                                itemSelectionTimer = Time.time;
                            }

                            if (toolScreen.CanHoldTriggers()) {
                                if (Input.GetAxis("Right Trigger") > 0.1) {
                                    toolScreen.PressRightTrigger(Input.GetAxis("Triggers"));
                                }
                                if (Input.GetAxis("Left Trigger") > 0.1) {
                                    toolScreen.PressLeftTrigger(Input.GetAxis("Triggers"));
                                }
                                if (Input.GetAxis("Right Trigger") < 0.1) {
                                    toolScreen.ReleaseRightTrigger();
                                }
                                if (Input.GetAxis("Left Trigger") < 0.1) {
                                    toolScreen.ReleaseLeftTrigger();
                                }
                            }
                        }

                        break;
                    case WorkDeskMode.Messages:

                        if (delayCurFunctionDeactivation && mailManager.IsInInactiveState()) {

                            curFunctionIsActive = false;
                            curCamPos = messengerCamPos;
                            ShowWorkDeskFunctionSelectorArrows(messengerCamPos);
                            itemSelectionTimer = Time.time;

                            delayCurFunctionDeactivation = false;
                        }

                        if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                            if (Input.GetButtonDown("X Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (!mailManager.IsActive()) {
                                    mailManager.MakeActive(true);
                                    curFunctionIsActive = true;
                                    curCamPos = messengerInspectionCamPos;
                                    HideWorkDeskFunctionSelectorArrows();
                                    itemSelectionTimer = Time.time;
                                }
                                else {
                                    mailManager.PressX();
                                    itemSelectionTimer = Time.time;
                                }
                            }

                            if (Input.GetButtonDown("Circle Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (mailManager.CanBeExited()) {
                                    mailManager.MakeActive(false);
                                    delayCurFunctionDeactivation = true;
                                }
                                else {
                                    mailManager.PressCircle();
                                }
                            }

                            if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                                mailManager.PressRightLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                                mailManager.PressLeftLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                                mailManager.PressUpLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                                mailManager.PressDownLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Horizontal") > 0.1) {
                                mailManager.PressRightRS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Horizontal") < -0.1) {
                                mailManager.PressLeftRS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Vertical") > 0.1) {
                                mailManager.PressUpRS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Vertical") < -0.1) {
                                mailManager.PressDownRS();
                                itemSelectionTimer = Time.time;
                            }
                        }
                        break;
                    case WorkDeskMode.Inventory:

                        if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                            if (Input.GetButtonDown("X Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (!deskInv.IsActiveAndSelecting()) {
                                    deskInv.MakeActive(true);
                                    curFunctionIsActive = true;
                                    storageLid.GetComponent<Animator>().SetTrigger(storageLidOpenTrigger);
                                    HideWorkDeskFunctionSelectorArrows();
                                    itemSelectionTimer = Time.time;
                                }
                                else {
                                    deskInv.PressX();
                                    itemSelectionTimer = Time.time;
                                }  
                            }

                            if (Input.GetButtonDown("Circle Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (deskInv.IsActiveAndSelecting()) {
                                    deskInv.MakeActive(false);
                                    curFunctionIsActive = false;
                                    storageLid.GetComponent<Animator>().SetTrigger(storageLidCloseTrigger);
                                    ShowWorkDeskFunctionSelectorArrows(storageBoxCamPos);
                                    itemSelectionTimer = Time.time;
                                }
                                else {
                                    deskInv.PressCircle();
                                    itemSelectionTimer = Time.time;
                                }
                            }

                            if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                                deskInv.PressRightLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                                deskInv.PressLeftLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                                deskInv.PressUpLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                                deskInv.PressDownLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Horizontal") > 0.1) {
                                deskInv.PressRightRS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Horizontal") < -0.1) {
                                deskInv.PressLeftRS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Vertical") > 0.1) {
                                deskInv.PressUpRS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Right Joystick Vertical") < -0.1) {
                                deskInv.PressDownRS();
                                itemSelectionTimer = Time.time;
                            }
                        }
                        break;
                    case WorkDeskMode.System:

                        if (curCamPos == fuseBoxInspectionCamPos && justArrivedAtCurCamPos) {
                            sysScreen.ActivateSystemOptions(true);
                        }

                        if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                            if (Input.GetButtonDown("X Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (!sysScreen.IsActive()) {
                                    curFunctionIsActive = true;
                                    curCamPos = fuseBoxInspectionCamPos;
                                    fuseBoxCover.GetComponent<Animator>().SetTrigger(fuseBoxCoverOpenTrigger);
                                    HideWorkDeskFunctionSelectorArrows();
                                    itemSelectionTimer = Time.time;
                                }
                                else {
                                    sysScreen.PressX();
                                    itemSelectionTimer = Time.time;
                                }
                            }

                            if (Input.GetButtonDown("Circle Button")) {
                                // IsActive() checks if the inventory function is active AND if the state is selecting
                                if (sysScreen.IsActive() && sysScreen.IsInSelectingMode()) {
                                    sysScreen.ActivateSystemOptions(false);
                                    curFunctionIsActive = false;
                                    curCamPos = fuseBoxCamPos;
                                    fuseBoxCover.GetComponent<Animator>().SetTrigger(fuseBoxCoverCloseTrigger);
                                    ShowWorkDeskFunctionSelectorArrows(fuseBoxCamPos);
                                    itemSelectionTimer = Time.time;
                                }
                                else {
                                    sysScreen.PressCircle();
                                }
                            }

                            if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                                sysScreen.PressRightLS(Input.GetAxis("Left Joystick Horizontal"));
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                                sysScreen.PressLeftLS(Input.GetAxis("Left Joystick Horizontal"));
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                                sysScreen.PressUpLS();
                                itemSelectionTimer = Time.time;
                            }
                            if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                                sysScreen.PressDownLS();
                                itemSelectionTimer = Time.time;
                            }
                        }

                        break;
                }
            }
        } else {
            //HideAllSpringScreens();

            if (workDeskIsTransitioning) {
                if (Time.time - workDeskTransitionRefTime > workDeskShutdownTransitionTime) {
                    if ((curWorkDeskTransitionFrameIndex < workDeskShutdownTransitionFrames.Count - 1) && (curWorkDeskTransitionFrameIndex < workDeskShutdownHeaderFrames.Count - 1)) {

                        curWorkDeskTransitionFrameIndex++;
                        workDeskCameraImage.texture = workDeskShutdownTransitionFrames[curWorkDeskTransitionFrameIndex];
                        workDeskHeader.texture = workDeskShutdownHeaderFrames[curWorkDeskTransitionFrameIndex];

                        workDeskTransitionRefTime = Time.time;
                    }
                    else {

                        workDeskCamera.enabled = false;
                        rightWindowReflectionCamera.enabled = false;
                        leftWindowReflectionCamera.enabled = false;

                        workDeskHeader.enabled = false;
                        workDeskCameraImage.enabled = false;
                        workDeskFunctionTitleBgd.enabled = false;

                        workDeskFunctionTitle.enabled = false;

                        workDeskIsTransitioning = false;
                    }
                }
            }
        }

        // Set joystickCentered equal to true if the left joystick is centered
        // This will prevent rapid selection
        if (Mathf.Abs(Input.GetAxis("Left Joystick Horizontal")) < 0.1 && Mathf.Abs(Input.GetAxis("Left Joystick Vertical")) < 0.1) { joystickCentered = true; }
    }

    private void MoveWorkDeskCameraIntoPosition() {
        if ((workDeskCamera.transform.position - curCamPos.position).magnitude > stopCamSmoothingAtThisDistance) {
            workDeskCamera.transform.position = new Vector3(Mathf.Lerp(workDeskCamera.transform.position.x, curCamPos.position.x, Time.deltaTime * camSmoothingRate),
                                                            Mathf.Lerp(workDeskCamera.transform.position.y, curCamPos.position.y, Time.deltaTime * camSmoothingRate),
                                                            Mathf.Lerp(workDeskCamera.transform.position.z, curCamPos.position.z, Time.deltaTime * camSmoothingRate));

            workDeskCamera.transform.rotation = Quaternion.Lerp(workDeskCamera.transform.rotation, curCamPos.rotation, Time.deltaTime * camSmoothingRate);

            atCurCamPos = false;
        }
        else {

            if (atCurCamPos) {
                justArrivedAtCurCamPos = false;
            }
            else {
                justArrivedAtCurCamPos = true;
                atCurCamPos = true;
            }
        }
    }

    private void HideAllSpringScreens() {
        ShowStatusWindows(false);
        deskInv.MakeActive(false);
        sysScreen.ActivateSystemOptions(false);
        //deskScreen.DeactivateDeskScreen();
        //mailManager.MakeActive(false);
        tickerFeed.showTicker = false;
        toolScreen.showTools = false;
        //sysScreen.showOptions = false;
    }

    // Initiate opening or closing sequence
    public void ShowWorkDesk(bool yesOrNo) {

        if (yesOrNo) {
            workDeskCamera.transform.position = startupCamPos.position;
            workDeskCamera.transform.rotation = startupCamPos.rotation;

            workDeskCameraImage.texture = workDeskStartupTransitionFrames[0];
            workDeskHeader.texture = workDeskStartupHeaderFrames[0];

            workDeskCamera.enabled = true;
            rightWindowReflectionCamera.enabled = true;

            workDeskHeader.enabled = true;
            workDeskCameraImage.enabled = true;

            curFunctionIsActive = false;
            atCurCamPos = false;
            justArrivedAtCurCamPos = false;

            if (WorkDeskState == WorkDeskMode.Stats) {
                hostModel.GetComponent<Animator>().SetTrigger("Approach");
                eyeModelDislodgeDelayRefTime = Time.time;
            }

            CallDeskOpenSound();
        }
        else {
            workDeskCameraImage.texture = workDeskShutdownTransitionFrames[0];
            workDeskHeader.texture = workDeskShutdownHeaderFrames[0];

            atCurCamPos = false;
            justArrivedAtCurCamPos = false;

            ShowStatusWindows(false);
            windowObservationBgd.enabled = false;
            windowObservationText.enabled = false;
            
            if (deskInv.IsActiveAndSelecting()) {
                deskInv.MakeActive(false);
                storageLid.GetComponent<Animator>().SetTrigger(storageLidCloseTrigger);
            }

            if (mailManager.IsActive()) {
                mailManager.ForceClose();
                curCamPos = messengerCamPos;
            }

            if (compendiumScreen.IsActive()) {
                compendiumScreen.ForceClose();
                curCamPos = pharmBookCamPos;
                pharmBook.GetComponent<Animator>().SetTrigger(pharmBookCloseTrigger);
            }

            if (!projReview.IsInactive()) {
                projReview.ForceClose();
                projectFolderIsMovingToInspectionPos = false;
                curCamPos = fileCabinetCamPos;
                projectFolderAnim.SetTrigger(projectFolderTriggerStringToClose);
                projectFolder.position = projectFolderPositions[0].position;
                projectFolder.rotation = projectFolderPositions[0].rotation;
                cabinetDrawer.GetComponent<Animator>().SetTrigger(cabinetDrawerCloseTrigger);
            }

            CallDeskCloseSound();
        }

        workDeskFunctionTitleBgd.enabled = false;
        workDeskFunctionTitle.enabled = false;

        upArrowSelector.enabled = false;
        downArrowSelector.enabled = false;
        leftArrowSelector.enabled = false;
        rightArrowSelector.enabled = false;

        tickerFeed.showTicker = false;

        deskEnabled = yesOrNo;

        workDeskTransitionRefTime = Time.time;
        curWorkDeskTransitionFrameIndex = 0;

        workDeskIsTransitioning = true;
    }

    private void ShowWorkDeskFunctionSelectorArrows(Transform curPos) {

        workDeskFunctionTitleBgd.enabled = true;
        workDeskFunctionTitle.enabled = true;

        if (curCamPos == fileCabinetCamPos) {
            workDeskFunctionTitle.text = fileCabinetFunctionTitle;

            upArrowSelector.enabled = true;
            downArrowSelector.enabled = false;
            leftArrowSelector.enabled = false;
            rightArrowSelector.enabled = true;
        }
        else if (curCamPos == pharmBookCamPos) {
            workDeskFunctionTitle.text = pharmaBookFunctionTitle;

            upArrowSelector.enabled = true;
            downArrowSelector.enabled = false;
            leftArrowSelector.enabled = true;
            rightArrowSelector.enabled = true;
        }
        else if (curCamPos == windowCamPos) {
            workDeskFunctionTitle.text = windowFunctionTitle;

            upArrowSelector.enabled = false;
            downArrowSelector.enabled = true;
            leftArrowSelector.enabled = true;
            rightArrowSelector.enabled = true;
        }
        else if (curCamPos == deskMatCamPos) {
            workDeskFunctionTitle.text = deskMatFunctionTitle;

            upArrowSelector.enabled = true;
            downArrowSelector.enabled = false;
            leftArrowSelector.enabled = true;
            rightArrowSelector.enabled = true;
        }
        else if (curCamPos == messengerCamPos) {
            workDeskFunctionTitle.text = messagerFunctionTitle;

            upArrowSelector.enabled = true;
            downArrowSelector.enabled = false;
            leftArrowSelector.enabled = true;
            rightArrowSelector.enabled = true;
        }
        else if (curCamPos == storageBoxCamPos) {
            workDeskFunctionTitle.text = storageBoxFunctionTitle;

            upArrowSelector.enabled = true;
            downArrowSelector.enabled = false;
            leftArrowSelector.enabled = true;
            rightArrowSelector.enabled = false;
        }
        else if (curCamPos == fuseBoxCamPos) {
            workDeskFunctionTitle.text = fuseBoxFunctionTitle;

            upArrowSelector.enabled = false;
            downArrowSelector.enabled = true;
            leftArrowSelector.enabled = true;
            rightArrowSelector.enabled = true;
        }
    }

    private void HideWorkDeskFunctionSelectorArrows() {

        workDeskFunctionTitleBgd.enabled = false;
        workDeskFunctionTitle.enabled = false;

        upArrowSelector.enabled = false;
        downArrowSelector.enabled = false;
        leftArrowSelector.enabled = false;
        rightArrowSelector.enabled = false;
    }

    // DIAGNOSTICS FUNCTIONS

    private void ShowStatusWindows(bool show) {

        eyeStatusTxt.enabled = show;
        eyeStatusHeaderTxt.enabled = show;
        hostStatusTxt.enabled = show;
        hostStatusHeaderTxt.enabled = show;

        if (!show) {
            eyeStatusTxt.text = "";
            hostStatusTxt.text = "";

            //eyeModel.SetParent(eyeModelParent);

            //eyeModel.localPosition = eyeModelInitialLocalPos;
            //eyeModel.localRotation = eyeModelInitialLocalRot;
        }
    }

    private void SmoothMoveEyeModelToCamPosition() {
        if ((eyeModel.position - eyeModelCamPos.position).magnitude > eyeModelSmoothMoveTolerance) {
            eyeModel.position = new Vector3(Mathf.Lerp(eyeModel.position.x, eyeModelCamPos.position.x, Time.deltaTime * eyeModelSmoothMoveRate),
                                            Mathf.Lerp(eyeModel.position.y, eyeModelCamPos.position.y, Time.deltaTime * eyeModelSmoothMoveRate),
                                            Mathf.Lerp(eyeModel.position.z, eyeModelCamPos.position.z, Time.deltaTime * eyeModelSmoothMoveRate));

            eyeModel.rotation = Quaternion.Lerp(eyeModel.localRotation, eyeModelCamPos.localRotation, Time.deltaTime * eyeModelSmoothMoveRate);
        }
    }



    private void MoveReflectionCamsIntoInspectionOrientation(Transform curLeftCamPos, Transform curRightCamPos) {
        if ((leftWindowReflectionCamera.transform.position - curLeftCamPos.position).magnitude > stopCamSmoothingAtThisDistance) {
            leftWindowReflectionCamera.transform.position = new Vector3(Mathf.Lerp(leftWindowReflectionCamera.transform.position.x, curLeftCamPos.position.x, Time.deltaTime * camSmoothingRate),
                                                            Mathf.Lerp(leftWindowReflectionCamera.transform.position.y, curLeftCamPos.position.y, Time.deltaTime * camSmoothingRate),
                                                            Mathf.Lerp(leftWindowReflectionCamera.transform.position.z, curLeftCamPos.position.z, Time.deltaTime * camSmoothingRate));

            leftWindowReflectionCamera.transform.rotation = Quaternion.Lerp(leftWindowReflectionCamera.transform.rotation, curLeftCamPos.rotation, Time.deltaTime * camSmoothingRate);

            leftWindowReflectionCamAtPos = false;
        }
        else {

            leftWindowReflectionCamAtPos = true;
        }

        if ((rightWindowReflectionCamera.transform.position - curRightCamPos.position).magnitude > stopCamSmoothingAtThisDistance) {
            rightWindowReflectionCamera.transform.position = new Vector3(Mathf.Lerp(rightWindowReflectionCamera.transform.position.x, curRightCamPos.position.x, Time.deltaTime * camSmoothingRate),
                                                            Mathf.Lerp(rightWindowReflectionCamera.transform.position.y, curRightCamPos.position.y, Time.deltaTime * camSmoothingRate),
                                                            Mathf.Lerp(rightWindowReflectionCamera.transform.position.z, curRightCamPos.position.z, Time.deltaTime * camSmoothingRate));

            rightWindowReflectionCamera.transform.rotation = Quaternion.Lerp(rightWindowReflectionCamera.transform.rotation, curRightCamPos.rotation, Time.deltaTime * camSmoothingRate);

            rightWindowReflectionCamAtPos = false;
        }
        else {

            rightWindowReflectionCamAtPos = true;
        }
    }

    private void MoveProjectFileFolder() {
        if (!projectFolderIsInFinalPosition) {

            Transform nextPos = projectFolderPositions[curProjectFolderPositionsIndex];

            if ((projectFolder.position - nextPos.position).magnitude > stopCamSmoothingAtThisDistance) {
                projectFolder.position = new Vector3(Mathf.Lerp(projectFolder.position.x, nextPos.position.x, projectFolderSmoothRate * Time.deltaTime),
                                                        Mathf.Lerp(projectFolder.position.y, nextPos.position.y, projectFolderSmoothRate * Time.deltaTime),
                                                        Mathf.Lerp(projectFolder.position.z, nextPos.position.z, projectFolderSmoothRate * Time.deltaTime));
            }
            else {
                if (projectFolderIsMovingToInspectionPos) {
                    if (curProjectFolderPositionsIndex < projectFolderPositions.Count - 1) {
                        curProjectFolderPositionsIndex++;
                    }
                    else {
                        projectFolderIsInFinalPosition = true;
                    }
                }
                else {
                    if (curProjectFolderPositionsIndex > 0) {
                        curProjectFolderPositionsIndex--;
                    }
                    else {
                        projectFolderIsInFinalPosition = true;
                    }
                }
            }

            projectFolder.rotation = Quaternion.Lerp(projectFolder.rotation, nextPos.rotation, projectFolderSmoothRate * Time.deltaTime);
        }
    }

    public void InitializeStatusTexts() {

        curHostNameString = "";
        curHostHeightString = "";
        curHostHealthString = "";
        curHostEffectsString = "";

        curEyeNameString = "";
        curEyeEffectsString = "";

        hostNameCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        hostHeightCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        hostHealthCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        hostEffectsCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);

        eyeNameCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        eyeEffectsCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);

        hostNameUnravelFrameRefTime = Time.time;
        hostHeightUnravelFrameRefTime = Time.time;
        hostHealthUnravelFrameRefTime = Time.time;
        hostEffectsUnravelFrameRefTime = Time.time;

        eyeNameUnravelFrameRefTime = Time.time;
        eyeEffectsUnravelFrameRefTime = Time.time;

        hostNameCharIndex = 0;
        hostHeightCharIndex = 0;
        hostHealthCharIndex = 0;
        hostEffectsCharIndex = 0;

        eyeNameCharIndex = eyeNameVersionConnection.ToCharArray().Length - 1;
        eyeEffectsCharIndex = eyeExtraEffects.ToCharArray().Length - 1;

        justDisplayedRandomHostNameChar = false;
        justDisplayedRandomHostHeightChar = false;
        justDisplayedRandomHostHealthChar = false;
        justDisplayedRandomHostEffectsChar = false;

        justDisplayedRandomEyeNameChar = false;
        justDisplayedRandomEyeEffectsChar = false;
    }

    private void ManageStatusScreenPedestrians() {

        if ((Time.time - spawnRefTime > timeBetweenSpawns) && (currentPedestrians.Count < pedestrianNumberMax)) {

            Transform spawnPt;
            bool spawningInSetOne;

            if (Random.Range(0, 2) == 0) {
                spawnPt = pedestrianSpawnPointsSetOne[Random.Range(0, pedestrianSpawnPointsSetOne.Count)];
                spawningInSetOne = true;
            }
            else {
                spawnPt = pedestrianSpawnPointsSetTwo[Random.Range(0, pedestrianSpawnPointsSetTwo.Count)];
                spawningInSetOne = false;
            }

            GameObject newPedestrian = earthPedestrians[Random.Range(0, earthPedestrians.Count)];
            GameObject newPerson =  Instantiate(newPedestrian, spawnPt.position, spawnPt.localRotation);


            if (newPerson.transform.GetComponent<SimplePedestrian>() != null) {

                newPerson.GetComponent<SimplePedestrian>().InitializePedestrian();

                if (spawningInSetOne) {
                    newPerson.transform.GetComponent<SimplePedestrian>().SetDestination(pedestrianDisposalPointsSetTwo[Random.Range(0, pedestrianDisposalPointsSetTwo.Count)]);
                }
                else {
                    newPerson.transform.GetComponent<SimplePedestrian>().SetDestination(pedestrianDisposalPointsSetOne[Random.Range(0, pedestrianDisposalPointsSetOne.Count)]);
                }
            }

            Debug.Log(newPerson.GetComponent<Rigidbody>());

            currentPedestrians.Add(newPerson.transform);

            spawnRefTime = Time.time;

        }
    }

    private void DestroyMarkedSpawnedPedestrians() {
        if (pedestrianDisposalPointsSetOne.Count > 0) {
            foreach (Transform trans in pedestrianDisposalPointsSetOne) {
                if (trans.GetComponent<SpawnedItemDestroyer>() != null) {

                    SpawnedItemDestroyer destroyer = trans.GetComponent<SpawnedItemDestroyer>();

                    if (destroyer.thoseMarkedForDeath.Count > 0) {

                        for (int i = 0; i < destroyer.thoseMarkedForDeath.Count; i++) {
                            if (currentPedestrians.Contains(destroyer.thoseMarkedForDeath[i])) { currentPedestrians.Remove(destroyer.thoseMarkedForDeath[i]); }
                            Destroy(destroyer.thoseMarkedForDeath[i].gameObject);
                        }

                        destroyer.thoseMarkedForDeath.Clear();
                    }
                }
            }
        }
        if (pedestrianDisposalPointsSetTwo.Count > 0) {
            foreach (Transform trans in pedestrianDisposalPointsSetTwo) {
                if (trans.GetComponent<SpawnedItemDestroyer>() != null) {

                    SpawnedItemDestroyer destroyer = trans.GetComponent<SpawnedItemDestroyer>();

                    if (destroyer.thoseMarkedForDeath.Count > 0) {

                        List<Transform> temps = new List<Transform>();

                        for (int i = 0; i < destroyer.thoseMarkedForDeath.Count; i++) {
                            if (currentPedestrians.Contains(destroyer.thoseMarkedForDeath[i])) { currentPedestrians.Remove(destroyer.thoseMarkedForDeath[i]); }
                            Destroy(destroyer.thoseMarkedForDeath[i].gameObject);
                        }

                        destroyer.thoseMarkedForDeath.Clear();
                    }
                }
            }
        }
    }

    public void CallDeskOpenSound() {
        musicBox.PlaySPRINGDeskOpenSound();
    }

    public void CallDeskCloseSound() {
        musicBox.PlaySPRINGDeskCloseSound();
    }
}
