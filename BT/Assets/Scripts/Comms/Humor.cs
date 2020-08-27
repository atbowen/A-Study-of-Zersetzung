using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terminal/Humor")]
public class Humor : InputVerbs
{
    public override void RespondToInput(CommsController controller, string[] separatedWords) {
        controller.commsNavigation.NextHumorLine(separatedWords[1]);
    }

    public override bool IsTheInputValid(CommsController controller, string[] separatedWords) {
        return controller.commsNavigation.IsHumorLineInputValid(separatedWords[1]);
    }
}
