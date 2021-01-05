using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImageCapture
{
    // What are the bounds of this image?
    public Vector2 topLeftCorner, bottomRightCorner;
    
    // Search name for finding match in database
    public string thingRevealed = "";

    // Description and image(s) to reveal in Tools screen
    public string resultRevealed;
    public Texture enlargedImage, otherImage;

    // Relevant database image, if it exists
    public ImageRefCapture imageRefRevealed;    

    // METHODS //

    // These properties are needed to check if a search box is adequately positioned and sized to reveal this image of interest
    public Vector2 GetCenter() {
        return new Vector2((topLeftCorner.x + bottomRightCorner.x) / 2, (topLeftCorner.y + bottomRightCorner.y) / 2);
    }

    public float GetArea() {
        return Mathf.Abs((bottomRightCorner.x - topLeftCorner.x) * (topLeftCorner.y - bottomRightCorner.y));
    }
}
