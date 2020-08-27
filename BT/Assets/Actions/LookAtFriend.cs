using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Look at Friend")]
public class LookAtFriend : Action {
    [SerializeField]
    private bool lookAtCurrentFriend;
    public bool LookAtCurrentFriend { get => lookAtCurrentFriend; set => lookAtCurrentFriend = value; }
    [SerializeField]
    private string nameOfFriendToLookAt;
    public string NameOfFriendToLookAt { get => nameOfFriendToLookAt; set => nameOfFriendToLookAt = value; }
    [SerializeField]
    private bool matchLookDurationToSpeechTime;                          // If set to true, look duration will be speech line length + response + this look duration
    public bool MatchLookDurationToSpeechTime { get => matchLookDurationToSpeechTime; set => matchLookDurationToSpeechTime = value; }
    [SerializeField]
    private float lookDuration;
    public float LookDuration { get => lookDuration; set => lookDuration = value; }

    public override void DoAction() {
        Transform actor = GameObject.Find(actorName).transform;

        if (actor.GetComponent<NPCIntelligence>() != null && actor.Find("TextAndSpeech").GetComponent<TextAndSpeech>() != null) {
            NPCIntelligence aiScript = actor.GetComponent<NPCIntelligence>();
            TextAndSpeech speechControl = actor.Find("TextAndSpeech").GetComponent<TextAndSpeech>();
            if (lookAtCurrentFriend && aiScript.currentFriend != null) {
                aiScript.LookAtPerson(lookDuration, aiScript.currentFriend.face, matchLookDurationToSpeechTime);
            }
            else {
                bool foundFriendName = false;
                foreach (ConversationStarter line in speechControl.friendLines) {
                    if (line.friendName == nameOfFriendToLookAt && !foundFriendName) {
                        aiScript.LookAtPerson(lookDuration, GameObject.Find(nameOfFriendToLookAt).transform, matchLookDurationToSpeechTime);
                    }
                }
            }
        }
    }
}
