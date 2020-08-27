using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Routine")]
public class ActionRoutine : ScriptableObject
{
    public bool interruptable;
    public List<ActionCue> cues;
}
