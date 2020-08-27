using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AnimationAudio
{
    [TextArea]
    public string animationCues, emotionCues, subtitle;
    public string triggerStringForEndingEmotion;
    public AudioClip audioLine;
    public float clipLength, startDelayInSec;
    public float mouthPoseOverrideTimeInterval, feelingOverrideTimeInterval;
}
