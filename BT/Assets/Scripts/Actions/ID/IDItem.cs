using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDItem : ID
{
    public override void Activate() {
        bCam.InitiateUseActionWithAnimationTrigger("Press button", 1f, false);
        tedInventory.AddItemToInventory(myself);
        wkDesk.AddItemToViewer(myself);
        statusWindow.FlashStatusText(myself.name + "  was  added  to  inventory.");
    }
}
