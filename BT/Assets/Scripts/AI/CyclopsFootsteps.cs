using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyclopsFootsteps : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] clips;

    private AudioSource footstepAudio;

    // Start is called before the first frame update
    void Start()
    {
        footstepAudio = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Step() {
        footstepAudio.PlayOneShot(PickRandomFootstepClip());
    }

    private AudioClip PickRandomFootstepClip() {
        return clips[Random.Range(0, clips.Length)];
    }
}
