using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public float doorSpeed;
    public float colliderSwitchDelay;

    private Animator anim;
    private Collider doorCol;
    private float colliderSwitchDelayRefTime;
    private bool doorIsMoving, doorFinishedMoving;

    private enum State { Open, Closed}
    private State DoorState;

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        doorCol = this.GetComponent<Collider>();
        colliderSwitchDelayRefTime = 0;

        anim.speed = doorSpeed;

        anim.SetBool("open", false);
        doorIsMoving = false;
        doorFinishedMoving = false;

        DoorState = State.Closed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (DoorState) {
            case State.Open:
                if (doorIsMoving) {
                    anim.SetTrigger("closing");
                    doorIsMoving = false;
                    doorFinishedMoving = true;
                    colliderSwitchDelayRefTime = Time.time;
                } else if (doorFinishedMoving) {
                    if (Time.time - colliderSwitchDelayRefTime > colliderSwitchDelay) {
                        anim.SetBool("open", false);
                        doorFinishedMoving = false;
                        doorCol.enabled = true;
                        DoorState = State.Closed;
                    }
                }
                break;
            case State.Closed:
                if (doorIsMoving) {
                    anim.SetTrigger("opening");
                    doorIsMoving = false;
                    doorFinishedMoving = true;
                    colliderSwitchDelayRefTime = Time.time;
                } else if (doorFinishedMoving) {
                    if (Time.time - colliderSwitchDelayRefTime > colliderSwitchDelay) {
                        anim.SetBool("open", true);
                        doorFinishedMoving = false;
                        doorCol.enabled = false;
                        DoorState = State.Open;
                    }
                }
                break;
        }
    }

    public void ActivateDoor() {
        doorIsMoving = true;
    }
}
