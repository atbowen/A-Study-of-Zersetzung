using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelsBottom : MonoBehaviour
{
    public RawImage panelTop, panelSpacer, panelBottom;
    public RawImage responsePanelTop, responsePanelSpacer, responsePanelBottom;
    public RawImage responseSelectionBar, openingPanelLoadingBar;
    public List<Texture> openingPanelLoadingFrames;

    public RawImage FakeTedFunctionIcon, callHQFunctionIcon, callStrikeFunction, playMusicFunctionIcon;
    public float specialFunctionsAvailableAlpha, specialFunctionsUnavailAlpha, specialFunctionsAlphaChange;

    public Text promptText, responseText;

    public float textHeightMultiplier;

    public List<RawImage> imagesToAlignWithTopEdge;
    public Vector2 imageAlignmentOffset;

    public float openingPanelChangeLoadingFrameTime, specialFunctionsFlashTime;
    public float selectionFlashFrequency, finishSelectionFlashFrequency, finishSelectionFlashTime, flashIncrement;
    public float flashAlphaMin, flashAlphaMax;

    public bool usingTextParser;

    private enum ActivationState { Closed, Opening, SpecialSelectionMenu, CommsWindow }
    private ActivationState ActivationMode;

    private enum SelectionState { Opening, NotSelecting, Selecting, Selected};
    private SelectionState SelectionMode;

    private float panelSpacerHeight, panelSpacerWidth;

    private Vector2 panelBottomInitialPos, responseTextInitialPos, responseSelectionBarInitialPos, openingPanelLoadingBarInitialPos;
    private float responseSelectionBarDefaultHeight, openingPanelLoadingBarInitialHeight;
    private float selectionFlashRefTime, finishSelectionTotalFlashTimeRef;
    private bool brighteningOrDimming;

    private int openingPanelCurrentFrame;
    private float openingPanelRefTime;

    private List<RawImage> availableFunctionsIcons = new List<RawImage>();
    private float usableTotalWidthForFunctionsIcons;

    private int specialFunctionsCurrentIndex;
    private float specialFunctionsFlashRefTime;
    private bool iconAlphaDecreasing;

    // Start is called before the first frame update
    void Start()
    {
        panelBottomInitialPos = panelBottom.rectTransform.anchoredPosition;
        responseTextInitialPos = responseText.rectTransform.anchoredPosition;
        responseSelectionBarInitialPos = responseSelectionBar.rectTransform.anchoredPosition;
        openingPanelLoadingBarInitialPos = openingPanelLoadingBar.rectTransform.anchoredPosition;

        panelSpacerHeight = panelSpacer.texture.height;
        panelSpacerWidth = panelSpacer.rectTransform.rect.width;
        responseSelectionBarDefaultHeight = responseSelectionBar.texture.height;
        openingPanelLoadingBarInitialHeight = openingPanelLoadingBar.rectTransform.rect.height;

        responseSelectionBar.enabled = false;
        openingPanelLoadingBar.enabled = false;
        ShowSpecialFunctionsIcons(false);

        ActivationMode = ActivationState.Closed;

        SelectionMode = SelectionState.NotSelecting;

        openingPanelLoadingBar.enabled = false;

        openingPanelCurrentFrame = 0;
        openingPanelRefTime = 0;

        specialFunctionsCurrentIndex = 0;
        specialFunctionsFlashRefTime = 0;

        iconAlphaDecreasing = true;
    }

    // Update is called once per frame
    void Update()
    {
        switch (ActivationMode) {

            case ActivationState.Closed:
                openingPanelLoadingBar.enabled = false;
                ShowSpecialFunctionsIcons(false);
                break;

            case ActivationState.Opening:

                openingPanelLoadingBar.enabled = true;
                openingPanelLoadingBar.texture = openingPanelLoadingFrames[openingPanelCurrentFrame];

                if (openingPanelCurrentFrame == (openingPanelLoadingFrames.Count - 1)) {
                    ShowSpecialSelectionPanel();
                    openingPanelLoadingBar.rectTransform.sizeDelta = new Vector2(openingPanelLoadingBar.rectTransform.rect.width, 25);
                }
                if ((Time.time - openingPanelRefTime > openingPanelChangeLoadingFrameTime) && openingPanelCurrentFrame < (openingPanelLoadingFrames.Count - 1)) {
                    openingPanelCurrentFrame = (openingPanelCurrentFrame + 1) % openingPanelLoadingFrames.Count;
                    openingPanelRefTime = Time.time;
                }

                break;

            case ActivationState.SpecialSelectionMenu:
                RawImage curImg = availableFunctionsIcons[specialFunctionsCurrentIndex];

                if (Time.time - specialFunctionsFlashRefTime > specialFunctionsFlashTime) {
                    if (iconAlphaDecreasing) {
                        if (curImg.color.a > specialFunctionsUnavailAlpha) {
                            curImg.color = new Color(curImg.color.r, curImg.color.g, curImg.color.b, curImg.color.a - specialFunctionsAlphaChange * Time.deltaTime);
                        }
                        else {
                            curImg.color = new Color(curImg.color.r, curImg.color.g, curImg.color.b, specialFunctionsUnavailAlpha);
                            iconAlphaDecreasing = false;
                        }
                    }
                    else {
                        if (curImg.color.a < specialFunctionsAvailableAlpha) {
                            curImg.color = new Color(curImg.color.r, curImg.color.g, curImg.color.b, curImg.color.a + specialFunctionsAlphaChange * Time.deltaTime);
                        }
                        else {
                            curImg.color = new Color(curImg.color.r, curImg.color.g, curImg.color.b, specialFunctionsAvailableAlpha);
                            iconAlphaDecreasing = true;
                        }
                    }

                    specialFunctionsFlashRefTime = Time.time;
                }

                break;

            case ActivationState.CommsWindow:
                openingPanelLoadingBar.enabled = false;
                ShowSpecialFunctionsIcons(false);

                switch (SelectionMode) {

                    case SelectionState.NotSelecting:
                
                        break;
                    case SelectionState.Selecting:
                        if (Time.time - selectionFlashRefTime > selectionFlashFrequency) {
                            if (brighteningOrDimming) {
                                float nextAlpha = responseSelectionBar.color.a * (flashIncrement * Time.deltaTime);
                                Color currentShade = responseSelectionBar.color;
                                Color nextShade = new Color(currentShade.r, currentShade.g, currentShade.b, nextAlpha);

                                if (currentShade.a < flashAlphaMax) {
                                    responseSelectionBar.color = nextShade;
                                }
                                else {
                                    responseSelectionBar.color = new Color(currentShade.r, currentShade.g, currentShade.b, flashAlphaMax);
                                    brighteningOrDimming = false;
                                }
                            }
                            else {
                                float nextAlpha = responseSelectionBar.color.a / (flashIncrement * Time.deltaTime);
                                Color currentShade = responseSelectionBar.color;
                                Color nextShade = new Color(currentShade.r, currentShade.g, currentShade.b, nextAlpha);

                                if (currentShade.a > flashAlphaMin) {
                                    responseSelectionBar.color = nextShade;
                                }
                                else {
                                    responseSelectionBar.color = new Color(currentShade.r, currentShade.g, currentShade.b, flashAlphaMin);
                                    brighteningOrDimming = true;
                                }
                            }
                            selectionFlashRefTime = Time.time;
                        }
                        break;
                    case SelectionState.Selected:
                        if (Time.time - finishSelectionTotalFlashTimeRef < finishSelectionFlashTime) {
                            if (Time.time - selectionFlashRefTime > finishSelectionFlashFrequency) {
                                if (brighteningOrDimming) {
                                    float nextAlpha = responseSelectionBar.color.a * (flashIncrement * Time.deltaTime);
                                    Color currentShade = responseSelectionBar.color;
                                    Color nextShade = new Color(currentShade.r, currentShade.g, currentShade.b, nextAlpha);

                                    if (currentShade.a < 0.9f) { responseSelectionBar.color = nextShade; }
                                    else { brighteningOrDimming = false; }
                                }
                                else {
                                    float nextAlpha = responseSelectionBar.color.a / (flashIncrement * Time.deltaTime);
                                    Color currentShade = responseSelectionBar.color;
                                    Color nextShade = new Color(currentShade.r, currentShade.g, currentShade.b, nextAlpha);

                                    if (currentShade.a > 0.1f) { responseSelectionBar.color = nextShade; }
                                    else { brighteningOrDimming = true; }
                                }
                                selectionFlashRefTime = Time.time;
                            }
                        }
                        else { }
                        break;
                }
                break;
        }
    }

    public void ShowOpeningPanel() {
        ActivationMode = ActivationState.Opening;

        panelTop.enabled = true;
        panelBottom.enabled = true;

        SizeOpeningPanel();

        openingPanelRefTime = Time.time;
        openingPanelCurrentFrame = 0;
    }

    public void ShowSpecialSelectionPanel() {
        ActivationMode = ActivationState.SpecialSelectionMenu;

        panelTop.enabled = true;
        panelSpacer.enabled = true;
        panelBottom.enabled = true;

        SizeSpecialSelectionPanel();
        UpdateAvailableSpecialFunctionsIcons();
        ShowSpecialFunctionsIcons(true);

        specialFunctionsCurrentIndex = 0;
        availableFunctionsIcons[specialFunctionsCurrentIndex].transform.GetChild(0).GetComponent<Text>().enabled = true;
    }

    public void ShowSpecialFunctionsIcons(bool yesOrNo) {
        FakeTedFunctionIcon.enabled = yesOrNo;
        callHQFunctionIcon.enabled = yesOrNo;
        callStrikeFunction.enabled = yesOrNo;
        playMusicFunctionIcon.enabled = yesOrNo;

        if (!yesOrNo) {
            FakeTedFunctionIcon.transform.GetChild(0).GetComponent<Text>().enabled = false;
            callHQFunctionIcon.transform.GetChild(0).GetComponent<Text>().enabled = false;
            callStrikeFunction.transform.GetChild(0).GetComponent<Text>().enabled = false;
            playMusicFunctionIcon.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
    }

    public void SelectSpecialFunction() {
        ActivationMode = ActivationState.Closed;
        HideInfoBgdPanel();
    }

    public bool IsCurrentSpecialFunctionFakeTed() {
        return (availableFunctionsIcons[specialFunctionsCurrentIndex] == FakeTedFunctionIcon);
    }

    public void HighlightPreviousFunction() {
        RawImage curImg = availableFunctionsIcons[specialFunctionsCurrentIndex];
        curImg.color = new Color(curImg.color.r, curImg.color.g, curImg.color.b, specialFunctionsAvailableAlpha);
        curImg.transform.GetChild(0).GetComponent<Text>().enabled = false;

        if (availableFunctionsIcons.Count > 0) {
            specialFunctionsCurrentIndex = (specialFunctionsCurrentIndex - 1 + availableFunctionsIcons.Count) % availableFunctionsIcons.Count;
            specialFunctionsFlashRefTime = Time.time;
        }

        availableFunctionsIcons[specialFunctionsCurrentIndex].transform.GetChild(0).GetComponent<Text>().enabled = true;
    }

    public void HighlightNextFunction() {
        RawImage curImg = availableFunctionsIcons[specialFunctionsCurrentIndex];
        curImg.color = new Color(curImg.color.r, curImg.color.g, curImg.color.b, specialFunctionsAvailableAlpha);
        curImg.transform.GetChild(0).GetComponent<Text>().enabled = false;

        if (availableFunctionsIcons.Count > 0) {
            specialFunctionsCurrentIndex = (specialFunctionsCurrentIndex + 1) % availableFunctionsIcons.Count;
            specialFunctionsFlashRefTime = Time.time;
        }

        availableFunctionsIcons[specialFunctionsCurrentIndex].transform.GetChild(0).GetComponent<Text>().enabled = true;
    }

    public void UpdateAvailableSpecialFunctionsIcons() {
        availableFunctionsIcons.Clear();

        if (FindObjectOfType<FakeTedAI>().IsFakeTedActive()) {
            availableFunctionsIcons.Add(FakeTedFunctionIcon);
            FakeTedFunctionIcon.color = new Color(FakeTedFunctionIcon.color.r, FakeTedFunctionIcon.color.g, FakeTedFunctionIcon.color.b, specialFunctionsAvailableAlpha);
        }
        else {
            FakeTedFunctionIcon.color = new Color(FakeTedFunctionIcon.color.r, FakeTedFunctionIcon.color.g, FakeTedFunctionIcon.color.b, specialFunctionsUnavailAlpha);
        }

        if (1 == 1) {
            availableFunctionsIcons.Add(callHQFunctionIcon);
            callHQFunctionIcon.color = new Color(callHQFunctionIcon.color.r, callHQFunctionIcon.color.g, callHQFunctionIcon.color.b, specialFunctionsAvailableAlpha);
        }
        else {
            callHQFunctionIcon.color = new Color(callHQFunctionIcon.color.r, callHQFunctionIcon.color.g, callHQFunctionIcon.color.b, specialFunctionsUnavailAlpha);
        }

        if (1 == 1) {
            availableFunctionsIcons.Add(callStrikeFunction);
            callStrikeFunction.color = new Color(callStrikeFunction.color.r, callStrikeFunction.color.g, callStrikeFunction.color.b, specialFunctionsAvailableAlpha);
        }
        else {
            callStrikeFunction.color = new Color(callStrikeFunction.color.r, callStrikeFunction.color.g, callStrikeFunction.color.b, specialFunctionsUnavailAlpha);
        }
        
        if (1 == 1) {
            availableFunctionsIcons.Add(playMusicFunctionIcon);
            playMusicFunctionIcon.color = new Color(playMusicFunctionIcon.color.r, playMusicFunctionIcon.color.g, playMusicFunctionIcon.color.b, specialFunctionsAvailableAlpha);
        }
        else {
            playMusicFunctionIcon.color = new Color(playMusicFunctionIcon.color.r, playMusicFunctionIcon.color.g, playMusicFunctionIcon.color.b, specialFunctionsUnavailAlpha);
        }
        
    }

    public void ShowInfoBgdPanel(int numOfLines, int numOfResponseLines) {
        ActivationMode = ActivationState.CommsWindow;

        panelTop.enabled = true;
        panelSpacer.enabled = true;
        panelBottom.enabled = true;

        responsePanelTop.enabled = true;
        responsePanelSpacer.enabled = true;
        responsePanelBottom.enabled = true;

        SizePanelsToText(numOfLines, numOfResponseLines);
    }

    public void ShowInfoBgdPanelPromptOnly(int numOfLines) {

        ActivationMode = ActivationState.CommsWindow;

        panelTop.enabled = true;
        panelSpacer.enabled = true;
        panelBottom.enabled = true;

        SizePanelsToTextPromptOnly(numOfLines);
    }

    public void HideInfoBgdPanel() {

        panelTop.enabled = false;
        panelSpacer.enabled = false;
        panelBottom.enabled = false;

        if (!usingTextParser) {
            responsePanelTop.enabled = false;
            responsePanelSpacer.enabled = false;
            responsePanelBottom.enabled = false;
        }

        openingPanelLoadingBar.enabled = false;
        ShowSpecialFunctionsIcons(false);
    }

    public void SizeOpeningPanel() {
        float panelTopHeight = panelTop.texture.height;
        float panelBottomHeight = panelBottom.texture.height;
        openingPanelLoadingBar.rectTransform.sizeDelta = new Vector2(openingPanelLoadingBar.rectTransform.rect.width, openingPanelLoadingBarInitialHeight);

        panelBottom.rectTransform.anchoredPosition = panelBottomInitialPos + new Vector2(0, -panelSpacerHeight * 3);
        
        panelTop.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                        new Vector2(0, ((panelTopHeight / 2) + (panelBottomHeight / 2)));

        openingPanelLoadingBar.rectTransform.anchoredPosition = openingPanelLoadingBarInitialPos;
    }

    public void SizeSpecialSelectionPanel() {
        float panelTopHeight = panelTop.texture.height;
        float panelBottomHeight = panelBottom.texture.height;
        float panelMiddleHeight = panelSpacerHeight + 13 + (panelSpacerHeight - 7);

        panelBottom.rectTransform.anchoredPosition = panelBottomInitialPos + new Vector2(0, -panelSpacerHeight * 3);
        promptText.rectTransform.anchoredPosition = panelBottomInitialPos + new Vector2(0, -panelSpacerHeight * 4);

        panelSpacer.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                    new Vector2(0, ((panelMiddleHeight / 2) + (panelBottomHeight / 2)));

        panelSpacer.rectTransform.sizeDelta = new Vector2(panelSpacerWidth, panelMiddleHeight);

        panelTop.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                        new Vector2(0, ((panelMiddleHeight) + (panelTopHeight / 2) + (panelBottomHeight / 2)));

        openingPanelLoadingBar.rectTransform.anchoredPosition = panelSpacer.rectTransform.anchoredPosition + new Vector2(1, 1);
    }

    public void SizePanelsToText(int numOfLines, int numOfResponseLines) {
        float panelTopHeight = panelTop.texture.height;
        float panelBottomHeight = panelBottom.texture.height;
        float panelMiddleHeight = panelSpacerHeight * numOfLines + (panelSpacerHeight - 7);

        float responsePanelTopHeight = responsePanelTop.texture.height;
        float responsePanelBottomHeight = responsePanelBottom.texture.height;
        float responsePanelMiddleHeight = panelSpacerHeight * numOfResponseLines + (panelSpacerHeight - 7);

        
        responsePanelSpacer.rectTransform.anchoredPosition = responsePanelBottom.rectTransform.anchoredPosition +
                                                    new Vector2(0, ((responsePanelMiddleHeight / 2) + (responsePanelBottomHeight / 2)  ));

        responsePanelSpacer.rectTransform.sizeDelta = new Vector2(panelSpacerWidth, responsePanelMiddleHeight);

        responsePanelTop.rectTransform.anchoredPosition = responsePanelBottom.rectTransform.anchoredPosition +
                                                    new Vector2(0, ((responsePanelMiddleHeight) + (responsePanelTopHeight / 2) + (responsePanelBottomHeight / 2)  ));
    

        
        //panelSpacer.enabled = true;

        // Shift the bottom panel (and therefore the middle and top) up based on the size of the response box (number of lines)
        panelBottom.rectTransform.anchoredPosition = responsePanelBottom.rectTransform.anchoredPosition + 
                                                                            new Vector2(0, responsePanelBottomHeight + responsePanelTopHeight + panelSpacerHeight * numOfResponseLines);
        promptText.rectTransform.anchoredPosition = responseTextInitialPos + new Vector2(0, responsePanelBottomHeight + panelSpacerHeight * numOfResponseLines);
        responseText.rectTransform.anchoredPosition = responseTextInitialPos + new Vector2(0, responsePanelBottomHeight - panelSpacerHeight * numOfResponseLines); ;
            
        panelSpacer.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                    new Vector2(0, ((panelMiddleHeight / 2) + (panelBottomHeight / 2)   ));

        panelSpacer.rectTransform.sizeDelta = new Vector2(panelSpacerWidth, panelMiddleHeight);

        panelTop.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                        new Vector2(0, ((panelMiddleHeight) + (panelTopHeight / 2) + (panelBottomHeight / 2)   ));
        
        //else {
        //    panelSpacer.enabled = false;
        //    panelTop.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
        //                                                    new Vector2(0, ((panelTopHeight / 2) + (panelBottomHeight / 2) + 1));
        //}

        AlignImagesWithTop();
    }

    public void SizePanelsToTextPromptOnly(int numOfLines) {
        float panelTopHeight = panelTop.texture.height;
        float panelBottomHeight = panelBottom.texture.height;
        float panelMiddleHeight = panelSpacerHeight * numOfLines + (panelSpacerHeight - 7);

        panelBottom.rectTransform.anchoredPosition = panelBottomInitialPos + new Vector2(0, -panelSpacerHeight * 3);
        promptText.rectTransform.anchoredPosition = panelBottomInitialPos + new Vector2(0, -panelSpacerHeight * 4);

        panelSpacer.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                    new Vector2(0, ((panelMiddleHeight / 2) + (panelBottomHeight / 2)));

        panelSpacer.rectTransform.sizeDelta = new Vector2(panelSpacerWidth, panelMiddleHeight);

        panelTop.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                        new Vector2(0, ((panelMiddleHeight) + (panelTopHeight / 2) + (panelBottomHeight / 2)));
    }

    public void MoveSelectionBar(float yPosRelToDefaultBottom, int nextNumOfLines) {
        if (SelectionMode == SelectionState.Selecting) {
            responseSelectionBar.rectTransform.anchoredPosition = responseSelectionBarInitialPos + new Vector2(0, yPosRelToDefaultBottom);
            responseSelectionBar.rectTransform.sizeDelta = new Vector2(panelSpacerWidth, panelSpacerHeight * nextNumOfLines);
        }
    }

    public void ChooseAResponse() {
        SelectionMode = SelectionState.Selecting;
        responseSelectionBar.enabled = true;
        selectionFlashRefTime = Time.deltaTime;
        brighteningOrDimming = false;
    }

    public void SelectResponse(int numOfSpacerHeights, int numOfLines) {

        if (SelectionMode == SelectionState.Selecting) {
            SelectionMode = SelectionState.Selected;
        }

        MoveSelectionBar(numOfSpacerHeights, numOfLines);
    }

    public void NoSelection() {
        SelectionMode = SelectionState.NotSelecting;
        responseSelectionBar.enabled = false;
        responseSelectionBar.rectTransform.anchoredPosition = responseSelectionBarInitialPos;
    }

    public float GetPanelSpacerHeight() {
        return panelSpacerHeight;
    }

    private void AlignImagesWithTop() {
        if (imagesToAlignWithTopEdge.Count > 0) {
            foreach (RawImage image in imagesToAlignWithTopEdge) {
                image.rectTransform.anchoredPosition = panelTop.rectTransform.anchoredPosition + imageAlignmentOffset;
            }
        }
    }
}
