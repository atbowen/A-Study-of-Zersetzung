using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSounds : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] flipOn, flipOff;
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

    private void FlipOn() {
        audioFX.PlayOneShot(PlayRandomClip(flipOn));
    }

    private void FlipOff() {
        audioFX.PlayOneShot(PlayRandomClip(flipOff));
    }

    private AudioClip PlayRandomClip(AudioClip[] clips) {
        return clips[Random.Range(0, clips.Length)];
    }
}
