using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class defines a text transition in comms input

[System.Serializable]
public class Transition {

    public string keyString;            // The phrase required to choose this transition
    public string transitDescription;   // The transition text
    public Prompt valuePrompt;        // The next prompt, caused by this transition
}
