using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles investigation of virtual crime scene
[System.Serializable]
public class CrimeSceneViewer {
    public Camera crimeSceneCam;
    public List<CrimeSceneObject> crimeSceneElements;
}