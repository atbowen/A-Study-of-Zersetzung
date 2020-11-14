using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDMoney : ID
{
    public float amount;
    public string owner, issuer;

    public override void Activate() {
        bCam.InitiateUseActionWithAnimationTrigger("Take item", 0.5f, false);
        tedFunds.IncreaseTotalAmount(amount);
        trashCan.DestroyItemAfterTimeDelay(myself.gameObject, 0.3f);
    }

    public override void DisplayID() {
        scanner.EnableCreditPopup(this);
    }
}
