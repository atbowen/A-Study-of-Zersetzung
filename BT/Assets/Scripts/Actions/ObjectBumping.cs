using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBumping : MonoBehaviour
{
    public float forceFactorWhenBumped, bumpThreshold;

    private Rigidbody rigid;
    private Collider coll;
    private Animator anim;
    private enum BumpState { Still, BumpedAndReacting, Recovering};
    private BumpState bState;
    private bool justCollided;
    private float collisionStrength;
    
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
        coll = this.GetComponent<Collider>();
        anim = this.GetComponent<Animator>();
        bState = BumpState.Still;

        justCollided = false;
        collisionStrength = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.GetComponent<DocumentHandling>() != null) {
            if (this.GetComponent<DocumentHandling>().buttonPressed && this.GetComponent<DocumentHandling>().documentBeingHeld) {
                this.transform.localRotation = Quaternion.identity;
                anim.SetTrigger("dropping");
                bState = BumpState.Recovering;
            }
        }

        if (GetComponent<Rigidbody>() != null) {
            switch (bState) {
                case BumpState.Still:
                    coll.enabled = true;

                    if (justCollided) {
                        coll.enabled = false;
                        GotBumped();

                        anim.SetTrigger("kicked up");
                        justCollided = false;
                        bState = BumpState.BumpedAndReacting;
                    }
                    break;
                case BumpState.BumpedAndReacting:
                    coll.enabled = false;

                    //if (justCollided) {
                    //    GotBumped();
                    //    anim.SetTrigger("kicked up");
                    //    justCollided = false;
                    //}

                    if (rigid.velocity.y <= 0 || rigid.velocity.magnitude < 40) {
                        this.transform.localRotation = Quaternion.identity;
                        anim.SetTrigger("dropping");
                        bState = BumpState.Recovering;
                    }

                    if (rigid.velocity.magnitude < 20) {
                        this.transform.localRotation = Quaternion.identity;
                        anim.SetTrigger("at rest");
                        bState = BumpState.Still;
                    }
                    break;
                case BumpState.Recovering:
                    coll.enabled = true;

                    //if (justCollided) {
                    //    coll.enabled = false;

                    //    GotBumped();
                    //    anim.SetTrigger("kicked up");
                    //    justCollided = false;
                    //    bState = BumpState.BumpedAndReacting;
                    //}

                    if (rigid.velocity.magnitude < 15) {
                        this.transform.localRotation = Quaternion.identity;
                        anim.SetTrigger("at rest");
                        bState = BumpState.Still;
                    }
                    break;
            }
        }
    }

    private void GotBumped() {
        //rigid.isKinematic = false;

        Vector3 upwardsForce = new Vector3(Random.Range(0, 0.1f), 1, Random.Range(0, 0.1f));
        rigid.AddForce(upwardsForce * forceFactorWhenBumped * collisionStrength);
    }

    private void OnCollisionEnter(Collision col) {
        if (bState == BumpState.Recovering) {
            this.transform.localRotation = Quaternion.identity;
            anim.SetTrigger("at rest");
            bState = BumpState.Still;
        } else if (bState == BumpState.Still && col.transform.tag == "Player" && col.impulse.magnitude > bumpThreshold) {
            justCollided = true;
            collisionStrength = Mathf.Sqrt(col.impulse.magnitude) / Time.fixedDeltaTime;
        }
    }
}
