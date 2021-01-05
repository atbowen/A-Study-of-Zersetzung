using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Evidence/ImageToAnalyze")]
public class ImageToAnalyze : EvidenceData
{
    // All of the images of interest contained in this image
    public Texture image;
    public List<ImageCapture> imagesOfInterest;

    // All of the images of interest found
    [SerializeField]
    private List<ImageCapture> imagesFound = new List<ImageCapture>();

    // If a search box is drawn on this image, the center position and area of the box will be compared to all images of interest contained in this image
    // If the comparative position and area are within these limits for an image of interest, the image of interest will be revealed
    [SerializeField]
    private float centerDistanceReq = 0.5f;
    [SerializeField]
    private float areaDiffReq = 5;

    // Running log of result text from all found images of interest
    private string resultsFound;

    // These are garbage character strings to add before and after each image of interest result string
    [SerializeField]
    private string matchPrefix = "::|  ";
    [SerializeField]
    private string matchSuffix = " >>.  ";

    // The color of a highlighted image of interest result string in the running result log
    [SerializeField]
    private Color highlightedColor, blurbNameColor, blurbOccupationColor, blurbDescriptionColor;

    // METHODS //

    // Check if search coordinates and search area match an image of interest
    // If so, check to see if there's a match in the database, and add that imageRefCapture into the image of interest if it exists
    // Return the image of interest
    public ImageCapture SearchForImage(Vector2 topLeft, Vector2 bottomRight) {
        Vector2 center = new Vector2((topLeft.x + bottomRight.x) / 2, (topLeft.y + bottomRight.y) / 2);
        float area = Mathf.Abs((bottomRight.x - topLeft.x) * (topLeft.y - bottomRight.y));

        foreach (ImageCapture img in imagesOfInterest) {
            if ((Mathf.Abs((center - img.GetCenter()).magnitude) < centerDistanceReq) && (Mathf.Abs(area - img.GetArea()) / img.GetArea() < areaDiffReq)) {

                if (!imagesFound.Contains(img)) { imagesFound.Add(img); }

                ImageRefCapture imageRef = SearchForImageRefInDatabase(img);
                if (imageRef) { img.imageRefRevealed = imageRef; }

                return img;
            }
        }

        return null;
    }

    // NOT USED
    public override void DetermineMatches() {

    }

    // Return the found image of interest closest to the cursor
    public ImageCapture SelectImage(Vector2 curPos) {

        List<ImageCapture> possibleSelections = new List<ImageCapture>();

        if (imagesFound.Count > 0) {
            foreach (ImageCapture img in imagesFound) {
                if ((curPos.x < img.bottomRightCorner.x) && (curPos.x > img.topLeftCorner.x) && (curPos.y < img.topLeftCorner.y) && (curPos.y > img.bottomRightCorner.y)) {
                    possibleSelections.Add(img);
                }
            }
        }

        ImageCapture closestImage = null;

        if (possibleSelections.Count > 0) {

            closestImage = possibleSelections[0];
            float closestDistance = Mathf.Abs((curPos - possibleSelections[0].GetCenter()).magnitude);

            foreach (ImageCapture img in possibleSelections) {
                if (Mathf.Abs((curPos - possibleSelections[0].GetCenter()).magnitude) < closestDistance) {
                    closestImage = img;
                    closestDistance = Mathf.Abs((curPos - possibleSelections[0].GetCenter()).magnitude);
                }
            }
        }

        return closestImage;
    }

    private ImageRefCapture SearchForImageRefInDatabase(ImageCapture imgCap) {
        ProjectHandler projHandler = FindObjectOfType<ProjectHandler>();

        if (projHandler.refImagesOfInterest.Count > 0) {
            foreach (ImageRefCapture imgRef in projHandler.refImagesOfInterest) {
                if (imgCap.thingRevealed == imgRef.thingRevealed) { return imgRef; }
            }
        }

        return null;
    }

    public override string GetResults() {

        string tempResults = "";

        // Return the compiled result blurb text
        if (imagesFound.Count > 0) {
            foreach (ImageCapture img in imagesFound) {
                tempResults = tempResults + matchPrefix + img.thingRevealed + matchSuffix;
            }
        }

        resultsFound = tempResults;
        return tempResults;
    }

