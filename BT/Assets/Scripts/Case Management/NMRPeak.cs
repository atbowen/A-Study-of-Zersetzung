using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NMRPeak
{
    [Range(0.0f, 75.0f)]
    public float shift;
    [Range(0.0f, 30.0f)]
    public float intensity;

    // This determines how many instances of the species are created relative to the intensity
    public float numOfEntitiesPerIntensity = 0.5f;

    public bool isMobile, isPhage;
    public float movementSpeedMin, movementSpeedMax, targetDetectionRange;

    public List<float> consumablePeaks;
    public List<Texture> moveFrames, consumptionFrames, deathFrames;
}
