using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terminal/Advise")]
public class Advise : InputVerbs
{
    public override void RespondToInput(CommsController controller, string[] separatedWords) {
        controller.commsNavigation.NextAdviceLine(separatedWords[1]);
    }

    public override bool IsTheInputValid(CommsController controller, string[] separatedWords) {
        return controller.commsNavigation.IsAdviseLineInputValid(separatedWords[1]);
    }
}
