using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Send Email")]
public class SendEmail : Action {
    [SerializeField]
    private EMail emailMessage;
    public EMail EmailMessage { get => emailMessage; set => emailMessage = value; }

    public override void DoAction() {
        Object.FindObjectOfType<MailScreen>().AddEMail(emailMessage);

        if (flashStatus) {
            statusMessageToFlash = ("Received  message  from  " + emailMessage.senderInfo + ".\nRE:  " + emailMessage.body);
        }
    }
}