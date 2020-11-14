using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BroadcastChannel {
    // Channel info
    public int channelNumber;
    public string channelName;
    public float signalStrength;
    public bool showingEmergencyMessage;

    // Broadcast info
    public List<Broadcast> schedule;
    public Broadcast currentBroadcast;
    public Texture[] offAirScreens;

    // Camera info
    public List<Camera> channelCameras;
    public RenderTexture channelCameraRT;

    public void PlayBroadcast(Broadcast show) {
        show.isPlaying = true;

        //foreach (ActionScene routine in show.routines) {
        //    foreach (Transform actor in show.actors) {
        //        bool actorFound = false;
        //        foreach (ParallelActions cues in routine.parallelActs) {
        //            foreach (Action actionCue in cues.actions) {
        //                if (actorFound == false && actor.name == actionCue.ActorName && actor.GetComponent<NPCIntelligence>() != null) {
        //                    NPCIntelligence actorAI = actor.GetComponent<NPCIntelligence>();
        //                    actorAI.BeginActingRoutine(routine);
        //                }
        //            }

        //            foreach (Prompt line in cues.prompts) {

        //            }
        //        }
        //    }
        //}
    }

    public void ResetBroadcast(Broadcast show) {

    }

    public void ShowOffAirScreens() {

    }

    public void ResetShowAndRescheduleToEnd(Broadcast show) {
        int indexOfShow = 0;
        for (int i = 0; i < schedule.Count; i++) {
            if (schedule[i] == show) { indexOfShow = i; }
        }

        int indexOfPreviousShow = (int)(indexOfShow + schedule.Count + 1) % schedule.Count;
        show.startTime = schedule[indexOfPreviousShow].startTime + schedule[indexOfPreviousShow].duration;
    }
}