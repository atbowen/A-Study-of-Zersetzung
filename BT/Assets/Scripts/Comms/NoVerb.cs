using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terminal/No Verb")]
public class NoVerb : InputVerbs
{
    public override void RespondToInput(CommsController controller, string[] separatedWords) {
        controller.commsNavigation.NextNoVerbLine(separatedWords[0]);
    }

    public override bool IsTheInputValid(CommsController controller, string[] separatedWords) {
        return controller.commsNavigation.IsNoVerbLineInputValid(separatedWords[0]);
    }
}
