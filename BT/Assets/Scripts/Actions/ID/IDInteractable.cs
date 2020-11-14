using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDInteractable : ID
{
    [SerializeField]
    public List<Action> triggeredActions;

    public bool hasObjectBumping;

    public bool isSwitch, isOn, needsAnimationWaitTimesAndHoldStates, switchColliderWithPointerColliders;

    public string tedAnimationTriggerString, tedSwitchOnAnimation, tedSwitchOffAnimation;
    public float tedAnimationTime, tedSwitchOnAnimationTime, tedSwitchOffAnimationTime;
    public bool shouldTedFreezeDuringAnimation, shouldSwitchOnFreeze, shouldSwitchOffFreeze;

    public string triggerStringToActivate, triggerStringToDeactivate, activationAnimClipName, deactivationAnimClipName;
    public bool isThereALightToTurnOn, isThereAnOnOffLightOnTheActivationSwitch, ActivationSwitchLightOnOffInversion;
    public Transform objectWithLightSwitch;
    public Light activationPointSwitchLightOnOff;

    public Animator alternateAnim;
    public List<Collider> collidersToDisableOnActivation, collidersToEnableOnActivation,
                            collidersToDisableOnDeactivation, collidersToEnableOnDeactivation,
                            switchColliders;
    public Collider currentSwitchCollider;
    
    public string interiorVehicleAnimationTrigger;

    private string defaultTedAnimationTriggerString = "Press button";
    private float defaultTedAnimationTime = 1f;

    //private bool usingForFirstTime;
    private float lastTimeHittingSwitch;
    private float currentAnimationClipLength = 0;       // Start this value at 0 to ensure the first use actually works

    public override void DisplayID() {
        scanner.EnableInfoPanelWithID(this);
    }

    public override void Activate() {

        // Decide what animates and what the animation is
        if (alternateAnim != null)  { anim = alternateAnim; }
        else                        { anim = this.transform.parent.GetComponent<Animator>(); }

        if (isSwitch) {
            string animationSwitchOnString, animationSwitchOffString;
            float animationSwitchOnTime, animationSwitchOffTime;

            if (tedSwitchOnAnimation != "") { animationSwitchOnString = tedSwitchOnAnimation; }
            else                            { animationSwitchOnString = defaultTedAnimationTriggerString; }

            if (tedSwitchOffAnimation != "")    { animationSwitchOffString = tedSwitchOffAnimation; }
            else                                { animationSwitchOffString = defaultTedAnimationTriggerString; }

            if (tedSwitchOnAnimationTime != 0)  { animationSwitchOnTime = tedSwitchOnAnimationTime; }
            else                                { animationSwitchOnTime = defaultTedAnimationTime; }

            if (tedSwitchOffAnimationTime != 0) { animationSwitchOffTime = tedSwitchOffAnimationTime; }
            else                                { animationSwitchOffTime = defaultTedAnimationTime; }

            if (!isOn)  { bCam.InitiateUseActionWithAnimationTrigger(animationSwitchOnString, animationSwitchOnTime, shouldSwitchOnFreeze); }
            else        { bCam.InitiateUseActionWithAnimationTrigger(animationSwitchOffString, animationSwitchOffTime, shouldSwitchOffFreeze); }

        }
        else {
            string animationString;
            float animationTimeLength;

            if (tedAnimationTriggerString != "") { animationString = tedAnimationTriggerString; }
            else { animationString = defaultTedAnimationTriggerString; }
            if (tedAnimationTime != 0) { animationTimeLength = tedAnimationTime; }
            else { animationTimeLength = defaultTedAnimationTime; }

            bCam.InitiateUseActionWithAnimationTrigger(animationString, animationTimeLength, shouldTedFreezeDuringAnimation);
        }

        // If the object should react to being used, call BumpIt method from the requisite ObjectBumping script
        if (hasObjectBumping) {
            if (this.transform.parent.GetComponent<ObjectBumping>() != null) {
                this.transform.parent.GetComponent<ObjectBumping>().BumpIt();
            }
        }

        // Run any actions included in the list
        foreach (Action act in triggeredActions) {
            act.DoAction();
        }


        //if (usingForFirstTime) {
        //    lastTimeHittingSwitch = -100;
        //}

        //usingForFirstTime = false;

        if (isSwitch) {        

            if (Time.time - lastTimeHittingSwitch > currentAnimationClipLength) {
                if (!isOn) {

                    if (collidersToDisableOnActivation.Count > 0) {
                        foreach (Collider col in collidersToDisableOnActivation) { col.enabled = false; }
                    }

                    if (collidersToEnableOnActivation.Count > 0) {
                        foreach (Collider col in collidersToEnableOnActivation) { col.enabled = true; }
                    }

                    anim.SetTrigger(triggerStringToActivate);
                    if (isThereALightToTurnOn && objectWithLightSwitch != null) {
                        objectWithLightSwitch.GetComponent<SwitchLight>().lightOn = false;
                    }
                    if (isThereAnOnOffLightOnTheActivationSwitch && activationPointSwitchLightOnOff != null) {
                        if (!ActivationSwitchLightOnOffInversion) {
                            activationPointSwitchLightOnOff.intensity = 0;
                        }
                        else {
                            activationPointSwitchLightOnOff.intensity = 1;
                        }
                    }

                    currentAnimationClipLength = ReturnClipLengthFromClipName(activationAnimClipName);

                    lastTimeHittingSwitch = Time.time;

                    isOn = true;                   
                }
                else {

                    if (collidersToDisableOnDeactivation.Count > 0) {
                        foreach (Collider col in collidersToDisableOnDeactivation) { col.enabled = false; }
                    }

                    if (collidersToEnableOnDeactivation.Count > 0) {
                        foreach (Collider col in collidersToEnableOnDeactivation) { col.enabled = true; }
                    }

                    anim.SetTrigger(triggerStringToDeactivate);
                    if (isThereALightToTurnOn && objectWithLightSwitch != null) {
                        objectWithLightSwitch.GetComponent<SwitchLight>().lightOn = true;
                    }
                    if (isThereAnOnOffLightOnTheActivationSwitch && activationPointSwitchLightOnOff != null) {
                        if (!ActivationSwitchLightOnOffInversion) {
                            activationPointSwitchLightOnOff.intensity = 1;
                        }
                        else {
                            activationPointSwitchLightOnOff.intensity = 0;
                        }
                    }

                    currentAnimationClipLength = ReturnClipLengthFromClipName(deactivationAnimClipName);

                    lastTimeHittingSwitch = Time.time;

                    isOn = false;
                }
            }
        }
    }

    public void AddColliderToSwitchCollidersList(Collider col) {
        switchColliderWithPointerColliders = true;
        switchColliders.Add(col);
    }

    // This determines the run length of an Animation clip from its name (and takes for granted that we have defined what the AnimationController is)
    private float ReturnClipLengthFromClipName(string clipName) {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips) {
            if (clip.name == clipName) {
                return clip.length;
            }
        }

        return 0;
    }
}
