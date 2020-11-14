using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class defines a text transition in comms input

[System.Serializable]
public class Transition {

    public string keyString;            // The phrase required to choose this transition
    [TextArea]
    public string transitDescription;   // Response text
    public int numberOfLines;
    public bool isExitLine;
    public Prompt valuePrompt;        // The next prompt, caused by this transition
}
