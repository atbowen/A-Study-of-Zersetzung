using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Broadcast")]
public class Broadcast : ScriptableObject {
    // Show info
    public string showName;
    public bool isEmergencyMessage, isPlaying, finishedPlaying;

    // Set and actors
    public string setName;
    public string[] actorNames;
    [HideInInspector]
    public List<Transform> actors;

    // Cameras
    public List<Camera> channelCameras;
    public Camera currentOperatingCamera;

    // Commercial info
    public bool hasCommercials;
    public List<float> commercialTimes;


    public float startTime, duration;

    // 
    public List<ActionScene> routines;
}