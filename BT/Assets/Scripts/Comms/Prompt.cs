using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the building unit of comms text

[CreateAssetMenu(menuName = "Terminal/Prompt")]
public class Prompt : ScriptableObject {

    [TextArea]                          
    public string line;                 // Actual prompt text

    public AnimationAudio animatedVoiceLine;

    public bool hasAudio, audioOnly, continues, noVerbType, adviseType, humorType;
    public string keyword;             // Name of this prompt line (is this necessary???) ---> YES.  For cueing lines spoken between NPCs
    public Prompt NPCresponseToThisLine;
    public bool revealsName, revealsActName, revealsAvName, revealsDescription, revealsStatus, makeIDTextFieldChanges, triggersActions;
    public string nameChange, actNameChange, avNameChange, descriptionChange, statusChange;
    
    public Prompt continuation;
    
    [SerializeField]
    public List<Action> triggeredActions;

    // Drop items at line prompt
    public bool dropItem;
    public List<string> itemsToDrop;

    // Look at Ted or other NPC at prompt
    public bool lookAtTed;
    public bool lookAtFriend;
    public bool lookAtCurrentFriend;

    public string nameOfFriendToLookAt;
    public bool matchLookDurationToSpeechTime;                          // If set to true, look duration will be speech line length + response + this look duration
    public float lookDuration;

    // Feed render texture to monitor
    public bool feedMonitor;
    public Texture cameraFeed;

    [SerializeField]
    public Transition[] transitions;    // All possible transitions leading to new prompt

}
