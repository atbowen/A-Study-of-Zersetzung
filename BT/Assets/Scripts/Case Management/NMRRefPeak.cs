using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NMRRefPeak
{
    public string speciesName;

    [Range(0.0f, 75.0f)]
    public float shift;
    [Range(0.0f, 30.0f)]
    public float intensity;

    public Texture speciesRefImage;
}
