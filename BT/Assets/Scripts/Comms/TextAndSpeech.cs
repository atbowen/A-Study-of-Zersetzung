using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAndSpeech : MonoBehaviour
{

    // This is for cataloguing this entities lines of text for comms interactivity
    public Prompt openingTextLine;
    public Prompt[] textLines;
    [SerializeField]
    public List<ConversationStarter> friendLines;
    public Transform face;
    public Vector3 faceOffsetToLookAt;
    public string triggerStringForRestingMouthShape, triggerStringForRestingEmotion, endingEmotion;
    public float mouthShapeDefaultTimeInterval, emotionDefaultTimeInterval;

    public bool active;

    private AudioSource speechAudio;
    private AudioClip currentClip;
    private Animator faceAnim;
    private bool faceIsAnimating, readyToPlayAudio;
    private float faceAnimationStartRefTime, faceAnimationStartDelay, mouthShapeTimeRef, mouthShapeTimeIntervalActual, emotionTimeRef, emotionTimeIntervalActual;
    private List<char> mouthShapeCode = new List<char>();
    private List<char> emotionCode = new List<char>();
    private string previousMouthShapeCharPair;
    private char previousEmotionChar;
    private int currentMouthShapeCharIndex, currentEmotionCharIndex;

    void Start() {
        speechAudio = this.GetComponent<AudioSource>();
        if (face != null) { faceAnim = face.GetComponent<Animator>(); }

        readyToPlayAudio = false;

        active = true;
        mouthShapeTimeIntervalActual = mouthShapeDefaultTimeInterval;
        mouthShapeTimeRef = 0;
        emotionTimeIntervalActual = emotionDefaultTimeInterval;
        emotionTimeRef = 0;
    }

    private void Update() {
        if (faceIsAnimating) {
            if (Time.time - faceAnimationStartRefTime > faceAnimationStartDelay) {

                if (readyToPlayAudio) {
                    speechAudio.PlayOneShot(currentClip);
                    readyToPlayAudio = false;
                }

                if (Time.time - mouthShapeTimeRef > mouthShapeTimeIntervalActual) {
                    ReadMouthShapeCharPairAndChangeFacePose();
                    if (currentMouthShapeCharIndex < mouthShapeCode.Count - 2) {
                        currentMouthShapeCharIndex = currentMouthShapeCharIndex + 2;
                        mouthShapeTimeRef = Time.time;
                    } else {
                        faceAnim.SetTrigger(triggerStringForRestingMouthShape);
                        faceIsAnimating = false;
                    }
                }
                if (Time.time - emotionTimeRef > emotionTimeIntervalActual) {
                    ReadEmotionCharAndChangeFacePose();
                    if (currentEmotionCharIndex < emotionCode.Count - 1) {
                        currentEmotionCharIndex++;
                        emotionTimeRef = Time.time;
                    } else {
                        //faceAnim.SetTrigger(endingEmotion);
                    }
                }
            }
        }
    }

    public void PlayClipAndStartAnimatingFace(AudioClip clip, float delay, 
                            string animationKey, float mouthShapeIntervalOverride, string emotionKey, float emotionIntervalOverride, string endingEmote) {
        faceIsAnimating = false;
        mouthShapeCode.Clear();
        emotionCode.Clear();
        char[] tempAnimationCode = animationKey.ToCharArray();
        foreach (char ch in tempAnimationCode) { mouthShapeCode.Add(ch); }
        char[] tempEmotionCode = emotionKey.ToCharArray();
        foreach (char ch in tempEmotionCode) { emotionCode.Add(ch); }

        faceAnimationStartDelay = delay;
        if (endingEmote != "") { endingEmotion = endingEmote; }
        if (mouthShapeIntervalOverride > 0) { mouthShapeTimeIntervalActual = mouthShapeIntervalOverride; }
        else { mouthShapeTimeIntervalActual = mouthShapeDefaultTimeInterval; }
        if (emotionIntervalOverride > 0) { emotionTimeIntervalActual = emotionIntervalOverride; }
        else { emotionTimeIntervalActual = emotionDefaultTimeInterval; }
        faceAnimationStartRefTime = Time.time;
        currentMouthShapeCharIndex = 0;
        currentEmotionCharIndex = 0;

        faceIsAnimating = true;
        currentClip = clip;
        speechAudio.Stop();
        readyToPlayAudio = true;
    }

    private void ReadMouthShapeCharPairAndChangeFacePose() {
        string tempPair = System.String.Concat(mouthShapeCode[currentMouthShapeCharIndex].ToString(),
                                                mouthShapeCode[currentMouthShapeCharIndex + 1].ToString());
        if (tempPair != previousMouthShapeCharPair) { faceAnim.SetTrigger(tempPair); }
        previousMouthShapeCharPair = tempPair;
    }

    private void ReadEmotionCharAndChangeFacePose() {
        if (emotionCode[currentEmotionCharIndex] != previousEmotionChar) { faceAnim.SetTrigger(emotionCode[currentEmotionCharIndex].ToString()); }
        previousEmotionChar = emotionCode[currentEmotionCharIndex];
    }
}
