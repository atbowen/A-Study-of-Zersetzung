using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDBurnerMessage : ID
{
    public string nameOfCollection = "Unknown";

    public bool flashStatus = true;
    public bool makeDefaultSound = true;

    public float popupDelay = 0.5f;

    [SerializeField]
    private string statusMessageToFlash;

    public List<EMail> messages;

    public EvidenceData[] piecesOfEvidence;

    public List<Action> triggersActions;

    public override void DisplayID() {
        scanner.EnableMessagePickupWithID(this);
    }

    public override void DisplayID(IDCharacter charID) {

    }

    public override void Activate() {
        bCam.InitiateUseActionWithAnimationTrigger("Take item", 0.5f, false);

        foreach (EMail mail in messages) {
            mailManager.AddEMail(mail);
        }

        if (flashStatus && messages.Count > 0) {
            if (messages.Count > 1) { statusMessageToFlash = ("Rec'd  " + nameOfCollection + "  bundle(" + messages.Count + ")."); }
            else                    { statusMessageToFlash = "Rec'd  message  from  " + messages[0].senderInfo + "."; }

            if (makeDefaultSound)   { statusWindow.FlashStatusAndPlayMessagePickupSound(statusMessageToFlash, popupDelay); }
            else                    { statusWindow.FlashStatusText(statusMessageToFlash, popupDelay); }
        }

        trashCan.DestroyItemAfterTimeDelay(myself.gameObject, 0.3f);
    }

    public float CalculateCumulativeRisk() {
        float cumulativeRisk = 0;

        if (messages.Count == 1) {
            cumulativeRisk = messages[0].risk;
        }
        if (messages.Count > 1) {
            foreach (EMail mail in messages) {
                cumulativeRisk += mail.risk;
            }

            cumulativeRisk = cumulativeRisk / messages.Count;
        }

        return cumulativeRisk;
    }
}
