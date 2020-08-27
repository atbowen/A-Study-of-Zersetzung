using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EMailAttachment : ScriptableObject
{
    public Texture attachmentIcon, attachmentPicture, attachmentCursor;
    //[TextArea]
    public Text attachmentText;

    private bool zoomable, zoomedIn;

    //void OnEnable()
    //{
    //    if (attachmentIcon != null)     { attachmentIcon.enabled = false; }
    //    if (attachmentPicture != null)  { attachmentPicture.enabled = false; }
    //    if (attachmentCursor != null)   { attachmentCursor.enabled = false; }
    //    if (attachmentText != null)     { attachmentText.text = ""; }
    //}

    public void ShowText(bool showOrNo) {
        if (showOrNo) {
            attachmentText.enabled = true;
        }
    }

    public abstract void ControlView();
    public abstract void SendDataToDesk();
    public abstract void ForwardAttachment();
}
