using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolsScreen : MonoBehaviour
{
    // Main tool menu--tool icons, notepad, and current project
    [SerializeField]
    private RawImage toolProjection, toolIconBorders, trajSciIcon, matSciIcon, imageSciIcon, toolIconSelectorBox, notesBgd, projectNumberBgd;
    public List<Texture> toolIconProjectionFrames, toolIconCloseFrames, analysisSelectionFrames, notesBgdOpeningFrames, notesBgdFlippingFrames,
                            trajSciIconFrames, matSciIconFrames, imageSciIconFrames;
    [SerializeField]
    private MeshRenderer springAttractScreenRend;
    public List<Material> springAttractFrames;

    [SerializeField]
    private Text notesTxt, projectNumberTxt;

    [SerializeField]
    private float delayTimeBeforePretransition, delayTimeBeforeTransition, transitionIconFrameTime, transitionNotebookFrameTime,
                    trajSciIconAnimationFrameTime, matSciIconAnimationFrameTime, imageSciIconAnimationFrameTime,
                    notebookClosingFrameTime, projectionClosingFrameTime;
    private float delayProjectionRefTime, delayNotebookRefTime, projectionFrameRefTime, notebookFrameRefTime;
    private int toolIconProjectionFrameIndex, toolIconCloseFrameIndex, notebookFrameIndex, trajSciIconFrameIndex, matSciIconFrameIndex, imageSciIconFrameIndex;
    [SerializeField]
    private int trajSciIconStaticFrameIndex, matSciIconStaticFrameIndex, imageSciIconStaticFrameIndex;

    // Running the SPRING Work Desk attract screen
    [SerializeField]
    private float springAttractExtraLengthBeforeShowingIcons, springAttractFrameTime;
    private int curSpringAttractFrameIndex;
    private float springAttractFrameRefTime;

    // Loading the SPRING Work Desk tools suite
    private string curNotepadText;
    private List<char> notepadTextChars = new List<char>();
    private int curNotepadCharIndex;
    private float notepadTextUnravelRefTime, notepadTextTrailingUnderscoreToggleRefTime;
    private bool notepadTextIsUnravelling, notepadTextTrailingUnderscoreToggleIsActive, notepadTextShowTrailingUnderscore;
    [SerializeField]
    private float delayTimeBeforeShowingNotepadText, notepadeTextUnravelTimePerChar, notepadTextTrailingUnderscoreToggleTime;
    [SerializeField, TextArea]
    private string trajSciIconSelectedNotepadText, matSciIconSelectedNotepadText, imageSciIconSelectedNotepadText;

    // Tools windows
    [SerializeField]
    private RawImage matDataListBgd, matDataListSelector, matDataChartWindow, matDataChartWindowAxes, matDataChartWindowFrame, matDataChartSampleCursor, matDataChartRefCursor, matResultsBgd,
                    matSciMicroscopeView, matSciMicroscopeViewMask, matSciMicroscopeViewFrame, matSciMicroscopeCursor,
                    imageListBgd, imageListSelector, imageSciWindow, imageSciWindowFrame, imageReticle, imageRefMatchImage, imageRefMatchFrame,
                    imageControlKnob, imageVerticalKnob, imageHorizontalKnob,
                    trajScenarioWindow, trajScenarioWindowFrame, trajNumOfPtsBgd, trajTypeBgd, trajNumOfPtsSelector, trajTypeSelector,
                    fileManagementWindow, addButton, deleteButton, techReqButton, fileManagementSelector,
                    fileSelectionWindow, okayButton, cancelButton, fileSelectionBgd;
    public List<Texture> imageControlKnobFrames, imageVerticalKnobFrames, imageHorizontalKnobFrames;
    private int imageControlKnobFrameIndex, imageVerticalKnobFrameIndex, imageHorizontalKnobFrameIndex;
    
    [SerializeField]
    private Text matDataListTxt, matResultsTxt,
                imageListTxt, imageRollingSearchTxt, imageMatchImageBlurb,
                trajNumOfPtsHeader, trajNumOfPtsTxt, trajTypeHeader, trajTypeTxt, trajMatchesHeader, trajMatchesTxt,
                fileSelectionWindowTxt, fileSelectionTxt;

    // TOOL SCREEN PARAMETERS

    // Use this switch to separate between the icon transition and tool-specific transition frames
    private bool iconTransitionSwitch;

    public List<Texture> momentusLoadingFrames, momentusClosingFrames;
    public Camera momentusCamOnScene, momentusStageCam;
    private List<Transform> curMomentusSceneObjects = new List<Transform>();
    private List<Transform> curMomentusSimulationObjects = new List<Transform>();
    private List<MeshRenderer> curMomentusMeshRends = new List<MeshRenderer>();
    private List<SkinnedMeshRenderer> curMomentusSkinnedMeshRends = new List<SkinnedMeshRenderer>();
    private Transform curSelectedScenObject, curSelectedSimulationObject;
    [SerializeField]
    private float momentusLoadingFrameTime, momentusClosingFrameTime;
    private float momentusLoadingFrameRefTime;
    [SerializeField, Range(0.0f, 1.0f)]
    private float momentusObjAlphaMin, momentusObjAlphaMax;
    [SerializeField]
    private float momentusObjFadeFactor, momentusObjFlashFactor;
    [SerializeField]
    private float momentusCamSmoothingFactor, momentusCamAcceptableDistanceToTarget;

    public List<Texture> nodicalLoadingFrames, nodicalWarmupImageFrames, nodicalImageChangeFrames, nodicalImageShutoffFrames;
    [SerializeField]
    private float nodicalLoadingFrameTime, nodicalClosingFrameTime, nodicalImageChangeFrameTime, nodicalImageShutoffFrameTime;
    private float nodicalTransitionFrameRefTime;
    private int nodicalTransitionFrameIndex, nodicalImageChangeFrameIndex;
    private bool imageIsChanging;
    [SerializeField]
    private Texture imageCursorNotBoxing, imageCursorBoxing;

    public List<Texture> microscopeLoadSlideFrames, microscopeChangeSlideFrames, microscopeRemoveSlideFrames,
                            matSciMicroscopeBorderFrames, matSciResultsBgdFrames, matSciDataBorderFrames, matSciListBgdFrames, matSciDataWaitingFrames, matSciDataBorderWaitingFrames,
                            matSciIconTransitionFrames;
    private List<Texture> curMicroscopeAnimationFrames = new List<Texture>();
    [SerializeField]
    private GameObject speciesVisPlaceholder;
    private List<NMRSpeciesVisualization> visibleSpecies = new List<NMRSpeciesVisualization>();
    private NMRSpeciesVisualization curSpeciesToTrack;
    private Vector2 microscopeWindowCenter;
    [SerializeField]
    private Vector2 microscopeSlideTravelLimits;
    [SerializeField]
    private float matSciIconTransitionFrameTime, microscopeFrameTime, matSciBgdsFrameTime, matSciDataWaitingFrameTime;
    private float matSciIconTransitionFrameRefTime,
                    microscopeFrameRefTime, microscopeBorderFrameRefTime, 
                    matSciResultsBgdFrameRefTime, matSciDataBorderFrameRefTime, matSciListBgdRefTime, matSciDataWaitingRefTime;
    private int matSciIconTransitionFrameIndex,
                microscopeFrameIndex, microscopeBorderFrameIndex,
                matSciResultsBgdFrameIndex, matSciDataFrameIndex, matSciListBgdFrameIndex, matSciDataWaitingFrameIndex, matSciDataBorderWaitingFrameIndex;
    private bool microscopeIsAnimating;
    [SerializeField, Range(0.0f, 1.0f)]
    private float matSciMicroscopeSelectorsAlphaMin, matSciMicroscopeSelectorsAlphaMax;
    [SerializeField]
    private float matSciMicroscopeSelectorsAlphaIncrement;
    private bool matSciMicroscopeSelectorsAlphaIsIncreasing;

    [SerializeField, Range(0.0f, 100.0f)]
    private float excellentMatchRating, goodMatchRating, okayMatchRating, badMatchRating;
    [SerializeField]
    private Color shiftValueColor, peakValueColor,
                    samplePeakSelectColor, refPeakSelectColor, firstRefMatchColor, secondRefMatchColor, thirdRefMatchColor,
                    resultsHeaderColor, resultsBodyColor, excellentMatchRatingColor, goodMatchRatingColor, okayMatchRatingColor, badMatchRatingColor;

    public Texture[] selectorBoxFrames;
    public bool showTools;
    public float selectorBoxTimer;
    [Range(0.0f, 1.0f)]
    public float enabledIconAlpha, disabledIconAlpha;  

    // ProjectHandler, and all of the LineRenderers used for the various tools!
    private ProjectReview projHandler;

    [SerializeField]
    private Transform stageFocus;
    [SerializeField]
    private Camera evidenceCamera, analysisCamera;
    [SerializeField]
    private RenderTexture crimeSceneView, analysisView;
    
    [SerializeField]
    private RawImage materialsAnalysisChart, imageAnalysisImage;
    [SerializeField]
    private LineRenderer materialsAnalysisLineRend, imageAnalysisLineRend;
    [SerializeField]
    private List<LineRenderer> materialsAnalysisRefLineRends;
    [SerializeField]
    private LineRenderer refPeakSelectorLineRend;

    [SerializeField]
    private LineRenderer searchBoxLineRend;

    private enum ToolMode { Inactive, Loading, Selecting, Closing,
                            LoadingTrajectory, Trajectory, ClosingTrajectory,
                            LoadingMatSci, MatSci, ClosingMatSci,
                            LoadingImageAnalysis, ImageAnalysis, ClosingImageAnalysis }
    private ToolMode ToolState;

    private enum ActiveToolMode { NotUsing, Using, FileOptions, Adding, Deleting, SendingTechRequest }
    private ActiveToolMode ActiveToolState;

    private enum TrajectoryAnalysisMode { PickingNumOfPoints, PickingTrajectoryType, LookingAtReferences}
    private TrajectoryAnalysisMode TrajectoryAnalysisState;
    
    private List<RawImage> mainWindows = new List<RawImage>();
    private List<RawImage> matSciAnalysisWindows = new List<RawImage>();
    private List<RawImage> imageAnalysisWindows = new List<RawImage>();
    private List<RawImage> trajAnalysisWindows = new List<RawImage>();
    private List<RawImage> fileManagementWindows = new List<RawImage>();
    private List<RawImage> fileSelectionWindows = new List<RawImage>();

    
    private List<Text> mainTexts = new List<Text>();
    private List<Text> matSciAnalysisTexts = new List<Text>(); 
    private List<Text> imageAnalysisTexts = new List<Text>();
    private List<Text> trajAnalysisTexts = new List<Text>();
    private List<Text> fileSelectionTexts = new List<Text>();

    private int selectorBoxCurrentFrame;
    private float selectorBoxTimerCurTime;
    private bool selectorBoxLightening;

    private bool evidenceListIsHighlighted = false;

    private RawImage currentFileManagementIcon, currentToolPickerIcon, currentFileManagementPopupIcon;
    private ProjectFile curProject;
    private List<ProjectFile> projectList = new List<ProjectFile>();
    private List<ProjectFile> curProjectsCanAddTo = new List<ProjectFile>();
    private List<string> availableTechReqServices = new List<string>();
    private EvidenceData curEvidenceData;
    private List<EvidenceData> curEvidenceDataList;
    private int currentProjectIndex, numberOfProjects, curEvidenceFileIndex, numOfCurrentEvidenceFiles, curProjectsCanAddToIndex;
    private int curMatSciIndex, curTrajIndex, curCrimeSceneIndex, curImageSciIndex;
    private bool noMatSciFiles, noTrajFiles, noCrimeScenes, noImageSciFiles;
    [SerializeField]
    private int maxNumOfEvidenceFiles = 10;
    [SerializeField]
    private int numOfVisibleEvidenceFilesInList = 6;
    [SerializeField]
    private int selectorTextHeight = 7;
    private Vector2 matDataListSelectorInitialPos, imageDataListSelectorInitialPos;
    [SerializeField]
    private float highlightAlphaMin, highlightAlphaMax, highlightChangeFrameTime, highlightChange;
    private float highlightChangeFrameRefTime;
    private bool highlightIsFading = false;

    // Does the current forensic tool employ continuous right joystick control?
    private bool canHoldRS = false;
    private bool canHoldTriggers = false;
    
    // Mat Sci stuff
    [SerializeField]
    private float maxShift;
    [SerializeField]
    private int lineRendOffsetX, lineRendOffsetY;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float refPeakSelectorMinAlpha, refPeakSelectorMaxAlpha;
    [SerializeField]
    private float refPeakSelectorFlashAmount;
    private bool refPeakSelectorIncreasingAlpha;

    private NMRResults curNMR;
    private bool showAllMatSciRefCurves;
    private int curMatSciRefIndex, curMatSciRefPeakIndex, curMatSciSamplePeakIndex;

    // Image Analysis stuff
    private bool IsDrawingIASearchBox, finishedDrawingIASearchBox, isZoomedOut;
    [SerializeField]
    private Vector2 cursorLimits;
    [SerializeField]
    private float cursorMoveSpeed;
    private Vector2 initialImagePosition, initialImageSize, initialIACursorPosition, IAcursorPosition, currentSearchTopLeftCorner, currentSearchBottomRightCorner;

    private ImageToAnalyze curImageToAnalyze;
    private int currentFoundImageIndex;

    // Trajectory stuff
    private TrajectorySet curTrajSet;
    [SerializeField]
    private float selectorBoxAlphaWhenWhosen;

    // Start is called before the first frame update
    void Start()
    {
        //fileManagementIcons.Add(loadButton);
        //fileManagementIcons.Add(deleteButton);
        //fileManagementIcons.Add(addButton);
        //fileManagementIcons.Add(cachedResultsListBgd);
        //fileManagementIcons.Add(projectNumberBgd);

        //fileManagementTexts.Add(cachedResultsListTxt);
        //fileManagementTexts.Add(projectNumberTxt);

        projHandler = FindObjectOfType<ProjectReview>();

        if (projHandler.projectList.Count > 0) {
            foreach (ProjectFile proj in projHandler.projectList) {
                if (proj.imagesToAnalyzeActual.Count > 0) {
                    foreach (ImageToAnalyze img in proj.imagesToAnalyzeActual) {
                        img.ClearFoundImagesList();
                    }
                }
            }
        }

        matDataListTxt.supportRichText = true;
        matResultsTxt.supportRichText = true;
        imageListTxt.supportRichText = true;
        imageMatchImageBlurb.supportRichText = true;

        // Fill all of the RawImage and Text lists with their relevant members
        mainWindows.Add(trajSciIcon);
        mainWindows.Add(matSciIcon);
        mainWindows.Add(imageSciIcon);
        mainWindows.Add(toolIconBorders);
        mainWindows.Add(notesBgd);
        mainWindows.Add(toolIconSelectorBox);

        mainTexts.Add(notesTxt);

        fileManagementWindows.Add(fileManagementWindow);
        fileManagementWindows.Add(addButton);
        fileManagementWindows.Add(deleteButton);
        fileManagementWindows.Add(techReqButton);
        fileManagementWindows.Add(fileManagementSelector);

        fileSelectionWindows.Add(fileSelectionWindow);
        fileSelectionWindows.Add(fileSelectionBgd);
        fileSelectionWindows.Add(okayButton);
        fileSelectionWindows.Add(cancelButton);

        matSciAnalysisWindows.Add(matDataListBgd);
        matSciAnalysisWindows.Add(matDataListSelector);
        matSciAnalysisWindows.Add(matDataChartWindow);
        matSciAnalysisWindows.Add(matDataChartWindowAxes);
        matSciAnalysisWindows.Add(matDataChartWindowFrame);
        matSciAnalysisWindows.Add(matDataChartSampleCursor);
        matSciAnalysisWindows.Add(matDataChartRefCursor);
        matSciAnalysisWindows.Add(matResultsBgd);
        matSciAnalysisWindows.Add(matSciMicroscopeView);
        matSciAnalysisWindows.Add(matSciMicroscopeViewMask);
        matSciAnalysisWindows.Add(matSciMicroscopeViewFrame);
        matSciAnalysisWindows.Add(matSciMicroscopeCursor);

        matSciAnalysisTexts.Add(matDataListTxt);
        matSciAnalysisTexts.Add(matResultsTxt);

        imageAnalysisWindows.Add(imageListBgd);
        imageAnalysisWindows.Add(imageListSelector);
        imageAnalysisWindows.Add(imageSciWindow);
        imageAnalysisWindows.Add(imageSciWindowFrame);
        imageAnalysisWindows.Add(imageControlKnob);
        imageAnalysisWindows.Add(imageHorizontalKnob);
        imageAnalysisWindows.Add(imageVerticalKnob);
        imageAnalysisWindows.Add(imageRefMatchImage);
        imageAnalysisWindows.Add(imageRefMatchFrame);
        imageAnalysisWindows.Add(imageReticle);

        imageAnalysisTexts.Add(imageListTxt);
        imageAnalysisTexts.Add(imageRollingSearchTxt);
        imageAnalysisTexts.Add(imageMatchImageBlurb);

        trajAnalysisWindows.Add(trajScenarioWindow);
        trajAnalysisWindows.Add(trajScenarioWindowFrame);
        trajAnalysisWindows.Add(trajNumOfPtsBgd);
        trajAnalysisWindows.Add(trajTypeBgd);
        trajAnalysisWindows.Add(trajNumOfPtsSelector);
        trajAnalysisWindows.Add(trajTypeSelector);

        trajAnalysisTexts.Add(trajNumOfPtsHeader);
        trajAnalysisTexts.Add(trajNumOfPtsTxt);
        trajAnalysisTexts.Add(trajTypeHeader);
        trajAnalysisTexts.Add(trajTypeTxt);
        trajAnalysisTexts.Add(trajMatchesHeader);
        trajAnalysisTexts.Add(trajMatchesTxt);

        fileSelectionTexts.Add(fileSelectionWindowTxt);
        fileSelectionTexts.Add(fileSelectionTxt);

        showTools = false;

        notesTxt.supportRichText = true;
        notepadTextIsUnravelling = false;
        notepadTextShowTrailingUnderscore = false;

        ForceCloseAllToolWindows();
        projectNumberBgd.enabled = false;
        projectNumberTxt.enabled = false;
        notesTxt.enabled = false;

        ToolState = ToolMode.Inactive;
        ActiveToolState = ActiveToolMode.NotUsing;
        TrajectoryAnalysisState = TrajectoryAnalysisMode.PickingNumOfPoints;

        curSpringAttractFrameIndex = springAttractFrames.Count - 1;

        currentToolPickerIcon = trajSciIcon;
        currentFileManagementIcon = techReqButton;

        currentProjectIndex = 0;
        matDataListSelectorInitialPos = matDataListSelector.rectTransform.anchoredPosition;

        showAllMatSciRefCurves = true;
        curMatSciIndex = 0;
        curMatSciRefIndex = 0;
        curMatSciRefPeakIndex = 0;
        curMatSciSamplePeakIndex = 0;
        refPeakSelectorIncreasingAlpha = true;
        microscopeWindowCenter = matSciMicroscopeView.rectTransform.anchoredPosition;

        isZoomedOut = false;
        imageControlKnobFrameIndex = 0;                                             // Big image control knob position 0 is at index 0
        imageVerticalKnobFrameIndex = imageVerticalKnobFrames.Count / 2 + 1;        // These initial indices for the rotation of the knobs assumes there are
        imageHorizontalKnobFrameIndex = imageHorizontalKnobFrames.Count / 2 + 1;    // an equal number of forward and backward rotations (plus a middle/neutral rotation)
        curImageSciIndex = 0;
        initialImagePosition = imageAnalysisImage.rectTransform.anchoredPosition;
        initialImageSize = imageAnalysisImage.rectTransform.sizeDelta;
        initialIACursorPosition = new Vector2(0, 0);
        currentFoundImageIndex = 0;

        curTrajIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //ShowAllWindows(showTools);

        evidenceCamera.transform.LookAt(stageFocus);

        // Continually update the open projects List
        projectList = projHandler.projectList;
        numberOfProjects = projHandler.projectList.Count;

        // If a Project is closed out and removed from the open projects List, for example, if the current project index is higher than the new open projects count, reset index to 0
        if ((currentProjectIndex > numberOfProjects - 1) || currentProjectIndex < 0) { currentProjectIndex = 0; }
        curProject = projectList[currentProjectIndex];

        // Continually correct index values, for when files are deleted
        if ((curMatSciIndex > projectList[currentProjectIndex].NMRDataActual.Count) || (curMatSciIndex < 0))                { curMatSciIndex = 0; }
        if ((curTrajIndex > projectList[currentProjectIndex].trajDataActual.Count) || (curTrajIndex < 0))                   { curMatSciIndex = 0; }
        if ((curImageSciIndex > projectList[currentProjectIndex].imagesToAnalyzeActual.Count) || (curImageSciIndex < 0))    { curMatSciIndex = 0; }
        if ((curCrimeSceneIndex > projectList[currentProjectIndex].crimeScenes.Count) || (curCrimeSceneIndex < 0))          { curMatSciIndex = 0; }

        if (projectList[currentProjectIndex].NMRDataActual.Count == 0)          { noMatSciFiles = true; }
        if (projectList[currentProjectIndex].trajDataActual.Count == 0)         { noTrajFiles = true; }
        if (projectList[currentProjectIndex].imagesToAnalyzeActual.Count == 0)  { noImageSciFiles = true; }
        if (projectList[currentProjectIndex].crimeScenes.Count == 0)            { noCrimeScenes = true; }

        if (projectList[currentProjectIndex].NMRDataActual.Count > 0) {
            curNMR = projectList[currentProjectIndex].NMRDataActual[curMatSciIndex];
        }
        if (projectList[currentProjectIndex].imagesToAnalyzeActual.Count > 0) {
            curImageToAnalyze = projectList[currentProjectIndex].imagesToAnalyzeActual[curImageSciIndex];
        }
        if (projectList[currentProjectIndex].trajDataActual.Count > 0) {
            curTrajSet = projectList[currentProjectIndex].trajDataActual[curTrajIndex];
        }

        switch (ToolState) {
            case ToolMode.Inactive:

                UpdateAttractScreen();

                break;
            case ToolMode.Loading:

                if (Time.time - delayNotebookRefTime < delayTimeBeforeTransition + springAttractExtraLengthBeforeShowingIcons) {
                    UpdateAttractScreen();
                }

                if (Time.time - delayNotebookRefTime > delayTimeBeforeTransition) {

                    if (Time.time - notebookFrameRefTime > transitionNotebookFrameTime) {
                        if (notebookFrameIndex < notesBgdOpeningFrames.Count) {
                            notesBgd.enabled = true;
                            //notesTxt.enabled = true;
                            notesBgd.texture = notesBgdOpeningFrames[notebookFrameIndex];
                            notebookFrameIndex++;
                            notebookFrameRefTime = Time.time;
                        }
                    }

                    if (Time.time - projectionFrameRefTime > transitionIconFrameTime) {
                        if (toolIconProjectionFrameIndex < toolIconProjectionFrames.Count && Time.time - delayNotebookRefTime > delayTimeBeforePretransition) {

                            springAttractScreenRend.enabled = false;

                            toolProjection.enabled = true;
                            toolProjection.texture = toolIconProjectionFrames[toolIconProjectionFrameIndex];
                            toolIconProjectionFrameIndex++;
                            projectionFrameRefTime = Time.time;
                        }
                    }

                    if (!(notebookFrameIndex < notesBgdOpeningFrames.Count || toolIconProjectionFrameIndex < toolIconProjectionFrames.Count)) {
                        toolProjection.enabled = false;
                        toolIconBorders.enabled = true;
                        trajSciIcon.enabled = true;
                        matSciIcon.enabled = true;
                        imageSciIcon.enabled = true;
                        toolIconSelectorBox.enabled = true;

                        //matSciIconAnimationRefTime = Time.time;     //???
                        projectionFrameRefTime = Time.time;
                        if (currentToolPickerIcon == trajSciIcon) { InitializeNotepadText(trajSciIconSelectedNotepadText); }
                        else if (currentToolPickerIcon == matSciIcon) { InitializeNotepadText(matSciIconSelectedNotepadText); }
                        else if (currentToolPickerIcon == imageSciIcon) { InitializeNotepadText(imageSciIconSelectedNotepadText); }

                        notesTxt.enabled = true;

                        ToolState = ToolMode.Selecting;
                    }
                }

                break;
            case ToolMode.Selecting:

                trajSciIcon.texture = trajSciIconFrames[trajSciIconFrameIndex];
                matSciIcon.texture = matSciIconFrames[matSciIconFrameIndex];
                imageSciIcon.texture = imageSciIconFrames[imageSciIconFrameIndex];

                if (Time.time - projectionFrameRefTime > trajSciIconAnimationFrameTime && currentToolPickerIcon == trajSciIcon) {
                    trajSciIconFrameIndex = (trajSciIconFrameIndex + 1) % trajSciIconFrames.Count;
                    projectionFrameRefTime = Time.time;
                }

                if (Time.time - projectionFrameRefTime > matSciIconAnimationFrameTime && currentToolPickerIcon == matSciIcon) {
                    matSciIconFrameIndex = (matSciIconFrameIndex + 1) % matSciIconFrames.Count;
                    projectionFrameRefTime = Time.time;
                }

                if (Time.time - projectionFrameRefTime > imageSciIconAnimationFrameTime && currentToolPickerIcon == imageSciIcon) {
                    imageSciIconFrameIndex = (imageSciIconFrameIndex + 1) % imageSciIconFrames.Count;
                    projectionFrameRefTime = Time.time;
                }

                if (notepadTextIsUnravelling) {
                    if (Time.time - notepadTextUnravelRefTime > notepadeTextUnravelTimePerChar) {
                        if (curNotepadCharIndex < notepadTextChars.Count) {
                            notesTxt.text += notepadTextChars[curNotepadCharIndex];
                            curNotepadCharIndex++;

                            notepadTextUnravelRefTime = Time.time;
                        }
                        else {
                            notepadTextIsUnravelling = false;
                            notepadTextTrailingUnderscoreToggleIsActive = true;
                        }
                    }
                }

                //if (notepadTextTrailingUnderscoreToggleIsActive) {
                //    if (Time.time - notepadTextTrailingUnderscoreToggleRefTime > notepadTextTrailingUnderscoreToggleTime) {
                //        if (notepadTextShowTrailingUnderscore) {
                //            notesTxt.text += "_";
                //        }
                //        else {
                //            char[] trimChars = { '_' };
                //            notesTxt.text = notesTxt.text.Trim(trimChars);
                //        }

                //        notepadTextShowTrailingUnderscore = !notepadTextShowTrailingUnderscore;
                //        notepadTextTrailingUnderscoreToggleRefTime = Time.time;
                //    }
                //}

                //trajSciIcon.color = new Color(trajSciIcon.color.r, trajSciIcon.color.g, trajSciIcon.color.b, trajSciIconAlphas[curTrajSciIconFrameIndex]);
                //curTrajSciIconFrameIndex = (curTrajSciIconFrameIndex + 1) % trajSciIconAlphas.Count;

                //toolSelectorBox.rectTransform.anchoredPosition = currentToolPickerIcon.rectTransform.anchoredPosition;
                //FlashHighlighter(toolSelectorBox);

                break;
            case ToolMode.LoadingMatSci:

                if (!iconTransitionSwitch) {
                    if (Time.time - matSciIconTransitionFrameRefTime > matSciIconTransitionFrameTime) {
                        if (matSciIconTransitionFrameIndex < matSciIconTransitionFrames.Count - 1) {
                            matSciIconTransitionFrameIndex++;
                            imageSciWindowFrame.texture = matSciIconTransitionFrames[matSciIconTransitionFrameIndex];
                            matSciIconTransitionFrameRefTime = Time.time;
                        }
                        else {
                            imageSciWindowFrame.enabled = false;

                            matSciResultsBgdFrameIndex = 0;
                            matResultsBgd.texture = matSciResultsBgdFrames[matSciResultsBgdFrameIndex];
                            matSciResultsBgdFrameRefTime = Time.time;

                            matSciListBgdFrameIndex = 0;
                            matDataListBgd.texture = matSciListBgdFrames[matSciListBgdFrameIndex];
                            matSciListBgdRefTime = Time.time;

                            matSciDataFrameIndex = 0;
                            matDataChartWindowFrame.texture = matSciDataBorderFrames[matSciDataFrameIndex];
                            matSciDataBorderFrameRefTime = Time.time;

                            microscopeBorderFrameIndex = 0;
                            matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[microscopeBorderFrameIndex];
                            microscopeBorderFrameRefTime = Time.time;

                            matResultsBgd.enabled = true;
                            matDataListBgd.enabled = true;
                            matDataChartWindowFrame.enabled = true;
                            matSciMicroscopeViewFrame.enabled = true;

                            microscopeIsAnimating = false;
                            curMicroscopeAnimationFrames = microscopeLoadSlideFrames;

                            imageAnalysisImage.enabled = false;
                            materialsAnalysisChart.enabled = true;

                            iconTransitionSwitch = true;
                        }
                    }
                }
                else {
                    if (Time.time - microscopeBorderFrameRefTime > matSciBgdsFrameTime) {
                        // Don't switch to the last microscope border frame until after the microscope view displays, or there will be a flash of clear inside the border
                        if (microscopeBorderFrameIndex < matSciMicroscopeBorderFrames.Count - 2) {
                            microscopeBorderFrameIndex++;
                            matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[microscopeBorderFrameIndex];
                            microscopeBorderFrameRefTime = Time.time;
                        }
                    }

                    if (Time.time - matSciDataBorderFrameRefTime > matSciBgdsFrameTime) {
                        if (matSciDataFrameIndex < matSciDataBorderFrames.Count - 1) {
                            matSciDataFrameIndex++;
                            matDataChartWindowFrame.texture = matSciDataBorderFrames[matSciDataFrameIndex];
                            matSciDataBorderFrameRefTime = Time.time;
                        }
                    }

                    if (Time.time - matSciListBgdRefTime > matSciBgdsFrameTime) {
                        if (matSciListBgdFrameIndex < matSciListBgdFrames.Count - 1) {
                            matSciListBgdFrameIndex++;
                            matDataListBgd.texture = matSciListBgdFrames[matSciListBgdFrameIndex];
                            matSciListBgdRefTime = Time.time;
                        }
                    }

                    if (Time.time - matSciResultsBgdFrameRefTime > matSciBgdsFrameTime) {
                        if (matSciResultsBgdFrameIndex < matSciResultsBgdFrames.Count - 1) {
                            matSciResultsBgdFrameIndex++;
                            matResultsBgd.texture = matSciResultsBgdFrames[matSciResultsBgdFrameIndex];
                            matSciResultsBgdFrameRefTime = Time.time;
                        }
                    }

                    if ((microscopeBorderFrameIndex > matSciMicroscopeBorderFrames.Count - 3) &&
                        (matSciDataFrameIndex > matSciDataBorderFrames.Count - 2) &&
                        (matSciListBgdFrameIndex > matSciListBgdFrames.Count - 2) &&
                        (matSciResultsBgdFrameIndex > matSciResultsBgdFrames.Count - 2)) {

                        materialsAnalysisChart.enabled = true;
                        materialsAnalysisLineRend.enabled = true;

                        // No continuous stick control
                        canHoldRS = false;
                        canHoldTriggers = false;

                        microscopeIsAnimating = true;
                        curMicroscopeAnimationFrames = microscopeLoadSlideFrames;
                        microscopeFrameIndex = 0;
                        matSciMicroscopeView.texture = curMicroscopeAnimationFrames[microscopeFrameIndex];
                        matSciMicroscopeView.enabled = true;
                        matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[matSciMicroscopeBorderFrames.Count - 1];
                        microscopeFrameRefTime = Time.time;

                        matSciDataWaitingFrameIndex = 0;
                        matDataChartWindow.texture = matSciDataWaitingFrames[matSciDataWaitingFrameIndex];
                        matDataChartWindow.enabled = true;

                        matSciDataBorderWaitingFrameIndex = 0;
                        matDataChartWindowFrame.texture = matSciDataBorderWaitingFrames[matSciDataBorderWaitingFrameIndex];

                        matResultsTxt.enabled = true;

                        ToolState = ToolMode.MatSci;
                    }
                }

                break;
            case ToolMode.MatSci:

                // Continually update the EvidenceData file index for displaying the file list
                curEvidenceFileIndex = curMatSciIndex;

                if (!microscopeIsAnimating) {

                    matSciMicroscopeViewMask.enabled = true;

                    //if (refPeakSelectorIncreasingAlpha) {
                    //    if ((refPeakSelectorLineRend.startColor.a < refPeakSelectorMaxAlpha) && (refPeakSelectorLineRend.endColor.a < refPeakSelectorMaxAlpha)) {
                    //        refPeakSelectorLineRend.startColor = new Color(refPeakSelectorLineRend.startColor.r, refPeakSelectorLineRend.startColor.g, refPeakSelectorLineRend.startColor.b,
                    //                                                        refPeakSelectorLineRend.startColor.a + refPeakSelectorFlashAmount * Time.deltaTime);
                    //        refPeakSelectorLineRend.endColor = new Color(refPeakSelectorLineRend.endColor.r, refPeakSelectorLineRend.endColor.g, refPeakSelectorLineRend.endColor.b,
                    //                                                        refPeakSelectorLineRend.endColor.a + refPeakSelectorFlashAmount * Time.deltaTime);
                    //    }
                    //    else {
                    //        refPeakSelectorIncreasingAlpha = false;
                    //    }
                    //}
                    //else {
                    //    if ((refPeakSelectorLineRend.startColor.a > refPeakSelectorMinAlpha) && (refPeakSelectorLineRend.endColor.a > refPeakSelectorMinAlpha)) {
                    //        refPeakSelectorLineRend.startColor = new Color(refPeakSelectorLineRend.startColor.r, refPeakSelectorLineRend.startColor.g, refPeakSelectorLineRend.startColor.b,
                    //                                                        refPeakSelectorLineRend.startColor.a - refPeakSelectorFlashAmount * Time.deltaTime);
                    //        refPeakSelectorLineRend.endColor = new Color(refPeakSelectorLineRend.endColor.r, refPeakSelectorLineRend.endColor.g, refPeakSelectorLineRend.endColor.b,
                    //                                                        refPeakSelectorLineRend.endColor.a - refPeakSelectorFlashAmount * Time.deltaTime);
                    //    }
                    //    else {
                    //        refPeakSelectorIncreasingAlpha = true;
                    //    }
                    //}

                    if (curMatSciRefIndex == 0 && showAllMatSciRefCurves) {

                        //refPeakSelectorLineRend.enabled = false;
                        matDataChartRefCursor.enabled = false;

                        for (int i = 0; i < curNMR.matches.Count; i++) {
                            SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                        }

                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, true);
                        notesTxt.text = curNMR.GetNotes();
                    }
                    else {
                        //refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        // Get the results and notes for the sample and references
                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, false) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    }

                    PlaceSamplePeakSelector(curNMR, curMatSciSamplePeakIndex);

                    if (visibleSpecies.Count > 0) {
                        //foreach (NMRSpeciesVisualization spec in visibleSpecies) {
                        //    spec.InitializeVictimList(visibleSpecies);
                        //    spec.UpdatePositionAndFrames();
                        //}

                        List<NMRSpeciesVisualization> temp = visibleSpecies;

                        for (int i=0; i < temp.Count; i++) {
                            temp[i].InitializeVictimList(visibleSpecies);
                            temp[i].UpdatePositionAndFrames();
                            //if (temp[i].IsDestroyed()) { visibleSpecies.Remove(temp[i]); }
                        }

                        SetTargetSpecies(curNMR, curMatSciSamplePeakIndex);
                    }
                }

                if (microscopeIsAnimating) {

                    matSciMicroscopeViewMask.enabled = false;
                    matDataChartWindowAxes.enabled = false;

                    if (Time.time - matSciDataWaitingRefTime > matSciDataWaitingFrameTime) {

                        matResultsTxt.alignment = TextAnchor.MiddleCenter;

                        if (curMicroscopeAnimationFrames == microscopeLoadSlideFrames) {
                            matResultsTxt.text = "LOADING SLIDE...";
                        } else if (curMicroscopeAnimationFrames == microscopeChangeSlideFrames) {
                            matResultsTxt.text = "CHANGING SLIDE...";
                        } else if (curMicroscopeAnimationFrames == microscopeRemoveSlideFrames) {
                            matResultsTxt.text = "CLOSING...";
                        }

                        matSciDataWaitingFrameIndex = (matSciDataWaitingFrameIndex + 1) % matSciDataWaitingFrames.Count;
                        matDataChartWindow.texture = matSciDataWaitingFrames[matSciDataWaitingFrameIndex];

                        if (matSciDataBorderWaitingFrameIndex < matSciDataBorderWaitingFrames.Count - 1) {
                            matSciDataBorderWaitingFrameIndex++;
                            matDataChartWindowFrame.texture = matSciDataBorderWaitingFrames[matSciDataBorderWaitingFrameIndex];
                        }

                        matSciDataWaitingRefTime = Time.time;
                    }

                    if (Time.time - microscopeFrameRefTime > microscopeFrameTime) {
                        if (microscopeFrameIndex < curMicroscopeAnimationFrames.Count - 1) {

                            microscopeFrameIndex++;
                            matSciMicroscopeView.texture = curMicroscopeAnimationFrames[microscopeFrameIndex];
                            microscopeFrameRefTime = Time.time;
                        }
                        else {
                            if (curMicroscopeAnimationFrames == microscopeRemoveSlideFrames) {

                                matResultsTxt.enabled = false;

                                matSciMicroscopeView.enabled = false;
                                matDataChartWindow.enabled = false;
                                matDataChartWindowAxes.enabled = false;
                                matDataChartRefCursor.enabled = false;
                                matDataChartSampleCursor.enabled = false;
                                matSciMicroscopeCursor.enabled = false;
                                matDataListTxt.enabled = false;

                                matSciResultsBgdFrameIndex = matSciResultsBgdFrames.Count - 1;
                                matResultsBgd.texture = matSciResultsBgdFrames[matSciResultsBgdFrameIndex];
                                matSciResultsBgdFrameRefTime = Time.time;

                                matSciListBgdFrameIndex = matSciListBgdFrames.Count - 1;
                                matDataListBgd.texture = matSciListBgdFrames[matSciListBgdFrameIndex];
                                matSciListBgdRefTime = Time.time;

                                // Starting with this frame flows better from the "WAITING" window
                                matSciDataFrameIndex = 1;
                                matDataChartWindowFrame.texture = matSciDataBorderFrames[matSciDataFrameIndex];
                                matSciDataBorderFrameRefTime = Time.time;

                                microscopeBorderFrameIndex = matSciMicroscopeBorderFrames.Count - 2;
                                matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[microscopeBorderFrameIndex];
                                microscopeBorderFrameRefTime = Time.time;

                                iconTransitionSwitch = false;

                                ToolState = ToolMode.ClosingMatSci;
                            }
                            else {

                                matResultsTxt.alignment = TextAnchor.UpperLeft;

                                matDataChartWindow.texture = analysisView;
                                matDataListTxt.enabled = true;
                                matDataChartWindow.enabled = true;
                                matDataChartWindowAxes.enabled = true;

                                matDataChartWindowFrame.texture = matSciDataBorderFrames[matSciDataBorderFrames.Count - 1];

                                curEvidenceData = curNMR;

                                GenerateSlideColony();

                                matDataChartSampleCursor.enabled = true;

                                // Draw NMRResult spectrum using Materials Analysis LineRenderer
                                //curNMR = projHandler.openProjects[currentProjectIndex].NMRDataActual[currentEvidenceFileIndex];
                                SetMaterialsAnalysisSampleSpectrum(curNMR);

                                // Determine NMRRefResult matches for the current NMRResult and add them to its matches list                
                                curNMR.DetermineMatches();

                                // Draw the NMRRefResult spectra using the ref LineRenderers

                                //if (curMatSciRefIndex == 0 && showAllMatSciRefCurves) {

                                //    refPeakSelectorLineRend.enabled = false;
                                //    matDataChartRefCursor.enabled = false;

                                //    for (int i = 0; i < curNMR.matches.Count; i++) {
                                //        SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                                //    }

                                //    matResultsTxt.text = curNMR.GetResults();
                                //    notesTxt.text = curNMR.GetNotes();
                                //}
                                //else {
                                //    refPeakSelectorLineRend.enabled = true;

                                //    SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                                //    PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                                //    // Get the results and notes for the sample and references
                                //    matResultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                                //    notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                                //}

                                //PlaceSamplePeakSelector(curNMR, curMatSciSamplePeakIndex);

                            }

                            microscopeIsAnimating = false;
                        }
                    }
                }

                break;
            case ToolMode.ClosingMatSci:

                if (!iconTransitionSwitch) {
                    if (Time.time - microscopeBorderFrameRefTime > matSciBgdsFrameTime) {
                        if (microscopeBorderFrameIndex > 0) {
                            microscopeBorderFrameIndex--;
                            matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[microscopeBorderFrameIndex];
                            microscopeBorderFrameRefTime = Time.time;
                        }
                    }

                    if (Time.time - matSciDataBorderFrameRefTime > matSciBgdsFrameTime) {
                        if (matSciDataFrameIndex > 0) {
                            matSciDataFrameIndex--;
                            matDataChartWindowFrame.texture = matSciDataBorderFrames[matSciDataFrameIndex];
                            matSciDataBorderFrameRefTime = Time.time;
                        }
                    }

                    if (Time.time - matSciListBgdRefTime > matSciBgdsFrameTime) {
                        if (matSciListBgdFrameIndex > 0) {
                            matSciListBgdFrameIndex--;
                            matDataListBgd.texture = matSciListBgdFrames[matSciListBgdFrameIndex];
                            matSciListBgdRefTime = Time.time;
                        }
                    }

                    if (Time.time - matSciResultsBgdFrameRefTime > matSciBgdsFrameTime) {
                        if (matSciResultsBgdFrameIndex > 0) {
                            matSciResultsBgdFrameIndex--;
                            matResultsBgd.texture = matSciResultsBgdFrames[matSciResultsBgdFrameIndex];
                            matSciResultsBgdFrameRefTime = Time.time;
                        }
                    }

                    if ((microscopeBorderFrameIndex < 1) &&
                        (matSciDataFrameIndex < 1) &&
                        (matSciListBgdFrameIndex < 1) &&
                        (matSciResultsBgdFrameIndex < 1)) {

                        ShowMatSciWindows(false);

                        matSciIconTransitionFrameIndex = matSciIconTransitionFrames.Count - 1;
                        imageSciWindowFrame.texture = matSciIconTransitionFrames[matSciIconTransitionFrameIndex];
                        matSciIconTransitionFrameRefTime = Time.time;
                        imageSciWindowFrame.enabled = true;

                        iconTransitionSwitch = true;
                    }
                }
                else {
                    if (Time.time - matSciIconTransitionFrameRefTime > matSciIconTransitionFrameTime) {
                        if (matSciIconTransitionFrameIndex > 0) {
                            matSciIconTransitionFrameIndex--;
                            imageSciWindowFrame.texture = matSciIconTransitionFrames[matSciIconTransitionFrameIndex];
                            matSciIconTransitionFrameRefTime = Time.time;
                        }
                        else {
                            imageSciWindowFrame.enabled = false;
                            ShowMainWindows(true);

                            ToolState = ToolMode.Selecting;
                            ActiveToolState = ActiveToolMode.NotUsing;
                        }
                    }
                }

                break;
            case ToolMode.LoadingTrajectory:

                
                InitializeMomentusObjects();

                break;
            case ToolMode.Trajectory:

                // Continually update the EvidenceData file index for displaying the file list
                curEvidenceFileIndex = curTrajIndex;

                //if (curTrajSet != null) {

                    switch (TrajectoryAnalysisState) {

                        case TrajectoryAnalysisMode.PickingNumOfPoints:

                            trajNumOfPtsSelector.enabled = true;
                            FlashHighlighter(trajNumOfPtsSelector);
                            trajTypeSelector.enabled = false;
                            trajMatchesHeader.enabled = false;
                            trajMatchesTxt.enabled = false;

                            break;
                        case TrajectoryAnalysisMode.PickingTrajectoryType:

                            trajNumOfPtsSelector.enabled = true;
                            trajTypeSelector.enabled = true;
                            FlashHighlighter(trajTypeSelector);
                            trajNumOfPtsSelector.color = new Color(trajNumOfPtsSelector.color.r, trajNumOfPtsSelector.color.g, trajNumOfPtsSelector.color.b, selectorBoxAlphaWhenWhosen);
                            trajMatchesHeader.enabled = false;
                            trajMatchesTxt.enabled = false;

                            break;
                        case TrajectoryAnalysisMode.LookingAtReferences:

                            trajMatchesHeader.enabled = true;
                            trajMatchesTxt.enabled = true;

                            trajNumOfPtsSelector.enabled = true;
                            trajTypeSelector.enabled = true;
                            trajNumOfPtsSelector.color = new Color(trajNumOfPtsSelector.color.r, trajNumOfPtsSelector.color.g, trajNumOfPtsSelector.color.b, selectorBoxAlphaWhenWhosen);
                            trajTypeSelector.color = new Color(trajTypeSelector.color.r, trajTypeSelector.color.g, trajTypeSelector.color.b, selectorBoxAlphaWhenWhosen);

                            break;
                    }
                //}

                break;
            case ToolMode.ClosingTrajectory:



                break;
            case ToolMode.LoadingImageAnalysis:

                if (Time.time - nodicalTransitionFrameRefTime > nodicalLoadingFrameTime) {
                    if (!imageIsChanging) {
                        if (nodicalTransitionFrameIndex < nodicalLoadingFrames.Count - 1) {
                            nodicalTransitionFrameIndex++;
                            imageSciWindowFrame.texture = nodicalLoadingFrames[nodicalTransitionFrameIndex];
                        }
                        else {
                            nodicalTransitionFrameIndex = 0;
                            imageSciWindow.texture = nodicalWarmupImageFrames[nodicalTransitionFrameIndex];
                            imageSciWindow.enabled = true;

                            imageIsChanging = true;
                        }
                    }
                    else {
                        if (nodicalTransitionFrameIndex < nodicalWarmupImageFrames.Count - 1) {
                            nodicalTransitionFrameIndex++;
                            imageSciWindow.texture = nodicalWarmupImageFrames[nodicalTransitionFrameIndex];
                        }
                        else {

                            imageIsChanging = false;

                            ToolState = ToolMode.ImageAnalysis;
                            imageSciWindow.texture = analysisView;
                            imageAnalysisImage.enabled = true;
                            imageAnalysisLineRend.enabled = true;
                            materialsAnalysisChart.enabled = false;
                            materialsAnalysisLineRend.enabled = false;
                            refPeakSelectorLineRend.enabled = false;
                            foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }
                            imageMatchImageBlurb.text = "";
                            ShowImageAnalysisWindows(true);
                            //imageSciWindowFrame.texture = nodicalLoadingFrames[nodicalLoadingFrames.Count - 1];

                            imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition;
                            IAcursorPosition = initialIACursorPosition;
                            IsDrawingIASearchBox = false;
                            isZoomedOut = false;

                            curEvidenceData = curImageToAnalyze;

                            imageAnalysisImage.texture = curImageToAnalyze.image;

                            canHoldRS = true;
                            canHoldTriggers = true;

                            currentFoundImageIndex = 0;

                            imageRollingSearchTxt.text = curImageToAnalyze.GetResults();
                            notesTxt.text = curImageToAnalyze.GetNotes();
                        }
                    }

                    nodicalTransitionFrameRefTime = Time.time;
                }

                break;
            case ToolMode.ImageAnalysis:

                // Continually update the EvidenceData file index for displaying the file list
                curEvidenceFileIndex = curImageSciIndex;

                imageControlKnob.texture = imageControlKnobFrames[imageControlKnobFrameIndex];
                imageHorizontalKnob.texture = imageHorizontalKnobFrames[imageHorizontalKnobFrameIndex];
                imageVerticalKnob.texture = imageVerticalKnobFrames[imageVerticalKnobFrameIndex];

                if (isZoomedOut || imageIsChanging) { imageReticle.enabled = false; }
                else                                { imageReticle.enabled = true; }

                if (!imageIsChanging) {

                    if (IsDrawingIASearchBox) {
                        imageAnalysisLineRend.enabled = true;
                        imageRefMatchImage.enabled = false;
                        imageRefMatchFrame.enabled = false;
                        imageMatchImageBlurb.text = "";

                        imageReticle.texture = imageCursorBoxing;

                        DrawImageSearchBox(currentSearchTopLeftCorner, IAcursorPosition);
                    }
                    else {
                        if (curImageToAnalyze) {

                            imageReticle.texture = imageCursorNotBoxing;

                            // If the cursor is hovering over a found image, select it and display its info and possible ref image and info
                            ImageCapture imgCap = curImageToAnalyze.SelectImage(IAcursorPosition);
                            if (imgCap != null && !isZoomedOut) {

                                // If the player closes Work Desk while on this screen, all windows are closed
                                // Bringing Work Desk back up refreshes this screen, but not all of the Image Analysis Windows
                                //ShowImageAnalysisWindows(true);

                                imageRollingSearchTxt.text = curImageToAnalyze.GetResults(imgCap);

                                imageAnalysisLineRend.enabled = true;
                                //imageAnalysisSelectedImage.enabled = true;
                                DrawImageSearchBox(imgCap.topLeftCorner, imgCap.bottomRightCorner);

                                if (imgCap.imageRefRevealed) {
                                    imageRefMatchImage.texture = imgCap.imageRefRevealed.refImage;
                                    imageRefMatchImage.enabled = true;
                                    imageRefMatchFrame.enabled = true;
                                    imageMatchImageBlurb.text = curImageToAnalyze.GetBlurb(imgCap);

                                    notesTxt.text = curImageToAnalyze.GetNotes(imgCap);
                                }
                                else {
                                    notesTxt.text = curImageToAnalyze.GetNotes();
                                }
                            }
                            else {
                                imageAnalysisLineRend.enabled = false;
                                //imageAnalysisSelectedImage.enabled = false;
                                imageRefMatchImage.enabled = false;
                                imageRefMatchFrame.enabled = false;
                                imageMatchImageBlurb.text = "";

                                imageRollingSearchTxt.text = curImageToAnalyze.GetResults();
                                notesTxt.text = curImageToAnalyze.GetNotes();
                            }
                        }
                    }
                }
                else {
                    if (Time.time - nodicalTransitionFrameRefTime > nodicalImageChangeFrameTime) {
                        if (nodicalTransitionFrameIndex < nodicalImageChangeFrames.Count - 1) {
                            nodicalTransitionFrameIndex++;
                            imageSciWindow.texture = nodicalImageChangeFrames[nodicalTransitionFrameIndex];
                            nodicalTransitionFrameRefTime = Time.time;
                        }
                        else {

                            curImageToAnalyze = curProject.imagesToAnalyzeActual[curImageSciIndex];

                            imageReticle.enabled = true;
                            imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition;
                            IAcursorPosition = initialIACursorPosition;
                            imageAnalysisImage.texture = curImageToAnalyze.image;
                            imageSciWindow.texture = analysisView;

                            IsDrawingIASearchBox = false;
                            isZoomedOut = false;

                            imageIsChanging = false;
                        }
                    }
                }
                break;
            case ToolMode.ClosingImageAnalysis:

                if (Time.time - nodicalTransitionFrameRefTime > nodicalClosingFrameTime) {

                    if (imageIsChanging) {
                        if (nodicalTransitionFrameIndex < nodicalImageShutoffFrames.Count - 1) {
                            nodicalTransitionFrameIndex++;
                            imageSciWindow.texture = nodicalImageShutoffFrames[nodicalTransitionFrameIndex];
                            nodicalTransitionFrameRefTime = Time.time;
                        }
                        else {

                            imageSciWindow.enabled = false;

                            nodicalTransitionFrameIndex = nodicalLoadingFrames.Count - 1;
                            imageSciWindowFrame.texture = nodicalLoadingFrames[nodicalTransitionFrameIndex];

                            imageIsChanging = false;
                        }
                    }
                    else {
                        if (nodicalTransitionFrameIndex > 1) {
                            nodicalTransitionFrameIndex--;
                            imageSciWindowFrame.texture = nodicalLoadingFrames[nodicalTransitionFrameIndex];
                            nodicalTransitionFrameRefTime = Time.time;
                        }
                        else {
                            imageSciWindowFrame.enabled = false;
                            ShowMainWindows(true);

                            ToolState = ToolMode.Selecting;
                        }
                    }
                }

                break;
            case ToolMode.Closing:

                if (Time.time - notebookFrameRefTime > notebookClosingFrameTime) {
                    if (notebookFrameIndex > 0) {
                        notesBgd.texture = notesBgdOpeningFrames[notebookFrameIndex];
                        notebookFrameIndex--;
                        notebookFrameRefTime = Time.time;
                    }
                }

                if (Time.time - projectionFrameRefTime > projectionClosingFrameTime) {
                    if (toolIconCloseFrameIndex < toolIconCloseFrames.Count) {
                        toolProjection.texture = toolIconCloseFrames[toolIconCloseFrameIndex];
                        toolIconCloseFrameIndex++;
                        projectionFrameRefTime = Time.time;
                    }
                    else {
                        toolProjection.enabled = false;

                        if (!springAttractScreenRend.enabled) {
                            curSpringAttractFrameIndex = springAttractFrames.Count - 1;
                            Material[] newMats = { springAttractFrames[curSpringAttractFrameIndex] };
                            springAttractScreenRend.enabled = true;
                            springAttractFrameRefTime = Time.time;
                        }
                        else {
                            UpdateAttractScreen();
                        }
                    }
                }

                if (!((notebookFrameIndex > 0) || (toolIconCloseFrameIndex < toolIconCloseFrames.Count))) {
                    notesBgd.enabled = false;
                    projectNumberBgd.enabled = false;
                    projectNumberTxt.enabled = false;

                    toolProjection.enabled = false;

                    ToolState = ToolMode.Inactive;
                }

                break;
        }

        switch (ActiveToolState) {
            case ActiveToolMode.NotUsing:
                break;
            case ActiveToolMode.Using:
                switch (ToolState) {
                    case ToolMode.MatSci:
                        FlashHighlighter(matDataListSelector);
                        break;
                    case ToolMode.ImageAnalysis:
                        FlashHighlighter(imageListSelector);
                        break;
                }
                break;
            case ActiveToolMode.FileOptions:
                FlashHighlighter(fileManagementSelector);
                break;
            case ActiveToolMode.Adding:
                FlashHighlighter(fileManagementSelector);
                break;
            case ActiveToolMode.Deleting:
                FlashHighlighter(fileManagementSelector);
                break;
            case ActiveToolMode.SendingTechRequest:
                FlashHighlighter(fileManagementSelector);
                break;
        }
    }

    // METHODS //

    private void ForceCloseAllToolWindows() {
        toolProjection.enabled = false;
        notesBgd.enabled = false;

        ShowMainWindows(false);
        ShowTrajectoryAnalysisWindows(false);
        ShowMatSciWindows(false);
        ShowImageAnalysisWindows(false);
        ShowFileManagementWindows(false);
        ShowFileSelectionWindows(false);
    }

    private void ShowMainWindows(bool showOrNo) {

        if (showOrNo) {
            toolIconBorders.enabled = true;
            matSciIcon.enabled = true;
            imageSciIcon.enabled = true;
            trajSciIcon.enabled = true;
            toolIconSelectorBox.enabled = true;
        }
        else {
            toolIconBorders.enabled = false;
            matSciIcon.enabled = false;
            imageSciIcon.enabled = false;
            trajSciIcon.enabled = false;
            toolIconSelectorBox.enabled = false;
            projectNumberBgd.enabled = false;
            projectNumberTxt.enabled = false;
        }
    }

    public void MakeActive(bool yesOrNo) {
        if (yesOrNo) {
            delayNotebookRefTime = Time.time;
            notebookFrameRefTime = Time.time;
            projectionFrameRefTime = Time.time;
            notebookFrameIndex = 0;
            toolIconProjectionFrameIndex = 0;

            if (currentToolPickerIcon == trajSciIcon) {
                trajSciIconFrameIndex = 0;
                matSciIconFrameIndex = matSciIconStaticFrameIndex;
                imageSciIconFrameIndex = imageSciIconStaticFrameIndex;
                toolIconSelectorBox.rectTransform.anchoredPosition = trajSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 49);
            }
            else if (currentToolPickerIcon == matSciIcon) {
                trajSciIconFrameIndex = trajSciIconStaticFrameIndex;
                imageSciIconFrameIndex = imageSciIconStaticFrameIndex;
                matSciIconFrameIndex = 0;
                toolIconSelectorBox.rectTransform.anchoredPosition = matSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 50);
            }
            else if (currentToolPickerIcon == imageSciIcon) {
                trajSciIconFrameIndex = trajSciIconStaticFrameIndex;
                matSciIconFrameIndex = matSciIconStaticFrameIndex;
                imageSciIconFrameIndex = 0;
                toolIconSelectorBox.rectTransform.anchoredPosition = imageSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 49);
            }

            ToolState = ToolMode.Loading;

        } else {
            ShowMainWindows(false);
            notesTxt.enabled = false;
            notebookFrameRefTime = Time.time;
            projectionFrameRefTime = Time.time;
            toolProjection.enabled = true;
            toolIconCloseFrameIndex = 0;
            toolProjection.texture = toolIconCloseFrames[0];
            notebookFrameIndex = notesBgdOpeningFrames.Count - 1;

            ToolState = ToolMode.Closing;
        }
    }

    public bool IsSelecting() {
        return ToolState == ToolMode.Selecting;
    }

    public bool IsInactive() {
        return ToolState == ToolMode.Inactive;
    }

    private void ShowMatSciWindows(bool showOrNo) {
        
        if (showOrNo) {

        }
        else {
            foreach (RawImage img in matSciAnalysisWindows) { img.enabled = false; }
            foreach (Text txt in matSciAnalysisTexts)       { txt.enabled = false; }
        }
    }

    // Image Analysis windows need to be disabled when the tool is not being used
    private void ShowImageAnalysisWindows(bool show) {

        if (show) {
            imageListBgd.enabled = true;
            imageListTxt.enabled = true;
            imageSciWindow.enabled = true;
            imageSciWindowFrame.enabled = true;
            imageControlKnob.enabled = true;
            imageHorizontalKnob.enabled = true;
            imageVerticalKnob.enabled = true;
            imageRollingSearchTxt.enabled = true;
            imageMatchImageBlurb.enabled = true;
            imageReticle.enabled = true;
        } else {
            foreach (RawImage img in imageAnalysisWindows)  { img.enabled = false; }
            foreach (Text txt in imageAnalysisTexts)        { txt.enabled = false; }
        }
    }

    // Trajectory Analysis windows need to be disabled when the tool is not being used
    private void ShowTrajectoryAnalysisWindows(bool show) {
        
        if (show) {
            trajNumOfPtsBgd.enabled = true;
            trajTypeBgd.enabled = true;
            trajNumOfPtsHeader.enabled = true;
            trajNumOfPtsTxt.enabled = true;
            trajTypeHeader.enabled = true;
            trajTypeTxt.enabled = true;
        }
        else {
            foreach (RawImage img in trajAnalysisWindows) { img.enabled = false; }
            foreach (Text txt in trajAnalysisTexts) { txt.enabled = false; }
        }
    }

    // File management popups need to be enabled/disabled when using one of the File management buttons
    private void ShowFileManagementWindows(bool showOrNo) {

        foreach (RawImage img in fileManagementWindows) { img.enabled = showOrNo; }
    }

    private void ShowFileSelectionWindows(bool showOrNo) {

        foreach (RawImage img in fileSelectionWindows)  { img.enabled = showOrNo; }
        foreach (Text txt in fileSelectionTexts)        { txt.enabled = showOrNo; }
    }

    // The following methods switch windows and texts between full alpha and low alpha/faded
    private void MakeMainWindowsActive(bool yesOrNo) {
        
        foreach (RawImage img in mainWindows) { SetVisibilityOfIcon(img, yesOrNo); }
    }

    private void MakeToolWindowsActive(bool yesOrNo) {
        foreach (RawImage img in imageAnalysisWindows)  { SetVisibilityOfIcon(img, yesOrNo); }
        foreach (Text txt in imageAnalysisTexts)        { SetVisibilityOfText(txt ,yesOrNo); }
        foreach (RawImage img in trajAnalysisWindows)   { SetVisibilityOfIcon(img, yesOrNo); }
        foreach (Text txt in trajAnalysisTexts)         { SetVisibilityOfText(txt, yesOrNo); }
    }

    private void SetVisibilityOfIcon(RawImage img, bool activeOrNo) {

        if (activeOrNo) { img.color = new Color(img.color.r, img.color.g, img.color.b, enabledIconAlpha); }
        else            { img.color = new Color(img.color.r, img.color.g, img.color.b, disabledIconAlpha); }
    }

    private void SetVisibilityOfText(Text txt, bool activeOrNo) {

        if (activeOrNo) { txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, enabledIconAlpha); }
        else            { txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, disabledIconAlpha); }
    }

    // Update attract screen
    private void UpdateAttractScreen() {
        if (Time.time - springAttractFrameRefTime > springAttractFrameTime) {
            curSpringAttractFrameIndex = (curSpringAttractFrameIndex - 1 + springAttractFrames.Count) % springAttractFrames.Count;

            Material[] newMats = { springAttractFrames[curSpringAttractFrameIndex] };
            springAttractScreenRend.materials = newMats;

            springAttractFrameRefTime = Time.time;
        }
    }

    // Controls flashing of general highlighter boxes
    private void FlashHighlighter(RawImage img) {
        if (highlightIsFading) {
            if (Time.time - highlightChangeFrameRefTime > highlightChangeFrameTime) {
                if (img.color.a > highlightAlphaMin) {
                    img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a - highlightChange * Time.deltaTime);
                    highlightChangeFrameRefTime = Time.time;
                }
                else {
                    highlightIsFading = false;
                    highlightChangeFrameRefTime = Time.time;
                }
            }
        }
        else {
            if (Time.time - highlightChangeFrameRefTime > highlightChangeFrameTime) {
                if (img.color.a < highlightAlphaMax) {
                    img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a + highlightChange * Time.deltaTime);
                    highlightChangeFrameRefTime = Time.time;
                }
                else {
                    highlightIsFading = true;
                    highlightChangeFrameRefTime = Time.time;
                }
            }
        }
    }

    private void InitializeNotepadText(string textToUnravel) {
        notepadTextTrailingUnderscoreToggleIsActive = false;
        notesTxt.text = "";
        
        notepadTextChars.Clear();

        char[] tempChars = textToUnravel.ToCharArray();

        foreach (char ch in tempChars) {
            notepadTextChars.Add(ch);
        }

        notepadTextIsUnravelling = true;
        notepadTextShowTrailingUnderscore = true;
        curNotepadCharIndex = 0;
        notepadTextUnravelRefTime = Time.time;
        notepadTextTrailingUnderscoreToggleRefTime = Time.time;
    }

    // METHODS - CONTROLS //

    // These are very cumbersome--We're dealing with two (2!) enum states
    // These control input methods are called from WorkDesk, which manages the Work Desk screens

    // Buttons

    public void PressX() {

        if (ActiveToolState == ActiveToolMode.NotUsing) {

            ShowMainWindows(false);

            // If player selects a tool, activate that tool and activate file selection
            if (ToolState == ToolMode.Selecting) {

                if (currentToolPickerIcon == imageSciIcon) {

                    nodicalTransitionFrameIndex = 0;
                    imageSciWindowFrame.texture = nodicalLoadingFrames[nodicalTransitionFrameIndex];
                    nodicalTransitionFrameRefTime = Time.time;

                    imageAnalysisImage.enabled = true;
                    materialsAnalysisChart.enabled = false;

                    imageSciWindowFrame.enabled = true;

                    ToolState = ToolMode.LoadingImageAnalysis;
                }
                else if (currentToolPickerIcon == trajSciIcon) {

                    //curEvidenceData = curTrajSet;

                    ShowTrajectoryAnalysisWindows(true);

                    ToolState = ToolMode.Trajectory;
                    canHoldRS = false;
                    canHoldTriggers = false;
                }
                else if (currentToolPickerIcon == matSciIcon) {

                    //We'll use the imageSciWindowFrame RawImage for the icon transition frames, for convenience
                    matSciIconTransitionFrameIndex = 0;
                    imageSciWindowFrame.texture = matSciIconTransitionFrames[matSciIconTransitionFrameIndex];
                    matSciIconTransitionFrameRefTime = Time.time;
                    imageSciWindowFrame.enabled = true;

                    iconTransitionSwitch = false;

                    ToolState = ToolMode.LoadingMatSci;
                }

                //MakeToolWindowsActive(true);
                ShowFileManagementWindows(false);

                SetVisibilityOfIcon(toolIconSelectorBox, true);

                ActiveToolState = ActiveToolMode.Using;
            }
        } else if (ActiveToolState == ActiveToolMode.Using) {

            //if (currentToolPickerIcon == packetryButton) {
            //    ToolPickState = ToolPickMode.ImageAnalysis;
            //}
            //else if (currentToolPickerIcon == momentusButton) {
            //    ToolPickState = ToolPickMode.Trajectory;
            //}
            //else if (currentToolPickerIcon == ARPAMatButton) {
            //    ToolPickState = ToolPickMode.MatSci;
            //}
            //else if (currentToolPickerIcon == canvasSystemsButton) {
            //    ToolPickState = ToolPickMode.CrimeSceneInspection;
            //}

            if (currentToolPickerIcon == imageSciIcon || currentToolPickerIcon == matSciIcon) {

                MakeToolWindowsActive(false);
                ShowFileManagementWindows(true);
                MakeMainWindowsActive(false);

                
                fileManagementSelector.enabled = true;
                currentFileManagementIcon = addButton;

                ActiveToolState = ActiveToolMode.FileOptions;
            }
            
            else if (currentToolPickerIcon == trajSciIcon) {
                if (TrajectoryAnalysisState == TrajectoryAnalysisMode.PickingNumOfPoints) {

                }
                else if (TrajectoryAnalysisState == TrajectoryAnalysisMode.PickingTrajectoryType) {

                }
            }

        } else if (ActiveToolState == ActiveToolMode.FileOptions) {

            // Before even allowing the player to use the Add To... function, determine whether there are any other projects the file may be added to
            // Repopulate the list of projects to add to--no duplicate files allowed
            curProjectsCanAddTo.Clear();

            foreach (ProjectFile proj in projectList) {

                bool foundDuplicate = false;

                if (curEvidenceData.GetType() == typeof(ImageToAnalyze)) {
                    if (proj.imagesToAnalyzeActual.Count > 0) {
                        foreach (ImageToAnalyze img in proj.imagesToAnalyzeActual) {
                            if (img == (ImageToAnalyze)curEvidenceData) { foundDuplicate = true; }
                        }
                    }
                }
                else if (curEvidenceData.GetType() == typeof(NMRResults)) {
                    if (proj.NMRDataActual.Count > 0) {
                        foreach (NMRResults NMRres in proj.NMRDataActual) {
                            if (NMRres == (NMRResults)curEvidenceData) { foundDuplicate = true; }
                        }
                    }
                }
                else if (currentToolPickerIcon == trajSciIcon) {
                    if (proj.trajDataActual.Count > 0) {
                        foreach (TrajectorySet trajSet in proj.trajDataActual) {
                            if (trajSet == curTrajSet) { foundDuplicate = true; }
                        }
                    }
                }

                if (!foundDuplicate) { curProjectsCanAddTo.Add(proj); }
            }

            if (currentFileManagementIcon == addButton && curProjectsCanAddTo.Count > 0) {

                curProjectsCanAddToIndex = 0;

                ShowFileManagementWindows(true);
                currentFileManagementPopupIcon = okayButton;
                fileManagementSelector.rectTransform.anchoredPosition = okayButton.rectTransform.anchoredPosition;

                fileSelectionBgd.enabled = true;
                fileSelectionTxt.enabled = true;
                fileSelectionWindowTxt.alignment = TextAnchor.UpperLeft;
                fileSelectionWindowTxt.text = "Add  to:";
                fileSelectionTxt.text = curProjectsCanAddTo[0].GetProjectTitle();
                

                ActiveToolState = ActiveToolMode.Adding;

            } else if (currentFileManagementIcon == deleteButton) {

                ShowFileManagementWindows(true);
                currentFileManagementPopupIcon = okayButton;
                fileManagementSelector.rectTransform.anchoredPosition = okayButton.rectTransform.anchoredPosition;

                fileSelectionWindowTxt.alignment = TextAnchor.UpperCenter;
                fileSelectionWindowTxt.text = "Delete  from  " + curProject.GetProjectTitle() + "?";

                ActiveToolState = ActiveToolMode.Deleting;

            } else if ((currentFileManagementIcon == techReqButton) && (availableTechReqServices.Count > 0)) {

                ShowFileManagementWindows(true);
                currentFileManagementPopupIcon = okayButton;
                fileManagementSelector.rectTransform.anchoredPosition = okayButton.rectTransform.anchoredPosition;

                fileSelectionBgd.enabled = true;
                fileSelectionTxt.enabled = true;
                fileSelectionWindowTxt.alignment = TextAnchor.UpperLeft;
                fileSelectionWindowTxt.text = "Req  service:";
                fileSelectionTxt.text = availableTechReqServices[0];

                ActiveToolState = ActiveToolMode.SendingTechRequest;

            }
        } else if (ActiveToolState == ActiveToolMode.Adding) {
            if (currentFileManagementPopupIcon == okayButton) {

                if (curEvidenceData.GetType() == typeof(NMRResults)) {
                    curProjectsCanAddTo[curProjectsCanAddToIndex].NMRDataActual.Add(curNMR);
                } else if (curEvidenceData.GetType() == typeof(ImageToAnalyze)) {
                    curProjectsCanAddTo[curProjectsCanAddToIndex].imagesToAnalyzeActual.Add(curImageToAnalyze);
                } else if (curEvidenceData.GetType() == typeof(TrajectorySet)) {
                    curProjectsCanAddTo[curProjectsCanAddToIndex].trajDataActual.Add(curTrajSet);
                }


            }
            else if (currentFileManagementPopupIcon == cancelButton) {



            }

            ShowFileManagementWindows(false);
            fileManagementSelector.rectTransform.anchoredPosition = addButton.rectTransform.anchoredPosition;

            ActiveToolState = ActiveToolMode.FileOptions;

        } else if (ActiveToolState == ActiveToolMode.Deleting) {
            if (currentFileManagementPopupIcon == okayButton) {

                if (curEvidenceData.GetType() == typeof(NMRResults)) {
                    curProject.NMRDataActual.Remove(curNMR);
                    if ((curMatSciIndex == curProject.NMRDataActual.Count - 1) && (curProject.NMRDataActual.Count > 1)) { curMatSciIndex -= 1; }        // If deleting last file and there's at least one left, go to previous file in list
                    else if (curProject.NMRDataActual.Count == 0)                                                       { noMatSciFiles = true; }       // If no files left, leave index at zero and signal noMatSciFiles
                }
                else if (curEvidenceData.GetType() == typeof(ImageToAnalyze)) {
                    curProject.imagesToAnalyzeActual.Remove(curImageToAnalyze);
                    if ((curImageSciIndex == curProject.imagesToAnalyzeActual.Count - 1) && (curProject.imagesToAnalyzeActual.Count > 1))   { curImageSciIndex -= 1; }      // Ditto
                    else if (curProject.imagesToAnalyzeActual.Count == 0)                                                                   { noImageSciFiles = true; }     //
                }
                else if (curEvidenceData.GetType() == typeof(TrajectorySet)) {
                    curProject.trajDataActual.Remove(curTrajSet);
                    if ((curTrajIndex == curProject.trajDataActual.Count - 1) && (curProject.trajDataActual.Count > 1)) { curTrajIndex -= 1; }          // Ditto
                    else if (curProject.trajDataActual.Count == 0)                                                      { noTrajFiles = true; }         //
                }



            }
            else if (currentFileManagementPopupIcon == cancelButton) {



            }

            ShowFileManagementWindows(false);
            fileManagementSelector.rectTransform.anchoredPosition = deleteButton.rectTransform.anchoredPosition;

            ActiveToolState = ActiveToolMode.FileOptions;

        } else if (ActiveToolState == ActiveToolMode.SendingTechRequest) {
            if (currentFileManagementPopupIcon == okayButton) {

            }
            else if (currentFileManagementPopupIcon == cancelButton) {

            }
        }
    }

    public void PressCircle() {
        if (ActiveToolState == ActiveToolMode.Using) {

            if (ToolState == ToolMode.ImageAnalysis) {

                imageReticle.enabled = false;
                imageListBgd.enabled = false;
                imageListTxt.enabled = false;
                imageListSelector.enabled = false;
                imageControlKnob.enabled = false;
                imageVerticalKnob.enabled = false;
                imageHorizontalKnob.enabled = false;
                imageRefMatchImage.enabled = false;
                imageRefMatchFrame.enabled = false;
                imageMatchImageBlurb.enabled = false;
                imageRollingSearchTxt.enabled = false;

                imageAnalysisLineRend.enabled = false;

                nodicalTransitionFrameIndex = 0;
                imageSciWindow.texture = nodicalImageShutoffFrames[nodicalTransitionFrameIndex];
 
                nodicalTransitionFrameRefTime = Time.time;

                imageIsChanging = true;

                ToolState = ToolMode.ClosingImageAnalysis;
                ActiveToolState = ActiveToolMode.NotUsing;
            }
            else if (ToolState == ToolMode.MatSci) {

                ClearColony();

                matSciMicroscopeCursor.enabled = false;
                matDataChartRefCursor.enabled = false;
                matDataChartSampleCursor.enabled = false;

                microscopeIsAnimating = true;
                curMicroscopeAnimationFrames = microscopeRemoveSlideFrames;
                microscopeFrameIndex = 0;
                matSciMicroscopeView.texture = curMicroscopeAnimationFrames[microscopeFrameIndex];
                matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[matSciMicroscopeBorderFrames.Count - 1];
                microscopeFrameRefTime = Time.time;

                matSciDataBorderWaitingFrameIndex = 0;
                matDataChartWindowFrame.texture = matSciDataBorderWaitingFrames[matSciDataBorderWaitingFrameIndex];
                matSciDataBorderFrameRefTime = Time.time;
            }
            else if (ToolState == ToolMode.Trajectory) {

            }

            //ShowFileManagementWindows(false);
            //MakeToolWindowsActive(false);
            //MakeMainWindowsActive(true);
            //ShowImageAnalysisWindows(false);
            //ShowMatSciWindows(false);
            //ShowTrajectoryAnalysisWindows(false);
            //imageAnalysisLineRend.enabled = false;
            //refPeakSelectorLineRend.enabled = false;

            //ShowMainWindows(true);

            //ActiveToolState = ActiveToolMode.NotUsing;
            //ToolState = ToolMode.Selecting;
        } else if (ActiveToolState == ActiveToolMode.FileOptions) {
            ShowFileManagementWindows(false);
            MakeToolWindowsActive(true);
            MakeMainWindowsActive(false);
            
            fileManagementSelector.enabled = false;
            fileManagementSelector.rectTransform.anchoredPosition = addButton.rectTransform.anchoredPosition;
            currentFileManagementIcon = addButton;

            ActiveToolState = ActiveToolMode.Using;
        } else if (ActiveToolState == ActiveToolMode.Adding || ActiveToolState == ActiveToolMode.Deleting || ActiveToolState == ActiveToolMode.SendingTechRequest) {
            ShowFileManagementWindows(false);
            fileManagementSelector.rectTransform.anchoredPosition = currentFileManagementIcon.rectTransform.anchoredPosition;

            ActiveToolState = ActiveToolMode.FileOptions;
        }
    }

    // Left Joystick

    public void PressUpLS() {
        if (ToolState == ToolMode.Selecting) {
            //    if (currentToolPickerIcon == packetryButton) {
            //        currentToolPickerIcon = ARPAMatButton;
            //    } else if (currentToolPickerIcon == momentusButton) {
            //        currentToolPickerIcon = canvasSystemsButton;
            //    } else if (currentToolPickerIcon == ARPAMatButton) {
            //        currentToolPickerIcon = packetryButton;
            //    } else if (currentToolPickerIcon == canvasSystemsButton) {
            //        currentToolPickerIcon = momentusButton;
            //    }

            //    toolSelectorBox.rectTransform.anchoredPosition = currentToolPickerIcon.rectTransform.anchoredPosition;

            if (currentToolPickerIcon == trajSciIcon) {
                currentToolPickerIcon = imageSciIcon;
                toolIconSelectorBox.rectTransform.anchoredPosition = imageSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 45);
                trajSciIconFrameIndex = trajSciIconStaticFrameIndex;
                trajSciIcon.texture = trajSciIconFrames[trajSciIconStaticFrameIndex];
                imageSciIconFrameIndex = 0;
                InitializeNotepadText(imageSciIconSelectedNotepadText);
            }
            else if (currentToolPickerIcon == matSciIcon) {
                currentToolPickerIcon = trajSciIcon;
                toolIconSelectorBox.rectTransform.anchoredPosition = trajSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 45);
                matSciIconFrameIndex = matSciIconStaticFrameIndex;
                matSciIcon.texture = matSciIconFrames[matSciIconStaticFrameIndex];
                trajSciIconFrameIndex = 0;
                InitializeNotepadText(trajSciIconSelectedNotepadText);
            }
            else if (currentToolPickerIcon == imageSciIcon) {
                currentToolPickerIcon = matSciIcon;
                toolIconSelectorBox.rectTransform.anchoredPosition = matSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 46);
                imageSciIconFrameIndex = imageSciIconStaticFrameIndex;
                imageSciIcon.texture = imageSciIconFrames[imageSciIconStaticFrameIndex];
                matSciIconFrameIndex = 0;
                InitializeNotepadText(matSciIconSelectedNotepadText);
            }
        }

        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {

                if (curImageToAnalyze) {
                    if (curImageToAnalyze.GetFoundImages().Count > 0 && !IsDrawingIASearchBox && !isZoomedOut) {
                        
                        currentFoundImageIndex = (currentFoundImageIndex + 1) % curImageToAnalyze.GetFoundImages().Count;
                        imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition - curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();
                        IAcursorPosition = curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();
                    }
                }

            } else if (ToolState == ToolMode.MatSci) {

                if (curNMR.matches.Count > 0) {
                    foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }

                    curMatSciRefPeakIndex = 0;

                    if (curMatSciRefIndex == 0 && showAllMatSciRefCurves) {
                        showAllMatSciRefCurves = false;

                        refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, false) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    } else if ((curMatSciRefIndex < curNMR.matches.Count - 1) && !showAllMatSciRefCurves) {
                        curMatSciRefIndex++;

                        refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, false) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    } else if (curMatSciRefIndex == curNMR.matches.Count - 1) {
                        showAllMatSciRefCurves = true;

                        refPeakSelectorLineRend.enabled = false;

                        curMatSciRefIndex = 0;
                        for (int i = 0; i < curNMR.matches.Count; i++) {
                            SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                        }

                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, true);
                        notesTxt.text = curNMR.GetNotes();
                    }
                }

            } else if (ToolState == ToolMode.Trajectory) {

            }
        }
    }

    public void PressDownLS() {
        if (ToolState == ToolMode.Selecting) {
            //    if (currentToolPickerIcon == packetryButton) {
            //        currentToolPickerIcon = ARPAMatButton;
            //    }
            //    else if (currentToolPickerIcon == momentusButton) {
            //        currentToolPickerIcon = canvasSystemsButton;
            //    }
            //    else if (currentToolPickerIcon == ARPAMatButton) {
            //        currentToolPickerIcon = packetryButton;
            //    }
            //    else if (currentToolPickerIcon == canvasSystemsButton) {
            //        currentToolPickerIcon = momentusButton;
            //    }

            //    toolSelectorBox.rectTransform.anchoredPosition = currentToolPickerIcon.rectTransform.anchoredPosition;

            if (currentToolPickerIcon == trajSciIcon) {
                currentToolPickerIcon = matSciIcon;
                toolIconSelectorBox.rectTransform.anchoredPosition = matSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 46);
                trajSciIconFrameIndex = trajSciIconStaticFrameIndex;
                trajSciIcon.texture = trajSciIconFrames[trajSciIconStaticFrameIndex];
                matSciIconFrameIndex = 0;
                InitializeNotepadText(matSciIconSelectedNotepadText);
            }
            else if (currentToolPickerIcon == matSciIcon) {
                currentToolPickerIcon = imageSciIcon;
                toolIconSelectorBox.rectTransform.anchoredPosition = imageSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 45);
                matSciIconFrameIndex = matSciIconStaticFrameIndex;
                matSciIcon.texture = matSciIconFrames[matSciIconStaticFrameIndex];
                imageSciIconFrameIndex = 0;
                InitializeNotepadText(imageSciIconSelectedNotepadText);
            }
            else if (currentToolPickerIcon == imageSciIcon) {
                currentToolPickerIcon = trajSciIcon;
                toolIconSelectorBox.rectTransform.anchoredPosition = trajSciIcon.rectTransform.anchoredPosition;
                toolIconSelectorBox.rectTransform.sizeDelta = new Vector2(190, 45);
                imageSciIconFrameIndex = imageSciIconStaticFrameIndex;
                imageSciIcon.texture = imageSciIconFrames[imageSciIconStaticFrameIndex];
                trajSciIconFrameIndex = 0;
                InitializeNotepadText(trajSciIconSelectedNotepadText);
            }
        }

        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {

                if (curImageToAnalyze) {
                    if (curImageToAnalyze.GetFoundImages().Count > 0 && !IsDrawingIASearchBox && !isZoomedOut) {
                        
                        currentFoundImageIndex = (currentFoundImageIndex - 1 + curImageToAnalyze.GetFoundImages().Count) % curImageToAnalyze.GetFoundImages().Count;
                        imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition - curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();
                        IAcursorPosition = curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();
                    }
                }

            }
            else if (ToolState == ToolMode.MatSci) {

                if (curNMR.matches.Count > 0) {
                    foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }

                    curMatSciRefPeakIndex = 0;
                    matDataChartRefCursor.enabled = true;

                    if (curMatSciRefIndex == 0 && !showAllMatSciRefCurves) {
                        showAllMatSciRefCurves = true;

                        refPeakSelectorLineRend.enabled = false;

                        for (int i = 0; i < curNMR.matches.Count; i++) {
                            SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                        }

                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, true);
                        notesTxt.text = curNMR.GetNotes();
                    }
                    else if ((curMatSciRefIndex > 0) && !showAllMatSciRefCurves) {
                        curMatSciRefIndex--;

                        refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, false) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    }
                    else if (curMatSciRefIndex == 0 && showAllMatSciRefCurves) {
                        showAllMatSciRefCurves = false;

                        refPeakSelectorLineRend.enabled = true;

                        curMatSciRefIndex = curNMR.matches.Count - 1;
                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, false) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    }
                }
                else {
                    matDataChartRefCursor.enabled = false;
                }

            }
            else if (ToolState == ToolMode.Trajectory) {

            }
        }
    }

    public void PressLeftLS() {
        if (ToolState == ToolMode.Selecting) {
            //if (currentToolPickerIcon == packetryButton) {
            //    currentToolPickerIcon = momentusButton;
            //}
            //else if (currentToolPickerIcon == momentusButton) {
            //    currentToolPickerIcon = packetryButton;
            //}
            //else if (currentToolPickerIcon == ARPAMatButton) {
            //    currentToolPickerIcon = canvasSystemsButton;
            //}
            //else if (currentToolPickerIcon == canvasSystemsButton) {
            //    currentToolPickerIcon = ARPAMatButton;
            //}

        } else if (ActiveToolState == ActiveToolMode.FileOptions) {
            if (currentFileManagementIcon == addButton) {
                currentFileManagementIcon = techReqButton;
            } else if (currentFileManagementIcon == deleteButton) {
                currentFileManagementIcon = addButton;
            } else if (currentFileManagementIcon == techReqButton) {
                currentFileManagementIcon = deleteButton;
            }

            fileManagementSelector.rectTransform.anchoredPosition = currentFileManagementIcon.rectTransform.anchoredPosition;
        } else if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {
                 
            } else if (ToolState == ToolMode.MatSci) {
                //if (!showAllMatSciRefCurves && curNMR.matches[curMatSciRefIndex].maxima.Count > 1) {
                //    curMatSciRefPeakIndex = (curMatSciRefPeakIndex - 1 + curNMR.matches[curMatSciRefIndex].maxima.Count) % curNMR.matches[curMatSciRefIndex].maxima.Count;
                //    PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                //    matResultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                //    notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                //}

                curMatSciSamplePeakIndex = (curMatSciSamplePeakIndex - 1 + curNMR.maxima.Count) % curNMR.maxima.Count;
                //PlaceSamplePeakSelector(curNMR, curMatSciSamplePeakIndex);
                //SetTargetSpecies(curNMR, curMatSciSamplePeakIndex);

            } else if (ToolState == ToolMode.Trajectory) {

            }
        } else if (ActiveToolState == ActiveToolMode.Adding || ActiveToolState == ActiveToolMode.Deleting || ActiveToolState == ActiveToolMode.SendingTechRequest) {
            if (currentFileManagementPopupIcon == okayButton) {
                fileManagementSelector.rectTransform.anchoredPosition = cancelButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = cancelButton;
            } else if (currentFileManagementPopupIcon == cancelButton) {
                fileManagementSelector.rectTransform.anchoredPosition = okayButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = okayButton;
            }
        }
    }

    public void PressRightLS() {
        if (ToolState == ToolMode.Selecting) {
            //if (currentToolPickerIcon == packetryButton) {
            //    currentToolPickerIcon = momentusButton;
            //}
            //else if (currentToolPickerIcon == momentusButton) {
            //    currentToolPickerIcon = packetryButton;
            //}
            //else if (currentToolPickerIcon == ARPAMatButton) {
            //    currentToolPickerIcon = canvasSystemsButton;
            //}
            //else if (currentToolPickerIcon == canvasSystemsButton) {
            //    currentToolPickerIcon = ARPAMatButton;
            //}
            
        } else if (ActiveToolState == ActiveToolMode.FileOptions) {
            if (currentFileManagementIcon == addButton) {
                currentFileManagementIcon = deleteButton;
            }
            else if (currentFileManagementIcon == deleteButton) {
                currentFileManagementIcon = techReqButton;
            }
            else if (currentFileManagementIcon == techReqButton) {
                currentFileManagementIcon = addButton;
            }

            fileManagementSelector.rectTransform.anchoredPosition = currentFileManagementIcon.rectTransform.anchoredPosition;
        } else if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {

            }
            else if (ToolState == ToolMode.MatSci) {
                
                //curMatSciRefPeakIndex = (curMatSciRefPeakIndex + 1 + curNMR.matches[curMatSciRefIndex].maxima.Count) % curNMR.matches[curMatSciRefIndex].maxima.Count;
                //PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                //matResultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                //notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);

                curMatSciSamplePeakIndex = (curMatSciSamplePeakIndex + 1) % curNMR.maxima.Count;
                //PlaceSamplePeakSelector(curNMR, curMatSciSamplePeakIndex);
                //SetTargetSpecies(curNMR, curMatSciSamplePeakIndex);
                
            }
            else if (ToolState == ToolMode.Trajectory) {

            }
        } else if (ActiveToolState == ActiveToolMode.Adding || ActiveToolState == ActiveToolMode.Deleting || ActiveToolState == ActiveToolMode.SendingTechRequest) {
            if (currentFileManagementPopupIcon == okayButton) {
                fileManagementSelector.rectTransform.anchoredPosition = cancelButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = cancelButton;
            }
            else if (currentFileManagementPopupIcon == cancelButton) {
                fileManagementSelector.rectTransform.anchoredPosition = okayButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = okayButton;
            }
        }
    }

    // Right Joystick

    public void PressUpRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                // Image Analysis cursor can move up if it's still below the top-left corner
                // Technically, the image is moving, but we're tracking the movement like it's the cursor
                if ((!IsDrawingIASearchBox || (IsDrawingIASearchBox && (IAcursorPosition.y < currentSearchTopLeftCorner.y))) && (IAcursorPosition.y < cursorLimits.y)) {
                    float moveY = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(0, moveY);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(0, -moveY);
                }

                imageVerticalKnobFrameIndex = (int)((value + 1) * imageVerticalKnobFrames.Count / 2);
                if (imageVerticalKnobFrameIndex < 0) { imageVerticalKnobFrameIndex = 0; }
                if (imageVerticalKnobFrameIndex > imageVerticalKnobFrames.Count - 1) { imageVerticalKnobFrameIndex = imageVerticalKnobFrames.Count - 1; }
            }
        }
    }

    public void PressDownRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                if (IAcursorPosition.y > -cursorLimits.y) {
                    float moveY = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(0, moveY);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(0, -moveY);
                }

                imageVerticalKnobFrameIndex = (int)((value + 1) * imageVerticalKnobFrames.Count / 2);
                if (imageVerticalKnobFrameIndex < 0) { imageVerticalKnobFrameIndex = 0; }
                if (imageVerticalKnobFrameIndex > imageVerticalKnobFrames.Count - 1) { imageVerticalKnobFrameIndex = imageVerticalKnobFrames.Count - 1; }
            }
        }
    }

    public void PressLeftRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                // Image Analysis cursor can move left if it's still to the right of the top-left corner
                // Technically, the image is moving, but we're tracking the movement like it's the cursor
                if ((!IsDrawingIASearchBox || (IsDrawingIASearchBox && (IAcursorPosition.x > currentSearchTopLeftCorner.x))) && (IAcursorPosition.x > -cursorLimits.x)) {
                    float moveX = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(moveX, 0);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(-moveX, 0);
                }

                imageHorizontalKnobFrameIndex = (int)((value + 1) * imageHorizontalKnobFrames.Count / 2);
                if (imageHorizontalKnobFrameIndex < 0) { imageHorizontalKnobFrameIndex = 0; }
                if (imageHorizontalKnobFrameIndex > imageHorizontalKnobFrames.Count - 1) { imageHorizontalKnobFrameIndex = imageHorizontalKnobFrames.Count - 1; }
            } else if (ToolState == ToolMode.MatSci) {
                if (!showAllMatSciRefCurves && curNMR.matches[curMatSciRefIndex].maxima.Count > 1) {
                    curMatSciRefPeakIndex = (curMatSciRefPeakIndex - 1 + curNMR.matches[curMatSciRefIndex].maxima.Count) % curNMR.matches[curMatSciRefIndex].maxima.Count;
                    //PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                    matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, false) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                    notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                }
            }
        }
    }

    public void PressRightRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                if (IAcursorPosition.x < cursorLimits.x) {
                    float moveX = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(moveX, 0);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(-moveX, 0);
                }

                imageHorizontalKnobFrameIndex = (int)((value + 1) * imageHorizontalKnobFrames.Count / 2);
                if (imageHorizontalKnobFrameIndex < 0)                                      { imageHorizontalKnobFrameIndex = 0; }
                if (imageHorizontalKnobFrameIndex > imageHorizontalKnobFrames.Count - 1)    { imageHorizontalKnobFrameIndex = imageHorizontalKnobFrames.Count - 1; }
            } else if (ToolState == ToolMode.MatSci) {
                if (!showAllMatSciRefCurves && curNMR.matches[curMatSciRefIndex].maxima.Count > 1) {
                    curMatSciRefPeakIndex = (curMatSciRefPeakIndex + 1 + curNMR.matches[curMatSciRefIndex].maxima.Count) % curNMR.matches[curMatSciRefIndex].maxima.Count;
                    //PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                    matResultsTxt.text = curNMR.GetMatSciResults(curMatSciSamplePeakIndex, false) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                    notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                }
            }
        }
    }

    public void ReleaseVerticalRS() {
        imageVerticalKnobFrameIndex = imageVerticalKnobFrames.Count / 2 + 1;
    }

    public void ReleaseHorizontalRS() {
        imageHorizontalKnobFrameIndex = imageHorizontalKnobFrames.Count / 2 + 1;
    }

    // Arrows

    public void PressUpArrow() {

        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.MatSci) {
                if (projHandler.projectList[currentProjectIndex].NMRDataActual.Count > 0) {
                    //if (curMatSciIndex > 0) {
                    //    curMatSciIndex--;                
                    //} else {
                    //    curMatSciIndex = projHandler.openProjects[currentProjectIndex].NMRDataActual.Count - 1;
                    //}

                    curMatSciIndex = (curMatSciIndex - 1 + curProject.NMRDataActual.Count) % curProject.NMRDataActual.Count;

                    matDataListSelector.rectTransform.anchoredPosition = matDataListSelectorInitialPos + new Vector2(0, -curMatSciIndex * selectorTextHeight);
                    curMatSciRefIndex = 0;
                    curMatSciRefPeakIndex = 0;
                    curMatSciSamplePeakIndex = 0;
                    foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }
                    

                    ClearColony();
                    curSpeciesToTrack = null;
                    matSciMicroscopeCursor.enabled = false;
                    matDataChartRefCursor.enabled = false;
                    matDataChartSampleCursor.enabled = false;
                    matSciMicroscopeCursor.color = new Color(matSciMicroscopeCursor.color.r, matSciMicroscopeCursor.color.g, matSciMicroscopeCursor.color.b,
                                                                matSciMicroscopeSelectorsAlphaMax);
                    matDataChartSampleCursor.color = new Color(matDataChartSampleCursor.color.r, matDataChartSampleCursor.color.g, matDataChartSampleCursor.color.b,
                                                                matSciMicroscopeSelectorsAlphaMax);
                    matSciMicroscopeSelectorsAlphaIsIncreasing = false;

                    curNMR = projHandler.projectList[currentProjectIndex].NMRDataActual[curMatSciIndex];

                    microscopeIsAnimating = true;
                    curMicroscopeAnimationFrames = microscopeChangeSlideFrames;
                    microscopeFrameIndex = 0;
                    matSciMicroscopeView.texture = curMicroscopeAnimationFrames[microscopeFrameIndex];
                    matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[matSciMicroscopeBorderFrames.Count - 1];
                    microscopeFrameRefTime = Time.time;

                    matSciDataBorderWaitingFrameIndex = 0;
                    matDataChartWindowFrame.texture = matSciDataBorderWaitingFrames[matSciDataBorderWaitingFrameIndex];
                    matSciDataBorderFrameRefTime = Time.time;

                    //SetMaterialsAnalysisSampleSpectrum(curNMR);
                    //curNMR.DetermineMatches();

                    //showAllMatSciRefCurves = true;
                    //refPeakSelectorLineRend.enabled = false;

                    //for (int i = 0; i < curNMR.matches.Count; i++) {
                    //    SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                    //}

                    //matResultsTxt.text = curNMR.GetResults();
                    //notesTxt.text = curNMR.GetNotes();
                }
            }
            else if (ToolState == ToolMode.ImageAnalysis) {
                if (curProject.imagesToAnalyzeActual.Count > 0) {

                    Debug.Log(curImageToAnalyze.name);

                    curImageSciIndex = (curImageSciIndex + 1) % curProject.imagesToAnalyzeActual.Count;

                    nodicalTransitionFrameIndex = 0;
                    imageSciWindow.texture = nodicalImageChangeFrames[nodicalTransitionFrameIndex];
                    nodicalTransitionFrameRefTime = Time.time;

                    imageControlKnobFrameIndex = (imageControlKnobFrameIndex + 1) % imageControlKnobFrames.Count;
                    imageReticle.enabled = false;

                    imageIsChanging = true;
                }
            }
        } else if ((ActiveToolState == ActiveToolMode.Adding) && (curProjectsCanAddTo.Count > 1)) {

            curProjectsCanAddToIndex = (curProjectsCanAddToIndex + 1) % curProjectsCanAddTo.Count;
            fileSelectionTxt.text = curProjectsCanAddTo[curProjectsCanAddToIndex].GetProjectTitle();

        }
    }

    public void PressDownArrow() {

        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.MatSci) {
                if (projHandler.projectList[currentProjectIndex].NMRDataActual.Count > 0) {
                    //if (curMatSciIndex < (projHandler.openProjects[currentProjectIndex].NMRDataActual.Count - 1)) {
                    //    curMatSciIndex++;
                    //}
                    //else {
                    //    curMatSciIndex = 0;
                    //}

                    curMatSciIndex = (curMatSciIndex + 1) % curProject.NMRDataActual.Count;

                    matDataListSelector.rectTransform.anchoredPosition = matDataListSelectorInitialPos + new Vector2(0, -curMatSciIndex * selectorTextHeight);
                    curMatSciRefIndex = 0;
                    curMatSciRefPeakIndex = 0;
                    curMatSciSamplePeakIndex = 0;
                    foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }

                    ClearColony();
                    curSpeciesToTrack = null;
                    matSciMicroscopeCursor.enabled = false;
                    matDataChartSampleCursor.enabled = false;
                    matDataChartRefCursor.enabled = false;
                    matSciMicroscopeCursor.color = new Color(matSciMicroscopeCursor.color.r, matSciMicroscopeCursor.color.g, matSciMicroscopeCursor.color.b,
                                                                matSciMicroscopeSelectorsAlphaMax);
                    matDataChartSampleCursor.color = new Color(matDataChartSampleCursor.color.r, matDataChartSampleCursor.color.g, matDataChartSampleCursor.color.b,
                                                                matSciMicroscopeSelectorsAlphaMax);
                    matSciMicroscopeSelectorsAlphaIsIncreasing = false;

                    curNMR = projHandler.projectList[currentProjectIndex].NMRDataActual[curMatSciIndex];

                    microscopeIsAnimating = true;
                    curMicroscopeAnimationFrames = microscopeChangeSlideFrames;
                    microscopeFrameIndex = 0;
                    matSciMicroscopeView.texture = curMicroscopeAnimationFrames[microscopeFrameIndex];
                    matSciMicroscopeViewFrame.texture = matSciMicroscopeBorderFrames[matSciMicroscopeBorderFrames.Count - 1];
                    microscopeFrameRefTime = Time.time;

                    matSciDataBorderWaitingFrameIndex = 0;
                    matDataChartWindowFrame.texture = matSciDataBorderWaitingFrames[matSciDataBorderWaitingFrameIndex];
                    matSciDataBorderFrameRefTime = Time.time;

                    //SetMaterialsAnalysisSampleSpectrum(curNMR);
                    //curNMR.DetermineMatches();

                    //showAllMatSciRefCurves = true;
                    //refPeakSelectorLineRend.enabled = false;

                    //for (int i = 0; i < curNMR.matches.Count; i++) {
                    //    SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                    //}

                    //matResultsTxt.text = curNMR.GetResults();
                    //notesTxt.text = curNMR.GetNotes();
                }
            }
            else if (ToolState == ToolMode.ImageAnalysis) {
                if (curProject.imagesToAnalyzeActual.Count > 0) {
                    curImageSciIndex = (curImageSciIndex - 1 + curProject.imagesToAnalyzeActual.Count) % curProject.imagesToAnalyzeActual.Count;

                    nodicalTransitionFrameIndex = 0;
                    imageSciWindow.texture = nodicalImageChangeFrames[nodicalTransitionFrameIndex];
                    nodicalTransitionFrameRefTime = Time.time;

                    imageControlKnobFrameIndex = (imageControlKnobFrameIndex - 1 + imageControlKnobFrames.Count) % imageControlKnobFrames.Count;
                    imageReticle.enabled = false;

                    imageIsChanging = true;
                }
            }
        } else if ((ActiveToolState == ActiveToolMode.Adding) && (curProjectsCanAddTo.Count > 1)) {

            curProjectsCanAddToIndex = (curProjectsCanAddToIndex - 1 + curProjectsCanAddTo.Count) % curProjectsCanAddTo.Count;
            fileSelectionTxt.text = curProjectsCanAddTo[curProjectsCanAddToIndex].GetProjectTitle();

        }
    }

    public void PressLeftArrow() {
        if (ActiveToolState == ActiveToolMode.Using && numberOfProjects > 0) {

            currentProjectIndex = (currentProjectIndex - 1 + numberOfProjects) % numberOfProjects;

            projectNumberTxt.text = projHandler.projectList[currentProjectIndex].GetProjectTitle();
        }
    }

    public void PressRightArrow() {
        if (ActiveToolState == ActiveToolMode.Using && numberOfProjects > 0) {

            currentProjectIndex = (currentProjectIndex + 1) % numberOfProjects;

            projectNumberTxt.text = projHandler.projectList[currentProjectIndex].GetProjectTitle();
        }
    }

    // Triggers

    public void PressLeftTrigger(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {
                //analysisCamera.fieldOfView = 60;
                if (!isZoomedOut) {
                    imageAnalysisImage.rectTransform.sizeDelta = new Vector2(128, 128);
                    imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition;
                    IAcursorPosition = initialIACursorPosition;
                    imageReticle.enabled = false;
                    isZoomedOut = true;
                }
            }
        }
    }

    public void ReleaseLeftTrigger() {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {
                //analysisCamera.fieldOfView = 120;
                imageAnalysisImage.rectTransform.sizeDelta = initialImageSize;
                //imageReticle.enabled = true;
                isZoomedOut = false;
            }
        }
    }

    public void PressRightTrigger(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {
                if (!IsDrawingIASearchBox) {
                    
                    currentSearchTopLeftCorner = IAcursorPosition;
                    IsDrawingIASearchBox = true;
                }
            }
        }
    }

    public void ReleaseRightTrigger() {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolState == ToolMode.ImageAnalysis) {
                if (IsDrawingIASearchBox) {

                    // If player was drawing a search box and releases the trigger, if the box matches an image of interest, make it a found image and snap to it
                    if (curImageToAnalyze) {
                        ImageCapture imgCap = curImageToAnalyze.SearchForImage(currentSearchTopLeftCorner, IAcursorPosition);

                        if (imgCap != null) {
                            imageAnalysisImage.rectTransform.anchoredPosition = -1 * imgCap.GetCenter();
                            IAcursorPosition = imgCap.GetCenter();

                            //resultsTxt.text = curImageToAnalyze.GetResults();
                            notesTxt.text = curImageToAnalyze.GetNotes();
                        }
                    }

                    finishedDrawingIASearchBox = true;
                    IsDrawingIASearchBox = false;
                }
            }
        }
    }

    public bool CanHoldRightStick() {
        return canHoldRS;
    }

    public bool CanHoldTriggers() {
        return canHoldTriggers;
    }

    private void UpdateProjectAndFileListDisplay(EvidenceData curSample, int currentLineNumber, int currentFileIndex, int numberOfFiles) {
        if (ActiveToolState == ActiveToolMode.Using) {
            projectNumberTxt.text = projHandler.projectList[currentProjectIndex].GetProjectTitle();

            int numberOfFilesBeforeSelected = numOfVisibleEvidenceFilesInList - currentLineNumber;
            //int numberOfFilesAfterSelected = numOfVisibleEvidenceFilesInList - numberOfFilesBeforeSelected - 1;
            
            
        }
    }

    // METHODS - TOOLS FUNCTIONS //

    // Trajectory Analysis

    private void InitializeMomentusObjects() {

        curMomentusMeshRends.Clear();
        curMomentusSkinnedMeshRends.Clear();

        foreach (Transform tf in curMomentusSimulationObjects) {
            if (tf.GetComponent<ID>() != null) {
                ID tfID = tf.GetComponent<ID>();

                if (tfID.meshRends.Count > 0) {
                    foreach (MeshRenderer mesh in tfID.meshRends) {
                        curMomentusMeshRends.Add(mesh);
                    }
                }
                if (tfID.skinnedMeshRends.Count > 0) {
                    foreach (SkinnedMeshRenderer mesh in tfID.skinnedMeshRends) {
                        curMomentusSkinnedMeshRends.Add(mesh);
                    }
                }
            }
        }

        foreach (MeshRenderer msh in curMomentusMeshRends) {
            foreach (Material mat in msh.materials) {
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, momentusObjAlphaMin);
            }
        }

        foreach (SkinnedMeshRenderer sk in curMomentusSkinnedMeshRends) {
            foreach (Material mat in sk.materials) {
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, momentusObjAlphaMin);
            }
        }
    }

    private bool FadeInMomentusObject(List<SkinnedMeshRenderer> skins) {

        bool allAlphasReached = true;

        foreach (MeshRenderer msh in curMomentusMeshRends) {
            foreach (Material mat in msh.materials) {
                if (mat.color.a < momentusObjAlphaMax) {
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a + momentusObjFadeFactor * Time.deltaTime);
                    allAlphasReached = false;
                }
            }
        }
        foreach (SkinnedMeshRenderer sk in curMomentusSkinnedMeshRends) {
            foreach (Material mat in sk.materials) {
                if (mat.color.a < momentusObjAlphaMax) {
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a + momentusObjFadeFactor * Time.deltaTime);
                    allAlphasReached = false;
                }
            }
        }

        return allAlphasReached;
    }

    private bool FadeOutMomentusObject() {

        bool allAlphasReached = true;

        foreach (SkinnedMeshRenderer sk in curMomentusSkinnedMeshRends) {
            foreach (Material mat in sk.materials) {
                if (mat.color.a > momentusObjAlphaMin) {
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a - momentusObjFadeFactor * Time.deltaTime);
                    allAlphasReached = false;
                }
            }
        }
        foreach (MeshRenderer msh in curMomentusMeshRends) {
            foreach (Material mat in msh.materials) {
                if (mat.color.a > momentusObjAlphaMin) {
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a - momentusObjFadeFactor * Time.deltaTime);
                    allAlphasReached = false;
                }
            }
        }

        return allAlphasReached;
    }

    private void MomentusSmoothCamUpdate(Transform curTarget) {
        if ((momentusCamOnScene.transform.position - curTarget.position).magnitude > momentusCamAcceptableDistanceToTarget) {
            momentusCamOnScene.transform.position = Vector3.Lerp(momentusCamOnScene.transform.position, curTarget.position, Time.deltaTime * momentusCamSmoothingFactor);
            momentusCamOnScene.transform.rotation = Quaternion.Lerp(momentusCamOnScene.transform.rotation, curTarget.rotation, Time.deltaTime * momentusCamSmoothingFactor * 2);

            momentusStageCam.transform.rotation = Quaternion.Lerp(momentusStageCam.transform.rotation, curTarget.rotation, Time.deltaTime * momentusCamSmoothingFactor);
        }

        
    }

    // Materials Analysis

    // Draw current sample curve
    private void SetMaterialsAnalysisSampleSpectrum(NMRResults result) {

        materialsAnalysisLineRend.enabled = true;

        if (result.PeakDataHasBeenCalculated()) {
            List<float> calcPeaks = result.GetCalculatedPeaks();

            for (int i = 0; i < maxShift; i++) {
                materialsAnalysisLineRend.SetPosition(i, new Vector3(i + lineRendOffsetX, calcPeaks[i] + lineRendOffsetY, -1));
            }
        }
        else {
            List<float> calcPeaks = new List<float>();

            for (int i = 0; i < maxShift; i++) {
                int rand = Random.Range(0, result.maxNoise);
                int noise = rand - rand / 2;

                Vector3 position = imageAnalysisImage.rectTransform.anchoredPosition;

                bool foundPeak = false;

                foreach (NMRPeak peak in result.maxima) {

                    if (Mathf.Floor(peak.shift) == i) {
                        materialsAnalysisLineRend.SetPosition(i, position + new Vector3(i + lineRendOffsetX, peak.intensity + lineRendOffsetY + noise, -1));
                        calcPeaks.Add(peak.intensity + noise);
                        foundPeak = true;
                    }
                }

                if (!foundPeak) {
                    materialsAnalysisLineRend.SetPosition(i, position + new Vector3(i + lineRendOffsetX, lineRendOffsetY + (noise), -1));
                    calcPeaks.Add(noise);
                }
            }

            result.SetCalculatedPeaks(calcPeaks);
        }        
    }

    // Draw reference curve by index
    // References are found by checking all of the possible references in ProjectHandler and getting the best three (acceptable) matches
    private void SetMaterialsAnalysisRefSpectrum(NMRRefResults result, int refLineRendIndex) {

        materialsAnalysisRefLineRends[refLineRendIndex].enabled = true;

        if (result.PeakDataHasBeenCalculated()) {
            List<float> calcPeaks = result.GetCalculatedPeaks();

            for (int i = 0; i < maxShift; i++) {
                materialsAnalysisRefLineRends[refLineRendIndex].SetPosition(i, new Vector3(i + lineRendOffsetX, calcPeaks[i] + lineRendOffsetY, -1));
            }
        }
        else {
            List<float> calcPeaks = new List<float>();

            for (int i = 0; i < maxShift; i++) {
                int rand = Random.Range(0, result.maxNoise);
                int noise = rand - rand / 2;

                Vector3 position = imageAnalysisImage.rectTransform.anchoredPosition;

                bool foundPeak = false;

                foreach (NMRRefPeak peak in result.maxima) {
                    if (Mathf.Floor(peak.shift) == i) {
                        materialsAnalysisRefLineRends[refLineRendIndex].SetPosition(i, position + new Vector3(i + lineRendOffsetX, peak.intensity + lineRendOffsetY + noise, -1));
                        calcPeaks.Add(peak.intensity + noise);
                        foundPeak = true;
                    }
                }
            
                if (!foundPeak) {
                    materialsAnalysisRefLineRends[refLineRendIndex].SetPosition(i, position + new Vector3(i + lineRendOffsetX, lineRendOffsetY + (noise), -1));
                    calcPeaks.Add(noise);
                }
            }

            result.SetCalculatedPeaks(calcPeaks);
        }
    }

    private void PlaceSamplePeakSelector(NMRResults sampleResult, int peakIndex) {
        matDataChartSampleCursor.enabled = true;

        Vector2 chartCenter = matDataChartWindow.rectTransform.anchoredPosition;

        Vector2 offset = new Vector2((Mathf.Floor(sampleResult.maxima[peakIndex].shift) + lineRendOffsetX) * 1.32f, Mathf.Floor(sampleResult.maxima[peakIndex].intensity) * 1.5f - 20);
        matDataChartSampleCursor.rectTransform.anchoredPosition = offset + chartCenter;

        // Flash the species cursor and sample data peak cursor
        if (matSciMicroscopeSelectorsAlphaIsIncreasing) {
            if (matSciMicroscopeCursor.color.a < matSciMicroscopeSelectorsAlphaMax) {
                matSciMicroscopeCursor.color = new Color(matSciMicroscopeCursor.color.r, matSciMicroscopeCursor.color.g, matSciMicroscopeCursor.color.b,
                                                            matSciMicroscopeCursor.color.a + matSciMicroscopeSelectorsAlphaIncrement * Time.deltaTime);
                matDataChartSampleCursor.color = new Color(matDataChartSampleCursor.color.r, matDataChartSampleCursor.color.g, matDataChartSampleCursor.color.b,
                                                            matDataChartSampleCursor.color.a + matSciMicroscopeSelectorsAlphaIncrement * Time.deltaTime);
            }
            else {
                matSciMicroscopeCursor.color = new Color(matSciMicroscopeCursor.color.r, matSciMicroscopeCursor.color.g, matSciMicroscopeCursor.color.b,
                                                            matSciMicroscopeSelectorsAlphaMax);
                matDataChartSampleCursor.color = new Color(matDataChartSampleCursor.color.r, matDataChartSampleCursor.color.g, matDataChartSampleCursor.color.b,
                                                            matSciMicroscopeSelectorsAlphaMax);
                matSciMicroscopeSelectorsAlphaIsIncreasing = false;
            }
        }
        else {
            if (matSciMicroscopeCursor.color.a > matSciMicroscopeSelectorsAlphaMin) {
                matSciMicroscopeCursor.color = new Color(matSciMicroscopeCursor.color.r, matSciMicroscopeCursor.color.g, matSciMicroscopeCursor.color.b,
                                                            matSciMicroscopeCursor.color.a - matSciMicroscopeSelectorsAlphaIncrement * Time.deltaTime);
                matDataChartSampleCursor.color = new Color(matDataChartSampleCursor.color.r, matDataChartSampleCursor.color.g, matDataChartSampleCursor.color.b,
                                                            matDataChartSampleCursor.color.a - matSciMicroscopeSelectorsAlphaIncrement * Time.deltaTime);
            }
            else {
                matSciMicroscopeCursor.color = new Color(matSciMicroscopeCursor.color.r, matSciMicroscopeCursor.color.g, matSciMicroscopeCursor.color.b,
                                                            matSciMicroscopeSelectorsAlphaMin);
                matDataChartSampleCursor.color = new Color(matDataChartSampleCursor.color.r, matDataChartSampleCursor.color.g, matDataChartSampleCursor.color.b,
                                                            matSciMicroscopeSelectorsAlphaMin);
                matSciMicroscopeSelectorsAlphaIsIncreasing = true;
            }
        }
    }

    // Positions and draws the LineRanderer selector box on a peak of the reference curve
    private void PlaceRefPeakSelector(NMRRefResults refResult, int peakIndex) {

        //refPeakSelectorLineRend.enabled = true;
        matDataChartRefCursor.enabled = true;

        Vector2 chartCenter = matDataChartWindow.rectTransform.anchoredPosition;

        Vector2 offset = new Vector2((Mathf.Floor(refResult.maxima[peakIndex].shift) + lineRendOffsetX) * 1.32f, Mathf.Floor(refResult.maxima[peakIndex].intensity) * 1.5f - 20);
        matDataChartRefCursor.rectTransform.anchoredPosition = offset + chartCenter;
        //refPeakSelectorLineRend.SetPosition(0, centerPos + new Vector3(-2, 2, -1));
        //refPeakSelectorLineRend.SetPosition(1, centerPos + new Vector3(2, 2, -1));
        //refPeakSelectorLineRend.SetPosition(2, centerPos + new Vector3(2, -2, -1));
        //refPeakSelectorLineRend.SetPosition(3, centerPos + new Vector3(-2, -2, -1));
        //refPeakSelectorLineRend.SetPosition(4, centerPos + new Vector3(-2, 2, -1));
    }

    private void SetTargetSpecies(NMRResults sampleResult, int peakIndex) {
        bool specFound = false;

        curSpeciesToTrack = null;

        if (visibleSpecies.Count > 0) {
            foreach (NMRSpeciesVisualization sp in visibleSpecies) {
                if (!specFound && sp.GetShift() == sampleResult.maxima[peakIndex].shift && sp.GetImage().enabled) {
                    curSpeciesToTrack = sp;
                    specFound = true;
                }
            }
        }

        if (specFound && curSpeciesToTrack != null) {
            //matSciMicroscopeCursor.rectTransform.SetParent(curSpeciesToTrack.GetImage().rectTransform);
            matSciMicroscopeCursor.enabled = true;
            //matSciMicroscopeCursor.rectTransform.anchoredPosition = curSpeciesToTrack.GetImage().rectTransform.anchoredPosition;
            //matSciMicroscopeCursor.rectTransform.localPosition = Vector2.zero;
            matSciMicroscopeCursor.rectTransform.position = curSpeciesToTrack.GetImage().rectTransform.position;
        }
        else {
            matSciMicroscopeCursor.enabled = false;
        }
    }

    private void GenerateSlideColony() {
        foreach (NMRPeak pk in curNMR.maxima) {
            int numOfEntities = Mathf.FloorToInt(pk.numOfEntitiesPerIntensity * pk.intensity);

            for (int i = 0; i < numOfEntities; i++) {
                RawImage img = Instantiate(speciesVisPlaceholder, matSciMicroscopeView.transform).GetComponent<RawImage>();

                img.rectTransform.anchoredPosition = new Vector2(Random.Range(-microscopeSlideTravelLimits.x, microscopeSlideTravelLimits.x),
                                                                Random.Range(-microscopeSlideTravelLimits.y, microscopeSlideTravelLimits.y));

                NMRSpeciesVisualization newSpec;

                if (pk.isPhage) {
                    newSpec = new NMRSpeciesVisualization(pk.shift, img, microscopeWindowCenter, microscopeSlideTravelLimits, pk.movementSpeedMin, pk.movementSpeedMax, pk.isMobile,
                                                                    pk.targetDetectionRange, pk.consumablePeaks, pk.moveFrames, pk.consumptionFrames, pk.deathFrames);
                }
                else {
                    newSpec = new NMRSpeciesVisualization(pk.shift, img, microscopeWindowCenter, microscopeSlideTravelLimits, pk.movementSpeedMin, pk.movementSpeedMax, pk.isMobile,
                                                                    pk.moveFrames, pk.deathFrames);
                }

                visibleSpecies.Add(newSpec);
            }
        }

        Debug.Log("Number entities generated: " + visibleSpecies.Count);

        if (visibleSpecies.Count > 0) {
            foreach (NMRSpeciesVisualization spec in visibleSpecies) {
                spec.InitializeVictimList(visibleSpecies);
            }
        }
    }

    private void ClearColony() {

        //matSciMicroscopeCursor.rectTransform.SetParent(null);

        while (visibleSpecies.Count > 0) {
            NMRSpeciesVisualization temp = visibleSpecies[0];
            visibleSpecies.Remove(temp);
            Destroy(temp.GetImage().gameObject);
        }

        Debug.Log("Number of entities remaining: " + visibleSpecies.Count);
    }

    public Color GetResultsHeaderColor() {
        return resultsHeaderColor;
    }

    public Color GetResultsBodyColor() {
        return resultsBodyColor;
    }

    public Color GetShiftValueColor() {
        return shiftValueColor;
    }

    public Color GetPeakValueColor() {
        return peakValueColor;
    }

    public Color GetSampleCursorColor() {
        return samplePeakSelectColor;
    }

    public Color GetRefCursorColor() {
        return refPeakSelectColor;
    }

    public Color GetMatchRefColor(int index) {
        switch (index) {
            case 0:
                return firstRefMatchColor;
                
            case 1:
                return secondRefMatchColor;
                
            case 2:
                return thirdRefMatchColor;
                
            default:
                return thirdRefMatchColor;
                
        }
    }

    public Color GetMatchRatingColor(float value) {
        if (value >= excellentMatchRating)                                  { return excellentMatchRatingColor; }
        else if (value < excellentMatchRating && value >= goodMatchRating)  { return goodMatchRatingColor; }
        else if (value < goodMatchRating && value >= okayMatchRating)       { return okayMatchRatingColor; }
        else                                                                { return badMatchRatingColor; }
    }

    // Image Analysis

    // Shows the image search box as it's being drawn
    private void DrawImageSearchBox(Vector2 topLeft, Vector2 bottomRight) {

        imageAnalysisLineRend.SetPosition(0, initialImagePosition + new Vector2(topLeft.x, topLeft.y));
        imageAnalysisLineRend.SetPosition(1, initialImagePosition + new Vector2(bottomRight.x, topLeft.y));
        imageAnalysisLineRend.SetPosition(2, initialImagePosition + new Vector2(bottomRight.x, bottomRight.y));
        imageAnalysisLineRend.SetPosition(3, initialImagePosition + new Vector2(topLeft.x, bottomRight.y));
        imageAnalysisLineRend.SetPosition(4, initialImagePosition + new Vector2(topLeft.x, topLeft.y));
    }
}
