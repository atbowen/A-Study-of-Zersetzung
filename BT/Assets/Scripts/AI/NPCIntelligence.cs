using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCIntelligence : MonoBehaviour
{
    // States of action
    public enum StateOfBeing { StandingStill, WalkingToPoint, Wandering, Conversing, InteractingWithTed, PerformingSpecialAction}
    public StateOfBeing NPCState;

    // Is this NPC just acting out a routine?
    public bool isActing;

    // Status parameters
    public float statMaxValue;
    public float health, stealthRating;
    public float motivationLevel, motivationThreshold,
                    curiosityLevel, curiosityThreshold,
                    alertLevel, alertThreshold,
                    threatLevel, threatThreshold,
                    fearLevel, fearThreshold,
                    friendlinessLevel, friendlinessThreshold,
                    humorLevel, humorThreshold;
    private float initialMotivation, initialCuriosity, initialAlert, initialThreat, initialFear, initialFriendliness, initialHumor;

    // Components
    private NavMeshAgent navAgent;
    private Animator faceAnim;
    private TeddyHead tedHead;
    private Transform headToLookAt;
    private TextAndSpeech speechController;
    private Inventory inv;
    private ActionSceneCoordinator actCoord;

    // Restrictions
    public bool canBeStoppedByTed, isConversingWithOtherNPC;

    // Locations the NPC can go
    public Transform[] wanderingWaypoints;
    public bool wanderingWaypointIsRandomlyPicked;

    // Friend info
    public List<string> friendNames;
    public NPCIntelligence currentFriend;
    public bool conversationStarted, contactedByFriend;

    // Head, face, eyes, parameters/rates for conversation and other actions
    public Transform head, face;
    public List<Transform> eyes;
    public float blinkTime, secPerBlink, secPerBlinkDelta,
                    eyesTurningTowardRate, eyesTurningAwayRate, headTurningTowardRate, headTurningAwayRate, eyesTurningTowardMaxAngle, headTurningTowardMaxAngle;
    public Vector3 relativePosFromPlayerToDropItem;    

    // Conversation parameters
    private bool isBlinking, lookingAtPerson, turningAwayFromPerson;
    private Quaternion headInitialRotation;
    private List<Quaternion> eyesInitialRotation = new List<Quaternion>();
    private Vector3 headUpDirection, headInitialForwardDirection;
    private List<Vector3> eyesUpDirection = new List<Vector3>();
    private List<Vector3> eyesInitialForwardDirection = new List<Vector3>();
    private float actualSecPerBlink, blinkRefTime, lookingAtPersonDuration, lookingAtPersonDurationRefTime;
    private Transform targetHead;

    // Conversation line cueing
    private TextAndSpeech currentFriendSpeechControl;
    private Prompt currentSpeechLine;
    private float currentSpeechLineRefTime, currentLineDuration;
    private bool speakingAndWaitingToCueOtherNPC;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize components
        faceAnim = face.GetComponent<Animator>();
        speechController = this.transform.GetComponent<TextAndSpeech>();
        tedHead = FindObjectOfType<TeddyHead>();

        if (this.transform.Find("Inventory") != null) {
            inv = this.transform.Find("Inventory").GetComponent<Inventory>();
        }
        actCoord = FindObjectOfType<ActionSceneCoordinator>();

        // Set blinking parameters
        isBlinking = true;
        blinkRefTime = 0;
        actualSecPerBlink = Random.Range(secPerBlink - secPerBlinkDelta, secPerBlink + secPerBlinkDelta);

        // Initialize head and eye rotations and directions
        headInitialRotation = head.transform.rotation;
        headInitialForwardDirection = head.forward;
        headUpDirection = head.up;
        foreach(Transform eye in eyes) {
            eyesInitialRotation.Add(eye.transform.rotation);
        }

        // Setting look parameters
        lookingAtPerson = false;
        lookingAtPersonDuration = 0;
        lookingAtPersonDurationRefTime = 0;
        turningAwayFromPerson = false;

        // Initialize action and interaction states
        speakingAndWaitingToCueOtherNPC = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (conversationStarted && currentFriend != null && currentFriend.transform.GetComponent<TextAndSpeech>() != null) {
            if (!currentFriend.conversationStarted) {
                currentFriendSpeechControl = currentFriend.transform.GetComponent<TextAndSpeech>();

                CommsNPCInitiate();
            }
        }
        if (contactedByFriend) {
            NPCState = StateOfBeing.Conversing;
        }

        switch (NPCState) {
            case StateOfBeing.StandingStill:
                
                break;
            case StateOfBeing.Conversing:
                if (speakingAndWaitingToCueOtherNPC) {
                    if (Time.time - currentSpeechLineRefTime > currentLineDuration) {
                        Debug.Log("cued, " + this.transform.name);
                        Debug.Log(currentFriend.name);

                        currentFriend.SpeechLineCued(currentSpeechLine.NPCresponseToThisLine);
                        speakingAndWaitingToCueOtherNPC = false;
                    }
                }
                break;
        }

        ControlBlinking();
        ControlGaze();
        
    }

    public void CommsNPCInitiate() {
        bool friendLineFound = false;

        foreach (ConversationStarter starterLine in speechController.friendLines) {
            if ((starterLine.friendName == currentFriend.transform.name) && !friendLineFound) {
                currentFriend.currentFriend = this;
                AnimationAudio line = starterLine.friendLine.animatedSpeechLine;
                currentSpeechLine = starterLine.friendLine;
                //speechController.PlayClipAndStartAnimatingFace(line.audioLine, line.startDelayInSec, line.animationCues,
                //                line.mouthPoseOverrideTimeInterval, line.emotionCues, line.feelingOverrideTimeInterval,
                //                line.triggerStringForEndingEmotion);
                //if (currentSpeechLine.triggersActions) {
                //    if (currentSpeechLine.acts.Length > 0) {
                //        for (int i = 0; i < currentSpeechLine.acts.Length; i++) {

                //            actCoord.TriggerAction(currentSpeechLine.acts[i]);
                //        }
                //    }
                //}
                RunActionsAndSpeechForLine(currentSpeechLine);

                currentLineDuration = line.clipLength;
                currentSpeechLineRefTime = Time.time;
                speakingAndWaitingToCueOtherNPC = true;
                currentFriend.contactedByFriend = true;
                friendLineFound = true;
                conversationStarted = false;
                NPCState = StateOfBeing.Conversing;
            }
        }
    }

    public void LookAtPerson(float duration, Transform head, bool matchDuration) {
        //headUpDirection = head.up;
        //headInitialForwardDirection = head.forward;
        eyesUpDirection.Clear();
        for (int i = 0; i < eyes.Count; i++) {eyesUpDirection.Add(eyes[i].up);}
        eyesInitialForwardDirection.Clear();
        for (int i=0; i < eyes.Count; i++) {eyesInitialForwardDirection.Add(eyes[i].forward);}

        targetHead = head;
        lookingAtPerson = true;
        if (matchDuration) {
            lookingAtPersonDuration = currentSpeechLine.animatedSpeechLine.clipLength + duration;
            if (currentSpeechLine.NPCresponseToThisLine != null) {
                lookingAtPersonDuration += currentSpeechLine.NPCresponseToThisLine.animatedSpeechLine.clipLength;
            }
        }
        else { lookingAtPersonDuration = duration; }
        lookingAtPersonDurationRefTime = Time.time;
    }

    public void SpeechLineCued(Prompt response) {
        bool friendLineFound = false;
        foreach (Prompt convoLine in speechController.textLines) {
            if (response == convoLine) {

                AnimationAudio line = convoLine.animatedSpeechLine;
                if (!friendLineFound) {
                    //currentSpeechLine = convoLine;
                    //speechController.PlayClipAndStartAnimatingFace(line.audioLine, line.startDelayInSec, line.animationCues,
                    //                line.mouthPoseOverrideTimeInterval, line.emotionCues, line.feelingOverrideTimeInterval,
                    //                line.triggerStringForEndingEmotion);
                    //if (currentSpeechLine.triggersActions) {
                    //    if (currentSpeechLine.acts.Length > 0) {
                    //        for (int i = 0; i < currentSpeechLine.acts.Length; i++) {

                    //            actCoord.TriggerAction(currentSpeechLine.acts[i]);
                    //        }
                    //    }


                    //}
                    currentSpeechLine = convoLine;
                    RunActionsAndSpeechForLine(currentSpeechLine);

                    currentLineDuration = line.clipLength;
                    currentSpeechLineRefTime = Time.time;
                    NPCState = StateOfBeing.Conversing;
                    speakingAndWaitingToCueOtherNPC = true;
                    friendLineFound = true;
                }
            }
        }
    }

    private void ControlBlinking() {
        if (isBlinking) {
            if (Time.time - blinkRefTime > actualSecPerBlink) {
                faceAnim.SetTrigger("blink");
                actualSecPerBlink = Random.Range(secPerBlink - secPerBlinkDelta, secPerBlink + secPerBlinkDelta);
                blinkRefTime = Time.time;
                isBlinking = false;
            }
        }
        else {
            if (Time.time - blinkRefTime > blinkTime) {
                faceAnim.SetTrigger("no blink");
                blinkRefTime = Time.time;
                isBlinking = true;
            }
        }
    }

    private void ControlGaze() {
        if (lookingAtPerson) {
            // Set incremental rotation of NPC head towards the target head
            Vector3 direction = Vector3.RotateTowards(head.forward, targetHead.transform.position - head.position, headTurningTowardRate * Time.deltaTime, 100);
            // Create list for the NPC's two eyes and set incremental rotation of each eye towards the target head
            List<Vector3> eyeDirections = new List<Vector3>();
            foreach (Transform eye in eyes) {
                eyeDirections.Add(Vector3.RotateTowards(eye.forward, targetHead.transform.position - head.position, eyesTurningTowardRate * Time.deltaTime, 100));
            }

            // For the duration of the gaze time, make the head and eyes look toward the target head
            if (Time.time - lookingAtPersonDurationRefTime < lookingAtPersonDuration) {
                if (Mathf.Abs(Vector3.Angle(head.forward, headInitialForwardDirection)) < headTurningTowardMaxAngle
                    || ((Vector3.Angle(head.forward, headInitialForwardDirection) > 0 && Vector3.Angle(direction, head.forward) < 0)
                        || (Vector3.Angle(head.forward, headInitialForwardDirection) < 0 && Vector3.Angle(direction, head.forward) > 0))) {

                    head.rotation = Quaternion.LookRotation(direction, headUpDirection);
                }

                for (int i = 0; i < eyes.Count; i++) {
                    if (Mathf.Abs(Vector3.Angle(eyes[i].forward, eyesInitialForwardDirection[i])) < eyesTurningTowardMaxAngle
                    || ((Vector3.Angle(eyes[i].forward, eyesInitialForwardDirection[i]) < 0 && Vector3.Angle(eyeDirections[i], eyes[i].forward) > 0)
                        || (Vector3.Angle(eyes[i].forward, eyesInitialForwardDirection[i]) > 0 && Vector3.Angle(eyeDirections[i], eyes[i].forward) < 0))) {

                        head.rotation = Quaternion.LookRotation(direction, headUpDirection);
                        for (int j = 0; j < eyes.Count; j++) {
                            eyes[j].rotation = Quaternion.LookRotation(eyeDirections[j], eyesUpDirection[j]);
                        }
                    }
                }
            // After the gaze time expires, turn back to the initial rotation
            } else {
                lookingAtPerson = false;
                turningAwayFromPerson = true;
            }
        // Likewise, control turning away motion of head and eyes by using incremental rotations
        } else if (turningAwayFromPerson) {
            bool allReturned = true;

            if (Mathf.Abs(Vector3.Angle(head.forward, headInitialForwardDirection)) > 0) {
                allReturned = false;
                Vector3 direction = Vector3.RotateTowards(head.forward, headInitialForwardDirection, headTurningAwayRate * Time.deltaTime, 100);
                head.rotation = Quaternion.LookRotation(direction, headUpDirection);
            }
            for (int i = 0; i < eyes.Count; i++) {
                if (Mathf.Abs(Vector3.Angle(eyes[i].forward, eyesInitialForwardDirection[i])) > 0) {
                    allReturned = false;
                    List<Vector3> eyeDirections = new List<Vector3>();
                    for (int j = 0; j < eyes.Count; j++) {
                        eyeDirections.Add(Vector3.RotateTowards(eyes[j].forward, targetHead.transform.position - head.position, eyesTurningTowardRate * Time.deltaTime, 100));
                    }
                    for (int j = 0; j < eyes.Count; j++) {
                        eyes[j].rotation = Quaternion.LookRotation(eyeDirections[j], eyesUpDirection[j]);
                    }
                }
            }

            // Once the head and eyes have returned to their original rotations, they should not be lookingAtPerson, nor turningAwayFromPerson
            if (allReturned == true) {
                turningAwayFromPerson = false;
            }
        }
    }

    public void BeginActingRoutine(ActionScene routine) {

    }

    // Activates speech audio, speech animation, and action triggers for the prompt
    public void RunActionsAndSpeechForLine(Prompt line) {

        if (line.hasAudio) {
            AnimationAudio voiceLine = line.animatedSpeechLine;
            speechController.PlayClipAndStartAnimatingFace(voiceLine);
        }

        if (line.lookAtFriend) {
            if (line.lookAtCurrentFriend && currentFriend != null) {
                LookAtPerson(line.lookDuration, currentFriend.face, line.matchLookDurationToSpeechTime);
            } else {
                bool foundFriendName = false;
                foreach (ConversationStarter speechLine in speechController.friendLines) {
                    if (speechLine.friendName == line.nameOfFriendToLookAt && !foundFriendName) {
                        LookAtPerson(line.lookDuration, GameObject.Find(line.nameOfFriendToLookAt).transform, line.matchLookDurationToSpeechTime);
                    }
                }
            }
        }
    }

    //public void DropItem(string itemName) {
    //    if (inv != null) {

    //        Transform item = inv.GetTransformByNameAndRemove(itemName);

    //        if (item != null) {
    //            item.position = transform.TransformPoint(relativePosFromPlayerToDropItem);
    //            if (item.GetComponent<MeshRenderer>() != null) { item.GetComponent<MeshRenderer>().enabled = true; }
    //            if (item.GetComponent<SkinnedMeshRenderer>() != null) { item.GetComponent<SkinnedMeshRenderer>().enabled = true; }
    //            if (item.GetComponent<Rigidbody>() != null) { item.GetComponent<Rigidbody>().isKinematic = false; }
    //            if (item.GetComponent<Collider>()) { item.GetComponent<Collider>().enabled = true; }

    //            item.parent = null;

    //        }
    //    }
    //}
}
