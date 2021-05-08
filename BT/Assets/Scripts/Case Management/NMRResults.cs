using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Actual NMR results to try to match to any of the NMRRefResults listed in the project
[CreateAssetMenu(menuName = "Evidence/NMR Results")]
public class NMRResults : EvidenceData {

    public List<NMRPeak> maxima;
    public int maxNoise = 2;
    public int defaultNumOfRefsToShow = 3;

    public List<NMRRefResults> matches;

    public List<float> calculatedPeaks;
    public int shiftRange = 75;

    [SerializeField]
    private Texture matrix;


    // Finds potential matches in ProjectHandler NMRRefResults, adds them to matches
    public override void DetermineMatches() {
        ProjectReview projHandler = FindObjectOfType<ProjectReview>();

        matches.Clear();

        if (projHandler.NMRRefs.Count > 0) {

            List<NMRRefResults> acceptableMatches = new List<NMRRefResults>();

            foreach (NMRRefResults NMRRef in projHandler.NMRRefs) {
                if (this.ReturnMatchAccuracyWithReferenceSet(NMRRef) > NMRRef.accuracyRequirement) {
                    acceptableMatches.Add(NMRRef);
                }
            }

            int numOfTopPicksFound = 0;
            float tempMatchRating = 0;
            NMRRefResults tempTopRef = null;

            while ((numOfTopPicksFound < defaultNumOfRefsToShow) && (acceptableMatches.Count > 0)) {
                foreach (NMRRefResults NMRRef in acceptableMatches) {

                    float rating = ReturnMatchRatingWithReferenceSet(NMRRef);

                    if (rating > tempMatchRating) {
                        tempMatchRating = rating;
                        tempTopRef = NMRRef;
                    }
                }

                if (tempTopRef && !matches.Contains(tempTopRef)) { matches.Add(tempTopRef); }

                acceptableMatches.Remove(tempTopRef);
                tempMatchRating = 0;
                tempTopRef = null;
                numOfTopPicksFound++;
            }
        }
    }

