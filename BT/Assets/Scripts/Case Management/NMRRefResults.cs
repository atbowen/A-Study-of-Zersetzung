using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A reference set of NMR data that player NMR data can match to
// Includes spectrum overlay for viewer
[CreateAssetMenu(menuName = "Evidence/NMR Ref Results")]
public class NMRRefResults : ScriptableObject
{
    public string sampleName, sampleAbbName;

    [SerializeField]
    public List<NMRRefPeak> maxima;
    public bool isActual;
    public float shiftTolerance, intensityTolerance;
    public int maxNoise = 1;
    public int minNumOfPeakMatchesRequired = 1;
    public float accuracyRequirement;

    public string refNotes, criticalNote;

    public List<float> calculatedPeaks;
    public int shiftRange = 75;

    public bool PeakDataHasBeenCalculated() {
        if (calculatedPeaks.Count == shiftRange) { return true; }
        else { return false; }
    }

    public void SetCalculatedPeaks(List<float> peaks) {
        calculatedPeaks = peaks;
    }

    public List<float> GetCalculatedPeaks() {
        return calculatedPeaks;
    }
}