    public override string GetResults(int referenceIndexToShow) {

        string HLColorCodeTxt = ColorUtility.ToHtmlStringRGBA(blurbNameColor);
        string tempResults = "";

        // Return the compiled result blurb text, but highlighting the indexed blurb
        if ((imagesFound.Count > 0) && (referenceIndexToShow < imagesFound.Count - 1)) {
            for (int i = 0; i < imagesOfInterest.Count; i++) {
                if (i == referenceIndexToShow)  { tempResults = tempResults + matchPrefix + "<color=" + HLColorCodeTxt + ">" + imagesFound[i].thingRevealed + "</color>" + matchSuffix; }
                else                            { tempResults = tempResults + matchPrefix + imagesFound[i].thingRevealed + matchSuffix; }
            }
        }

        return tempResults;
    }

    public string GetResults(ImageCapture imgCap) {

        string HLColorCodeTxt = ColorUtility.ToHtmlStringRGBA(blurbNameColor);
        string tempResults = "";

        // Return the compiled result blurb text, but highlighting the indexed blurb
        if (imagesFound.Count > 0) {
            foreach (ImageCapture img in imagesFound) {
                if (img == imgCap) { tempResults = tempResults + matchPrefix + "<color=#" + HLColorCodeTxt + ">" + img.thingRevealed + "</color>" + matchSuffix; }
                else { tempResults = tempResults + matchPrefix + img.thingRevealed + matchSuffix; }
            }
        }

        return tempResults;
    }

    public string GetBlurb(ImageCapture imgCap) {

        string nameColorHex = ColorUtility.ToHtmlStringRGB(blurbNameColor);
        string occColorHex = ColorUtility.ToHtmlStringRGB(blurbOccupationColor);
        string descColorHex = ColorUtility.ToHtmlStringRGB(blurbDescriptionColor);

        string blurb = "";

        if (imgCap.imageRefRevealed != null) {
            blurb = "<color=#" + nameColorHex + ">" + imgCap.imageRefRevealed.blurbName + "</color>"
                    + "<color=#" + occColorHex + ">" + imgCap.imageRefRevealed.blurbOccupation + "</color>"
                    + "<color=#" + descColorHex + ">" + imgCap.imageRefRevealed.blurbDescription + "</color>";
        }

        return blurb;
    }

    public override string GetNotes() {

        string notesTxt = notes;

        if (imagesFound.Count == 0) { return notesTxt + "\n\n(0)  Subjects  found"; }

        if (imagesFound.Count > 0) {

            if (imagesFound.Count == 1) { notesTxt = notesTxt + "\n\n(1)  Subject  found:\n"; }
            else { notesTxt = notesTxt + "\n\n(" + imagesFound.Count + ")  Subjects  found:\n"; }

            foreach (ImageCapture img in imagesFound) {
                if (img.imageRefRevealed) {
                    notesTxt = notesTxt + img.imageRefRevealed.thingRevealed + img.imageRefRevealed.shortNote + "\n";
                }
            }
        }

        return notesTxt;
    }

    public override string GetNotes(int referenceIndexToShow) {

        string notesTxt = notes + "\n\n";

        if (referenceIndexToShow < imagesFound.Count) {
            ImageCapture img = imagesFound[referenceIndexToShow];

            if (img.imageRefRevealed) {
                notesTxt = notesTxt + img.thingRevealed + "  ==>  " + img.imageRefRevealed.criticalNote;
            }
        }

        return notesTxt;
    }

    public string GetNotes(ImageCapture imgCap) {

        string notesTxt = notes + "\n\n";

        if (imagesOfInterest.Contains(imgCap)) {
            if (imgCap.imageRefRevealed != null) {
                return notesTxt + "Subject:\n" + imgCap.imageRefRevealed.thingRevealed + "\n\nObservation:\n" + imgCap.imageRefRevealed.criticalNote;
            }
        }

        return notesTxt;
    }

    public List<ImageCapture> GetFoundImages() {
        return imagesFound;
    }

    public void ClearFoundImagesList() {
        imagesFound.Clear();
    }
}
