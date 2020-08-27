using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Broadcast : ScriptableObject {
    // Show info
    public string showName;
    public bool isEmergencyMessage, isPlaying, finishedPlaying;

    // Set and actors
    public Transform studioSet;
    public List<Transform> actors;

    // Commercial info
    public bool hasCommercials;
    public List<float> commercialTimes;


    public float startTime, duration;

    // 
    public List<ActionScene> routines;
}