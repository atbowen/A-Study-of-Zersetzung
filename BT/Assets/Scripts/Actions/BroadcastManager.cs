using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages the newscast
public class BroadcastManager : MonoBehaviour {

    public List<BroadcastChannel> channels;

    void Start() {
        foreach (BroadcastChannel chann in channels) {
            foreach (Broadcast show in chann.schedule) {
                show.isPlaying = false;
                show.finishedPlaying = false;
            }

            if (chann.currentBroadcast != null) {
                chann.PlayBroadcast(chann.currentBroadcast);
            } else {
                chann.ShowOffAirScreens();
            }
        }
    }

    void Update() {
        foreach (BroadcastChannel chann in channels) {
            CheckScheduleForBroadcastSwitchover(chann);
        }
    }

    public void CancelShowByName(string searchName) {
        foreach (BroadcastChannel chann in channels) {
            foreach (Broadcast show in chann.schedule) {
                if (show.showName == searchName) {
                    if (show.isPlaying) {
                        chann.ShowOffAirScreens();
                    }
                    chann.schedule.Remove(show);
                }
            }
        }
    }

    private void CheckScheduleForBroadcastSwitchover(BroadcastChannel channel) {
        Broadcast cuedShow = null;
        Broadcast currentShow = null;
        bool foundScheduledShow = false;
        bool foundCurrentShow = false;
        foreach (Broadcast show in channel.schedule) {
            if (Time.time > show.startTime && !show.isPlaying && !show.finishedPlaying) {
                cuedShow = show;
                foundScheduledShow = true;
            }
            if (show.isPlaying) {
                currentShow = show;
                foundCurrentShow = true;
            }
        }

        if (foundScheduledShow) {
            if (foundCurrentShow) {
                channel.PlayBroadcast(cuedShow);
                cuedShow.isPlaying = true;
                channel.ResetBroadcast(currentShow);
                currentShow.isPlaying = false;
                currentShow.finishedPlaying = true;
            } else {
                if (Time.time > currentShow.startTime + currentShow.duration) {
                    channel.ShowOffAirScreens();
                }
            }
        }
    }
}