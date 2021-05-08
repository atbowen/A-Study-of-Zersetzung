using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectFile
{
    [SerializeField]
    private string projectTitle;
    [SerializeField, TextArea]
    private string projectDescription;
    [SerializeField]
    private Texture mainImage;

    public List<ProjectBulletPoint> bulletPoints;

    public List<CrimeScene> crimeScenes;

    public List<NMRResults> NMRDataActual;
    public List<ImageToAnalyze> imagesToAnalyzeActual;
    public List<TrajectorySet> trajDataActual;

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

    public string GetProjectTitle() {
        return projectTitle;
    }

    public string GetProjectDescription() {
        return projectDescription;
    }

    public Texture GetProjectMainImage() {
        return mainImage;
    }

    public void ChangeProjectTitle(string newTitle) {
        projectTitle = newTitle;
    }

    public void ChangeProjectDescription(string newDescription) {
        projectDescription = newDescription;
    }
}


