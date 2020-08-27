using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionCue
{
    // Cue name and priority parameters
    public string cueName, actorName, nextCueName;
    public bool started, ended, allowNextCueToOverlap, allowNextCueToInterrupt;

    public List<AccessibilityRequirement> requirements;

    // Type of action to perform
    public enum ActionType { Talking, Animating, Walking, Running, DroppingItem, Idling, Resetting }
    public ActionType Act, ActAtConclusion;

    public Prompt audioLine;
    public string animationTriggerString, animationAtConclusionTriggerString,
                    droppedItemName, targetName;
    public bool holdItem, lookAtTarget;

    public float totalDuration, talkTime, delayBeforeTalk, animationTime, delayBeforeAnimation;

    public List<Vector3> destinations;
}
