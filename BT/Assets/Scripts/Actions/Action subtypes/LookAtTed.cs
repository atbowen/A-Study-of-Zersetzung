using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Look at Ted")]
public class LookAtTed : Action {
    [SerializeField]
    private bool matchLookDurationToSpeechTime;
    public bool MatchLookDurationToSpeechTime { get => matchLookDurationToSpeechTime; set => matchLookDurationToSpeechTime = value; }
    [SerializeField]
    private float lookDuration;
    public float LookDuration { get => lookDuration; set => lookDuration = value; }

    public override void DoAction() {
        if (GameObject.Find(actorName).transform.GetComponent<NPCIntelligence>() != null) {
            GameObject.Find(actorName).transform.GetComponent<NPCIntelligence>().LookAtPerson(lookDuration, FindObjectOfType<TeddyHead>().transform, matchLookDurationToSpeechTime);
        }
    }
}