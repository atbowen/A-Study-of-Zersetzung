using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Evidence/Trajectory Set")]
public class TrajectorySet : EvidenceData
{
    public string setLabel;
    public List<Trajectory> trajectories;
    public Texture trajSetImage;

    public override void SetProjectHandlerMode() {
        projHandler = FindObjectOfType<ProjectHandler>();
        projHandler.activeDataType = ProjectHandler.DataType.TrajView;
    }

    public override void UpdateResultsAndNotes() {
        ProjectFile currentProject = projHandler.currentWorkingProject;
        CrimeScene currentScene = projHandler.currentWorkingCrimeScene;
        string results;

        // Generate trajectory info
        results = "Traj  pts:  " + trajectories.Count + "\n";

        switch (trajectories.Count) {
            case 0:
                results = results + "()---<  EMPTY";
                break;
            default:
                results = results + "(";
                for (int i = 0; i < trajectories.Count; i++) {
                    results = results + trajectories[i].labelStart + "-->" + trajectories[i].labelEnd + ",  ";
                }
                results = results + ")";
                break;
        }

        // Generate match rating and info text, and notes
        string trajMatchRatingTxt = "Match  Rating:  ";
        string trajMatchInfoTxt = "\nMatches:  ";
        string trajNotesTxt = "";

        foreach (RawImage img in projHandler.trajSetOverlays) { img.enabled = true; }

        projHandler.activeDataWindow.texture = projHandler.crimeSceneStageView;
        
        foreach (Trajectory traj in this.trajectories) {
            Vector3 dispFromCenter = traj.transform.position - currentScene.centerObject.obj.position;
            traj.transform.position = currentScene.centerObject.objCopy.position + dispFromCenter;
            //traj.transform.GetComponent<MeshRenderer>().enabled = traj.IsVisibleOnActivation();
            traj.transform.GetComponent<MeshRenderer>().enabled = true;
        }
        foreach (TrajectoryRefSet trajRef in currentProject.trajRefSet) {
            if (this.ReturnMatchAccuracyWithReferenceSet(trajRef) > trajRef.accuracyRequirement) {
                // Add match info to results text
                trajMatchRatingTxt = trajMatchRatingTxt + "(" + trajRef.setLabel + ")-" +
                                    this.ReturnMatchRatingWithReferenceSet(trajRef) + ";  ";
                trajMatchInfoTxt = trajMatchInfoTxt + trajRef.comments + ";  ";
                if (trajRef.notes != "") { trajNotesTxt = trajNotesTxt + trajRef.notes + "\n"; }

                bool refSetAlreadyInOverlayList = false;
                foreach (RawImage img in projHandler.trajSetOverlays) {
                    if (img.texture.name == trajRef.trajRefOverlay.name) { refSetAlreadyInOverlayList = true; }
                }

                bool refSetAdded = false;
                if (!refSetAlreadyInOverlayList) {
                    foreach (RawImage img in projHandler.trajSetOverlays) {
                        if (img.texture.name == "blank texture" && !refSetAdded) {
                            img.texture = trajRef.trajRefOverlay;
                            refSetAdded = true;
                        }
                    }
                }
            }
            else {
                foreach (RawImage img in projHandler.trajSetOverlays) {
                    if (img.texture.name == trajRef.trajRefOverlay.name) { img.texture = projHandler.blankTexture; }
                }
            }
        }

        projHandler.resultsTxt.text = results + "\n\n" + trajMatchRatingTxt + "\n" + trajMatchInfoTxt;
        projHandler.notesTxt.text = trajNotesTxt + "\n" + this.notes;
    }

    public float ReturnMatchRatingWithReferenceSet(TrajectoryRefSet otherSet) {
        return ReturnMatchAccuracyWithReferenceSet(otherSet) - otherSet.accuracyRequirement;
    }

    public float ReturnMatchAccuracyWithReferenceSet(TrajectoryRefSet otherSet) {
        if (trajectories.Count == 0 && 
            trajectories.Count != otherSet.trajectories.Count &&
            !otherSet.isActual) { return 0; }                                       // Make sure set is non-zero, has same # of pts as
        else {                                                                      // actual set, and only actual set is flagged actual
            float accuracyTotal = 0;
            for (int i = 0; i < trajectories.Count; i++) {
                float ptDiffStart = (Mathf.Abs(trajectories[i].trajStart.x - otherSet.trajectories[i].trajStart.x) +
                                Mathf.Abs(trajectories[i].trajStart.y - otherSet.trajectories[i].trajStart.y) +
                                Mathf.Abs(trajectories[i].trajStart.z - otherSet.trajectories[i].trajStart.z)) / 3;
                float ptDiffEnd = (Mathf.Abs(trajectories[i].trajEnd.x - otherSet.trajectories[i].trajEnd.x) +
                                Mathf.Abs(trajectories[i].trajEnd.y - otherSet.trajectories[i].trajEnd.y) +
                                Mathf.Abs(trajectories[i].trajEnd.z - otherSet.trajectories[i].trajEnd.z)) / 3;
                accuracyTotal += (100f - ((ptDiffStart + ptDiffEnd) / 2f));
            }

            return (accuracyTotal / trajectories.Count);
        }
    }
}
