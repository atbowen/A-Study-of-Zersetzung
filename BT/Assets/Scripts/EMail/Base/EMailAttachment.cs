using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EMailAttachment
{
    public Texture attachmentPicture;
    [TextArea]
    public string attachmentText;
    public string attachmentShortDescription;

    public List<ImagePointOfInterest> pointsOfInterest;

    public enum AttachmentType { Image, Data }
    public AttachmentType TypeOfAttachment;

    public List<EvidenceData> includedEvidence;

    private bool zoomable, zoomedIn;
}
