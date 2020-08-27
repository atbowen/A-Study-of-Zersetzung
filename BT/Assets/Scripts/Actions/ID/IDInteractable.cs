using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDInteractable : ID
{
    public override void Activate() {
        bCam.InitiateUseActionWithAnimationTrigger("Press button", 1f, false);
        TriggerInteraction();
    }
}
