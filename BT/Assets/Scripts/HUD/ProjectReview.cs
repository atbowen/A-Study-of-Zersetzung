using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectReview : MonoBehaviour
{
    // Case management
    public List<ProjectFile> projectList;
    public ProjectFile unassignedData;

    public List<NMRRefResults> NMRRefs;
    public List<TrajectoryRefSet> trajRefSets;
    public List<ImageRefCapture> refImagesOfInterest;

    private List<CrimeSceneObject> evidencePool = new List<CrimeSceneObject>();

    public List<Texture> projectListFrameFrames, projectListOverlayFrames, projectImageFrames;
    [SerializeField]
    private RawImage projListFrame, projectListBgd, projListOverlay, projListSelectorBar, currentProjectImage, currentProjectImageBgd,
                        frontImageBP, leftImageBP, rightImageBP, backImageBP;
    [SerializeField]
    private Text projectListTxt, currentProjectTitleTxt, currentProjectNotesTxt, currentProjectBulletPointsTxt;

    [SerializeField]
    private float loadingFrameTime, closingFrameTime;
    private float transitionFrameRefTime;
    private int transitionFrameIndex;   

    private ProjectFile curProjectFile;
    private CrimeScene currentWorkingCrimeScene;
    [SerializeField]
    private int numOfVisibleProjectTitles;
    [SerializeField]
    private Color projectListTextColor, projectListTextFadedColor;

    [SerializeField]
    private float projListTxtShiftTime;
    private float projListTxtShiftRefTime;
    [SerializeField]
    private int projListTxtLineHeight;
    private int projListTxtCurPixelOffset;
    private Vector2 projListTxtInitialPos;
    private bool projListTxtIsShiftingUp, projListTxtIsShiftingDown;

    private List<ProjectBulletPoint> curProjectBulletPoints = new List<ProjectBulletPoint>();
    private int bulletPointFrontImageWidth, bulletPointFrontImageHeight,
                    bulletPointSideImageWidth, bulletPointSideImageHeight,
                    bulletPointBackImageWidth, bulletPointBackImageHeight;
    private int curBulletPointFrontImageWidth, curBulletPointFrontImageHeight,
                curBulletPointLeftImageWidth, curBulletPointLeftImageHeight,
                curBulletPointRightImageWidth, curBulletPointRightImageHeight,
                curBulletPointBackImageWidth, curBulletPointBackImageHeight;
    [SerializeField]
    private float bulletPointImageResizeSmoothingFactor, bulletPointImagePositionOffsetTolerance;
    private Vector2 bulletPointFrontImageInitialPos, bulletPointLeftImageInitialPos, bulletPointRightImageInitialPos, bulletPointBackImageInitialPos,
                    bulletPointFrontImageCurPos, bulletPointLeftImageCurPos, bulletPointRightImageCurPos, bulletPointBackImageCurPos;
    private bool bulletPointImagesAreCyclingLeft, bulletPointImagesAreCyclingRight;
    [SerializeField]
    private Color curProjectDescriptionColor, bulletPointTextColor, bulletPointTextFadedColor;
    private Color curBulletPointTextColor;

    [SerializeField]
    private float projectListSelectorAlphaChange;
    [SerializeField, Range(0.0f, 1.0f)]
    private float projectListSelectorAlphaMin, projectListSelectorAlphaMax;
    private bool projectListSelectorAlphaIsIncreasing;

    [SerializeField]
    private bool bulletPointsShouldFlash;
    [SerializeField]
    private float bulletPointTextAlphaChange;
    [SerializeField, Range(0.0f, 1.0f)]
    private float bulletPointTextAlphaMin, bulletPointTextAlphaMax;
    private bool bulletPointTextAlphaIsIncreasing;

    private int curProjectFileIndex, curVisibleProjectFileIndex, curBulletPointIndex, curCrimeSceneIndex;
    private enum ProjectReviewMode { Inactive, Loading, Active , Closing }
    private ProjectReviewMode ProjectReviewState;

    // Start is called before the first frame update
    void Start()
    {
        projectListTxt.supportRichText = true;

        projListFrame.enabled = false;
        projListOverlay.enabled = false;
        currentProjectImageBgd.enabled = false;

        projListTxtInitialPos = projectListTxt.rectTransform.anchoredPosition;
        projListTxtIsShiftingUp = false;
        projListTxtIsShiftingDown = false;

        bulletPointFrontImageInitialPos = frontImageBP.rectTransform.anchoredPosition;
        bulletPointLeftImageInitialPos = leftImageBP.rectTransform.anchoredPosition;
        bulletPointRightImageInitialPos = rightImageBP.rectTransform.anchoredPosition;
        bulletPointBackImageInitialPos = backImageBP.rectTransform.anchoredPosition;

        bulletPointFrontImageWidth = (int)frontImageBP.rectTransform.rect.width;
        bulletPointFrontImageHeight = (int)frontImageBP.rectTransform.rect.height;
        bulletPointSideImageWidth = (int)leftImageBP.rectTransform.rect.width;
        bulletPointSideImageHeight = (int)leftImageBP.rectTransform.rect.height;
        bulletPointBackImageWidth = (int)backImageBP.rectTransform.rect.width;
        bulletPointBackImageHeight = (int)backImageBP.rectTransform.rect.height;

        bulletPointImagesAreCyclingLeft = false;
        bulletPointImagesAreCyclingRight = false;

        curBulletPointTextColor = bulletPointTextColor;

        ShowProjectReviewScreens(false);
        ProjectReviewState = ProjectReviewMode.Inactive;

        curProjectFileIndex = 0;
        curVisibleProjectFileIndex = 0;

        curBulletPointIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (projectList.Count > 0) {
            curProjectFile = projectList[curProjectFileIndex];
        }
        else {
            curProjectFile = null;
        }

        switch (ProjectReviewState) {
            case ProjectReviewMode.Inactive:

                

                break;
            case ProjectReviewMode.Loading:

                if (Time.time - transitionFrameRefTime > loadingFrameTime) {
                    if (transitionFrameIndex < projectListFrameFrames.Count) {
                        
                        projListFrame.texture = projectListFrameFrames[transitionFrameIndex];
                        projListOverlay.texture = projectListOverlayFrames[transitionFrameIndex];
                        currentProjectImageBgd.texture = projectImageFrames[transitionFrameIndex];

                        transitionFrameIndex++;
                        transitionFrameRefTime = Time.time;
                    }
                    else {

                        projectListBgd.enabled = true;
                        projListSelectorBar.enabled = true;
                        projectListTxt.enabled = true;

                        ProjectReviewState = ProjectReviewMode.Active;
                    }
                }

                break;
            case ProjectReviewMode.Active:

                FlashProjectListSelector();
                UpdateProjectListText();

                if (curProjectFile != null) {
                    currentProjectImage.enabled = true;
                    currentProjectImage.texture = curProjectFile.GetProjectMainImage();

                    currentProjectTitleTxt.enabled = true;
                    currentProjectTitleTxt.text = curProjectFile.GetProjectTitle();
                    currentProjectNotesTxt.enabled = true;
                    //currentProjectNotesTxt.text = curProjectFile.GetProjectDescription();

                    if (curProjectBulletPoints.Count > 0) {
                        UpdateBulletPointDisplay();
                        currentProjectBulletPointsTxt.enabled = true;
                    }
                    else {
                        frontImageBP.enabled = false;
                        leftImageBP.enabled = false;
                        rightImageBP.enabled = false;
                        backImageBP.enabled = false;
                        currentProjectBulletPointsTxt.enabled = false;
                    }

                }

                break;
            case ProjectReviewMode.Closing:

                if (Time.time - transitionFrameRefTime > closingFrameTime) {
                    if (transitionFrameIndex > 0) {
                        
                        projListFrame.texture = projectListFrameFrames[transitionFrameIndex];
                        projListOverlay.texture = projectListOverlayFrames[transitionFrameIndex];
                        currentProjectImageBgd.texture = projectImageFrames[transitionFrameIndex];

                        transitionFrameIndex--;
                        transitionFrameRefTime = Time.time;
                    }
                    else {

                        projListFrame.enabled = false;
                        projListOverlay.enabled = false;
                        currentProjectImageBgd.enabled = false;

                        ProjectReviewState = ProjectReviewMode.Inactive;
                    }
                }

                break;
        }
    }

    public void ShowProjectReviewScreens(bool showOrNo) {

        if (showOrNo) {

            projListFrame.enabled = true;
            projListOverlay.enabled = true;
            currentProjectImageBgd.enabled = true;

            curProjectFileIndex = 0;
            curBulletPointIndex = 0;

            curProjectBulletPoints.Clear();

            curProjectFile = projectList[curProjectFileIndex];

            if (curProjectFile != null) {
                if (curProjectFile.bulletPoints.Count > 0) {
                    foreach (ProjectBulletPoint bulletPt in curProjectFile.bulletPoints) {
                        if (bulletPt.isKnown) { curProjectBulletPoints.Add(bulletPt); }
                    }
                }
            }

            transitionFrameIndex = 0;

            projListFrame.texture = projectListFrameFrames[transitionFrameIndex];
            projListOverlay.texture = projectListOverlayFrames[transitionFrameIndex];
            currentProjectImageBgd.texture = projectImageFrames[transitionFrameIndex];

            transitionFrameRefTime = Time.time;

        } else {
            projectListBgd.enabled = false;
            projListSelectorBar.enabled = false;
            currentProjectImage.enabled = false;
            frontImageBP.enabled = false;
            leftImageBP.enabled = false;
            rightImageBP.enabled = false;
            backImageBP.enabled = false;

            projectListTxt.enabled = false;
            currentProjectTitleTxt.enabled = false;
            currentProjectNotesTxt.enabled = false;
            currentProjectBulletPointsTxt.enabled = false;

            transitionFrameIndex = projectListFrameFrames.Count - 1;
            transitionFrameRefTime = Time.time;
        }
    }

    public void MakeActive(bool yesOrNo) {
        if (yesOrNo) {
            ShowProjectReviewScreens(true);
            ProjectReviewState = ProjectReviewMode.Loading;
        } else {
            ShowProjectReviewScreens(false);
            ProjectReviewState = ProjectReviewMode.Closing;
        }
    }

    public void ForceClose() {
        ShowProjectReviewScreens(false);
        projListFrame.enabled = false;
        projListOverlay.enabled = false;
        currentProjectImageBgd.enabled = false;
        ProjectReviewState = ProjectReviewMode.Inactive;
    }

    public bool IsActive() {
        return ProjectReviewState == ProjectReviewMode.Active;
    }

    public bool IsInactive() {
        return ProjectReviewState == ProjectReviewMode.Inactive;
    }

    private void UpdateProjectListText() {

        if (projListTxtIsShiftingUp) {
            if (Time.time - projListTxtShiftRefTime > projListTxtShiftTime) {
                if (projListTxtCurPixelOffset > -projListTxtLineHeight) {
                    projListTxtCurPixelOffset--;
                    projectListTxt.rectTransform.anchoredPosition = projListTxtInitialPos + new Vector2(0, projListTxtCurPixelOffset);
                    projListTxtShiftRefTime = Time.time;
                }
                else {
                    projectListTxt.rectTransform.anchoredPosition = projListTxtInitialPos;

                    curProjectFileIndex = (curProjectFileIndex - 1 + projectList.Count) % projectList.Count;
                    curProjectFile = projectList[curProjectFileIndex];

                    curBulletPointIndex = 0;
                    curProjectBulletPoints.Clear();

                    if (curProjectFile.bulletPoints.Count > 0) {
                        foreach (ProjectBulletPoint bulletPt in curProjectFile.bulletPoints) {
                            if (bulletPt.isKnown) { curProjectBulletPoints.Add(bulletPt); }
                        }
                    }

                    projListTxtIsShiftingUp = false;
                }
            }
        }
        else if (projListTxtIsShiftingDown) {
            if (Time.time - projListTxtShiftRefTime > projListTxtShiftTime) {
                if (projListTxtCurPixelOffset < projListTxtLineHeight) {
                    projListTxtCurPixelOffset++;
                    projectListTxt.rectTransform.anchoredPosition = projListTxtInitialPos + new Vector2(0, projListTxtCurPixelOffset);
                    projListTxtShiftRefTime = Time.time;
                }
                else {
                    projectListTxt.rectTransform.anchoredPosition = projListTxtInitialPos;

                    curProjectFileIndex = (curProjectFileIndex + 1) % projectList.Count;
                    curProjectFile = projectList[curProjectFileIndex];

                    curBulletPointIndex = 0;
                    curProjectBulletPoints.Clear();

                    if (curProjectFile.bulletPoints.Count > 0) {
                        foreach (ProjectBulletPoint bulletPt in curProjectFile.bulletPoints) {
                            if (bulletPt.isKnown) { curProjectBulletPoints.Add(bulletPt); }
                        }
                    }

                    projListTxtIsShiftingDown = false;
                }
            }
        }

        string colorHex = ColorUtility.ToHtmlStringRGBA(new Color(projectListTextColor.r, projectListTextColor.g, projectListTextColor.b, projectListTextColor.a));
        string fadedColorHex = ColorUtility.ToHtmlStringRGBA(new Color(projectListTextFadedColor.r, projectListTextFadedColor.g, projectListTextFadedColor.b, projectListTextFadedColor.a));

        string line1 = "<Empty>", line2 = "<Emtpy>", line3 = "<Empty>", line4 = "<Empty>", line5 = "<Empty>";

        if (projectList.Count == 1) {
            line2 = projectList[0].GetProjectTitle();
        }
        else if (projectList.Count > 1) {
            if (projListTxtIsShiftingUp) {
                line1 = projectList[(curProjectFileIndex - 2 + projectList.Count) % projectList.Count].GetProjectTitle();
                line2 = projectList[(curProjectFileIndex - 1 + projectList.Count) % projectList.Count].GetProjectTitle();
                line3 = projectList[curProjectFileIndex].GetProjectTitle();
                line4 = projectList[(curProjectFileIndex + 1) % projectList.Count].GetProjectTitle();
                line5 = "";
            } else if (projListTxtIsShiftingDown) {
                line1 = "";
                line2 = projectList[(curProjectFileIndex - 1 + projectList.Count) % projectList.Count].GetProjectTitle();
                line3 = projectList[curProjectFileIndex].GetProjectTitle();
                line4 = projectList[(curProjectFileIndex + 1) % projectList.Count].GetProjectTitle();
                line5 = projectList[(curProjectFileIndex + 2) % projectList.Count].GetProjectTitle();
            }
            else {
                line1 = projectList[(curProjectFileIndex - 2 + projectList.Count) % projectList.Count].GetProjectTitle();
                line2 = projectList[(curProjectFileIndex - 1 + projectList.Count) % projectList.Count].GetProjectTitle();
                line3 = projectList[curProjectFileIndex].GetProjectTitle();
                line4 = projectList[(curProjectFileIndex + 1) % projectList.Count].GetProjectTitle();
                line5 = projectList[(curProjectFileIndex + 2) % projectList.Count].GetProjectTitle();
            }
        }

        projectListTxt.text = "<color=#" + fadedColorHex + ">" + line1 + "\n" + line2 + "</color>\n<color=#" + colorHex + ">" + line3 + "</color>\n<color=#" + fadedColorHex + ">" 
                            + line4 + "\n" + line5 + "</color>";

        
    }

    private void UpdateBulletPointDisplay() {

        if (bulletPointImagesAreCyclingLeft) {
            if ((frontImageBP.rectTransform.anchoredPosition - bulletPointLeftImageInitialPos).magnitude > bulletPointImagePositionOffsetTolerance) {
                ShiftBulletPointImage(frontImageBP, bulletPointLeftImageInitialPos);
                ShiftBulletPointImage(leftImageBP, bulletPointBackImageInitialPos);
                ShiftBulletPointImage(backImageBP, bulletPointRightImageInitialPos);
                ShiftBulletPointImage(rightImageBP, bulletPointFrontImageInitialPos);

                AdjustBulletPointImageSize(frontImageBP, bulletPointSideImageWidth, bulletPointSideImageHeight);
                AdjustBulletPointImageSize(leftImageBP, bulletPointBackImageWidth, bulletPointBackImageHeight);
                AdjustBulletPointImageSize(backImageBP, bulletPointSideImageWidth, bulletPointSideImageHeight);
                AdjustBulletPointImageSize(rightImageBP, bulletPointFrontImageWidth, bulletPointFrontImageHeight);
            }
            else {
                frontImageBP.rectTransform.anchoredPosition = bulletPointFrontImageInitialPos;
                leftImageBP.rectTransform.anchoredPosition = bulletPointLeftImageInitialPos;
                backImageBP.rectTransform.anchoredPosition = bulletPointBackImageInitialPos;
                rightImageBP.rectTransform.anchoredPosition = bulletPointRightImageInitialPos;

                frontImageBP.rectTransform.sizeDelta = new Vector2(bulletPointFrontImageWidth, bulletPointFrontImageHeight);
                leftImageBP.rectTransform.sizeDelta = new Vector2(bulletPointSideImageWidth, bulletPointSideImageHeight);
                backImageBP.rectTransform.sizeDelta = new Vector2(bulletPointBackImageWidth, bulletPointBackImageHeight);
                rightImageBP.rectTransform.sizeDelta = new Vector2(bulletPointSideImageWidth, bulletPointSideImageHeight);

                bulletPointImagesAreCyclingLeft = false;
                curBulletPointIndex = (curBulletPointIndex + 1) % curProjectBulletPoints.Count;
            }
        }
        else if (bulletPointImagesAreCyclingRight) {
            if ((frontImageBP.rectTransform.anchoredPosition - bulletPointRightImageInitialPos).magnitude > bulletPointImagePositionOffsetTolerance) {
                ShiftBulletPointImage(frontImageBP, bulletPointRightImageInitialPos);
                ShiftBulletPointImage(rightImageBP, bulletPointBackImageInitialPos);
                ShiftBulletPointImage(backImageBP, bulletPointLeftImageInitialPos);
                ShiftBulletPointImage(leftImageBP, bulletPointFrontImageInitialPos);

                AdjustBulletPointImageSize(frontImageBP, bulletPointSideImageWidth, bulletPointSideImageHeight);
                AdjustBulletPointImageSize(rightImageBP, bulletPointBackImageWidth, bulletPointBackImageHeight);
                AdjustBulletPointImageSize(backImageBP, bulletPointSideImageWidth, bulletPointSideImageHeight);
                AdjustBulletPointImageSize(leftImageBP, bulletPointFrontImageWidth, bulletPointFrontImageHeight);
            }
            else {
                frontImageBP.rectTransform.anchoredPosition = bulletPointFrontImageInitialPos;
                leftImageBP.rectTransform.anchoredPosition = bulletPointLeftImageInitialPos;
                backImageBP.rectTransform.anchoredPosition = bulletPointBackImageInitialPos;
                rightImageBP.rectTransform.anchoredPosition = bulletPointRightImageInitialPos;

                frontImageBP.rectTransform.sizeDelta = new Vector2(bulletPointFrontImageWidth, bulletPointFrontImageHeight);
                leftImageBP.rectTransform.sizeDelta = new Vector2(bulletPointSideImageWidth, bulletPointSideImageHeight);
                backImageBP.rectTransform.sizeDelta = new Vector2(bulletPointBackImageWidth, bulletPointBackImageHeight);
                rightImageBP.rectTransform.sizeDelta = new Vector2(bulletPointSideImageWidth, bulletPointSideImageHeight);

                bulletPointImagesAreCyclingRight = false;
                curBulletPointIndex = (curBulletPointIndex - 1 + curProjectBulletPoints.Count) % curProjectBulletPoints.Count;
            }
        }

        string descripColorHex = ColorUtility.ToHtmlStringRGBA(new Color(curProjectDescriptionColor.r, curProjectDescriptionColor.g, curProjectDescriptionColor.b, curProjectDescriptionColor.a));
        string bulletColorHex = ColorUtility.ToHtmlStringRGBA(new Color(bulletPointTextColor.r, bulletPointTextColor.g, bulletPointTextColor.b, bulletPointTextColor.a));
        string bulletFadedHex = ColorUtility.ToHtmlStringRGBA(new Color(bulletPointTextFadedColor.r, bulletPointTextFadedColor.g, bulletPointTextFadedColor.b, bulletPointTextFadedColor.a));

        curBulletPointTextColor = new Color(curBulletPointTextColor.r, curBulletPointTextColor.g, curBulletPointTextColor.b, GetFlashingBulletPointAlpha(curBulletPointTextColor.a));
        string curBulletPointColorHex = ColorUtility.ToHtmlStringRGBA(curBulletPointTextColor);

        if (!bulletPointsShouldFlash) {
            curBulletPointColorHex = bulletColorHex;
        }

        string allBulletPointsText = "";

        if (curProjectBulletPoints.Count == 1) {

            frontImageBP.enabled = true;
            leftImageBP.enabled = false;
            rightImageBP.enabled = false;
            backImageBP.enabled = false;
            leftImageBP.texture = curProjectBulletPoints[0].GetImage();

            currentProjectNotesTxt.text = "<color=#" + descripColorHex + ">" + curProjectFile.GetProjectDescription() + "</color>\n\n<color=#" + 
                                            curBulletPointColorHex + ">-" + curProjectBulletPoints[0].GetDescription() + "</color>";
        }
        else if (curProjectBulletPoints.Count > 1) {

            frontImageBP.enabled = true;
            leftImageBP.enabled = true;
            rightImageBP.enabled = true;
            backImageBP.enabled = true;

            frontImageBP.texture = curProjectBulletPoints[curBulletPointIndex].GetImage();
            leftImageBP.texture = curProjectBulletPoints[(curBulletPointIndex - 1 + curProjectBulletPoints.Count) % curProjectBulletPoints.Count].GetImage();
            rightImageBP.texture = curProjectBulletPoints[(curBulletPointIndex + 1) % curProjectBulletPoints.Count].GetImage();

            for (int i = 0; i < curProjectBulletPoints.Count; i++) {
                if (i == curBulletPointIndex)   { allBulletPointsText += "<color=#" + curBulletPointColorHex + ">-" + curProjectBulletPoints[i].GetDescription() + "</color>\n"; }
                else                            { allBulletPointsText += "<color=#" + bulletFadedHex + ">-" + curProjectBulletPoints[i].GetDescription() + "</color>\n"; }
            }

            currentProjectNotesTxt.text = "<color=#" + descripColorHex + ">" + curProjectFile.GetProjectDescription() + "</color>\n\n" + allBulletPointsText;
        }

        currentProjectBulletPointsTxt.text = curProjectBulletPoints[curBulletPointIndex].GetTitle();
    }

    private void ShiftBulletPointImage(RawImage img, Vector2 newPos) {
        float newX = Mathf.Lerp(img.rectTransform.anchoredPosition.x, newPos.x, bulletPointImageResizeSmoothingFactor * Time.deltaTime);
        float newY = Mathf.Lerp(img.rectTransform.anchoredPosition.y, newPos.y, bulletPointImageResizeSmoothingFactor * Time.deltaTime);

        img.rectTransform.anchoredPosition = new Vector2(newX, newY);
    }

    private void AdjustBulletPointImageSize(RawImage img, int targetWidth, int targetHeight) {
        float newWidth = Mathf.Lerp(img.rectTransform.rect.width, targetWidth, bulletPointImageResizeSmoothingFactor * Time.deltaTime);
        float newHeight = Mathf.Lerp(img.rectTransform.rect.height, targetHeight, bulletPointImageResizeSmoothingFactor * Time.deltaTime);

        img.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
    }

    private void FlashProjectListSelector() {
        if (projectListSelectorAlphaIsIncreasing) {
            if (projListSelectorBar.color.a < projectListSelectorAlphaMax) {
                projListSelectorBar.color = new Color(projListSelectorBar.color.r, projListSelectorBar.color.g, projListSelectorBar.color.b, 
                                                        projListSelectorBar.color.a + projectListSelectorAlphaChange * Time.deltaTime);
            }
            else {
                projectListSelectorAlphaIsIncreasing = false;
            }
        }
        else {
            if (projListSelectorBar.color.a > projectListSelectorAlphaMin) {
                projListSelectorBar.color = new Color(projListSelectorBar.color.r, projListSelectorBar.color.g, projListSelectorBar.color.b,
                                                        projListSelectorBar.color.a - projectListSelectorAlphaChange * Time.deltaTime);
            }
            else {
                projectListSelectorAlphaIsIncreasing = true;
            }
        }
    }

    private float GetFlashingBulletPointAlpha(float alpha) {
        if (bulletPointTextAlphaIsIncreasing) {
            if (alpha < bulletPointTextAlphaMax) {
                return alpha + bulletPointTextAlphaChange * Time.deltaTime;
            }
            else {
                bulletPointTextAlphaIsIncreasing = false;
                return alpha;
            }
        }
        else {
            if (alpha > bulletPointTextAlphaMin) {
                return alpha - bulletPointTextAlphaChange * Time.deltaTime;
            }
            else {
                bulletPointTextAlphaIsIncreasing = true;
                return alpha;
            }
        }
    }

    ///
    /// CONTROLS
    /// 

    public void PressUpLS() {
        if (ProjectReviewState == ProjectReviewMode.Active) {
            if (projectList.Count > 1) {

                projListTxtIsShiftingUp = true;
                projListTxtCurPixelOffset = 0;
                projListTxtShiftRefTime = Time.time;

                //curProjectFileIndex = (curProjectFileIndex - 1 + projectList.Count) % projectList.Count;
                //curProjectFile = projectList[curProjectFileIndex];

                //curBulletPointIndex = 0;
                //curProjectBulletPoints.Clear();

                //if (curProjectFile.bulletPoints.Count > 0) {
                //    foreach (ProjectBulletPoint bulletPt in curProjectFile.bulletPoints) {
                //        if (bulletPt.isKnown) { curProjectBulletPoints.Add(bulletPt); }
                //    }
                //}
            }
        }
    }

    public void PressDownLS() {
        if (ProjectReviewState == ProjectReviewMode.Active) {
            if (projectList.Count > 1) {

                projListTxtIsShiftingDown = true;
                projListTxtCurPixelOffset = 0;
                projListTxtShiftRefTime = Time.time;

                //curProjectFileIndex = (curProjectFileIndex + 1) % projectList.Count;
                //curProjectFile = projectList[curProjectFileIndex];

                //curBulletPointIndex = 0;
                //curProjectBulletPoints.Clear();

                //if (curProjectFile.bulletPoints.Count > 0) {
                //    foreach (ProjectBulletPoint bulletPt in curProjectFile.bulletPoints) {
                //        if (bulletPt.isKnown) { curProjectBulletPoints.Add(bulletPt); }
                //    }
                //}
            }
        }
    }

    public void PressUpRS() {
        if (ProjectReviewState == ProjectReviewMode.Active) {

            if (curProjectBulletPoints.Count > 1) {
                bulletPointImagesAreCyclingRight = true;
                //curBulletPointIndex = (curBulletPointIndex - 1 + curProjectBulletPoints.Count) % curProjectBulletPoints.Count;
                backImageBP.texture = curProjectBulletPoints[(curBulletPointIndex - 2 + curProjectBulletPoints.Count) % curProjectBulletPoints.Count].GetImage();
                Debug.Log((frontImageBP.rectTransform.anchoredPosition - bulletPointLeftImageInitialPos).magnitude > bulletPointImagePositionOffsetTolerance);
            }
        }
    }

    public void PressDownRS() {
        if (ProjectReviewState == ProjectReviewMode.Active) {

            if (curProjectBulletPoints.Count > 1) {
                bulletPointImagesAreCyclingLeft = true;
                //curBulletPointIndex = (curBulletPointIndex + 1) % curProjectBulletPoints.Count;
                backImageBP.texture = curProjectBulletPoints[(curBulletPointIndex + 2) % curProjectBulletPoints.Count].GetImage();
                Debug.Log((frontImageBP.rectTransform.anchoredPosition - bulletPointLeftImageInitialPos).magnitude > bulletPointImagePositionOffsetTolerance);
            }
        }
    }
}
