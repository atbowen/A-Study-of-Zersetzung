using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextInput : MonoBehaviour {

    public InputField inputField;

    private CommsController controller;
    private CommsNavigation commsNav;

    private bool inputIsEmpty, foundResponse;

    void Awake() {

        controller = this.GetComponent<CommsController>();
        commsNav = FindObjectOfType<CommsNavigation>();
        inputField.onEndEdit.AddListener(AcceptStringInput);

        inputIsEmpty = false;
    }

    void AcceptStringInput(string userInput) {
        if (controller.textActivated) {
            userInput = userInput.ToLower();
            controller.AddToLog(userInput);

            char[] delimiterChars = { ' ' };
            string[] separatedInputWords = userInput.Split(delimiterChars);

            if (separatedInputWords.Length == 1) {                                              // If the input is only one word, use the No Verb InputVerb
                InputVerbs actionWord = controller.inputNoVerb;                                 //
                actionWord.RespondToInput(controller, separatedInputWords);
                foundResponse = actionWord.IsTheInputValid(controller, separatedInputWords);
            } else if (separatedInputWords.Length > 1) {                                        // If input is more than one word, try to match the first word with an Input Verb
                for (int i = 0; i < controller.inputVerbs.Length; i++) {                        //
                    InputVerbs actionWord = controller.inputVerbs[i];                           //
                    for (int j = 0; j < actionWord.phraseInput.Length; j++) {                   //
                        if (actionWord.phraseInput[j] == separatedInputWords[0]) {              //
                            actionWord.RespondToInput(controller, separatedInputWords);         //
                            foundResponse = actionWord.IsTheInputValid(controller, separatedInputWords);
                        }
                    }
                }
            }

            if (userInput == "") {
                inputIsEmpty = true;
                foundResponse = true;
            } else {
                inputIsEmpty = false;
            }

            InputComplete();
        }
    }

    void InputComplete() {

        if (inputIsEmpty) {
            if (controller.IsCommsTextUnravelled() && commsNav.currentLine.continues) { commsNav.ContinueLine(commsNav.currentLine); } 
            else { controller.SkipUnravelling(); }
        }
        if (foundResponse) { controller.AssembleAndDisplayCommsText(); }
        //controller.AssembleAndDisplayCommsText();
        inputField.ActivateInputField();
        inputField.text = null;
        Debug.Log(foundResponse);
        Debug.Log(commsNav.currentLine);
    }
}
