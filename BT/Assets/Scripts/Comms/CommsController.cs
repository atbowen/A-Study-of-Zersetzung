using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommsController : MonoBehaviour {

    [HideInInspector]   public CommsNavigation commsNavigation;
    [HideInInspector]   public List<string> interactionDescriptionsForPrompt = new List<string>();

    [HideInInspector]   public List<string> responseLog = new List<string>();

    public InputField textInput;
    public bool textActivated, textActive;
    public Prompt noTargetPrompt, startingPrompt;
    public string currentText;
    public bool textIsUnravelled;
    public int numOfCharsToUnravelPerInterval;

    public InputVerbs[] inputVerbs;
    public InputVerbs inputNoVerb;
    public Text commsDisplay;
    public Transform bgdPanels;
    public Transform transmissionSource;
    public RawImage portraitFrame, portrait;

    private RawImage commsImg;
    private PanelsBottom panels;
    private DatingScreen dateScreen;
    private CameraMaster camMaster;

    private int numOfTextLines, prevNumOfTextLines;
    public float unravellingTextTimePerChar;
    private string unravellingPromptText;
    private bool unravellingText, unravellingRandomChar, skipUnravelling, isUnravelled;
    private List<char> unravellingChars = new List<char>();
    private int unravellingTextCharIndex, remainingCharsInUnravellingText;
    private float unravellingTextTimeRef, unravelingTextTimeRefForRandChar;

	// Use this for initialization
	void Awake () {
        commsNavigation = GetComponent<CommsNavigation>();
        commsDisplay = GetComponent<Text>();
        commsImg = this.transform.parent.GetComponent<RawImage>();
        panels = bgdPanels.GetComponent<PanelsBottom>();
        dateScreen = FindObjectOfType<DatingScreen>();
        camMaster = FindObjectOfType<CameraMaster>();

        commsDisplay.enabled = false;
        commsImg.enabled = false;
        panels.HideInfoBgdPanel();

        textActivated = false;
        textActive = false;

        currentText = "";

        transmissionSource = null;

        portrait.enabled = false;
        portraitFrame.enabled = false;

        numOfTextLines = 0;
        prevNumOfTextLines = 0;

        unravellingText = false;
        unravellingRandomChar = true;
        skipUnravelling = false;
        unravellingTextCharIndex = 0;
        unravellingTextTimeRef = 0;
        isUnravelled = false;

        //startingPrompt = null;
    }

    void Start () {
        
    }

    // Update is called once per frame
    void Update() {
        Canvas.ForceUpdateCanvases();

        if (textActive) {
            panels.ShowInfoBgdPanel(commsDisplay.cachedTextGenerator.lineCount);
            commsDisplay.text = unravellingPromptText;

            if (textIsUnravelled) {
                if (unravellingText) {
                    if (Time.time - unravellingTextTimeRef > unravellingTextTimePerChar) {
                        string charString = "";
                        int numOfCharsToAdd = Mathf.Min(unravellingChars.Count - unravellingTextCharIndex, numOfCharsToUnravelPerInterval);
                        for (int i = 0; i < numOfCharsToAdd; i++) {
                            charString = string.Concat(charString, unravellingChars[unravellingTextCharIndex + i].ToString());
                        }
                        unravellingPromptText = string.Concat(unravellingPromptText, charString);
                        if (unravellingTextCharIndex < unravellingChars.Count - 1) {
                            unravellingTextCharIndex = unravellingTextCharIndex + numOfCharsToUnravelPerInterval;
                            //unravelingRandomChar = true;
                            unravelingTextTimeRefForRandChar = Time.time;
                        } else {
                            StopUnravellingText();
                        }
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
            }
        }

        if (textActivated && !textActive) {
            
            if (transmissionSource != null && transmissionSource.Find("ID").GetComponent<ID>() != null) {
                ID ident = transmissionSource.Find("ID").GetComponent<ID>();

                if (ident.IDType == ID.IdentificationType.Date) {
                    dateScreen.EnableScreen(true);
                    camMaster.OverrideReticle(true);
                } else if (ident.IDType == ID.IdentificationType.Character) {
                    IDCharacter idChar = (IDCharacter)ident;
                    portrait.texture = idChar.portrait;
                    portraitFrame.enabled = true;
                    portrait.enabled = true;
                }      
            }

            if (startingPrompt != null) {
                commsNavigation.currentLine = startingPrompt;
            } else {
                commsNavigation.currentLine = noTargetPrompt;
            }

            AssembleAndDisplayCommsText();
            if (transmissionSource.GetComponent<NPCIntelligence>() != null) {
                transmissionSource.GetComponent<NPCIntelligence>().RunActionsAndSpeechForLine(commsNavigation.currentLine);
            }

            textInput.ActivateInputField();
            textInput.GetComponent<Image>().enabled = true;
            textInput.textComponent.enabled = true;
            //panels.ShowInfoBgdPanel(commsDisplay.cachedTextGenerator.lineCount);
            this.GetComponent<Text>().enabled = true;

            textActive = true;
        } else if (!textActivated && textActive) {
            portrait.enabled = false;
            portraitFrame.enabled = false;
            dateScreen.EnableScreen(false);
            camMaster.OverrideReticle(false);

            textInput.GetComponent<Image>().enabled = false;
            textInput.textComponent.enabled = false;
            panels.HideInfoBgdPanel();
            this.GetComponent<Text>().enabled = false;

            textInput.DeactivateInputField();
            //textInput.GetComponent<Image>().enabled = false;
            textActive = false;
            StopUnravellingText();
        }      
    }

    public void AssembleAndDisplayCommsText() {
        //Debug.Log(commsNavigation.currentLine);
        if (!skipUnravelling) {
            ClearConversationLogs();

            UnpackPrompt();

            string combinedText = commsNavigation.currentLine.line; // + "\n" + string.Join("\n", this.interactionDescriptionsForPrompt.ToArray());
            AddToLog(combinedText);

            string runningLog = string.Join("\n", responseLog.ToArray());
            if (commsNavigation.currentLine.continues) { runningLog = runningLog + "  (...)"; }
            currentText = runningLog;
            if (textIsUnravelled) { InitializePromptCharArrayToUnravel(runningLog); } 
            else { unravellingPromptText = runningLog; }

            //panels.SizePanelToText(commsDisplay.cachedTextGenerator.lineCount);
        }
        skipUnravelling = false;
    }

    public void ClearConversationLogs() {
        interactionDescriptionsForPrompt.Clear();
        responseLog.Clear();
        commsNavigation.ClearDictionary();
    }

    public void AddToLog(string text) {
        //responseLog.Add(text + "\n");
        responseLog.Add(text);
    }

    private void UnpackPrompt() {
        commsNavigation.UnpackTransitionsForPrompt();
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
    }

    private void StopUnravellingText() {
        unravellingText = false;
        unravellingTextCharIndex = 0;
        unravellingChars.Clear();
        unravellingPromptText = currentText;
        isUnravelled = true;
    }

    public void SkipUnravellingText() {
        skipUnravelling = true;
        StopUnravellingText();
    }

    public bool IsCommsTextUnravelled() {
        return isUnravelled;
    }
}
