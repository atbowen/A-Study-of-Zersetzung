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

    // Finds potential matches in ProjectHandler NMRRefResults, adds them to matches
    public override void DetermineMatches() {
        ProjectHandler projHandler = FindObjectOfType<ProjectHandler>();

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
        ProjectHandler projHandler = FindObjectOfType<ProjectHandler>();

        string results;

        results = "Sample  peaks: " + maxima.Count + "\n";

        if (maxima.Count > 0) {
            for (int i = 0; i < maxima.Count; i++) {
                results = results + "(" + (i + 1) + ")\t\t" + maxima[i].shift + "\t\t{" + maxima[i].intensity + "}\n";
            }
        } else { results = results + "()---<  EMPTY"; }

        string matchRatingTxt = "\nMatch  Rating:\n";
        //string matchInfoTxt = "\nMatches:  ";
        //string underMinimumPeakNumTxt = "\nPeak min not met for:  ";

        if (matches.Count > 0) {
            foreach (NMRRefResults NMRRef in matches) {
                
                // Add match info to results text
                matchRatingTxt = matchRatingTxt + "(" + NMRRef.sampleAbbName + ") -\t" + this.ReturnMatchRatingWithReferenceSet(NMRRef).ToString("0.#") + ";\n";
                //matchInfoTxt   = matchInfoTxt + NMRRef.matchNotes + ";  ";

                //if (this.maxima.Count < NMRRef.minNumOfPeakMatchesRequired) {
                //    underMinimumPeakNumTxt = underMinimumPeakNumTxt + NMRRef.sampleName + ";  ";
                //}
                
            }
        }

        return results + matchRatingTxt;  //+ underMinimumPeakNumTxt;
    }

    public override string GetResults(int refIndexToShow) {
        ProjectHandler projHandler = FindObjectOfType<ProjectHandler>();

        string results;

        results = "Sample  peaks: " + maxima.Count + "\n";

        if (maxima.Count > 0) {
            for (int i = 0; i < maxima.Count; i++) {
                results = results + "(" + (i + 1) + ")--->  " + maxima[i].shift + " {" + maxima[i].intensity + "}\n";
            }
        }
        else { results = results + "()---<  EMPTY"; }

        return results;
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
        NMRRefPeak peak = matches[refIndex].maxima[peakIndex];

        return "\n" + matches[refIndex].sampleName + ":\n(" + (peakIndex + 1) + ")--->  " + peak.shift + " {" + peak.intensity + "}\n" + peak.speciesName; 
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