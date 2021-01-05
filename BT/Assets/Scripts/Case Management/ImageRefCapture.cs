using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Evidence/ImageRefCapture")]
public class ImageRefCapture : ScriptableObject
{
    // The name that will need to be searched for to retrieve this reference as a match
    public string thingRevealed;

    // Main info, note, and image to reveal if this reference is a match
    [TextArea]
    public string blurbName, blurbOccupation, blurbDescription, shortNote, criticalNote;
    public Texture refImage;
}
