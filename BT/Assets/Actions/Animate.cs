using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Animate")]
public class Animate : Action {
    [SerializeField]
    private string triggerString;
    public string TriggerString { get => triggerString; set => triggerString = value; }

    public override void DoAction() {
        Transform actor = GameObject.Find(actorName).transform;
        if (actor.GetComponent<Animator>() != null) {
            actor.GetComponent<Animator>().SetTrigger(triggerString);
        }

        if (disablePhysics) {
            if (actor.GetComponent<Collider>() != null) { actor.GetComponent<Collider>().enabled = false; }
            if (actor.GetComponent<Rigidbody>() != null) { actor.GetComponent<Rigidbody>().isKinematic = true; }
        }

        if (disableControl) { FindObjectOfType<BodyCam>().bodyControl = false; }
    }
}
