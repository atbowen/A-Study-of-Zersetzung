using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentRandomAnimationNoInteraction : MonoBehaviour
{
    public float animationChangeTime, animationChangeTimeDelta, animationSpeed, animationSpeedDelta;
    public List<string> animationTriggerStrings = new List<string>();

    private Animator anim;
    private float animationChangeTimeActual, animationTimerRefTime;
    
    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();

        if (animationTriggerStrings.Count > 0) {
            anim.speed = Random.Range(animationSpeed - animationSpeedDelta, animationSpeed + animationSpeedDelta);
            anim.SetTrigger(animationTriggerStrings[Random.Range(0, animationTriggerStrings.Count - 1)]);
        }

        animationChangeTimeActual = Random.Range(animationChangeTime - animationChangeTimeDelta, animationChangeTime + animationChangeTimeDelta);
        animationTimerRefTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - animationTimerRefTime > animationChangeTimeActual) {
            if (animationTriggerStrings.Count > 0) {
                anim.speed = Random.Range(animationSpeed - animationSpeedDelta, animationSpeed + animationSpeedDelta);
                anim.SetTrigger(animationTriggerStrings[Random.Range(0, animationTriggerStrings.Count)]);
                animationChangeTimeActual = Random.Range(animationChangeTime - animationChangeTimeDelta, animationChangeTime + animationChangeTimeDelta);
            }

            animationTimerRefTime = Time.time;
        }
    }
}
