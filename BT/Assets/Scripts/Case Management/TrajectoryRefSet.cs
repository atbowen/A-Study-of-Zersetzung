using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrajectoryRefSet : ScriptableObject
{
    public string setLabel;
    public bool isActual;
    public float accuracyRequirement;
    public List<Trajectory> trajectories;
    public Texture trajRefOverlay;

    public string comments, notes;
}
