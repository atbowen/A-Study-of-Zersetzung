using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chatter : MonoBehaviour
{

    public AudioClip[] idleSpeech, lookSpeech, patrolSpeech, greetSpeech, replyToGreetingSpeech, alertSpeech, chaseSpeech, fleeSpeech, attackSpeech, killTedSpeech;

    private AudioSource voice;

    // Start is called before the first frame update
    void Start()
    {
        voice = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SayLookLine(float delay) {
        voice.clip = lookSpeech[Random.Range(0, lookSpeech.Length)];
        voice.PlayDelayed(delay);
    }
}
