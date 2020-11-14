using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CyclopsAI : MonoBehaviour {

    private NavMeshAgent agent;
    private Rigidbody rigid;
    private Animator anim;
    private Teddy ted;
    private BodyCam tedBody;
    private CyclopsAI[] friendAI;
    private Chatter speech;
    private viewCylinder sight;
    private ID badge;
    private CameraMaster camControl;
    
    private Transform currentWaypoint;

    private float initialMotivation, initialCuriosity, initialAlert, initialThreat, initialFear, initialFriendliness, initialHumor;

    private int numOfFriends;

    private float timeStartAttack;
    private float lookTimer, lookTimerStart;
    private float patrolIdleTimer, patrolIdleTimerStart;
    private bool isPatrolling;

    private bool hitTed;

    private float animResumeSpeed;

    public Transform[] friends;    
    public bool contacted;
    public bool conversationStarted;
    public float minTalkDistance;

    public float lookTimerMin, lookTimerMax, lookTimerAnimationDelay;
    public float patrolIdleTimerMin, patrolIdleTimerMax;

    public Transform viewFinder;

    public Transform target;
    public Transform[] waypoints;
    public int waypointIndex;
    public Vector3 targetLastKnownLocation;

    public float    motivationLevel, motivationThreshold,
                    curiosityLevel, curiosityThreshold,
                    alertLevel, alertThreshold,
                    threatLevel, threatThreshold,
                    fearLevel, fearThreshold,
                    friendlinessLevel, friendlinessThreshold,
                    humorLevel, humorThreshold,
                    statMaxValue;

    public enum State { Idle, Looking, Patrolling, Alerted, Pursuing, Attacking, Evading, Friendly, Talking, WeirdingOut };

    public State behavior;
    public float alertMultiplier;

    public bool gotPinched;

    // Start is called before the first frame update
    void Start() {
        agent = this.GetComponent<NavMeshAgent>();
        rigid = this.GetComponent<Rigidbody>();
        anim = this.GetComponent<Animator>();
        ted = FindObjectOfType<Teddy>();
        tedBody = FindObjectOfType<BodyCam>();
        sight = viewFinder.GetComponent<viewCylinder>();
        badge = GetComponentInChildren<ID>();
        camControl = FindObjectOfType<CameraMaster>();

        initialMotivation = motivationLevel;
        initialCuriosity = curiosityLevel;
        initialAlert = alertLevel;
        initialThreat = threatLevel;
        initialFear = fearLevel;
        initialFriendliness = friendlinessLevel;
        initialHumor = humorLevel;

        if (this.transform.Find("Chatter") != null) {
            speech = this.transform.Find("Chatter").GetComponent<Chatter>();
        }

        numOfFriends = friends.Length;
        friendAI = new CyclopsAI[numOfFriends];

        for (int i=0; i < numOfFriends; i++) {
            friendAI[i] = friends[i].GetComponent<CyclopsAI>();
        } 

        contacted = false;
        conversationStarted = false;

        waypointIndex = Random.Range(0, waypoints.Length - 1);
        targetLastKnownLocation = target.position;

        //alertMultiplier = 10;

        timeStartAttack = -100;

        lookTimerStart = 0;
        lookTimer = Random.Range(lookTimerMin, lookTimerMax) + lookTimerAnimationDelay;

        hitTed = false;

        animResumeSpeed = 0;

        behavior = State.Idle;

        gotPinched = false;

        //agent.autoRepath = true;

    }

    // Update is called once per frame
    void Update() {

        //Debug.Log(behavior);

        if (!camControl.gamePaused) {

            if (agent.enabled && agent.isStopped) {
                agent.isStopped = false;
                anim.speed = animResumeSpeed;
            }
            rigid.constraints &= ~RigidbodyConstraints.FreezePosition;

            currentWaypoint = waypoints[waypointIndex];


            float distToTarget = Mathf.Abs(Vector3.Distance(this.transform.position, target.position));

            if (!tedBody.dead && sight.iSeeYou && alertLevel < 100) {
                alertLevel += Time.deltaTime * alertMultiplier;
            }

            if (gotPinched) {
                SetAnimatorToIdle(5, 5, 5, 5, 5, 5, 5);
                agent.enabled = false;
                anim.SetTrigger("captured");
            }

            switch (behavior) {
                case State.Idle:
                    CheckLevels();
                    SendStatusToID();

                    if (agent.enabled) {
                        agent.destination = this.transform.position;
                    }

                    if (friendlinessLevel > friendlinessThreshold) {
                        behavior = State.Friendly;
                    }

                    if (alertLevel > alertThreshold) {
                        behavior = State.Alerted;
                        Debug.Log(behavior);
                    }

                    if (motivationLevel > motivationThreshold) {
                        behavior = State.Patrolling;
                    }

                    

                    if (curiosityLevel > curiosityThreshold && friendlinessLevel < friendlinessThreshold) {
                        behavior = State.Looking;
                    }
                    break;
                case State.Looking:
                    CheckLevels();
                    SendStatusToID();

                    if (Time.time - lookTimerStart > lookTimer) {
                        //isLooking = true;
                        anim.SetBool("look", true);
                        speech.SayLookLine(Random.Range(1f, 1.5f));

                        lookTimerStart = Time.time;
                        lookTimer = Random.Range(lookTimerMin, lookTimerMax) + lookTimerAnimationDelay;
                    } else if (Time.time - lookTimerStart > lookTimerAnimationDelay) {
                        anim.SetBool("look", false);
                    }

                    if (curiosityLevel < curiosityThreshold) {
                        SetAnimatorToIdle(motivationThreshold, curiosityLevel, 0, 0, 0, initialFriendliness, initialHumor);
                    }

                    if (alertLevel > alertThreshold) {

                        Debug.Log(friendlinessLevel + ", " + behavior);
                        if (friendlinessLevel > friendlinessThreshold) {
                            behavior = State.Friendly;
                            Debug.Log(behavior);
                        } else {
                            behavior = State.Alerted;
                        }
                    }
                    break;
                case State.Patrolling:
                    CheckLevels();
                    SendStatusToID();

                    agent.speed = 45;
                    agent.destination = currentWaypoint.position;
                    target = currentWaypoint;

                    if (Mathf.Abs(Vector3.Distance(this.transform.position, target.position)) < 50) {
                        if (waypointIndex < waypoints.Length - 1) {
                            waypointIndex++;
                        } else { waypointIndex = 0; }
                    }

                    if (motivationLevel < motivationThreshold) {
                        behavior = State.Idle;
                    }

                    if (alertLevel > alertThreshold) {
                        behavior = State.Alerted;
                        target = ted.transform;
                        agent.destination = target.position;
                    }
                    break;
                case State.Alerted:
                    CheckLevels();
                    SendStatusToID();

                    agent.speed = 30;

                    if (!tedBody.dead && distToTarget < 100) {
                        behavior = State.Pursuing;
                        threatLevel = threatThreshold + 1;
                        alertLevel = statMaxValue;
                    }

                    if (!sight.iSeeYou && alertLevel > 0) {
                        alertLevel -= Time.deltaTime * (alertMultiplier);
                    }

                    if (alertLevel < alertThreshold) {

                        if (motivationLevel > motivationThreshold) {
                            behavior = State.Patrolling;
                            alertLevel = 0;

                        } else if (motivationLevel < 20) {
                            behavior = State.Looking;
                            alertLevel = 0;
                        }
                    } else if (threatLevel > threatThreshold) {
                        behavior = State.Pursuing;

                        alertLevel = statMaxValue;
                    }
                    break;
                case State.Pursuing:
                    CheckLevels();
                    SendStatusToID();

                    target = ted.transform;

                    if (tedBody.dead) {
                        SetAnimatorToIdle(initialMotivation, initialCuriosity, initialAlert, initialThreat, initialFear, initialFriendliness, initialHumor);
                    }

                    if (!sight.iSeeYou && threatLevel > 0) {
                        threatLevel -= Time.deltaTime * (alertMultiplier);
                    }

                    for (int i = 0; i < numOfFriends; i++) {
                        if ((contacted) && (!friendAI[i].contacted) && Mathf.Abs(Vector3.Distance(this.transform.position, friendAI[i].transform.position)) < 600) {
                            friendAI[i].alertLevel = 100;
                            friendAI[i].motivationLevel = motivationThreshold;
                            friendAI[i].threatLevel = statMaxValue;
                        }
                    }

                    contacted = true;

                    if (threatLevel < threatThreshold) {
                        behavior = State.Alerted;

                        target = ted.transform;
                        agent.destination = target.position;

                        for (int i = 0; i < numOfFriends; i++) {
                            if (((friendAI[i].contacted) && (Mathf.Abs(Vector3.Distance(this.transform.position, target.transform.position)) <
                                (Mathf.Abs(Vector3.Distance(friendAI[i].transform.position, target.transform.position))))) || tedBody.dead) {

                                friendAI[i].SetAnimatorToIdle(initialMotivation, initialCuriosity, initialAlert, initialThreat, initialFear, initialFriendliness, initialHumor);

                                Debug.Log("friend #" + i + " set to idle");

                                friendAI[i].contacted = false;
                            }
                        }

                        contacted = false;
                    }

                    if (Mathf.Abs(Vector3.Distance(this.transform.position, ted.transform.position)) > 60) {
                        agent.speed = 60;
                        //anim.speed = 1.2f;
                        agent.destination = target.position;
                    } else if (!tedBody.dead) {

                        if (!hitTed && sight.iSeeYou && ((Time.time - timeStartAttack > 0.7) && (Time.time - timeStartAttack < 0.8))) {
                            tedBody.health -= 20;
                            tedBody.numberOfAttacks += 1;
                            hitTed = true;
                            Debug.Log("Attacked " + tedBody.numberOfAttacks + " times.  Health: " + tedBody.health + ".  Dead yet???  " + tedBody.dead + ".");
                        } else if (Time.time - timeStartAttack > 2) {
                            behavior = State.Attacking;
                            //anim.speed = 2;
                            anim.SetTrigger("attack now");

                            timeStartAttack = Time.time;
                            hitTed = false;
                        }
                    }
                    break;
                case State.Attacking:
                    CheckLevels();
                    SendStatusToID();

                    anim.SetTrigger("attack complete");
                    behavior = State.Pursuing;

                    break;
                case State.Evading:
                    CheckLevels();
                    SendStatusToID();

                    break;
                case State.Friendly:

                    if (alertLevel > alertThreshold && distToTarget < minTalkDistance) {
                        this.transform.LookAt(ted.transform);
                    }

                    break;
                case State.WeirdingOut:
                    CheckLevels();
                    SendStatusToID();

                    break;

            }
        } else {
            rigid.constraints = RigidbodyConstraints.FreezeAll;
            if (!agent.isStopped) {
                animResumeSpeed = anim.speed;
            }

            agent.isStopped = true;
            anim.speed = 0;
        }
    }

    void CheckLevels() {
        if (motivationLevel >= motivationThreshold) {
            anim.SetBool("lazy", false);
        } else { anim.SetBool("lazy", true); }

        if (curiosityLevel >= curiosityThreshold) {
            anim.SetBool("curious", true);
        } else { anim.SetBool("curious", false); }

        if (alertLevel >= alertThreshold) {
            anim.SetBool("alerted", true);
        } else { anim.SetBool("alerted", false); }

        if (threatLevel >= threatThreshold) {
            anim.SetBool("threatened", true);
        } else { anim.SetBool("threatened", false); }

        if (fearLevel >= fearThreshold) {
            anim.SetBool("scared", true);
        } else { anim.SetBool("scared", false); }

        if (friendlinessLevel >= friendlinessThreshold) {
            anim.SetBool("friendly", true);
        } else { anim.SetBool("friendly", false); }

        if (humorLevel >= humorThreshold) {
            anim.SetBool("being weird", true);
        } else { anim.SetBool("being weird", false); }
    }

    public void SetAnimatorToIdle(float motivation, float curiosity, float alert, float threat, float fear, float friendly, float weird) {
        behavior = State.Idle;

        motivationLevel = Mathf.Clamp(motivation, 0, 100);
        curiosityLevel = Mathf.Clamp(curiosity, 0, 100);
        alertLevel = Mathf.Clamp(alert, 0, 100);
        threatLevel = Mathf.Clamp(threat, 0, 100);
        fearLevel = Mathf.Clamp(fear, 0, 100);
        friendlinessLevel = Mathf.Clamp(friendly, 0, 100);
        humorLevel = Mathf.Clamp(weird, 0, 100);
    }

    public void SetAnimatorToIdleInitialLevels() {
        behavior = State.Idle;

        motivationLevel = initialMotivation;
        curiosityLevel = initialCuriosity;
        alertLevel = initialAlert;
        threatLevel = initialThreat;
        fearLevel = initialFear;
        friendlinessLevel = initialFriendliness;
        humorLevel = initialHumor;
    }

    void SendStatusToID() {
        if (badge != null) {
            if (badge.hacked) {
                badge.ObjStatus = this.behavior.ToString();
            } else {
                badge.ObjStatus = "**DENIED**";
            }
        }
    }

    public void GotPinched() {
        SetAnimatorToIdle(5, 5, 5, 5, 5, 5, 5);
        agent.enabled = false;
        anim.SetTrigger("captured");
    }
}
