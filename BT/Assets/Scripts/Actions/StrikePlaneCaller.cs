using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikePlaneCaller : MonoBehaviour
{
    public List<Light> lights;

    [SerializeField]
    private Vector3 strikePlaneLocationStartRelativeToTed, strikeLocationRelativeToTed, strikeDefaultAreaOfEffect;
    [SerializeField]
    private float strikePlaneSpeed, planarProximityToTedBeforeActualStrike, strikeDuration;
    private float strikeDurationRefTime;

    private Vector3 tedsLastPosition;

    [SerializeField]
    private SkinnedMeshRenderer mesh;
    private Collider areaOfEffect;
    private Animator anim;
    [SerializeField]
    private AudioSource engineAudio, scramblingAudio;
    private Teddy ted;

    [SerializeField]
    private AudioClip strikePlaneFlyingNoise, scramblingNoise;

    private bool isAvailable, isScrambling;

    private enum StrikeMode { Idle, Striking }
    private StrikeMode StrikeState;

    [SerializeField]
    private string idleAnimationTriggerString, strikeAnimationTriggerString;

    // Start is called before the first frame update
    void Start()
    {
        areaOfEffect = this.transform.GetComponent<Collider>();
        anim = this.transform.GetComponent<Animator>();
        ted = FindObjectOfType<Teddy>();

        foreach (Light lite in lights) { lite.enabled = false; }

        engineAudio.loop = true;
        scramblingAudio.loop = true;
        if (strikePlaneFlyingNoise) { engineAudio.clip = strikePlaneFlyingNoise; }
        if (scramblingNoise) { scramblingAudio.clip = scramblingNoise; }

        StrikeState = StrikeMode.Idle;

        isAvailable = true;
        isScrambling = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (StrikeState) {

            case StrikeMode.Idle:

                mesh.enabled = false;

                break;
            case StrikeMode.Striking:

                mesh.enabled = true;

                if (Time.time - strikeDurationRefTime < strikeDuration) {
                    this.transform.Translate(0, 0, strikePlaneSpeed * Time.deltaTime, Space.Self);

                    if ((this.transform.position - new Vector3(tedsLastPosition.x, this.transform.position.y, tedsLastPosition.z)).magnitude < planarProximityToTedBeforeActualStrike) {
                        if (!isScrambling) { scramblingAudio.Play(); }
                        isScrambling = true;
                    }
                }
                else {
                    Suspend();
                }

                break;
        }
    }

    public void CallStrike() {
        
        this.transform.position = ted.transform.TransformPoint(strikePlaneLocationStartRelativeToTed);
        this.transform.LookAt(new Vector3(ted.transform.position.x, this.transform.position.y - ted.transform.position.y, ted.transform.position.z));

        tedsLastPosition = ted.transform.position;

        isScrambling = false;
        foreach (Light lite in lights) { lite.enabled = true; }
        anim.SetTrigger("Move forward");
        engineAudio.Play();
        scramblingAudio.Stop();
        strikeDurationRefTime = Time.time;
        StrikeState = StrikeMode.Striking;
    }

    public void Suspend() {
        
        isScrambling = false;
        foreach (Light lite in lights) { lite.enabled = false; }
        engineAudio.Stop();
        scramblingAudio.Stop();
        anim.SetTrigger("Be still");
        StrikeState = StrikeMode.Idle;
    }

    public bool CheckAvailability() {
        return isAvailable;
    }

    private void OnTriggerEnter(Collider other) {
        if (isScrambling) {
            //if (other.transform.Find("ID")) {
            //    if (other.transform.Find("ID").GetComponent<ID>().GetType() == typeof(IDInteractable)) {
            //        IDInteractable interactable = (IDInteractable)other.transform.Find("ID").GetComponent<ID>();

            //        if (interactable.canBeScrambled) {
            //            interactable.ScrambleObject();

            //            if (!scrambledObjects.Contains(interactable)) { scrambledObjects.Add(interactable); }
            //        }
            //    }
            //}

            if (other.transform.GetComponent<ObjectScrambling>()) {
                Debug.Log("should be scrambling " + other.transform.name);

                ObjectScrambling scrambleControl = other.transform.GetComponent<ObjectScrambling>();
                scrambleControl.ScrambleObject();
            }
        }
    }
}
