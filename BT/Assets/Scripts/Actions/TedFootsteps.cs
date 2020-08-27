using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TedFootsteps : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] footstepClips, switchFlipClips, doorOpenClips;

    private AudioSource footstepAudio;
    private BodyCam view;

    // Start is called before the first frame update
    void Awake()
    {
        footstepAudio = this.GetComponent<AudioSource>();
        view = FindObjectOfType<BodyCam>();
    }

    // Update is called once per frame
    void Update()
    {
        if (view.runMode) {
            if (view.sneakMode) { footstepAudio.volume = 0.3f; } 
            else { footstepAudio.volume = 1f; }
        } 
        else {
            if (view.sneakMode) { footstepAudio.volume = 0.2f; } 
            else { footstepAudio.volume = 0.4f; }
        }
    }

    private void Step() {
        if (footstepClips.Length > 0) { footstepAudio.PlayOneShot(PickRandomClip(footstepClips)); }
    }

    private void UseSwitch() {
        if (switchFlipClips.Length > 0) { footstepAudio.PlayOneShot(PickRandomClip(switchFlipClips)); }
    }

    private void OpenDoor() {
        if (doorOpenClips.Length > 0) { footstepAudio.PlayOneShot(PickRandomClip(doorOpenClips)); }
    }

    private AudioClip PickRandomClip(AudioClip[] audioClips) {
        return audioClips[Random.Range(0, audioClips.Length)];
    }
}
