using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolsScreen : MonoBehaviour
{
    public RawImage cachedResultsListBgd, resultsBgd, notesBgd, projectNumberBgd, imageWindow, imageWindowFrame,
                    imageAnalysisSelectedImage, imageAnalysisReticle, imageAnalysisRefMatchImage, imageAnalysisRefMatchFrame,
                    trajNumOfPtsBgd, trajTypeBgd, trajNumOfPtsSelector, trajTypeSelector,
                    packetryButton, momentusButton, ARPAMatButton, canvasSystemsButton, addButton, deleteButton, techReqButton,
                    fileManagementPopupWindow, fileManagementOkayButton, fileManagementCancelButton, fileManagementPopupSelection,
                    toolSelectorBox, fileManagementSelector, cachedResultsListHighlighter;
    public Text cachedResultsListTxt, resultsTxt, notesTxt, projectNumberTxt,
                imageAnalysisRollingSearchTxt, imageAnalysisMatchImageBlurb,
                trajNumOfPtsHeader, trajNumOfPtsTxt, trajTypeHeader, trajTypeTxt, trajMatchesHeader, trajMatchesTxt,
                fileManagementPopupTxt, fileManagementPopupSelectionTxt;

    public Texture[] selectorBoxFrames;
    public bool showTools;
    public float selectorBoxTimer;
    [Range(0.0f, 1.0f)]
    public float enabledIconAlpha, disabledIconAlpha;    

    // ProjectHandler, and all of the LineRenderers used for the various tools!
    private ProjectHandler projHandler;

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

    private enum ToolPickMode { Selecting, Trajectory, MatSci, ImageAnalysis, CrimeSceneInspection }
    private ToolPickMode ToolPickState;

    private enum ActiveToolMode { NotUsing, Using, FileOptions, Adding, Deleting, SendingTechRequest }
    private ActiveToolMode ActiveToolState;

    private enum TrajectoryAnalysisMode { PickingNumOfPoints, PickingTrajectoryType, LookingAtReferences}
    private TrajectoryAnalysisMode TrajectoryAnalysisState;

    private List<RawImage> fileManagementIcons = new List<RawImage>();
    private List<RawImage> toolIcons = new List<RawImage>();
    private List<RawImage> activeToolWindows = new List<RawImage>();
    private List<RawImage> imageAnalysisWindows = new List<RawImage>();
    private List<RawImage> trajAnalysisWindows = new List<RawImage>();
    private List<RawImage> fileManagementPopupIcons = new List<RawImage>();

    private List<Text> fileManagementTexts = new List<Text>();
    private List<Text> activeToolTexts = new List<Text>();
    private List<Text> imageAnalysisTexts = new List<Text>();
    private List<Text> trajAnalysisTexts = new List<Text>();
    private List<Text> fileManagementPopupTexts = new List<Text>();

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
    private int cachedResultsListSelectorHeight = 7;
    private Vector2 cachedResultsListSelectorInitialPos;
    [SerializeField]
    private float highlightAlphaMin, highlightAlphaMax, highlightChangeFrameTime, highlightChange;
    private float highlightChangeFrameRefTime;
    private bool highlightIsFading = false;

    // Does the current forensic tool employ continuous right joystick control?
    private bool canHoldRS = false;
    private bool canHoldTriggers = false;

    // Crime scene stuff
    [SerializeField]
    private float evidenceCameraInitialRotX, evidenceCameraInitialFOV;
    [SerializeField]
    private float evidenceCameraRotSpeed, evidenceCameraMinRotX, evidenceCameraMaxRotX;         // Note that evidence camera rotation is really the rotation of the stage focus transform
    [SerializeField]
    private float evidenceCameraZoomSpeed, evidenceCameraMinFOV, evidenceCameraMaxFOV;

    private Vector3 evidenceCameraInitialPos;
    private Quaternion evidenceCameraInitialRot;

    private float evidenceCameraRotX, evidenceCameraFOV;
    
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
    private int curMatSciRefIndex, curMatSciRefPeakIndex;

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

        projHandler = FindObjectOfType<ProjectHandler>();

        if (projHandler.openProjects.Count > 0) {
            foreach (ProjectFile proj in projHandler.openProjects) {
                if (proj.imagesToAnalyzeActual.Count > 0) {
                    foreach (ImageToAnalyze img in proj.imagesToAnalyzeActual) {
                        img.ClearFoundImagesList();
                    }
                }
            }
        }

        evidenceCameraInitialPos = evidenceCamera.transform.localPosition;
        evidenceCameraRotX = evidenceCameraInitialRotX;
        stageFocus.Rotate(evidenceCameraRotX, 0, 0);
        evidenceCameraInitialRot = stageFocus.localRotation;

        evidenceCameraFOV = evidenceCameraInitialFOV;
        evidenceCamera.fieldOfView = evidenceCameraFOV;

        resultsTxt.supportRichText = true;
        imageAnalysisMatchImageBlurb.supportRichText = true;

        // Fill all of the RawImage and Text lists with their relevant members
        toolIcons.Add(packetryButton);
        toolIcons.Add(momentusButton);
        toolIcons.Add(ARPAMatButton);
        toolIcons.Add(canvasSystemsButton);

        activeToolWindows.Add(imageWindow);
        activeToolWindows.Add(imageWindowFrame);
        activeToolWindows.Add(projectNumberBgd);
        activeToolWindows.Add(resultsBgd);
        activeToolWindows.Add(notesBgd);
        activeToolWindows.Add(cachedResultsListBgd);
        activeToolWindows.Add(projectNumberBgd);

        activeToolTexts.Add(resultsTxt);
        activeToolTexts.Add(notesTxt);
        activeToolTexts.Add(cachedResultsListTxt);
        activeToolTexts.Add(projectNumberTxt);

        fileManagementIcons.Add(addButton);
        fileManagementIcons.Add(deleteButton);
        fileManagementIcons.Add(techReqButton);

        imageAnalysisWindows.Add(imageAnalysisSelectedImage);
        imageAnalysisWindows.Add(imageAnalysisRefMatchImage);
        imageAnalysisWindows.Add(imageAnalysisRefMatchFrame);
        imageAnalysisWindows.Add(imageAnalysisReticle);

        imageAnalysisTexts.Add(imageAnalysisRollingSearchTxt);
        imageAnalysisTexts.Add(imageAnalysisMatchImageBlurb);

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

        fileManagementPopupIcons.Add(fileManagementPopupWindow);
        fileManagementPopupIcons.Add(fileManagementOkayButton);
        fileManagementPopupIcons.Add(fileManagementCancelButton);
        fileManagementPopupIcons.Add(fileManagementPopupSelection);

        fileManagementPopupTexts.Add(fileManagementPopupTxt);
        fileManagementPopupTexts.Add(fileManagementPopupSelectionTxt);

        showTools = false;

        MakeToolIconsActive(true);
        MakeToolWindowsActive(false);
        MakeFileManagementIconsActive(false);

        ToolPickState = ToolPickMode.Selecting;
        ActiveToolState = ActiveToolMode.NotUsing;
        TrajectoryAnalysisState = TrajectoryAnalysisMode.PickingNumOfPoints;

        SelectingATool(false);
        selectorBoxCurrentFrame = 0;
        selectorBoxTimerCurTime = 0;
        selectorBoxLightening = false;

        currentToolPickerIcon = packetryButton;
        currentFileManagementIcon = techReqButton;

        currentProjectIndex = 0;
        cachedResultsListSelectorInitialPos = cachedResultsListHighlighter.rectTransform.anchoredPosition;

        showAllMatSciRefCurves = true;
        curMatSciIndex = 0;
        curMatSciRefIndex = 0;
        curMatSciRefPeakIndex = 0;
        refPeakSelectorIncreasingAlpha = true;

        isZoomedOut = false;
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
        ShowAllWindows(showTools);

        evidenceCamera.transform.LookAt(stageFocus);

        // Continually update the open projects List
        projectList = projHandler.openProjects;
        numberOfProjects = projHandler.openProjects.Count;

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

        if (showTools) {
            switch (ToolPickState) {
                case ToolPickMode.Selecting:

                    FlashHighlighter(toolSelectorBox);

                    break;
                case ToolPickMode.MatSci:

                    // Continually update the EvidenceData file index for displaying the file list
                    curEvidenceFileIndex = curMatSciIndex;

                    if (refPeakSelectorIncreasingAlpha) {
                        if ((refPeakSelectorLineRend.startColor.a < refPeakSelectorMaxAlpha) && (refPeakSelectorLineRend.endColor.a < refPeakSelectorMaxAlpha)) {
                            refPeakSelectorLineRend.startColor = new Color(refPeakSelectorLineRend.startColor.r, refPeakSelectorLineRend.startColor.g, refPeakSelectorLineRend.startColor.b,
                                                                            refPeakSelectorLineRend.startColor.a + refPeakSelectorFlashAmount * Time.deltaTime);
                            refPeakSelectorLineRend.endColor = new Color(refPeakSelectorLineRend.endColor.r, refPeakSelectorLineRend.endColor.g, refPeakSelectorLineRend.endColor.b,
                                                                            refPeakSelectorLineRend.endColor.a + refPeakSelectorFlashAmount * Time.deltaTime);
                        }
                        else {
                            refPeakSelectorIncreasingAlpha = false;
                        }
                    }
                    else {
                        if ((refPeakSelectorLineRend.startColor.a > refPeakSelectorMinAlpha) && (refPeakSelectorLineRend.endColor.a > refPeakSelectorMinAlpha)) {
                            refPeakSelectorLineRend.startColor = new Color(refPeakSelectorLineRend.startColor.r, refPeakSelectorLineRend.startColor.g, refPeakSelectorLineRend.startColor.b,
                                                                            refPeakSelectorLineRend.startColor.a - refPeakSelectorFlashAmount * Time.deltaTime);
                            refPeakSelectorLineRend.endColor = new Color(refPeakSelectorLineRend.endColor.r, refPeakSelectorLineRend.endColor.g, refPeakSelectorLineRend.endColor.b,
                                                                            refPeakSelectorLineRend.endColor.a - refPeakSelectorFlashAmount * Time.deltaTime);
                        }
                        else {
                            refPeakSelectorIncreasingAlpha = true;
                        }
                    }

                    break;
                case ToolPickMode.Trajectory:

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
                case ToolPickMode.ImageAnalysis:

                    // Continually update the EvidenceData file index for displaying the file list
                    curEvidenceFileIndex = curImageSciIndex;

                    if (IsDrawingIASearchBox) {
                        imageAnalysisLineRend.enabled = true;
                        imageAnalysisRefMatchImage.enabled = false;
                        imageAnalysisRefMatchFrame.enabled = false;
                        imageAnalysisMatchImageBlurb.text = "";

                        DrawImageSearchBox(currentSearchTopLeftCorner, IAcursorPosition);
                    }
                    else {
                        if (curImageToAnalyze) {

                            // If the cursor is hovering over a found image, select it and display its info and possible ref image and info
                            ImageCapture imgCap = curImageToAnalyze.SelectImage(IAcursorPosition);
                            if (imgCap != null && !isZoomedOut) {

                                // If the player closes Work Desk while on this screen, all windows are closed
                                // Bringing Work Desk back up refreshes this screen, but not all of the Image Analysis Windows
                                ShowImageAnalysisWindows(true);

                                imageAnalysisRollingSearchTxt.text = curImageToAnalyze.GetResults(imgCap);

                                imageAnalysisLineRend.enabled = true;
                                //imageAnalysisSelectedImage.enabled = true;
                                DrawImageSearchBox(imgCap.topLeftCorner, imgCap.bottomRightCorner);

                                if (imgCap.imageRefRevealed) {
                                    imageAnalysisRefMatchImage.texture = imgCap.imageRefRevealed.refImage;
                                    imageAnalysisRefMatchImage.enabled = true;
                                    imageAnalysisRefMatchFrame.enabled = true;
                                    imageAnalysisMatchImageBlurb.text = curImageToAnalyze.GetBlurb(imgCap);

                                    notesTxt.text = curImageToAnalyze.GetNotes(imgCap);
                                }
                                else {
                                    notesTxt.text = curImageToAnalyze.GetNotes();
                                }
                            }
                            else {
                                imageAnalysisLineRend.enabled = false;
                                //imageAnalysisSelectedImage.enabled = false;
                                imageAnalysisRefMatchImage.enabled = false;
                                imageAnalysisRefMatchFrame.enabled = false;
                                imageAnalysisMatchImageBlurb.text = "";

                                imageAnalysisRollingSearchTxt.text = curImageToAnalyze.GetResults();
                                notesTxt.text = curImageToAnalyze.GetNotes();
                            }
                        }
                    }
                    break;
                case ToolPickMode.CrimeSceneInspection:



                    break;
            }

            switch (ActiveToolState) {
                case ActiveToolMode.NotUsing:
                    break;
                case ActiveToolMode.Using:
                    FlashHighlighter(cachedResultsListHighlighter);
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
    }

    // METHODS //

    // Show or hide the Tools screen windows and texts
    private void ShowAllWindows(bool showOrNo) {

        foreach (RawImage img in fileManagementIcons)   { img.enabled = showOrNo; }
        foreach (RawImage img in toolIcons)             { img.enabled = showOrNo; }
        foreach (RawImage img in activeToolWindows)     { img.enabled = showOrNo; }

        foreach (Text txt in fileManagementTexts)       { txt.enabled = showOrNo; }
        foreach (Text txt in activeToolTexts)           { txt.enabled = showOrNo; }

        toolSelectorBox.enabled = showOrNo;

        if (showOrNo == false) {
            cachedResultsListHighlighter.enabled = false;
            fileManagementSelector.enabled = false;

            // Image Analysis windows are a special case, and enabling/disabling them needs to be controllable within the ActiveToolStates
            ShowImageAnalysisWindows(false);
            ShowTrajectoryAnalysisWindows(false);
            ShowFileManagementPopups(false);
        }
    }

    // Image Analysis windows need to be disabled when the tool is not being used
    private void ShowImageAnalysisWindows(bool show) {

        if (show) {
            //imageAnalysisRefMatchImage.enabled = true;
            imageAnalysisRollingSearchTxt.enabled = true;
            imageAnalysisMatchImageBlurb.enabled = true;
            imageAnalysisReticle.enabled = true;
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
    private void ShowFileManagementPopups(bool show) {

        if (show) {
            fileManagementPopupWindow.enabled = true;
            fileManagementOkayButton.enabled = true;
            fileManagementCancelButton.enabled = true;
            fileManagementPopupTxt.enabled = true;
        }
        else {
            foreach (RawImage img in fileManagementPopupIcons)  { img.enabled = false; }
            foreach (Text txt in fileManagementPopupTexts)      { txt.enabled = false; }
        }
    }

    // The following methods switch windows and texts between full alpha and low alpha/faded
    private void MakeToolIconsActive(bool yesOrNo) {
        
        foreach (RawImage img in toolIcons) { SetVisibilityOfIcon(img, yesOrNo); }
    }

    private void MakeToolWindowsActive(bool yesOrNo) {

        foreach (RawImage img in activeToolWindows)     { SetVisibilityOfIcon(img, yesOrNo); }
        foreach (Text txt in activeToolTexts)           { SetVisibilityOfText(txt, yesOrNo); }
        foreach (RawImage img in imageAnalysisWindows)  { SetVisibilityOfIcon(img, yesOrNo); }
        foreach (Text txt in imageAnalysisTexts)        { SetVisibilityOfText(txt ,yesOrNo); }
        foreach (RawImage img in trajAnalysisWindows)   { SetVisibilityOfIcon(img, yesOrNo); }
        foreach (Text txt in trajAnalysisTexts)         { SetVisibilityOfText(txt, yesOrNo); }
    }

    private void MakeFileManagementIconsActive(bool yesOrNo) {
        
        foreach (RawImage img in fileManagementIcons)       { SetVisibilityOfIcon(img, yesOrNo); }
        foreach (Text txt in fileManagementTexts)           { SetVisibilityOfText(txt, yesOrNo); }
        foreach (RawImage img in fileManagementPopupIcons)  { SetVisibilityOfIcon(img, yesOrNo); }
        foreach (Text txt in fileManagementPopupTexts)      { SetVisibilityOfText(txt, yesOrNo); }
    }

    private void SetVisibilityOfIcon(RawImage img, bool activeOrNo) {

        if (activeOrNo) { img.color = new Color(img.color.r, img.color.g, img.color.b, enabledIconAlpha); }
        else            { img.color = new Color(img.color.r, img.color.g, img.color.b, disabledIconAlpha); }
    }

    private void SetVisibilityOfText(Text txt, bool activeOrNo) {

        if (activeOrNo) { txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, enabledIconAlpha); }
        else            { txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, disabledIconAlpha); }
    }

    // Controls flashing of toolSelectorBox
    private void SelectingATool(bool isSelecting) {
        toolSelectorBox.enabled = isSelecting;

        if (isSelecting) {
            if (Time.time - selectorBoxTimerCurTime > selectorBoxTimer) {
                if (selectorBoxLightening) {
                    if (selectorBoxCurrentFrame > 0) { selectorBoxCurrentFrame -= 1; } 
                    else { selectorBoxLightening = !selectorBoxLightening; }
                } else {
                    if (selectorBoxCurrentFrame < (selectorBoxFrames.Length - 1)) { selectorBoxCurrentFrame += 1; } 
                    else { selectorBoxLightening = !selectorBoxLightening; }
                }
                selectorBoxTimerCurTime = Time.time;
            }
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

    // METHODS - CONTROLS //

    // These are very cumbersome--We're dealing with two (2!) enum states
    // These control input methods are called from WorkDesk, which manages the Work Desk screens

    // Buttons

    public void PressX() {

        if (ActiveToolState == ActiveToolMode.NotUsing) {

            // If player selects a tool, activate that tool and activate file selection
            if (ToolPickState == ToolPickMode.Selecting) {

                if (currentToolPickerIcon == packetryButton) {

                    ToolPickState = ToolPickMode.ImageAnalysis;
                    imageWindow.texture = analysisView;
                    imageAnalysisImage.enabled = true;
                    imageAnalysisLineRend.enabled = true;
                    materialsAnalysisChart.enabled = false;
                    materialsAnalysisLineRend.enabled = false;
                    refPeakSelectorLineRend.enabled = false;
                    foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }
                    resultsTxt.text = "";
                    ShowImageAnalysisWindows(true);

                    imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition;
                    IAcursorPosition = initialIACursorPosition;
                    IsDrawingIASearchBox = false;
                    isZoomedOut = false;

                    curEvidenceData = curImageToAnalyze;

                    imageAnalysisImage.texture = curImageToAnalyze.image;

                    canHoldRS = true;
                    canHoldTriggers = true;

                    currentFoundImageIndex = 0;

                    imageAnalysisRollingSearchTxt.text = curImageToAnalyze.GetResults();
                    notesTxt.text = curImageToAnalyze.GetNotes();
                }
                else if (currentToolPickerIcon == momentusButton) {

                    //curEvidenceData = curTrajSet;

                    ShowTrajectoryAnalysisWindows(true);

                    ToolPickState = ToolPickMode.Trajectory;
                    canHoldRS = false;
                    canHoldTriggers = false;
                }
                else if (currentToolPickerIcon == ARPAMatButton) {

                    // Enable/disable relevant windows for Materials Analysis
                    ToolPickState = ToolPickMode.MatSci;
                    imageWindow.texture = analysisView;
                    imageAnalysisImage.enabled = false;
                    imageAnalysisLineRend.enabled = false;
                    materialsAnalysisChart.enabled = true;
                    materialsAnalysisLineRend.enabled = true;

                    // No continuous stick control
                    canHoldRS = false;
                    canHoldTriggers = false;

                    curEvidenceData = curNMR;

                    // Draw NMRResult spectrum using Materials Analysis LineRenderer
                    //curNMR = projHandler.openProjects[currentProjectIndex].NMRDataActual[currentEvidenceFileIndex];
                    SetMaterialsAnalysisSampleSpectrum(curNMR);

                    // Determine NMRRefResult matches for the current NMRResult and add them to its matches list                
                    curNMR.DetermineMatches();

                    // Draw the NMRRefResult spectra using the ref LineRenderers

                    if (curMatSciRefIndex == 0 && showAllMatSciRefCurves) {

                        refPeakSelectorLineRend.enabled = false;

                        for (int i = 0; i < curNMR.matches.Count; i++) {
                            SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                        }

                        resultsTxt.text = curNMR.GetResults();
                        notesTxt.text = curNMR.GetNotes();
                    }
                    else {
                        refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        // Get the results and notes for the sample and references
                        resultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    }
                }
                else if (currentToolPickerIcon == canvasSystemsButton) {
                    ToolPickState = ToolPickMode.CrimeSceneInspection;
                    imageWindow.texture = crimeSceneView;
                    //stageFocus.localRotation = evidenceCameraInitialRot;
                    canHoldRS = true;
                    canHoldTriggers = true;
                }

                MakeToolWindowsActive(true);
                MakeFileManagementIconsActive(false);
                MakeToolIconsActive(false);

                SetVisibilityOfIcon(toolSelectorBox, true);

                cachedResultsListHighlighter.enabled = true;

                stageFocus.localRotation = evidenceCameraInitialRot;

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

            if (currentToolPickerIcon == packetryButton || currentToolPickerIcon == ARPAMatButton) {

                MakeToolWindowsActive(false);
                MakeFileManagementIconsActive(true);
                MakeToolIconsActive(false);

                cachedResultsListHighlighter.color = new Color(cachedResultsListHighlighter.color.r, cachedResultsListHighlighter.color.g, cachedResultsListHighlighter.color.b, disabledIconAlpha);

                fileManagementSelector.enabled = true;
                currentFileManagementIcon = addButton;

                ActiveToolState = ActiveToolMode.FileOptions;
            }
            else if (currentToolPickerIcon == momentusButton) {
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
                else if (currentToolPickerIcon == momentusButton) {
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

                ShowFileManagementPopups(true);
                currentFileManagementPopupIcon = fileManagementOkayButton;
                fileManagementSelector.rectTransform.anchoredPosition = fileManagementOkayButton.rectTransform.anchoredPosition;

                fileManagementPopupSelection.enabled = true;
                fileManagementPopupSelectionTxt.enabled = true;
                fileManagementPopupTxt.alignment = TextAnchor.UpperLeft;
                fileManagementPopupTxt.text = "Add  to:";
                fileManagementPopupSelectionTxt.text = curProjectsCanAddTo[0].projectName;
                

                ActiveToolState = ActiveToolMode.Adding;

            } else if (currentFileManagementIcon == deleteButton) {

                ShowFileManagementPopups(true);
                currentFileManagementPopupIcon = fileManagementOkayButton;
                fileManagementSelector.rectTransform.anchoredPosition = fileManagementOkayButton.rectTransform.anchoredPosition;

                fileManagementPopupTxt.alignment = TextAnchor.UpperCenter;
                fileManagementPopupTxt.text = "Delete  from  " + curProject.projectName + "?";

                ActiveToolState = ActiveToolMode.Deleting;

            } else if ((currentFileManagementIcon == techReqButton) && (availableTechReqServices.Count > 0) && (ToolPickState != ToolPickMode.CrimeSceneInspection)) {

                ShowFileManagementPopups(true);
                currentFileManagementPopupIcon = fileManagementOkayButton;
                fileManagementSelector.rectTransform.anchoredPosition = fileManagementOkayButton.rectTransform.anchoredPosition;

                fileManagementPopupSelection.enabled = true;
                fileManagementPopupSelectionTxt.enabled = true;
                fileManagementPopupTxt.alignment = TextAnchor.UpperLeft;
                fileManagementPopupTxt.text = "Req  service:";
                fileManagementPopupSelectionTxt.text = availableTechReqServices[0];

                ActiveToolState = ActiveToolMode.SendingTechRequest;

            }
        } else if (ActiveToolState == ActiveToolMode.Adding) {
            if (currentFileManagementPopupIcon == fileManagementOkayButton) {

                if (curEvidenceData.GetType() == typeof(NMRResults)) {
                    curProjectsCanAddTo[curProjectsCanAddToIndex].NMRDataActual.Add(curNMR);
                } else if (curEvidenceData.GetType() == typeof(ImageToAnalyze)) {
                    curProjectsCanAddTo[curProjectsCanAddToIndex].imagesToAnalyzeActual.Add(curImageToAnalyze);
                } else if (curEvidenceData.GetType() == typeof(TrajectorySet)) {
                    curProjectsCanAddTo[curProjectsCanAddToIndex].trajDataActual.Add(curTrajSet);
                }


            }
            else if (currentFileManagementPopupIcon == fileManagementCancelButton) {



            }

            ShowFileManagementPopups(false);
            fileManagementSelector.rectTransform.anchoredPosition = addButton.rectTransform.anchoredPosition;

            ActiveToolState = ActiveToolMode.FileOptions;

        } else if (ActiveToolState == ActiveToolMode.Deleting) {
            if (currentFileManagementPopupIcon == fileManagementOkayButton) {

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
            else if (currentFileManagementPopupIcon == fileManagementCancelButton) {



            }

            ShowFileManagementPopups(false);
            fileManagementSelector.rectTransform.anchoredPosition = deleteButton.rectTransform.anchoredPosition;

            ActiveToolState = ActiveToolMode.FileOptions;

        } else if (ActiveToolState == ActiveToolMode.SendingTechRequest) {
            if (currentFileManagementPopupIcon == fileManagementOkayButton) {

            }
            else if (currentFileManagementPopupIcon == fileManagementCancelButton) {

            }
        }
    }

    public void PressCircle() {
        if (ActiveToolState == ActiveToolMode.Using) {
            MakeFileManagementIconsActive(false);
            MakeToolWindowsActive(false);
            MakeToolIconsActive(true);

            ShowImageAnalysisWindows(false);
            ShowTrajectoryAnalysisWindows(false);
            imageAnalysisLineRend.enabled = false;
            refPeakSelectorLineRend.enabled = false;

            cachedResultsListHighlighter.enabled = false;

            evidenceCameraRotX = evidenceCameraInitialRotX;
            stageFocus.localRotation = evidenceCameraInitialRot;

            ActiveToolState = ActiveToolMode.NotUsing;
            ToolPickState = ToolPickMode.Selecting;
        } else if (ActiveToolState == ActiveToolMode.FileOptions) {
            MakeFileManagementIconsActive(false);
            MakeToolWindowsActive(true);
            MakeToolIconsActive(false);

            cachedResultsListHighlighter.enabled = true;
            fileManagementSelector.enabled = false;
            fileManagementSelector.rectTransform.anchoredPosition = addButton.rectTransform.anchoredPosition;
            currentFileManagementIcon = addButton;

            ActiveToolState = ActiveToolMode.Using;
        } else if (ActiveToolState == ActiveToolMode.Adding || ActiveToolState == ActiveToolMode.Deleting || ActiveToolState == ActiveToolMode.SendingTechRequest) {
            ShowFileManagementPopups(false);
            fileManagementSelector.rectTransform.anchoredPosition = currentFileManagementIcon.rectTransform.anchoredPosition;

            ActiveToolState = ActiveToolMode.FileOptions;
        }
    }

    // Left Joystick

    public void PressUpLS() {
        if (ToolPickState == ToolPickMode.Selecting) {
            if (currentToolPickerIcon == packetryButton) {
                currentToolPickerIcon = ARPAMatButton;
            } else if (currentToolPickerIcon == momentusButton) {
                currentToolPickerIcon = canvasSystemsButton;
            } else if (currentToolPickerIcon == ARPAMatButton) {
                currentToolPickerIcon = packetryButton;
            } else if (currentToolPickerIcon == canvasSystemsButton) {
                currentToolPickerIcon = momentusButton;
            }

            toolSelectorBox.rectTransform.anchoredPosition = currentToolPickerIcon.rectTransform.anchoredPosition;
        }

        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {
                
            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {

                if (curImageToAnalyze) {
                    if (curImageToAnalyze.GetFoundImages().Count > 0 && !IsDrawingIASearchBox && !isZoomedOut) {                       

                        imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition - curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();
                        IAcursorPosition = curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();

                        currentFoundImageIndex = (currentFoundImageIndex + 1) % curImageToAnalyze.GetFoundImages().Count;
                    }
                }

            } else if (ToolPickState == ToolPickMode.MatSci) {

                if (curNMR.matches.Count > 0) {
                    foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }

                    curMatSciRefPeakIndex = 0;

                    if (curMatSciRefIndex == 0 && showAllMatSciRefCurves) {
                        showAllMatSciRefCurves = false;

                        refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        resultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    } else if ((curMatSciRefIndex < curNMR.matches.Count - 1) && !showAllMatSciRefCurves) {
                        curMatSciRefIndex++;

                        refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        resultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    } else if (curMatSciRefIndex == curNMR.matches.Count - 1) {
                        showAllMatSciRefCurves = true;

                        refPeakSelectorLineRend.enabled = false;

                        curMatSciRefIndex = 0;
                        for (int i = 0; i < curNMR.matches.Count; i++) {
                            SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                        }

                        resultsTxt.text = curNMR.GetResults();
                        notesTxt.text = curNMR.GetNotes();
                    }
                }

            } else if (ToolPickState == ToolPickMode.Trajectory) {

            }
        }
    }

    public void PressDownLS() {
        if (ToolPickState == ToolPickMode.Selecting) {
            if (currentToolPickerIcon == packetryButton) {
                currentToolPickerIcon = ARPAMatButton;
            }
            else if (currentToolPickerIcon == momentusButton) {
                currentToolPickerIcon = canvasSystemsButton;
            }
            else if (currentToolPickerIcon == ARPAMatButton) {
                currentToolPickerIcon = packetryButton;
            }
            else if (currentToolPickerIcon == canvasSystemsButton) {
                currentToolPickerIcon = momentusButton;
            }

            toolSelectorBox.rectTransform.anchoredPosition = currentToolPickerIcon.rectTransform.anchoredPosition;
        }

        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {

            }
            else if (ToolPickState == ToolPickMode.ImageAnalysis) {

                if (curImageToAnalyze) {
                    if (curImageToAnalyze.GetFoundImages().Count > 0 && !IsDrawingIASearchBox && !isZoomedOut) {
                        
                        imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition - curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();
                        IAcursorPosition = curImageToAnalyze.GetFoundImages()[currentFoundImageIndex].GetCenter();

                        currentFoundImageIndex = (currentFoundImageIndex - 1 + curImageToAnalyze.GetFoundImages().Count) % curImageToAnalyze.GetFoundImages().Count;
                    }
                }

            }
            else if (ToolPickState == ToolPickMode.MatSci) {

                if (curNMR.matches.Count > 0) {
                    foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }

                    curMatSciRefPeakIndex = 0;

                    if (curMatSciRefIndex == 0 && !showAllMatSciRefCurves) {
                        showAllMatSciRefCurves = true;

                        refPeakSelectorLineRend.enabled = false;

                        for (int i = 0; i < curNMR.matches.Count; i++) {
                            SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
                        }

                        resultsTxt.text = curNMR.GetResults();
                        notesTxt.text = curNMR.GetNotes();
                    }
                    else if ((curMatSciRefIndex > 0) && !showAllMatSciRefCurves) {
                        curMatSciRefIndex--;

                        refPeakSelectorLineRend.enabled = true;

                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        resultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    }
                    else if (curMatSciRefIndex == 0 && showAllMatSciRefCurves) {
                        showAllMatSciRefCurves = false;

                        refPeakSelectorLineRend.enabled = true;

                        curMatSciRefIndex = curNMR.matches.Count - 1;
                        SetMaterialsAnalysisRefSpectrum(curNMR.matches[curMatSciRefIndex], curMatSciRefIndex);
                        PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                        resultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                        notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                    }
                }

            }
            else if (ToolPickState == ToolPickMode.Trajectory) {

            }
        }
    }

    public void PressLeftLS() {
        if (ToolPickState == ToolPickMode.Selecting) {
            if (currentToolPickerIcon == packetryButton) {
                currentToolPickerIcon = momentusButton;
            }
            else if (currentToolPickerIcon == momentusButton) {
                currentToolPickerIcon = packetryButton;
            }
            else if (currentToolPickerIcon == ARPAMatButton) {
                currentToolPickerIcon = canvasSystemsButton;
            }
            else if (currentToolPickerIcon == canvasSystemsButton) {
                currentToolPickerIcon = ARPAMatButton;
            }

            toolSelectorBox.rectTransform.anchoredPosition = currentToolPickerIcon.rectTransform.anchoredPosition;
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
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {

            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {
                 
            } else if (ToolPickState == ToolPickMode.MatSci) {
                if (!showAllMatSciRefCurves && curNMR.matches[curMatSciRefIndex].maxima.Count > 1) {
                    curMatSciRefPeakIndex = (curMatSciRefPeakIndex - 1 + curNMR.matches[curMatSciRefIndex].maxima.Count) % curNMR.matches[curMatSciRefIndex].maxima.Count;
                    PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                    resultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                    notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                }
            } else if (ToolPickState == ToolPickMode.Trajectory) {

            }
        } else if (ActiveToolState == ActiveToolMode.Adding || ActiveToolState == ActiveToolMode.Deleting || ActiveToolState == ActiveToolMode.SendingTechRequest) {
            if (currentFileManagementPopupIcon == fileManagementOkayButton) {
                fileManagementSelector.rectTransform.anchoredPosition = fileManagementCancelButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = fileManagementCancelButton;
            } else if (currentFileManagementPopupIcon == fileManagementCancelButton) {
                fileManagementSelector.rectTransform.anchoredPosition = fileManagementOkayButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = fileManagementOkayButton;
            }
        }
    }

    public void PressRightLS() {
        if (ToolPickState == ToolPickMode.Selecting) {
            if (currentToolPickerIcon == packetryButton) {
                currentToolPickerIcon = momentusButton;
            }
            else if (currentToolPickerIcon == momentusButton) {
                currentToolPickerIcon = packetryButton;
            }
            else if (currentToolPickerIcon == ARPAMatButton) {
                currentToolPickerIcon = canvasSystemsButton;
            }
            else if (currentToolPickerIcon == canvasSystemsButton) {
                currentToolPickerIcon = ARPAMatButton;
            }

            toolSelectorBox.rectTransform.anchoredPosition = currentToolPickerIcon.rectTransform.anchoredPosition;
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
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {

            }
            else if (ToolPickState == ToolPickMode.ImageAnalysis) {

            }
            else if (ToolPickState == ToolPickMode.MatSci) {
                if (!showAllMatSciRefCurves && curNMR.matches[curMatSciRefIndex].maxima.Count > 1) {
                    curMatSciRefPeakIndex = (curMatSciRefPeakIndex + 1 + curNMR.matches[curMatSciRefIndex].maxima.Count) % curNMR.matches[curMatSciRefIndex].maxima.Count;
                    PlaceRefPeakSelector(curNMR.matches[curMatSciRefIndex], curMatSciRefPeakIndex);

                    resultsTxt.text = curNMR.GetResults(curMatSciRefIndex) + curNMR.GetPeak(curMatSciRefIndex, curMatSciRefPeakIndex);
                    notesTxt.text = curNMR.GetNotes(curMatSciRefIndex);
                }
            }
            else if (ToolPickState == ToolPickMode.Trajectory) {

            }
        } else if (ActiveToolState == ActiveToolMode.Adding || ActiveToolState == ActiveToolMode.Deleting || ActiveToolState == ActiveToolMode.SendingTechRequest) {
            if (currentFileManagementPopupIcon == fileManagementOkayButton) {
                fileManagementSelector.rectTransform.anchoredPosition = fileManagementCancelButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = fileManagementCancelButton;
            }
            else if (currentFileManagementPopupIcon == fileManagementCancelButton) {
                fileManagementSelector.rectTransform.anchoredPosition = fileManagementOkayButton.rectTransform.anchoredPosition;
                currentFileManagementPopupIcon = fileManagementOkayButton;
            }
        }
    }

    // Right Joystick

    public void PressUpRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {
                if (evidenceCameraRotX < evidenceCameraMaxRotX) {
                    float rotX = value * evidenceCameraRotSpeed * Time.deltaTime;
                    evidenceCameraRotX += rotX;
                    stageFocus.Rotate(rotX, 0, 0, Space.Self);
                }
            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                // Image Analysis cursor can move up if it's still below the top-left corner
                // Technically, the image is moving, but we're tracking the movement like it's the cursor
                if ((!IsDrawingIASearchBox || (IsDrawingIASearchBox && (IAcursorPosition.y < currentSearchTopLeftCorner.y))) && (IAcursorPosition.y < cursorLimits.y)) {
                    float moveY = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(0, moveY);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(0, -moveY);
                }
            }
        }
    }

    public void PressDownRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {
                if (evidenceCameraRotX > evidenceCameraMinRotX) {
                    float rotX = value * evidenceCameraRotSpeed * Time.deltaTime;
                    evidenceCameraRotX += rotX;
                    stageFocus.Rotate(rotX, 0, 0, Space.Self);
                }
            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                if (IAcursorPosition.y > -cursorLimits.y) {
                    float moveY = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(0, moveY);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(0, -moveY);
                }
            }
        }
    }

    public void PressLeftRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {
                stageFocus.Rotate(0, value * evidenceCameraRotSpeed * Time.deltaTime, 0, Space.World);
            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                // Image Analysis cursor can move left if it's still to the right of the top-left corner
                // Technically, the image is moving, but we're tracking the movement like it's the cursor
                if ((!IsDrawingIASearchBox || (IsDrawingIASearchBox && (IAcursorPosition.x > currentSearchTopLeftCorner.x))) && (IAcursorPosition.x > -cursorLimits.x)) {
                    float moveX = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(moveX, 0);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(-moveX, 0);
                }
            }
        }
    }

    public void PressRightRS(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {
                stageFocus.Rotate(0, value * evidenceCameraRotSpeed * Time.deltaTime, 0, Space.World);
            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {

                // If the the cursor is moved, reset the imageOfInterest index to 0 for purposes of cycling through them with the left joystick
                currentFoundImageIndex = 0;

                if (IAcursorPosition.x < cursorLimits.x) {
                    float moveX = value * cursorMoveSpeed * Time.deltaTime;

                    IAcursorPosition += new Vector2(moveX, 0);
                    imageAnalysisImage.rectTransform.anchoredPosition += new Vector2(-moveX, 0);
                }
            }
        }
    }

    // Arrows

    public void PressUpArrow() {

        if (ActiveToolState == ActiveToolMode.Using && projHandler.openProjects[currentProjectIndex].NMRDataActual.Count > 0) {
            //if (curMatSciIndex > 0) {
            //    curMatSciIndex--;                
            //} else {
            //    curMatSciIndex = projHandler.openProjects[currentProjectIndex].NMRDataActual.Count - 1;
            //}

            curMatSciIndex = (curMatSciIndex - 1 + curProject.NMRDataActual.Count) % curProject.NMRDataActual.Count;

            cachedResultsListHighlighter.rectTransform.anchoredPosition = cachedResultsListSelectorInitialPos + new Vector2(0, -curMatSciIndex * cachedResultsListSelectorHeight);
            curMatSciRefIndex = 0;
            foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }
            curNMR = projHandler.openProjects[currentProjectIndex].NMRDataActual[curMatSciIndex];

            SetMaterialsAnalysisSampleSpectrum(curNMR);            
            curNMR.DetermineMatches();

            showAllMatSciRefCurves = true;
            refPeakSelectorLineRend.enabled = false;

            for (int i = 0; i < curNMR.matches.Count; i++) {
                SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
            }

            resultsTxt.text = curNMR.GetResults();
            notesTxt.text = curNMR.GetNotes();
        } else if ((ActiveToolState == ActiveToolMode.Adding) && (curProjectsCanAddTo.Count > 1)) {

            curProjectsCanAddToIndex = (curProjectsCanAddToIndex + 1) % curProjectsCanAddTo.Count;
            fileManagementPopupSelectionTxt.text = curProjectsCanAddTo[curProjectsCanAddToIndex].projectName;

        }
    }

    public void PressDownArrow() {

        if (ActiveToolState == ActiveToolMode.Using && projHandler.openProjects[currentProjectIndex].NMRDataActual.Count > 0) {
            //if (curMatSciIndex < (projHandler.openProjects[currentProjectIndex].NMRDataActual.Count - 1)) {
            //    curMatSciIndex++;
            //}
            //else {
            //    curMatSciIndex = 0;
            //}

            curMatSciIndex = (curMatSciIndex + 1) % curProject.NMRDataActual.Count;

            cachedResultsListHighlighter.rectTransform.anchoredPosition = cachedResultsListSelectorInitialPos + new Vector2(0, -curMatSciIndex * cachedResultsListSelectorHeight);
            curMatSciRefIndex = 0;
            foreach (LineRenderer lineRend in materialsAnalysisRefLineRends) { lineRend.enabled = false; }
            curNMR = projHandler.openProjects[currentProjectIndex].NMRDataActual[curMatSciIndex];

            SetMaterialsAnalysisSampleSpectrum(curNMR);
            curNMR.DetermineMatches();

            showAllMatSciRefCurves = true;
            refPeakSelectorLineRend.enabled = false;

            for (int i = 0; i < curNMR.matches.Count; i++) {
                SetMaterialsAnalysisRefSpectrum(curNMR.matches[i], i);
            }

            resultsTxt.text = curNMR.GetResults();
            notesTxt.text = curNMR.GetNotes();
        } else if ((ActiveToolState == ActiveToolMode.Adding) && (curProjectsCanAddTo.Count > 1)) {

            curProjectsCanAddToIndex = (curProjectsCanAddToIndex - 1 + curProjectsCanAddTo.Count) % curProjectsCanAddTo.Count;
            fileManagementPopupSelectionTxt.text = curProjectsCanAddTo[curProjectsCanAddToIndex].projectName;

        }
    }

    public void PressLeftArrow() {
        if (ActiveToolState == ActiveToolMode.Using && numberOfProjects > 0) {

            currentProjectIndex = (currentProjectIndex - 1 + numberOfProjects) % numberOfProjects;

            projectNumberTxt.text = projHandler.openProjects[currentProjectIndex].projectName;
        }
    }

    public void PressRightArrow() {
        if (ActiveToolState == ActiveToolMode.Using && numberOfProjects > 0) {

            currentProjectIndex = (currentProjectIndex + 1) % numberOfProjects;

            projectNumberTxt.text = projHandler.openProjects[currentProjectIndex].projectName;
        }
    }

    // Triggers

    public void PressLeftTrigger(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {
                if (evidenceCameraFOV < evidenceCameraMaxFOV) {
                    float fovChange = -value * evidenceCameraZoomSpeed * Time.deltaTime;
                    evidenceCameraFOV += fovChange;
                    evidenceCamera.fieldOfView += fovChange;
                }
            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {
                //analysisCamera.fieldOfView = 60;
                if (!isZoomedOut) {
                    imageAnalysisImage.rectTransform.sizeDelta = new Vector2(128, 128);
                    imageAnalysisImage.rectTransform.anchoredPosition = initialImagePosition;
                    IAcursorPosition = initialIACursorPosition;
                    imageAnalysisReticle.enabled = false;
                    isZoomedOut = true;
                }
            }
        }
    }

    public void ReleaseLeftTrigger() {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.ImageAnalysis) {
                //analysisCamera.fieldOfView = 120;
                imageAnalysisImage.rectTransform.sizeDelta = initialImageSize;
                imageAnalysisReticle.enabled = true;
                isZoomedOut = false;
            }
        }
    }

    public void PressRightTrigger(float value) {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.CrimeSceneInspection) {
                if (evidenceCameraFOV > evidenceCameraMinFOV) {
                    float fovChange = -value * evidenceCameraZoomSpeed * Time.deltaTime;
                    evidenceCameraFOV += fovChange;
                    evidenceCamera.fieldOfView += fovChange;
                }
            } else if (ToolPickState == ToolPickMode.ImageAnalysis) {
                if (!IsDrawingIASearchBox) {
                    
                    currentSearchTopLeftCorner = IAcursorPosition;
                    IsDrawingIASearchBox = true;
                }
            }
        }
    }

    public void ReleaseRightTrigger() {
        if (ActiveToolState == ActiveToolMode.Using) {
            if (ToolPickState == ToolPickMode.ImageAnalysis) {
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
            projectNumberTxt.text = projHandler.openProjects[currentProjectIndex].projectName;

            int numberOfFilesBeforeSelected = numOfVisibleEvidenceFilesInList - currentLineNumber;
            //int numberOfFilesAfterSelected = numOfVisibleEvidenceFilesInList - numberOfFilesBeforeSelected - 1;
            
            
        }
    }

    // METHODS - TOOLS FUNCTIONS //

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

    // Positions and draws the LineRanderer selector box on a peak of the reference curve
    private void PlaceRefPeakSelector(NMRRefResults refResult, int peakIndex) {

        refPeakSelectorLineRend.enabled = true;

        Vector3 centerPos = new Vector3(Mathf.Floor(refResult.maxima[peakIndex].shift) + lineRendOffsetX, Mathf.Floor(refResult.maxima[peakIndex].intensity) + lineRendOffsetY, -1);

        Debug.Log(centerPos);

        refPeakSelectorLineRend.SetPosition(0, centerPos + new Vector3(-2, 2, -1));
        refPeakSelectorLineRend.SetPosition(1, centerPos + new Vector3(2, 2, -1));
        refPeakSelectorLineRend.SetPosition(2, centerPos + new Vector3(2, -2, -1));
        refPeakSelectorLineRend.SetPosition(3, centerPos + new Vector3(-2, -2, -1));
        refPeakSelectorLineRend.SetPosition(4, centerPos + new Vector3(-2, 2, -1));
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
