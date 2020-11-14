using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Action Scene")]
public class ActionScene : ScriptableObject {
    public List<ParallelActions> parallelActs;
    public ParallelActions activeParallelActs;

    public string sceneName;
    public bool active, interruptable;
    public int priority;

    private int currentIndex;
    private float refTime;

    public void InitializeScene() {
        active = true;
        currentIndex = 0;
        activeParallelActs = parallelActs[currentIndex];
        activeParallelActs.active = true;

        foreach (ParallelActions parActs in parallelActs) {
            foreach (Action act in parActs.actions) {
                act.Active = false;
                act.Finished = false;
            }
        }

        refTime = Time.time;
    }

    public void CueNextParallelActions() {
        //for (int i = 0; i < parallelActs.Count; i++) {
        //    ParallelActions parActs = parallelActs[i];
        //    if (parActs == activeParallelActs) {
        //        bool allActionsFinished = true;
        //        foreach (Action act in parActs.actions) {
        //            if (act.Finished == false) { allActionsFinished = false; }
        //        }

        //        if (allActionsFinished && (i < (parallelActs.Count - 1))) {
        //            foreach (Action act in parActs.actions) {
        //                act.Finished = false;
        //                act.Active = false;
        //            }
        //        }
        //    }
        //}

        bool allActionsFinished = true;

        if (active && activeParallelActs != null) {
            foreach (Action act in activeParallelActs.actions) {
                if (!act.Active && !act.Finished) {
                    act.Active = true;
                    act.DoAction();                    
                } else {
                    if (act.GetType() == typeof(AnimationCombo)) {
                        AnimationCombo animCombo = (AnimationCombo)act;
                        animCombo.CueNextAnimation();
                    }
                    if (Time.time - refTime > act.Duration) {
                        act.Active = false;
                        act.Finished = true;
                    }
                }

                if (act.Active) { allActionsFinished = false; }
            }

            if (allActionsFinished) {
                activeParallelActs.active = false;
                if (currentIndex < parallelActs.Count - 1) {
                    currentIndex += 1;
                    activeParallelActs = parallelActs[currentIndex];
                    refTime = Time.time;
                }
                else {
                    active = false;
                }
            }
        }
    }
}
