using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBumping : MonoBehaviour {
    public float forceFactorWhenBumped, bumpThreshold, useBumpAmount;

    public bool triggersRigidbodyAction, triggersSimpleAction;

    public enum BumpType { SimpleTranslateNRotate, TriggersAnimations }
    public BumpType bumpMode;

    public AudioClip bumpSound;

    [SerializeField]
    public List<BumpedPart> partsToBump;

    private Rigidbody rigid;
    private Collider coll;
    private Animator anim;
    private AudioSource audioFX;
    private enum BumpState { Still, BumpedAndReacting, Recovering};
    private BumpState bState;
    private bool justCollided, simpleReactionIsActive;
    private float collisionStrength;
    
    // Start is called before the first frame update
    void Start()
    {
        if (triggersRigidbodyAction)   { rigid = this.GetComponent<Rigidbody>(); }
        else                           { rigid = null; }

        coll = this.GetComponent<Collider>();
        anim = this.GetComponent<Animator>();
        audioFX = this.GetComponent<AudioSource>();
        bState = BumpState.Still;

        justCollided = false;
        collisionStrength = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (this.GetComponent<DocumentHandling>() != null) {
        //    if (this.GetComponent<DocumentHandling>().buttonPressed && this.GetComponent<DocumentHandling>().documentBeingHeld) {
        //        this.transform.localRotation = Quaternion.identity;
        //        anim.SetTrigger("dropping");
        //        bState = BumpState.Recovering;
        //    }
        //}

        if (triggersRigidbodyAction && rigid != null) {
            switch (bState) {
                case BumpState.Still:
                    coll.enabled = true;

                    if (justCollided) {
                        coll.enabled = false;
                        GotBumped();

                        //anim.SetTrigger("kicked up");
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
                        //anim.SetTrigger("dropping");
                        bState = BumpState.Recovering;
                    }

                    if (rigid.velocity.magnitude < 20) {
                        this.transform.localRotation = Quaternion.identity;
                        //anim.SetTrigger("at rest");
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
                        //anim.SetTrigger("at rest");
                        bState = BumpState.Still;
                    }
                    break;
            }
        }
        else {
            
            switch (bumpMode) {
                case BumpType.SimpleTranslateNRotate:
                    if (simpleReactionIsActive && partsToBump.Count > 0) {

                        bool allPartsDoneReacting = true;

                        foreach (BumpedPart bPart in partsToBump) {
                            if (bPart.rotates && Time.time - bPart.simpleRotationRefTime < bPart.rotationDuration) {
                                allPartsDoneReacting = false;
                                if (bPart.rotatesAroundOwnOrigin)   { bPart.part.Rotate(bPart.rotationDirection * (1 / ((1 + Time.time - bPart.simpleRotationRefTime) * bPart.rotationDecay)) * Time.deltaTime); }
                                else                                { bPart.part.RotateAround(bPart.rotationOrigin, bPart.rotationDirection, 
                                                                        bPart.rotationInterval * (1 / ((1 + Time.time - bPart.simpleRotationRefTime) * bPart.rotationDecay)) * Time.deltaTime); }
                            }

                            if (bPart.translates && Time.time - bPart.simpleTranslationRefTime < bPart.translationDuration) {
                                bPart.part.Translate(bPart.translationDisplacementInterval * (1 / ((1 + Time.time - bPart.simpleTranslationRefTime) * bPart.translationDecay)) * Time.deltaTime);
                            }
                        }

                        if (allPartsDoneReacting) { simpleReactionIsActive = false; }
                    }
                    break;
                case BumpType.TriggersAnimations:
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
        if (triggersRigidbodyAction && rigid != null) {
            if (bState == BumpState.Recovering) {
                this.transform.localRotation = Quaternion.identity;
                anim.SetTrigger("at rest");
                bState = BumpState.Still;
            }
            else if (bState == BumpState.Still && col.transform.tag == "Player" && col.impulse.magnitude > bumpThreshold) {
                justCollided = true;
                collisionStrength = Mathf.Sqrt(col.impulse.magnitude) / Time.fixedDeltaTime;
                if (bumpSound != null) { audioFX.PlayOneShot(bumpSound); }
            }
        }
        if (triggersSimpleAction) {
            if (col.transform.tag == "Player" && col.impulse.magnitude > bumpThreshold) {
                simpleReactionIsActive = true;
                if (partsToBump.Count > 0) {
                    foreach (BumpedPart bPart in partsToBump) {
                        bPart.simpleRotationRefTime = Time.time;
                        bPart.simpleTranslationRefTime = Time.time;

                        if (bPart.rotationBackOrForthIsRandom && Random.Range(0, 2) == 0) {
                            bPart.rotationDirection = bPart.rotationDirection * -1;
                        }
                    }
                }

                if (bumpSound != null) { audioFX.PlayOneShot(bumpSound); }
            }
        }
    }

    public void BumpIt() {
        if (triggersSimpleAction && rigid != null) {
            if (bState == BumpState.Recovering) {
                this.transform.localRotation = Quaternion.identity;
                anim.SetTrigger("at rest");
                bState = BumpState.Still;
            }
            else if (bState == BumpState.Still) {
                justCollided = true;
                collisionStrength = Mathf.Sqrt(useBumpAmount) / Time.fixedDeltaTime;
                if (bumpSound != null) { audioFX.PlayOneShot(bumpSound); }
            }
        }
        if (triggersSimpleAction) {
            simpleReactionIsActive = true;
            if (partsToBump.Count > 0) {
                foreach (BumpedPart bPart in partsToBump) {
                    bPart.simpleRotationRefTime = Time.time;
                    bPart.simpleTranslationRefTime = Time.time;

                    if (bPart.rotationBackOrForthIsRandom && Random.Range(0, 2) == 0) {
                        bPart.rotationDirection = bPart.rotationDirection * -1;
                    }
                }
            }

            if (bumpSound != null) { audioFX.PlayOneShot(bumpSound); }
        }
    }

    [System.Serializable]
    public class BumpedPart {
        public Transform part;
        public bool rotates, rotatesAroundOwnOrigin, rotationBackOrForthIsRandom, translates;
        public float rotationDuration, translationDuration, simpleRotationRefTime, simpleTranslationRefTime;
        public Vector3 rotationAxis, rotationOrigin, rotationDirection, translationDisplacementInterval;
        public float rotationInterval, rotationDecay, translationDecay;
    }
}
