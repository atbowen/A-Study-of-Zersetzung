using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Action/AnimationCombo"))]
public class AnimationCombo : Action
{
    public bool randomOrder;
    public float minRandomTime, maxRandomTime;

    public List<string> triggerStrings;

    public string currentAnimation;

    private Animator anim;
    private bool cueing;
    private int currentRandomAnimationIndex;
    private float currentRandomTime, currentRefTime;

    public override void DoAction() {
        if (GameObject.Find(actorName).transform.GetComponent<Animator>() != null) {
            anim = GameObject.Find(actorName).transform.GetComponent<Animator>();
            cueing = true;
        }
    }

    public void CueNextAnimation() {
        if (finished) { anim.StopPlayback(); }

        if (cueing) {
            currentRandomAnimationIndex = Random.Range(0, triggerStrings.Count - 1);

            if (minRandomTime != 0 && maxRandomTime != 0)   { currentRandomTime = Random.Range(minRandomTime, maxRandomTime); }
            else                                            { currentRandomTime = Random.Range(0, 5); }

            currentRefTime = Time.time;
            anim.SetTrigger(triggerStrings[currentRandomAnimationIndex]);
            cueing = false;
        }
        else {
            if (Time.time - currentRefTime > currentRandomTime) { cueing = true; }
        }
    }
}
