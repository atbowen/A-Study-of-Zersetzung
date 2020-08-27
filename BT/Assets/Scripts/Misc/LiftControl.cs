using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftControl : MonoBehaviour
{
    public float gateColliderSwitchDelay;
    public float heightBetweenFloors, liftSpeed;
    public bool liftIsUpInitially;
    public List<Collider> gates;
    public AudioClip moving, stopping, gatesUp, gatesDown;

    private Animator anim;
    private AudioSource audioFX;
    private float colliderSwitchDelayRefTime;
    private bool liftIsMoving, liftJustStartedMoving, exitedLift, gateIsMoving, gateStartedMoving, gateFinishedMoving;
    private Vector3 upPosition;

    private enum Position { UpReady, DownReady, Up, Down}
    private Position LiftState;

    private enum State { Open, Closed }
    private State GateState;

    // Start is called before the first frame update
    void Start()
    {
        anim = this.transform.GetComponent<Animator>();
        audioFX = this.GetComponent<AudioSource>();
        colliderSwitchDelayRefTime = 0;

        anim.SetBool("open", true);
        if (gates.Count > 0) {
            foreach (Collider coll in gates) { coll.enabled = false; }
        }

        liftIsMoving = false;
        liftJustStartedMoving = false;
        exitedLift = false;
        gateIsMoving = false;
        gateStartedMoving = false;
        gateFinishedMoving = false;

        if (liftIsUpInitially) {
            LiftState = Position.UpReady;
            upPosition = this.transform.position;
        } else {
            LiftState = Position.DownReady;
            upPosition = this.transform.position + new Vector3(0, heightBetweenFloors, 0);
        }
        GateState = State.Open;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (LiftState) {
            case Position.UpReady:
                if (liftIsMoving) {
                    if (liftJustStartedMoving) {
                        ActivateGate();
                        audioFX.PlayOneShot(moving);
                        liftJustStartedMoving = false;
                    }
                    if (upPosition.y - this.transform.position.y < heightBetweenFloors) {
                        this.transform.Translate(0, -liftSpeed * Time.deltaTime, 0);
                    } else {
                        liftIsMoving = false;
                        LiftState = Position.Down;
                        //audioFX.PlayOneShot(stopping);
                        ActivateGate();
                    }
                }
                break;
            case Position.DownReady:
                if (liftIsMoving) {
                    if (liftJustStartedMoving) {
                        ActivateGate();
                        audioFX.PlayOneShot(moving);
                        liftJustStartedMoving = false;
                    }
                    if (upPosition.y - this.transform.position.y > 0) {
                        this.transform.Translate(0, liftSpeed * Time.deltaTime, 0);
                    } else {
                        liftIsMoving = false;
                        LiftState = Position.Up;
                        //audioFX.PlayOneShot(stopping);
                        ActivateGate();
                    }
                }
                break;
            case Position.Up:
                if (exitedLift) {
                    exitedLift = false;
                    LiftState = Position.UpReady;
                }
                break;
            case Position.Down:
                if (exitedLift) {
                    exitedLift = false;
                    LiftState = Position.DownReady;
                }
                break;
        }

        switch (GateState) {
            case State.Open:
                if (gateIsMoving) {
                    anim.SetTrigger("closing");
                    gateIsMoving = false;
                    gateFinishedMoving = true;
                    colliderSwitchDelayRefTime = Time.time;
                } else if (gateFinishedMoving) {
                    if (Time.time - colliderSwitchDelayRefTime > gateColliderSwitchDelay) {
                        anim.SetBool("open", false);
                        audioFX.Stop();
                        audioFX.PlayOneShot(gatesUp);
                        gateFinishedMoving = false;
                        if (gates.Count > 0) {
                            foreach (Collider coll in gates) { coll.enabled = true; }
                        }
                        GateState = State.Closed;
                    }
                }
                break;
            case State.Closed:
                if (gateIsMoving) {
                    anim.SetTrigger("opening");
                    gateIsMoving = false;
                    gateFinishedMoving = true;
                    colliderSwitchDelayRefTime = Time.time;
                } else if (gateFinishedMoving) {
                    if (Time.time - colliderSwitchDelayRefTime > gateColliderSwitchDelay) {
                        anim.SetBool("open", true);
                        audioFX.Stop();
                        audioFX.PlayOneShot(gatesDown);
                        gateFinishedMoving = false;
                        if (gates.Count > 0) {
                            foreach (Collider coll in gates) { coll.enabled = false; }
                        }
                        GateState = State.Open;
                    }
                }
                break;
        }
    }

    public void EnteredLiftNowActivate() {
        liftIsMoving = true;
        liftJustStartedMoving = true;
    }

    public void ExitedLiftNowStay() {
        exitedLift = true;
    }

    public void ActivateGate() {
        gateIsMoving = true;
    }
}
