using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A reference set of NMR data that player NMR data can match to
// Includes spectrum overlay for viewer
[System.Serializable]
public class NMRRefResults : ScriptableObject
{
    public string sampleName;
    public Texture NMRRefOverlay;

    public List<NMRPeak> maxima;
    public bool isActual;
    public float shiftTolerance, intensityTolerance;
    public int minNumOfPeakMatchesRequired;
    public float accuracyRequirement;

    public string matchNotes, refNotes, underPeakNumMinimumRefNotes;
}
