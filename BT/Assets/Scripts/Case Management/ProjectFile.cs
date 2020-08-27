using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectFile
{
    public string projectName;

    public List<CrimeScene> crimeScenes;
    public List<EvidenceData> piecesOfEvidence;

    public EvidenceData currentEvidence;

    public List<NMRResults> NMRDataActual;
    public List<TrajectorySet> trajDataActual;
    public List<MaterialsID> matSciMatchActual;

    public List<NMRRefResults> NMRRefSet;
    public List<TrajectoryRefSet> trajRefSet;

    public NMRResults currentNMRData;
    public TrajectorySet currentTrajSet;
    public MaterialsID currentMaterialsID;

    public bool openAndActive;
    public string nothingCompleteStatus, infraCompleteStatus, trajCompleteStatus,
                    matSciCompleteStatus, infraAndTrajCompleteStatus, infraAndMatSciCompleteStatus,
                    trajAndMatSciCompleteStatus, allCompleteStatus;

    public void SetMessage(ProjectStatusMessageSet messageSet) {
        nothingCompleteStatus = messageSet.nothingCompleteStatus;
        infraCompleteStatus = messageSet.infraCompleteStatus;
        trajCompleteStatus = messageSet.trajCompleteStatus;
        matSciCompleteStatus = messageSet.matSciCompleteStatus;
        infraAndTrajCompleteStatus = messageSet.infraAndMatSciCompleteStatus;
        infraAndMatSciCompleteStatus = messageSet.infraAndMatSciCompleteStatus;
        trajAndMatSciCompleteStatus = messageSet.trajAndMatSciCompleteStatus;
        allCompleteStatus = messageSet.allCompleteStatus;
    }
}


