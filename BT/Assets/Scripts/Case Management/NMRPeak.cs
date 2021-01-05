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
}
