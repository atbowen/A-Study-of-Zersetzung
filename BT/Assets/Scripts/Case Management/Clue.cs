using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A bundle of information relating to a case
[System.Serializable]
public class Clue
{
    // When the clue is discovered, the information will be passed to the project with this name
    public string projectName;

    // Can this clue kick off the investigation?
    public bool canInitiateInvestigation;

    // Clue types
    public enum TypesOfClues { Traj, NMR, MatID, GeneralInfo }
    public TypesOfClues ClueType;
    public List<TrajectoryRefSet> trajectoryRefClues;
    public List<TrajectorySet> trajectoryClues;
    public List<NMRRefResults> NMRRefClues;
    public List<NMRResults> NMRClues;
    public List<MaterialsID> matIDClues;

    public List<string> info;
}
