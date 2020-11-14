using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommsController : MonoBehaviour {

    [HideInInspector]
    public CommsNavigation commsNavigation;

    [HideInInspector]
    public List<Transition> responses = new List<Transition>();

    // Character comms text and response text
    [HideInInspector]
    public Text commsDisplay, responseText, responseTextCopy;

    [HideInInspector]
    public List<string> interactionDescriptionsForPrompt = new List<string>();

    [HideInInspector]
    public List<string> responseLog = new List<string>();

    public InputField textInput;
    public bool textActivated, textActive;
    public Prompt noTargetPrompt, startingPrompt;
    public string currentText, currentResponseText;
    public bool textIsUnravelled, responseTextIsUnravelled;
    public int numOfCharsToUnravelPerInterval, numOfResponseCharsToUnravelPerInterval;
    public float unravellingTextTimePerChar, unravellingResponseTextTimePerChar;
    public string loadingString;
    public float delayBeforeShowingLoadingString, loadingTextTimePerChar, totalLoadingTime;

    public string defaultExitConvoText;

    public Color promptColor, responseNumberColor, exitConvoColor;

    public InputVerbs[] inputVerbs;
    public InputVerbs inputNoVerb;
    public Transform bgdPanels;
    public RawImage portraitFrame, portrait;
    public AudioClip noSignalClip;

    private RawImage commsImg;
    private PanelsBottom panels;
    private DatingScreen dateScreen;
    private CameraMaster camMaster;
    private MusicPlayer musicBox;
    public InfoScan scanner;

    private TeddyRightEye rightEye;
    private BodyCam bCam;
    private ID transmissionID;
    private TextAndSpeech transmissionSpeech;
    private AudioSource transmissionAudio;
    private NPCIntelligence transmissionAI;
    private ActionSceneCoordinator actCoord;

    private Prompt currentLine;

    private int numOfTextLines, prevNumOfTextLines;    
    private string unravellingPromptText, unravellingResponseTransitionText, listOfResponses;
    private bool unravellingText, unravellingTextStartTimeSet, unravellingResponseText, unravellingRandomChar, skipUnravelling, canSkipUnravelling, isUnravelled, showingLoadingText;
    private List<char> unravellingChars = new List<char>();
    private List<char> unravellingResponseChars = new List<char>();
    private List<char> loadingChars = new List<char>();
    private int unravellingTextCharIndex, remainingCharsInUnravellingText,
                unravellingResponseTextCharIndex, remainingCharsInUnravellingResponseText,
                loadingStringCharIndex;
    private float unravellingTextStartTime, unravellingTextTimeRef, unravellingResponseTextTimeRef, unravelingTextTimeRefForRandChar, loadingTextTimeRef;

    // This list will be used to keep track of the line count of each response in the list for the current Prompt
    // For moving and resizing the selection bar
    private List<ResponseLineCount> lineCountsOfResponses = new List<ResponseLineCount>();
    private int currentResponseNumber, responseLineCountTotal;
    private bool readyForSelection;

    private bool closeEnoughToCommunicate, talkingToFakeTed;

	// Use this for initialization
	void Awake () {
        commsNavigation = FindObjectOfType<CommsNavigation>();
        commsDisplay = GetComponent<Text>();
        responseText = this.transform.Find("ResponseText").GetComponent<Text>();
        responseTextCopy = this.transform.Find("ResponseTextCopy").GetComponent<Text>();
        commsImg = this.transform.parent.GetComponent<RawImage>();
        panels = bgdPanels.GetComponent<PanelsBottom>();
        dateScreen = FindObjectOfType<DatingScreen>();
        camMaster = FindObjectOfType<CameraMaster>();
        musicBox = FindObjectOfType<MusicPlayer>();
        scanner = FindObjectOfType<InfoScan>();
        actCoord = FindObjectOfType<ActionSceneCoordinator>();

        rightEye = FindObjectOfType<TeddyRightEye>();
        bCam = FindObjectOfType<BodyCam>();

        commsDisplay.enabled = false;
        commsImg.enabled = false;
        responseText.enabled = false;
        responseTextCopy.enabled = false;           // responseTextCopy is only used for determining the line count of individual responses; it is never displayed
        panels.HideInfoBgdPanel();

        textActivated = false;
        textActive = false;

        commsDisplay.supportRichText = true;
        responseText.supportRichText = true;
        responseTextCopy.supportRichText = true;

        currentText = "";
        currentResponseText = "";
        listOfResponses = "";
        responseTextCopy.text = "";

        currentResponseNumber = 0;
        readyForSelection = false;

        portrait.enabled = false;
        portraitFrame.enabled = false;

        unravellingText = false;
        unravellingResponseText = false;
        showingLoadingText = false;
        isUnravelled = false;
        unravellingTextStartTimeSet = false;

        char[] tempChars = loadingString.ToCharArray();
        for (int i = 0; i < tempChars.Length; i++) {
            loadingChars.Add(tempChars[i]);
        }

        skipUnravelling = false;
        canSkipUnravelling = false;
        unravellingTextCharIndex = 0;
        loadingStringCharIndex = 0;
        unravellingTextTimeRef = 0;
        unravellingResponseTextTimeRef = 0;
        loadingTextTimeRef = 0;

        closeEnoughToCommunicate = false;
    }

    void Start () {
        
    }

    // Update is called once per frame
    void Update() {

        if (textActive) {
            scanner.HideReticleAndText(true);

            if ((transmissionSpeech != null && closeEnoughToCommunicate && !currentLine.continues) || (startingPrompt != null && talkingToFakeTed)) 
                                                                    { panels.ShowInfoBgdPanel(commsDisplay.cachedTextGenerator.lineCount, responseText.cachedTextGenerator.lineCount); }
            else                                                    { panels.ShowInfoBgdPanelPromptOnly(commsDisplay.cachedTextGenerator.lineCount); }
            commsDisplay.text = unravellingPromptText;
            responseText.text = unravellingResponseTransitionText;

            if (textIsUnravelled) {
                if (unravellingText) {

                    if (!skipUnravelling) {
                        if (!unravellingTextStartTimeSet) {
                            unravellingTextStartTime = Time.time;
                            unravellingTextStartTimeSet = true;
                        }

                        canSkipUnravelling = true;

                        if (Time.time - unravellingTextTimeRef > unravellingTextTimePerChar) {
                            string charString = "";
                            int numOfCharsToAdd = Mathf.Min(unravellingChars.Count - unravellingTextCharIndex, numOfCharsToUnravelPerInterval);
                            for (int i = 0; i < numOfCharsToAdd; i++) {
                                charString = string.Concat(charString, unravellingChars[unravellingTextCharIndex + i].ToString());
                            }
                            unravellingPromptText = string.Concat(unravellingPromptText, charString);
                            if (unravellingTextCharIndex < unravellingChars.Count - 1) {
                                unravellingTextCharIndex = unravellingTextCharIndex + numOfCharsToUnravelPerInterval;
                                unravellingTextTimeRef = Time.time;
                            }
                            else {
                                StopUnravellingText();
                                unravellingTextTimeRef = Time.time;

                                string hexPromptColor = ColorUtility.ToHtmlStringRGB(promptColor);
                                unravellingPromptText = "<color=#" + hexPromptColor + ">" + unravellingPromptText + "</color>";

                                // Can click through to next line if the current one is a continuation--no loading text or response text needed
                                if (currentLine.continues) {
                                    readyForSelection = true;
                                }
                            }
                        }
                    }
                    else {
                        canSkipUnravelling = false;

                        StopUnravellingText();
                        unravellingTextTimeRef = Time.time;

                        string hexPromptColor = ColorUtility.ToHtmlStringRGB(promptColor);
                        unravellingPromptText = "<color=#" + hexPromptColor + ">" + unravellingPromptText + "</color>";

                        if (currentLine.continues) {
                            readyForSelection = true;
                        }

                        skipUnravelling = false;
                    }

                    //if (unravelingRandomChar) {
                    //    int rand;
                    //    if (unravelingTextCharIndex < unravelingChars.Count - 1) { rand = Random.Range(unravelingTextCharIndex, unravelingChars.Count); } else { rand = unravelingTextCharIndex; }

                    //    if (Time.time - unravelingTextTimeRefForRandChar > unravelingTextTimePerChar) {
                    //        //unravelingPromptText = string.Concat(unravelingPromptText, unravelingChars[rand].ToString());
                    //        unravelingPromptText = string.Concat(unravelingPromptText, "-");
                    //        unravelingRandomChar = false;
                    //        unravelingTextTimeRef = Time.time;
                    //    }
                    //} else {
                    //    if (Time.time - unravelingTextTimeRef > unravelingTextTimePerChar) {
                    //        unravelingPromptText = string.Concat(unravelingPromptText.Substring(0, unravelingPromptText.Length - 1),
                    //                unravelingChars[unravelingTextCharIndex].ToString());
                    //        if (unravelingTextCharIndex < unravelingChars.Count - 1) {
                    //            unravelingTextCharIndex++;
                    //            unravelingRandomChar = true;
                    //            unravelingTextTimeRefForRandChar = Time.time;
                    //        } else {
                    //            StopUnravellingText();
                    //        }
                    //    }
                    //}
                }

                if ((transmissionSpeech != null && closeEnoughToCommunicate) || (startingPrompt != null && talkingToFakeTed)) {
                    // Show loading text for responses--responses load once the current prompt text is shown completely
                    // Loading text (and subsequent response text) does not show if the current prompt continues
                    if (showingLoadingText && !currentLine.continues && (Time.time - unravellingTextStartTime > delayBeforeShowingLoadingString)) {
                        if (Time.time - loadingTextTimeRef > loadingTextTimePerChar) {

                            string charLoadingString = "";

                            for (int i = 0; i < loadingStringCharIndex; i++) {
                                charLoadingString = string.Concat(charLoadingString, loadingChars[i]);
                            }

                            if (loadingStringCharIndex < loadingChars.Count - 1) { loadingStringCharIndex++; }
                            else { loadingStringCharIndex = 0; }

                            unravellingResponseTransitionText = charLoadingString;

                            loadingTextTimeRef = Time.time;
                        }

                        if (Time.time - unravellingTextTimeRef > totalLoadingTime) {

                            showingLoadingText = false;
                            loadingStringCharIndex = 0;
                            panels.ChooseAResponse();
                            currentResponseNumber = 0;
                            panels.MoveSelectionBar(lineCountsOfResponses[currentResponseNumber].pixelCountDistanceFromDefaultBottomPosition, lineCountsOfResponses[currentResponseNumber].lineCount);
                            readyForSelection = true;
                        }
                    }
                    if (!showingLoadingText) {
                        if (responseTextIsUnravelled) {
                            if (unravellingResponseText) {
                                if (Time.time - unravellingResponseTextTimeRef > unravellingResponseTextTimePerChar) {

                                    string charString = "";
                                    int numOfCharsToAdd = Mathf.Min(unravellingResponseChars.Count - unravellingResponseTextCharIndex, numOfResponseCharsToUnravelPerInterval);
                                    for (int i = 0; i < numOfCharsToAdd; i++) {
                                        charString = string.Concat(charString, unravellingResponseChars[unravellingResponseTextCharIndex + i].ToString());
                                    }
                                    unravellingResponseTransitionText = string.Concat(unravellingResponseTransitionText, charString);
                                    if (unravellingResponseTextCharIndex < unravellingResponseChars.Count - 1) {
                                        unravellingResponseTextCharIndex = unravellingResponseTextCharIndex + numOfResponseCharsToUnravelPerInterval;
                                        unravellingResponseTextTimeRef = Time.time;
                                    }
                                    else {
                                        StopUnravellingResponseText();
                                    }
                                }
                            }
                        }
                        else {
                            if (responses.Count > 0) {
                                unravellingResponseTransitionText = listOfResponses;
                            }
                        }
                    }
                }
            }
        }

        // NOTE: textActivated is toggled by player button press -- see Camera Master
        // Only makes one pass before switching to !textActivated & textActive
        if (textActivated && !textActive) {

            //scanner.reticle.enabled = false;
            scanner.HideReticleAndText(true);

            ClearConversationLogs();
            panels.NoSelection();

            if (rightEye.RightEyeTargetID() != null) {
                transmissionID = rightEye.RightEyeTargetID();

                //if (transmissionID.transform.parent.GetComponent<Collider>() != null) {
                //    Collider col = transmissionID.transform.parent.GetComponent<Collider>();
                //    distance = Vector3.Distance(col.ClosestPoint(bCam.transform.position), bCam.transform.position);
                //}
                //else {
                //    distance = Vector3.Distance(transmissionID.transform.parent.position, bCam.transform.position);
                //}

                if (transmissionID.GetDistanceToActiveIDCollider(bCam.transform.position) < transmissionID.maxDistanceToActivate) {

                    closeEnoughToCommunicate = true;

                    if (transmissionID.GetType() == typeof(IDDate)) {
                        dateScreen.EnableScreen(true);
                        camMaster.OverrideReticle(true);
                    }
                    else if (transmissionID.GetType() == typeof(IDCharacter)) {
                        IDCharacter idChar = (IDCharacter)transmissionID;
                        if (idChar.portrait != null) {
                            portrait.texture = idChar.portrait;
                            portraitFrame.enabled = true;
                            portrait.enabled = true;
                        }
                    }
                }
            }

            if (rightEye.RightEyeTargetSpeech() != null) {
                transmissionSpeech = rightEye.RightEyeTargetSpeech();

                if (closeEnoughToCommunicate) {
                    startingPrompt = transmissionSpeech.openingTextLine;
                    responseText.enabled = true;
                }
                else {
                    startingPrompt = null;
                    responseText.enabled = false;
                }
            }

            if (rightEye.RightEyeTargetAI() != null) {
                transmissionAI = rightEye.RightEyeTargetAI();
            }

            if (rightEye.RightEyeTargetAudio() != null) {
                transmissionAudio = rightEye.RightEyeTargetAudio();
            }            

            if (startingPrompt != null) {
                currentLine = startingPrompt;
            } else {
                currentLine = noTargetPrompt;
            }

            AssembleAndDisplayCommsText();

            commsDisplay.enabled = true;

            textActive = true;

            //textInput.ActivateInputField();

            //textInput.GetComponent<Image>().enabled = true;
            //textInput.textComponent.enabled = true;
            //panels.ShowInfoBgdPanel(commsDisplay.cachedTextGenerator.lineCount);

        } else if (textActive && !textActivated) {

            portrait.enabled = false;
            portraitFrame.enabled = false;
            dateScreen.EnableScreen(false);
            //camMaster.OverrideReticle(false);
            //scanner.reticle.enabled = true;
            scanner.HideReticleAndText(false);

            transmissionID = null;
            transmissionSpeech = null;
            transmissionAI = null;
            transmissionAudio = null;

            panels.ShowInfoBgdPanelPromptOnly(1);
            panels.HideInfoBgdPanel();
            panels.NoSelection();
            commsDisplay.enabled = false;
            responseText.enabled = false;

            textActive = false;
            StopUnravellingText();
            StopUnravellingResponseText();

            ClearConversationLogs();
            startingPrompt = null;
            unravellingTextStartTimeSet = false;

            closeEnoughToCommunicate = false;
            talkingToFakeTed = false;

            //textInput.DeactivateInputField();
            //textInput.GetComponent<Image>().enabled = false;
            //textInput.GetComponent<Image>().enabled = false;
            //textInput.textComponent.enabled = false;
        }
    }

    /// 
    /// ///////////////////////////////////////////////////////////////////////////////////////////
    /// METHODS


    public void AssembleAndDisplayCommsText() {

        // Wipe out the comms text
        ClearConversationLogs();
        // Clear out the list containing responses and their respective line counts
        lineCountsOfResponses.Clear();
        panels.NoSelection();

        // Load the current line text
        UnpackTransitionsForPrompt();

        // Update Target Info Box, run Actions attached to the current line
        // Also run an AnimatedVoiceLine/LookAtTed Action combo if there's an AnimationAudio in the currentLine and hasAudio is checked
        MakeIDChanges(currentLine);
        RunPromptActions(currentLine);

        if (currentLine == noTargetPrompt) {
            musicBox.PlayCommsNoSignal();
        }
        if (currentLine.hasAudio && currentLine.animatedSpeechLine != null && transmissionSpeech != null && transmissionAI != null) {
            transmissionSpeech.PlayClipAndStartAnimatingFace(currentLine.animatedSpeechLine);
            transmissionAI.LookAtPerson(currentLine.lookDuration, FindObjectOfType<TeddyHead>().transform, currentLine.matchLookDurationToSpeechTime);
        }

        // A lot of this crap probably isn't needed, since we're not having a scrolling log history of Prompts and responses
        string combinedText = currentLine.line;
        AddToLog(combinedText);

        string runningLog = string.Join("\n", responseLog.ToArray());
            
        // Tack on an ellipse if the next line is a continuation of the current Prompt
        if (currentLine.continues) { runningLog = runningLog + "  (...)"; }
        currentText = runningLog;

        // Generate block of text for list of responses, and call methods to generate char arrays for both Prompt and response list, to unravel the Prompt string (and maybe the response string)
        // Also populate the list of responses and their line counts
        if (textIsUnravelled) {
            InitializePromptCharArrayToUnravel(runningLog);
                
            string listNumberColor = ColorUtility.ToHtmlStringRGB(responseNumberColor);
            string exitResponseColor = ColorUtility.ToHtmlStringRGB(exitConvoColor);

            if (responses.Count > 0) {
                // This will look like shit if it's unravelled--the colors aren't applied until the starting tag is completed
                for (int i = 0; i < responses.Count; i++) {
                    lineCountsOfResponses.Add(new ResponseLineCount(responses[i].transitDescription, responses[i].numberOfLines));

                    if (i < responses.Count - 1) {
                        if (responses[i].isExitLine) {
                            listOfResponses += "<color=#" + listNumberColor + ">" + (i + 1).ToString() + ")</color>  <color=#" + exitResponseColor + ">" + responses[i].transitDescription + "</color>\n";
                        }
                        else {
                            listOfResponses += "<color=#" + listNumberColor + ">" + (i + 1).ToString() + ")</color>  " + responses[i].transitDescription + "\n";
                        }
                    }
                    else {
                        if (responses[i].isExitLine) {
                            listOfResponses += "<color=#" + listNumberColor + ">" + (i + 1).ToString() + ")</color>  <color=#" + exitResponseColor + ">" + responses[i].transitDescription + "</color>";
                        }
                        else {
                            listOfResponses += "<color=#" + listNumberColor + ">" + (i + 1).ToString() + ")</color>  " + responses[i].transitDescription;
                        }
                    }
                }
            }
            else {
                if (transmissionSpeech != null) {
                    Transition exitTransition = new Transition();
                    exitTransition.transitDescription = defaultExitConvoText;
                    exitTransition.numberOfLines = 1;
                    exitTransition.isExitLine = true;

                    responses.Add(exitTransition);
                    listOfResponses += "<color=#" + listNumberColor + ">1)</color>  <color=#" + exitResponseColor + ">" + exitTransition.transitDescription + "</color>";
                    lineCountsOfResponses.Add(new ResponseLineCount(exitTransition.transitDescription, exitTransition.numberOfLines));
                }
            }

            currentResponseText = listOfResponses;

            // This sets the relative Y position from the default bottom position for each response in the line count of responses list
            for (int i = lineCountsOfResponses.Count; i > 0; i--) {
                if (i == lineCountsOfResponses.Count) {
                    lineCountsOfResponses[i - 1].pixelCountDistanceFromDefaultBottomPosition = (-panels.GetPanelSpacerHeight() / 2) + panels.GetPanelSpacerHeight() * lineCountsOfResponses[i - 1].lineCount / 2;
                }
                else {
                    lineCountsOfResponses[i - 1].SetPixelYDistanceRelToBottomResponse(lineCountsOfResponses[i].pixelCountDistanceFromDefaultBottomPosition, 
                                                                                        lineCountsOfResponses[i].lineCount, panels.GetPanelSpacerHeight());
                }
            }

            InitializeResponseCharArrayToUnravel(listOfResponses);

        } 
        else {
            unravellingPromptText = runningLog;
            unravellingResponseTransitionText = currentResponseText;
        }
    }

    public void UnpackTransitionsForPrompt() {
        for (int i = 0; i < currentLine.transitions.Length; i++) {
            Transition transit = currentLine.transitions[i];
            responses.Add(transit);
        }       
    }

    public void ClearConversationLogs() {
        interactionDescriptionsForPrompt.Clear();
        responseLog.Clear();
        //commsNavigation.ClearDictionary();
        responses.Clear();
        unravellingPromptText = "";
        unravellingResponseTransitionText = "";
        listOfResponses = "";
    }

    public void AddToLog(string text) {
        responseLog.Add(text);
    }

    private void InitializePromptCharArrayToUnravel(string promptText) {
        char[] tempChars = new char[promptText.ToCharArray().Length];
        System.Array.Copy(promptText.ToCharArray(), tempChars, promptText.ToCharArray().Length);
        unravellingChars.Clear();

        for (int i = 0; i < tempChars.Length; i++) {
            unravellingChars.Add(tempChars[i]);
        }

        unravellingPromptText = "";
        unravellingText = true;
        unravellingTextCharIndex = 0;
        unravellingRandomChar = true;
        isUnravelled = false;

        showingLoadingText = true;
    }

    private void InitializeResponseCharArrayToUnravel(string rspTxt) {
        char[] tempChars = new char[rspTxt.ToCharArray().Length];
        System.Array.Copy(rspTxt.ToCharArray(), tempChars, rspTxt.ToCharArray().Length);
        unravellingResponseChars.Clear();

        for (int i = 0; i < tempChars.Length; i++) {
            unravellingResponseChars.Add(tempChars[i]);
        }

        unravellingResponseTransitionText = "";
        unravellingResponseTextCharIndex = 0;
        unravellingRandomChar = true;
    }

    private void StopUnravellingText() {
        unravellingText = false;
        unravellingTextCharIndex = 0;
        unravellingChars.Clear();
        unravellingPromptText = currentText;
    }

    private void StopUnravellingResponseText() {
        unravellingResponseText = false;
        unravellingResponseTextCharIndex = 0;
        unravellingResponseChars.Clear();
        unravellingResponseTransitionText = currentResponseText;
        isUnravelled = true;
    }

    public bool IsCommsTextUnravelled() {
        return isUnravelled;
    }

    public void OpenCommsWindow() {
        panels.ShowOpeningPanel();
    }

    public void OpenSpecialOptionsMenu() {
        panels.ShowSpecialSelectionPanel();
    }

    public void SelectSpecialOption() {
        if (panels.IsCurrentSpecialFunctionFakeTed()) {
            SetFakeTedAsCurrentID();
            textActivated = true;
        }

        panels.SelectSpecialFunction();
    }

    public void HighlightPreviousSpecialOption() {
        panels.HighlightPreviousFunction();
    }

    public void HighlightNextSpecialOption() {
        panels.HighlightNextFunction();
    }

    public void ClearCommsWindow() {
        panels.HideInfoBgdPanel();
    }

    public void SetFakeTedAsCurrentID() {
        startingPrompt = FindObjectOfType<FakeTedAI>().transform.Find("TextAndSpeech").GetComponent<TextAndSpeech>().openingTextLine;
        portrait.texture = FindObjectOfType<FakeTedAI>().fakeTedPortrait;
        portraitFrame.enabled = true;
        portrait.enabled = true;
        talkingToFakeTed = true;
        responseText.enabled = true;
    }

    public bool ReadyForSelection() {
        return readyForSelection;
    }

    public void MoveResponseSelectorBarUp() {
        if (currentResponseNumber < 1) { currentResponseNumber = responses.Count - 1; }
        else                            { currentResponseNumber--; }
        panels.MoveSelectionBar(lineCountsOfResponses[currentResponseNumber].pixelCountDistanceFromDefaultBottomPosition, lineCountsOfResponses[currentResponseNumber].lineCount);
    }

    public void MoveResponseSelectorBarDown() {
        if (currentResponseNumber > responses.Count - 2) { currentResponseNumber = 0; }
        else { currentResponseNumber++; }

        panels.MoveSelectionBar(lineCountsOfResponses[currentResponseNumber].pixelCountDistanceFromDefaultBottomPosition, lineCountsOfResponses[currentResponseNumber].lineCount);
    }

    public void SelectResponse() {
        readyForSelection = false;
        //camMaster.ResetKeyTimer();

        if (currentLine.continues && currentLine.continuation != null) {
            currentLine = currentLine.continuation;
            currentResponseNumber = 0;
            AssembleAndDisplayCommsText();
        }
        else {
            if (responses[currentResponseNumber].isExitLine) {
                textActivated = false;
                camMaster.commsEnabled = false;
                panels.NoSelection();
            }
            else {
                currentLine = responses[currentResponseNumber].valuePrompt;
                currentResponseNumber = 0;
                AssembleAndDisplayCommsText();
            }
        }
    }

    public bool CanSkipUnravelling() {
        return canSkipUnravelling;
    }

    public void SkipUnravelling() {
        if (canSkipUnravelling) {
            skipUnravelling = true;
        }
    }

    private class ResponseLineCount {
        public string line;
        public int lineCount;
        public float pixelCountDistanceFromDefaultBottomPosition;

        public ResponseLineCount(string responseLine, int count) {
            line = responseLine;
            lineCount = count;
        }

        public void SetPixelYDistanceRelToBottomResponse(float relYOfResponseUnderneath, int lineCountOfResponseUnderneath, float spacerHeight) {
            pixelCountDistanceFromDefaultBottomPosition = relYOfResponseUnderneath + (spacerHeight * lineCountOfResponseUnderneath / 2) + (spacerHeight * lineCount / 2);
        }
    }

    private void MakeIDChanges(Prompt line) {

        if (transmissionID != null) {
            transmissionID.KnowName = line.revealsName ? true : transmissionID.KnowName;
            transmissionID.KnowActName = line.revealsActName ? true : transmissionID.KnowActName;
            transmissionID.KnowAvatarName = line.revealsAvName ? true : transmissionID.KnowAvatarName;
            transmissionID.KnowDescription = line.revealsDescription ? true : transmissionID.KnowDescription;
            transmissionID.KnowStatus = line.revealsStatus ? true : transmissionID.KnowStatus;

            if (line.makeIDTextFieldChanges) {
                transmissionID.ObjName = (line.nameChange == "") ? transmissionID.ObjName : line.nameChange;
                transmissionID.ObjActualName = (line.actNameChange == "") ? transmissionID.ObjActualName : line.actNameChange;
                transmissionID.ObjAvatarName = (line.avNameChange == "") ? transmissionID.ObjAvatarName : line.avNameChange;
                transmissionID.ObjDescription = (line.descriptionChange == "") ? transmissionID.ObjDescription : line.descriptionChange;
                transmissionID.ObjStatus = (line.statusChange == "") ? transmissionID.ObjStatus : line.statusChange;
            }
        }
    }

    private void RunPromptActions(Prompt line) {
        foreach (Action act in line.triggeredActions) {
            actCoord.TriggerAction(act);
        }
    }
}
