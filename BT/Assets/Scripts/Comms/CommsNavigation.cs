using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 

public class CommsNavigation : MonoBehaviour {

    public Prompt currentLine;      // This is the current prompt

    Dictionary<string, Prompt> noVerbPromptTree = new Dictionary<string, Prompt>();
    Dictionary<string, Prompt> advisePromptTree = new Dictionary<string, Prompt>();
    Dictionary<string, Prompt> humorPromptTree = new Dictionary<string, Prompt>();

    private CommsController controller;
    private ActionSceneCoordinator actCoord;
    private ID ident;
    private TextAndSpeech targetSpeech;
    private NPCIntelligence targetAI;
    private AudioSource targetAudio;

    void Awake() {
        controller = this.GetComponent<CommsController>();
        actCoord = FindObjectOfType<ActionSceneCoordinator>();
        //currentLine = null;
    }

    // Displays in comms window all transitions for next possible prompts
    public void UnpackTransitionsForPrompt() {
        if (controller.transmissionSource != null) {
            if (controller.transmissionSource.Find("ID").GetComponent<ID>() != null) {
                ident = controller.transmissionSource.Find("ID").GetComponent<ID>();
            }

            if (controller.transmissionSource.Find("TextAndSpeech").GetComponent<TextAndSpeech>() != null) {
                targetSpeech = controller.transmissionSource.Find("TextAndSpeech").GetComponent<TextAndSpeech>();
            }

            if (controller.transmissionSource.GetComponent<AudioSource>() != null) {
                targetAudio = controller.transmissionSource.GetComponent<AudioSource>();
            }

            if (controller.transmissionSource.GetComponent<NPCIntelligence>() != null) {
                targetAI = controller.transmissionSource.GetComponent<NPCIntelligence>();
            }
        }

        for (int i=0; i < currentLine.transitions.Length; i++) {
            Transition transit = currentLine.transitions[i];
            if (transit.valuePrompt.noVerbType) {
                noVerbPromptTree.Add(transit.keyString, transit.valuePrompt);
                if (transit.transitDescription.Length > 0) {
                    controller.interactionDescriptionsForPrompt.Add(transit.transitDescription);
                }
            }
            if (transit.valuePrompt.adviseType) {
                advisePromptTree.Add(transit.keyString, transit.valuePrompt);
                if (transit.transitDescription.Length > 0) {
                    controller.interactionDescriptionsForPrompt.Add(transit.transitDescription);
                }
            }
            if (transit.valuePrompt.humorType) {
                humorPromptTree.Add(transit.keyString, transit.valuePrompt);
                if (transit.transitDescription.Length > 0) {
                    controller.interactionDescriptionsForPrompt.Add(transit.transitDescription);
                }
            }
        }
    }

    public void ContinueLine(Prompt continuingLine) {
        if (continuingLine.continues && continuingLine.continuation != null) {
            currentLine = continuingLine.continuation;
            //targetAI.RunActionsAndSpeechForLine(currentLine);

            RunPromptActions(currentLine);
            //controller.AssembleAndDisplayCommsText();
        }
    }

    public void NextNoVerbLine(string keyPhrase) {
        if (noVerbPromptTree.ContainsKey(keyPhrase)) {
            currentLine = noVerbPromptTree[keyPhrase];
            MakeIDChanges(currentLine);
            targetAI.RunActionsAndSpeechForLine(currentLine);

            RunPromptActions(currentLine);
            //controller.AssembleAndDisplayCommsText();
        } else {
            //controller.AssembleAndDisplayCommsText();
        }
    }

    public void NextAdviceLine(string keyPhrase) {
        if (advisePromptTree.ContainsKey(keyPhrase)) {
            currentLine = advisePromptTree[keyPhrase];
            MakeIDChanges(currentLine);
            //targetAI.RunActionsAndSpeechForLine(currentLine);

            RunPromptActions(currentLine);
            //controller.AssembleAndDisplayCommsText();
        } else {
            //controller.AssembleAndDisplayCommsText();
        }
    }

    public void NextHumorLine(string keyPhrase) {
        if (humorPromptTree.ContainsKey(keyPhrase)) {
            currentLine = humorPromptTree[keyPhrase];
            MakeIDChanges(currentLine);
            targetAI.RunActionsAndSpeechForLine(currentLine);

            RunPromptActions(currentLine);
            //controller.AssembleAndDisplayCommsText();
        } else {
            //controller.AssembleAndDisplayCommsText();
        }
    }

    public bool IsNoVerbLineInputValid(string keyPhrase) {
        return noVerbPromptTree.ContainsKey(keyPhrase);
    }

    public bool IsAdviseLineInputValid(string keyPhrase) {
        return advisePromptTree.ContainsKey(keyPhrase);
    }

    public bool IsHumorLineInputValid(string keyPhrase) {
        return humorPromptTree.ContainsKey(keyPhrase);
    }

    private void MakeIDChanges(Prompt line) {
        ident.KnowName = line.revealsName ? true : ident.KnowName;
        ident.KnowActName = line.revealsActName ? true : ident.KnowActName;
        ident.KnowAvatarName = line.revealsAvName ? true : ident.KnowAvatarName;
        ident.KnowDescription = line.revealsDescription ? true : ident.KnowDescription;
        ident.KnowStatus = line.revealsStatus ? true : ident.KnowStatus;

        if (line.makeIDTextFieldChanges) {
            ident.ObjName = (line.nameChange == "") ? ident.ObjName : line.nameChange;
            ident.ObjActualName = (line.actNameChange == "") ? ident.ObjActualName : line.actNameChange;
            ident.ObjAvatarName = (line.avNameChange == "") ? ident.ObjAvatarName : line.avNameChange;
            ident.ObjDescription = (line.descriptionChange == "") ? ident.ObjDescription : line.descriptionChange;
            ident.ObjStatus = (line.statusChange == "") ? ident.ObjStatus : line.statusChange;
        }
    }

    public void ClearDictionary() {
        noVerbPromptTree.Clear();
        advisePromptTree.Clear();
        humorPromptTree.Clear();
    }

    private void RunPromptActions(Prompt line) {
        foreach (Action act in line.triggeredActions) {
            actCoord.TriggerAction(act);
        }
    }
}
