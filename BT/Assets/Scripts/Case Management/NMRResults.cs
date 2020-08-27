using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Actual NMR results to try to match to any of the NMRRefResults listed in the project
[CreateAssetMenu(menuName = "Evidence/NMR Results")]
public class NMRResults : EvidenceData {

    public Texture NMRSpectrum;
    public List<NMRPeak> maxima;

    public override void SetProjectHandlerMode() {
        projHandler = FindObjectOfType<ProjectHandler>();
        projHandler.activeDataType = ProjectHandler.DataType.NMRView;
    }

    public override void UpdateResultsAndNotes() {
        ProjectFile currentProject = projHandler.currentWorkingProject;
        string results;

        results = "Unique carbons: " + maxima.Count + "\n\n";

        if (maxima.Count > 0) {
            for (int i = 0; i < maxima.Count; i++) {
                results = results + "(" + (i + 1) + ")--->  " + maxima[i].shift + " {" + maxima[i].intensity + "}\n";
            }
        } else { results = results + "()---<  EMPTY"; }

        string matchRatingTxt = "Match  Rating:  ";
        string matchInfoTxt = "\nMatches:  ";
        string underMinimumPeakNumTxt = "\nPeak min not met for:  ";
        string notesInfoTxt = "";

        projHandler.activeDataWindow.texture = this.NMRSpectrum;

        foreach (NMRRefResults NMRRef in currentProject.NMRRefSet) {
            if (this.ReturnMatchAccuracyWithReferenceSet(NMRRef) > NMRRef.accuracyRequirement) {
                // Add match info to results text
                matchRatingTxt = matchRatingTxt + "(" + NMRRef.sampleName + ")-" +
                                    this.ReturnMatchRatingWithReferenceSet(NMRRef) + ";  ";
                matchInfoTxt = matchInfoTxt + NMRRef.matchNotes + ";  ";
                if (this.maxima.Count < NMRRef.minNumOfPeakMatchesRequired) {
                    underMinimumPeakNumTxt = underMinimumPeakNumTxt + NMRRef.underPeakNumMinimumRefNotes + ";  ";
                }
                if (NMRRef.refNotes != "") { notesInfoTxt = notesInfoTxt + NMRRef.refNotes + "\n"; }

                bool refSpectrumAlreadyInOverlayList = false;
                foreach (RawImage img in projHandler.NMROverlays) {
                    if (img.texture.name == NMRRef.NMRRefOverlay.name) { refSpectrumAlreadyInOverlayList = true; }
                }

                bool refSpectrumAdded = false;
                if (!refSpectrumAlreadyInOverlayList) {
                    foreach (RawImage img in projHandler.NMROverlays) {
                        if (img.texture.name == "blank texture" && !refSpectrumAdded) {
                            img.texture = NMRRef.NMRRefOverlay;
                            refSpectrumAdded = true;
                        }
                    }
                }
            }
            else {
                foreach (RawImage img in projHandler.NMROverlays) {
                    if (img.texture.name == NMRRef.NMRRefOverlay.name) { img.texture = projHandler.blankTexture; }
                }
            }
        }

        projHandler.resultsTxt.text = results + "\n\n" + matchInfoTxt + "\n" + underMinimumPeakNumTxt;
        projHandler.notesTxt.text = notesInfoTxt;
    }

    public float ReturnMatchRatingWithReferenceSet(NMRRefResults otherSet) {
        return ReturnMatchAccuracyWithReferenceSet(otherSet) - otherSet.accuracyRequirement;
    }

    public float ReturnMatchAccuracyWithReferenceSet(NMRRefResults otherSet) {
        int numOfMatches = 0;
        float peakMatchAccuracyTotal = 0;

        foreach (NMRPeak peak in maxima) {
            foreach (NMRPeak refPeak in otherSet.maxima) {
                if (Mathf.Abs(peak.shift - refPeak.shift) < otherSet.shiftTolerance) {
                    numOfMatches++;
                    peakMatchAccuracyTotal += peak.intensity / refPeak.intensity;
                }
            }
        }

        return peakMatchAccuracyTotal / numOfMatches;
    }
}