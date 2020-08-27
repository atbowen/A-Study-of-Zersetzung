using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CrimeSceneObject {
    public Transform obj, objCopy;
    public string projectName, sceneName;
    public enum ObjClass { Victim, Suspect, Witness, Evidence }
    public ObjClass objClassification;

    public CrimeSceneObject(Transform crimeObject, ObjClass classification) {
        obj = crimeObject;
        objClassification = classification;
        if (crimeObject.Find("ID").GetComponent<ID>() != null) {
            ID objectID = crimeObject.Find("ID").GetComponent<ID>();

            projectName = objectID.RelevantProjectName;
            sceneName = objectID.RelevantCrimeSceneName;
        }
    }
}