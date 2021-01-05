using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDDate : ID
{
    public override void Activate() {

    }

    public override void DisplayID() {
        scanner.EnableInfoPanelWithID(this);
    }

    public override void DisplayID(IDCharacter charID) {

    }
}
