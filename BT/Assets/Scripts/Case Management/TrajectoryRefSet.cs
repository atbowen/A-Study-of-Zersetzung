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

    public List<TrajectoryObject> trajObjects;

    

    public bool hasThreePoints;

    public string comments, notes;

    public void ShowAndOrientTrajectoryObjects(Transform origin) {
        if (trajObjects.Count > 0) {
            foreach (TrajectoryObject obj in trajObjects) {

                if (!obj.obj) {
                    obj.obj = GameObject.Find("Trajectory Reference Objects").transform.Find(obj.refObjectName);
                }

                if (obj.obj.GetComponent<MeshRenderer>()) {
                    obj.obj.transform.GetComponent<MeshRenderer>().enabled = true;
                }

                obj.obj.transform.localPosition = obj.posRelToSceneOrigin;
                obj.obj.transform.localRotation = Quaternion.LookRotation(origin.forward, origin.up) * Quaternion.Euler(obj.rotRelToSceneOrigin);
            }
        }
    }

    public void HideTrajectoryObjects() {
        if (trajObjects.Count > 0) {
            foreach (TrajectoryObject obj in trajObjects) {

                if (!obj.obj) {
                    obj.obj = GameObject.Find("Trajectory Reference Objects").transform.Find(obj.refObjectName);
                }

                if (obj.obj.transform.GetComponent<MeshRenderer>()) {
                    obj.obj.transform.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }
}
