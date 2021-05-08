using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Digest Entry")]
public class DigestEntry : ScriptableObject
{
    [SerializeField]
    private string subjectName;
    [SerializeField, TextArea]
    private string subjectDescription;

    [SerializeField]
    private Texture subjectPic;

    [SerializeField]
    private bool nameIsKnown, descriptionIsKnown, imageIsKnown;

    public DigestScreen.DigestEntryType typeOfDigestEntry;

    public string GetName() {
        if (nameIsKnown)    { return subjectName; }
        else                return null;
    }

    public string GetDescription() {
        if (descriptionIsKnown) { return subjectDescription; }
        else                    return null;
    }

    public Texture GetPic() {
        if (imageIsKnown)   { return subjectPic; }
        else                return null;
    }

    public void ChangeDescription(string newDescription) {
        subjectDescription = newDescription;
    }

    public void MakeNameKnown(bool yesOrNo) {
        nameIsKnown = yesOrNo;
    }

    public void MakeDescriptionKnown(bool yesOrNo) {
        descriptionIsKnown = yesOrNo;
    }

    public void MakeImageKnown(bool yesOrNo) {
        imageIsKnown = yesOrNo;
    }
}
