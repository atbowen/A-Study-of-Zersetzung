using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FakeTedAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Rigidbody rigid;
    private Animator anim;
    private Teddy ted;
    private BodyCam tedBody;
    //private List<HumanAI> otherHumans;
    private List<CyclopsAI> cyclops;
    private Chatter speech;
    //private viewCylinder sight;
    private ID badge;
    private CameraMaster camControl;

    private bool activated;

    private Transform currentWaypoint;

    private float initialMotivation, initialCuriosity, initialAlert, initialThreat, initialFear, initialFriendliness, initialHumor;

    private int numOfFriends;

    private float timeStartAttack;
    private float lookTimer, lookTimerStart;
    private bool isLooking;
    private float patrolIdleTimer, patrolIdleTimerStart;
    private bool isPatrolling;

    private bool hitTed;

    private float animResumeSpeed;
    
    public bool contacted;
    public bool conversationStarted;
    public float minTalkDistance;

    public float lookTimerMin, lookTimerMax, lookTimerAnimationDelay;
    public float patrolIdleTimerMin, patrolIdleTimerMax;

    public float fakeTedFadeTime;
    private float refFakeTedFadeTime;
    public float fakeTedMaxAlpha;
    public float fakeTedFadeStep;

    //public Transform viewFinder;

    public List<SkinnedMeshRenderer> renderers;
    public Transform target;
    public Transform[] waypoints;
    public int waypointIndex;
    public Vector3 spawnPointFromTed;
    public Vector3 targetLastKnownLocation;

    public float motivationLevel, motivationThreshold,
                    curiosityLevel, curiosityThreshold,
                    alertLevel, alertThreshold,
                    threatLevel, threatThreshold,
                    fearLevel, fearThreshold,
                    friendlinessLevel, friendlinessThreshold,
                    humorLevel, humorThreshold,
                    statMaxValue;

    public enum State { Deactivated, Idle, Looking, Patrolling, Alerted, Pursuing, Attacking, Evading, Friendly, Talking, WeirdingOut };

    public State behavior;
    public float alertMultiplier;

    public Texture fakeTedPortrait;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        rigid = this.GetComponent<Rigidbody>();
        anim = this.GetComponent<Animator>();
        ted = FindObjectOfType<Teddy>();
        tedBody = FindObjectOfType<BodyCam>();
        //sight = viewFinder.GetComponent<viewCylinder>();
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

        contacted = false;
        conversationStarted = false;

        waypointIndex = Random.Range(0, waypoints.Length - 1);
        targetLastKnownLocation = target.position;

        //alertMultiplier = 10;

        timeStartAttack = -100;

        lookTimerStart = 0;
        lookTimer = Random.Range(lookTimerMin, lookTimerMax) + lookTimerAnimationDelay;
        isLooking = false;

        hitTed = false;

        animResumeSpeed = 0;

        refFakeTedFadeTime = Time.time;
        DeactivateFakeTed();

        behavior = State.Idle;

    }

    // Update is called once per frame
    void Update()
    {
        if (!camControl.gamePaused) {

            if (agent.isStopped) {
                agent.isStopped = false;
                anim.speed = animResumeSpeed;
            }
            rigid.constraints &= ~RigidbodyConstraints.FreezePosition;

            currentWaypoint = waypoints[waypointIndex];


            float distToTarget = Mathf.Abs(Vector3.Distance(this.transform.position, target.position));

            //if (!tedBody.dead && sight.iSeeYou && alertLevel < 100) {
            //    alertLevel += Time.deltaTime * alertMultiplier;
            //}

            switch (behavior) {
                case State.Deactivated:
                    if (Time.time - refFakeTedFadeTime > fakeTedFadeTime) {
                        this.transform.position = ted.transform.TransformPoint(spawnPointFromTed);
                        anim.SetTrigger("end fade");
                        DeactivateFakeTed();
                    } else {
                        foreach (SkinnedMeshRenderer skin in renderers) {
                            if (skin.material.GetColor("_Color").a > 0) {
                                foreach (Material mat in skin.materials) {
                                    Color origColor = mat.GetColor("_Color");
                                    Color newColor = new Color(origColor.r, origColor.g, origColor.b, origColor.a - fakeTedFadeStep);
                                    mat.SetColor("_Color", newColor);
                                }
                            }
                        }
                    }
                    break;
                case State.Idle:
                    CheckLevels();
                    SendStatusToID();

                    this.transform.position = ted.transform.TransformPoint(spawnPointFromTed);

                    if (activated) {
                        if (friendlinessLevel > friendlinessThreshold) {
                            behavior = State.Friendly;
                            Debug.Log(behavior);
                        }

                        if (alertLevel > alertThreshold) {
                            behavior = State.Alerted;
                            Debug.Log(behavior);
                        }

                        if (motivationLevel > motivationThreshold) {
                            behavior = State.Patrolling;
                            Debug.Log(behavior);
                        }

                        if (curiosityLevel > curiosityThreshold && friendlinessLevel < friendlinessThreshold) {
                            behavior = State.Looking;
                        }
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

                    agent.speed = 30;
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

                    //if (!sight.iSeeYou && alertLevel > 0) {
                    //    alertLevel -= Time.deltaTime * (alertMultiplier);
                    //}

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

                    //if (!sight.iSeeYou && threatLevel > 0) {
                    //    threatLevel -= Time.deltaTime * (alertMultiplier);
                    //}

                    contacted = true;

                    if (threatLevel < threatThreshold) {
                        behavior = State.Alerted;

                        target = ted.transform;
                        agent.destination = target.position;

                        contacted = false;
                    }

                    //if (Mathf.Abs(Vector3.Distance(this.transform.position, ted.transform.position)) > 60) {
                    //    agent.speed = 60;
                    //    //anim.speed = 1.2f;
                    //    agent.destination = target.position;
                    //} else if (!tedBody.dead) {

                    //    if (!hitTed && sight.iSeeYou && ((Time.time - timeStartAttack > 0.7) && (Time.time - timeStartAttack < 0.8))) {
                    //        tedBody.health -= 20;
                    //        tedBody.numberOfAttacks += 1;
                    //        hitTed = true;
                    //        Debug.Log("Attacked " + tedBody.numberOfAttacks + " times.  Health: " + tedBody.health + ".  Dead yet???  " + tedBody.dead + ".");
                    //    } else if (Time.time - timeStartAttack > 2) {
                    //        behavior = State.Attacking;
                    //        //anim.speed = 2;
                    //        anim.SetTrigger("attack now");

                    //        timeStartAttack = Time.time;
                    //        hitTed = false;
                    //    }
                    //}
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

    private void SendStatusToID() {
        if (badge != null) {
            if (badge.hacked) {
                badge.ObjStatus = this.behavior.ToString();
            } else {
                badge.ObjStatus = "**DENIED**";
            }
        }
    }

    private void CheckLevels() {
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

    public void ToggleFakeTedAI() {
        if (activated) {
            refFakeTedFadeTime = Time.time;
            behavior = State.Deactivated;
            anim.SetTrigger("fade");
        }
        else { ActivateFakeTed(); }
    }

    public void ActivateFakeTed() {
        foreach (SkinnedMeshRenderer rend in renderers) {
            if (rend.material.GetColor("_Color").a < fakeTedMaxAlpha) {
                foreach (Material mat in rend.materials) {
                    Color origColor = mat.GetColor("_Color");
                    Color newColor = new Color(origColor.r, origColor.g, origColor.b, fakeTedMaxAlpha);
                    mat.SetColor("_Color", newColor);
                }
            }
            rend.enabled = true;
        }
        behavior = State.Idle;

        activated = true;
    }

    public bool IsFakeTedActive() {
        return activated;
    }

    public void DeactivateFakeTed() {
        foreach (SkinnedMeshRenderer rend in renderers) { rend.enabled = false; }
        this.transform.position = ted.transform.position + spawnPointFromTed;
        //target = ted.transform;

        activated = false;
        behavior = State.Idle;
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
}
