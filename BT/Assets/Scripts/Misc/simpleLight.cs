using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class simpleLight
{
    public Light lightObject;
    public bool flicker, flash;
    public float flickerRandOn, flickerRandOff, flashOn, flashOff, intensityLow, intensityHigh, lightOrigIntensity,
                 flickerTimerOn, flickerTimerOff, flickerTimerRef, flashTimerRef;

}
