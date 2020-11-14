using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Animate")]
public class Animate : Action {
    [SerializeField]
    private string triggerString;
    public string TriggerString { get => triggerString; set => triggerString = value; }

    [SerializeField]
    private string boolString;
    public string BoolString { get => boolString; set => boolString = value; }

    public bool boolStringValue;

    public bool isTedAndSetIdleFirst;

    public override void DoAction() {
        Transform actor = GameObject.Find(actorName).transform;
        if (actor.GetComponent<Animator>() != null) {
            //if (isTedAndSetIdleFirst && actorName == "_Ted") { FindObjectOfType<BodyCam>().SwitchAnimationStateToIdle(); }

            if (triggerString != "") {
                actor.GetComponent<Animator>().SetTrigger(triggerString);
            }
            else if (boolString != "")
            {
                actor.GetComponent<Animator>().SetBool(boolString, boolStringValue);
            }
        }

        if (disablePhysics) {
            if (actor.GetComponent<Collider>() != null) { actor.GetComponent<Collider>().enabled = false; }
            if (actor.GetComponent<Rigidbody>() != null) { actor.GetComponent<Rigidbody>().isKinematic = true; }
        }

        if (disableControl) { FindObjectOfType<BodyCam>().bodyControl = false; }
    }
}
