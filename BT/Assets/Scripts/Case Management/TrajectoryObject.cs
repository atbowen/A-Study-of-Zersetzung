using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrajectoryObject
{
    public string refObjectName;
    public Transform obj;
    public Vector3 posRelToSceneOrigin, rotRelToSceneOrigin;
}
