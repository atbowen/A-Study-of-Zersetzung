using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSounds : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] open, close, motion1, motion2, motion3;
    private AudioSource audioFX;
    
    // Start is called before the first frame update
    void Awake()
    {
        audioFX = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OpenDoor() {
        audioFX.PlayOneShot(PlayRandomClip(open));
    }

    private void CloseDoor() {
        audioFX.PlayOneShot(PlayRandomClip(close));
    }

    private void PlayMotion1Sound() {
        audioFX.PlayOneShot(PlayRandomClip(motion1));
    }

    private void PlayMotion2Sound() {
        audioFX.PlayOneShot(PlayRandomClip(motion2));
    }

    private void PlayMotion3Sound() {
        audioFX.PlayOneShot(PlayRandomClip(motion3));
    }

    private AudioClip PlayRandomClip(AudioClip[] clips) {
        return clips[0];
    }
}
