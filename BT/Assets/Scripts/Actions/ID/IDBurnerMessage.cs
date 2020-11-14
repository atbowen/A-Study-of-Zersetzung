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

    public override void Activate() {
        bCam.InitiateUseActionWithAnimationTrigger("Take item", 0.5f, false);

        foreach (EMail mail in messages) {
            Object.FindObjectOfType<MailScreen>().AddEMail(mail);
        }

        if (flashStatus && messages.Count > 0) {
            if (messages.Count > 1) { statusMessageToFlash = ("Received  " + nameOfCollection + "  message  bundle(" + messages.Count + ")."); }
            else                    { statusMessageToFlash = "Received  message  from  " + messages[0].senderInfo + "."; }

            if (makeDefaultSound)   { FindObjectOfType<StatusPopup>().FlashStatusAndPlayMessagePickupSound(statusMessageToFlash, popupDelay); }
            else                    { FindObjectOfType<StatusPopup>().FlashStatusText(statusMessageToFlash, popupDelay); }
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
