using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputVerbs : ScriptableObject
{
    public string[] phraseInput;

    public abstract void RespondToInput(CommsController controller, string[] separatedWords);
    public abstract bool IsTheInputValid(CommsController controller, string[] separatedWords);
}
