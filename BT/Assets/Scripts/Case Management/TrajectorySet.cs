using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TrajectorySets are unlike EvidenceData in that they aren't simply found--the player has to choose actual points in space
// While they are "evidence", it makes no sense to make them ScriptableObjects
[System.Serializable]
public class TrajectorySet
{
    public string setLabel, notes;
    // Trajectory sets will be limited to one or two trajectories
    public List<Trajectory> trajectories;

    public int defaultNumOfRefsToShow;

    public List<TrajectoryRefSet> matches;

    public void DetermineMatches() {
        ProjectHandler projHandler = GameObject.FindObjectOfType<ProjectHandler>();

        matches.Clear();

        if (projHandler.trajRefSets.Count > 0) {

            List<TrajectoryRefSet> acceptableMatches = new List<TrajectoryRefSet>();

            foreach (TrajectoryRefSet trajRef in projHandler.trajRefSets) {
                if (this.ReturnMatchAccuracyWithReferenceSet(trajRef) > trajRef.accuracyRequirement) {
                    acceptableMatches.Add(trajRef);
                }
            }

            int numOfTopPicksFound = 0;
            float tempMatchRating = 0;
            TrajectoryRefSet tempTopRef = null;

            while ((numOfTopPicksFound < defaultNumOfRefsToShow) && (acceptableMatches.Count > 0)) {
                foreach (TrajectoryRefSet trajRef in acceptableMatches) {

                    float rating = ReturnMatchRatingWithReferenceSet(trajRef);

                    if (rating > tempMatchRating) {
                        tempMatchRating = rating;
                        tempTopRef = trajRef;
                    }
                }

                if (tempTopRef && !matches.Contains(tempTopRef)) { matches.Add(tempTopRef); }

                acceptableMatches.Remove(tempTopRef);
                tempTopRef = null;
                numOfTopPicksFound++;
            }
        }
    }

    public string GetResults() {
        ProjectHandler projHandler = GameObject.FindObjectOfType<ProjectHandler>();

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

        //foreach (RawImage img in projHandler.trajSetOverlays) { img.enabled = true; }

        //projHandler.activeDataWindow.texture = projHandler.crimeSceneStageView;

        //foreach (Trajectory traj in this.trajectories) {
        //    Vector3 dispFromCenter = traj.transform.position - currentScene.centerObject.obj.position;
        //    traj.transform.position = currentScene.centerObject.objCopy.position + dispFromCenter;
        //    traj.transform.GetComponent<MeshRenderer>().enabled = traj.IsVisibleOnActivation();
        //    traj.transform.GetComponent<MeshRenderer>().enabled = true;
        //}
        if (matches.Count > 0) {
            foreach (TrajectoryRefSet trajRef in matches) {

                // Add match info to results text
                trajMatchRatingTxt = trajMatchRatingTxt + "(" + trajRef.setLabel + ")-" + this.ReturnMatchRatingWithReferenceSet(trajRef) + ";  ";
                trajMatchInfoTxt = trajMatchInfoTxt + ".  " + trajRef.comments;
            }
        }

        return results + "\n\n" + trajMatchRatingTxt + "\n" + trajMatchInfoTxt;
    }

    public string GetResults(int referenceIndexToShow) {

        ProjectHandler projHandler = GameObject.FindObjectOfType<ProjectHandler>();

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

        //foreach (RawImage img in projHandler.trajSetOverlays) { img.enabled = true; }

        //projHandler.activeDataWindow.texture = projHandler.crimeSceneStageView;

        //foreach (Trajectory traj in this.trajectories) {
        //    Vector3 dispFromCenter = traj.transform.position - currentScene.centerObject.obj.position;
        //    traj.transform.position = currentScene.centerObject.objCopy.position + dispFromCenter;
        //    traj.transform.GetComponent<MeshRenderer>().enabled = traj.IsVisibleOnActivation();
        //    traj.transform.GetComponent<MeshRenderer>().enabled = true;
        //}
        if (referenceIndexToShow < matches.Count) {

            // Add match info to results text
            trajMatchRatingTxt = trajMatchRatingTxt + "(" + matches[referenceIndexToShow].setLabel + ")-" + this.ReturnMatchRatingWithReferenceSet(matches[referenceIndexToShow]) + ";  ";
            trajMatchInfoTxt = trajMatchInfoTxt + ".  " + matches[referenceIndexToShow].comments;
        }

        return results + "\n\n" + trajMatchRatingTxt + "\n" + trajMatchInfoTxt;
    }

    public string GetNotes() {

        string trajNotesTxt = notes;

        if (matches.Count > 0) {
            foreach (TrajectoryRefSet trajRef in matches) {
                if (trajRef.notes != "") { trajNotesTxt = trajNotesTxt + trajRef.notes + "\n"; }
            }
        }

        return trajNotesTxt;
    }

    public string GetNotes(int referenceIndexToShow) {

        string trajNotesTxt = notes;

        if (referenceIndexToShow < matches.Count) {
            if (matches[referenceIndexToShow].notes != "") { trajNotesTxt = trajNotesTxt + matches[referenceIndexToShow].notes + "\n"; }
        }

        return trajNotesTxt;
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
