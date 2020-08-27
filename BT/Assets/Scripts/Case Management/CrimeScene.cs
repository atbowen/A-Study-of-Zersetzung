using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Crime scene to investigate
[System.Serializable]
public class CrimeScene {
    public string sceneName;
    public bool isSelected;
    public List<CrimeSceneObject> crimeSceneElements;
    public CrimeSceneObject centerObject;
    public List<Transform> crimeSceneAvailableCameraTransforms;
    public float maxViewCamDistance;
}