    public override string GetResults() {
        ProjectReview projHandler = FindObjectOfType<ProjectReview>();
        ToolsScreen toolStuff = FindObjectOfType<ToolsScreen>();

        string results;

        string headerHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetResultsHeaderColor());
        string bodyHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetResultsBodyColor());
        string shiftHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetShiftValueColor());
        string peakHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetPeakValueColor());
        string sampleCursorHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetSampleCursorColor());

        if (maxima.Count > 0) {

            results = "<color=#" + headerHex + ">Signals  found: " + maxima.Count + "</color>\n";

            for (int i = 0; i < maxima.Count; i++) {
                results = results + "<color=#" + bodyHex + ">" + (i + 1) + ".  (</color><color=#" + shiftHex + ">" + maxima[i].shift + 
                            "</color><color=#" + bodyHex + ">,  </color><color=#" + peakHex + ">" + maxima[i].intensity.ToString("0.#") + "</color><color=#" + bodyHex + ">)</color>\n";
            }
        } else { results = "<color=#" + bodyHex + ">NO  SIGNALS</color>"; }

        string matchRatingTxt = "<color=#" + headerHex + ">Match  Rating:</color>\n";
        //string matchInfoTxt = "\nMatches:  ";
        //string underMinimumPeakNumTxt = "\nPeak min not met for:  ";

        if (matches.Count > 0) {
            for (int i = 0; i < matches.Count; i++) {

                float rating = this.ReturnMatchRatingWithReferenceSet(matches[i]);
                string ratingHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetMatchRatingColor(rating));
                string matchRefHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetMatchRefColor(i));

                // Add match info to results text
                //matchRatingTxt = matchRatingTxt + "(" + NMRRef.sampleAbbName + ") -\t" + this.ReturnMatchRatingWithReferenceSet(NMRRef).ToString("0.#") + ";\n";
                matchRatingTxt = matchRatingTxt + "<color=#" + matchRefHex + ">" + matches[i].sampleAbbName + "    </color><color=#" + ratingHex + ">" + rating.ToString("0.#") + "</color>\n";
                //matchInfoTxt   = matchInfoTxt + NMRRef.matchNotes + ";  ";

                //if (this.maxima.Count < NMRRef.minNumOfPeakMatchesRequired) {
                //    underMinimumPeakNumTxt = underMinimumPeakNumTxt + NMRRef.sampleName + ";  ";
                //}
                
            }
        }

        return results + matchRatingTxt;  //+ underMinimumPeakNumTxt;
    }

    public override string GetResults(int samplePeakIndex) {
        ProjectReview projHandler = FindObjectOfType<ProjectReview>();

        ToolsScreen toolStuff = FindObjectOfType<ToolsScreen>();

        string results;

        string headerHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetResultsHeaderColor());
        string bodyHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetResultsBodyColor());
        string shiftHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetShiftValueColor());
        string peakHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetPeakValueColor());

        if (maxima.Count > 0) {

            results = "<color=#" + headerHex + ">Signals  found: </color><color=#" + bodyHex + ">" + maxima.Count + "</color>\n";

            for (int i = 0; i < maxima.Count; i++) {
                results = results + "<color=#" + bodyHex + ">" + (i + 1) + ".  (</color><color=#" + shiftHex + ">" + maxima[i].shift +
                            "</color><color=#" + bodyHex + ">,  </color><color=#" + peakHex + ">" + maxima[i].intensity.ToString("0.#") + "</color><color=#" + bodyHex + ">)</color>\n";
            }
        }
        else { results = "<color=#" + bodyHex + ">NO  SIGNALS</color>"; }

        return results;
    }

    public string GetMatSciResults(int samplePeakIndex, bool showRatings) {
        ProjectReview projHandler = FindObjectOfType<ProjectReview>();
        ToolsScreen toolStuff = FindObjectOfType<ToolsScreen>();

        string results;

        string headerHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetResultsHeaderColor());
        string bodyHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetResultsBodyColor());
        string shiftHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetShiftValueColor());
        string peakHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetPeakValueColor());
        string sampleCursorHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetSampleCursorColor());

        if (maxima.Count > 0) {

            results = "<color=#" + headerHex + ">Signals  found: " + maxima.Count + "</color>\n";

            for (int i = 0; i < maxima.Count; i++) {
                if (i == samplePeakIndex) {
                    results = results + "<color=#" + sampleCursorHex + "> < </color><color=#" + shiftHex + ">" + maxima[i].shift +
                                "</color><color=#" + sampleCursorHex + ">    </color><color=#" + peakHex + ">" + maxima[i].intensity.ToString("0.#") +
                                "</color><color=#" + sampleCursorHex + ">></color>\n";
                }
                else {
                    results = results + "<color=#" + bodyHex + "> < </color><color=#" + shiftHex + ">" + maxima[i].shift +
                                "</color><color=#" + bodyHex + ">    </color><color=#" + peakHex + ">" + maxima[i].intensity.ToString("0.#") + "</color><color=#" + bodyHex + ">></color>\n";
                }
            }
        }
        else { results = "<color=#" + bodyHex + ">NO  SIGNALS</color>"; }

        string matchRatingTxt = "";

        if (showRatings) {

            matchRatingTxt = "<color=#" + headerHex + ">Match  Rating:</color>\n";

            //string matchInfoTxt = "\nMatches:  ";
            //string underMinimumPeakNumTxt = "\nPeak min not met for:  ";

            if (matches.Count > 0) {
                for (int i = 0; i < matches.Count; i++) {

                    float rating = this.ReturnMatchRatingWithReferenceSet(matches[i]);
                    string ratingHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetMatchRatingColor(rating));
                    string matchRefHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetMatchRefColor(i));

                    // Add match info to results text
                    //matchRatingTxt = matchRatingTxt + "(" + NMRRef.sampleAbbName + ") -\t" + this.ReturnMatchRatingWithReferenceSet(NMRRef).ToString("0.#") + ";\n";
                    matchRatingTxt = matchRatingTxt + "<color=#" + matchRefHex + ">" + matches[i].sampleAbbName + "   </color><color=#" + ratingHex + ">" + rating.ToString("0.#") + "</color>\n";
                    //matchInfoTxt   = matchInfoTxt + NMRRef.matchNotes + ";  ";

                    //if (this.maxima.Count < NMRRef.minNumOfPeakMatchesRequired) {
                    //    underMinimumPeakNumTxt = underMinimumPeakNumTxt + NMRRef.sampleName + ";  ";
                    //}

                }
            }
        }

        return results + matchRatingTxt;  //+ underMinimumPeakNumTxt;
    }

    public override string GetNotes() {

        string notesInfoTxt = "Description:  " + notes + "\n\n";

        if (matches.Count > 0) {

            if (matches[0].refNotes != "") { notesInfoTxt = notesInfoTxt + "Likely:  " + matches[0].refNotes + "\n"; }

            foreach (NMRRefResults NMRRef in matches) {

                if (NMRRef.criticalNote != "") { notesInfoTxt = notesInfoTxt + "+ " + NMRRef.refNotes + "\n"; }

            }
        }

        return notesInfoTxt;
    }

    public override string GetNotes(int referenceIndexToShow) {

        string notesInfoTxt = "Description:  " + notes + "\n\n";

        if (referenceIndexToShow < matches.Count) {
            if (matches[referenceIndexToShow].refNotes != "")       { notesInfoTxt = notesInfoTxt + matches[referenceIndexToShow].refNotes + "\n"; }
            if (matches[referenceIndexToShow].criticalNote != "")   { notesInfoTxt = notesInfoTxt + matches[referenceIndexToShow].criticalNote; }
        }
        
        return notesInfoTxt;
    }

    public string GetPeak(int refIndex, int peakIndex) {
        ToolsScreen toolStuff = FindObjectOfType<ToolsScreen>();
        NMRRefPeak peak = matches[refIndex].maxima[peakIndex];

        string headerHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetResultsHeaderColor());
        string shiftHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetShiftValueColor());
        string peakHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetPeakValueColor());
        string matchRefHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetMatchRefColor(refIndex));
        string refCursorHex = ColorUtility.ToHtmlStringRGBA(toolStuff.GetRefCursorColor());

        return "\n<color=#" + matchRefHex + ">" + matches[refIndex].sampleName + "</color><color=#" + headerHex + ">:</color>\n" +
                    "<color=#" + refCursorHex + "> < </color><color=#" + shiftHex + ">" + peak.shift + "</color>    <color=#" +
                    peakHex + ">" + peak.intensity.ToString("0.#") + "</color><color=#" + refCursorHex + ">" + "></color>\n" +
                    "<color=#" + refCursorHex + ">" + peak.speciesName + "</color>"; 
    }

    public float ReturnMatchRatingWithReferenceSet(NMRRefResults otherSet) {
        return ReturnMatchAccuracyWithReferenceSet(otherSet) - otherSet.accuracyRequirement;
    }

    public float ReturnMatchAccuracyWithReferenceSet(NMRRefResults otherSet) {
        int numOfMatches = 0;
        float peakMatchAccuracyTotal = 0;

        foreach (NMRPeak peak in maxima) {
            foreach (NMRRefPeak refPeak in otherSet.maxima) {
                if (Mathf.Abs(peak.shift - refPeak.shift) < otherSet.shiftTolerance) {
                    numOfMatches++;
                    float acc = 1 - Mathf.Abs(1 - peak.intensity / refPeak.intensity);
                    peakMatchAccuracyTotal += acc;
                }
            }
        }

        return (peakMatchAccuracyTotal * 100.0f / numOfMatches);
    }

    public bool PeakDataHasBeenCalculated() {
        if (calculatedPeaks.Count == shiftRange)    { return true; }
        else                                        { return false; }
    }

    public void SetCalculatedPeaks(List<float> peaks) {
        calculatedPeaks = peaks;
    }

    public List<float> GetCalculatedPeaks() {
        return calculatedPeaks;
    }
}