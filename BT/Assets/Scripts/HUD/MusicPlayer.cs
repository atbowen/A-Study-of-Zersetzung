using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour {

    [Range(0.0f, 100.0f)]
    public float mainVolume, musicVolume, SFXVolume, voiceVolume;

    public Text playlist;
    public List<AudioClip> songs;
    
    public float fadeOutFactor;

    public List<AudioSource> SFXChannels;
    public List<AudioSource> outsideSFXChannels;
    public List<AudioSource> voiceChannels;
    public AudioClip SPRINGDeskOpen, SPRINGDeskClose, SPRINGDeskChangeItem, SPRINGDeskNoItemToChange, SPRINGDeskChangeTab,
                    kaChing, commsNoSignal;

    public bool playOnStart;

    private AudioSource muzakPlayer;
    private AudioClip currentSong;
    private CameraMaster camControl;
    private int index;
    private bool paused;
    private float runningTime, initialVolume;

    private void Awake() {
        muzakPlayer = this.GetComponent<AudioSource>();
        camControl = FindObjectOfType<CameraMaster>();

        initialVolume = mainVolume * 0.01f * musicVolume * 0.01f;

        muzakPlayer.volume = initialVolume;
        
        if (SFXChannels.Count > 0) {
            foreach (AudioSource channel in SFXChannels) { channel.volume = mainVolume * 0.01f * SFXVolume * 0.01f; }
        }

        if (outsideSFXChannels.Count > 0) {
            foreach (AudioSource channel in outsideSFXChannels) { channel.volume = mainVolume * 0.01f * SFXVolume * 0.01f; }
        }

        playlist.supportRichText = true;

        index = 0;
        currentSong = songs[index];

        muzakPlayer.clip = currentSong;
        if (playOnStart) {
            muzakPlayer.Play();
        }
        paused = false;

        runningTime = 0;
        
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (muzakPlayer.isPlaying && !paused) {
            runningTime += Time.deltaTime;
        }

        if (paused) { playlist.text = songs[index].name + "\n" + "||  " + Mathf.Floor(runningTime / 60).ToString("0") + ":" + Mathf.Floor(runningTime % 60).ToString("00") + "/"
                                                                                  + Mathf.Floor(songs[index].length / 60).ToString("0") + ":" + Mathf.Floor(songs[index].length % 60).ToString("00"); } 
        else        { playlist.text = songs[index].name + "\n" + "SP  " + Mathf.Floor(runningTime / 60).ToString("0") + ":" + Mathf.Floor(runningTime % 60).ToString("00") + "/"
                                                                                   + Mathf.Floor(songs[index].length / 60).ToString("0") + ":" + Mathf.Floor(songs[index].length % 60).ToString("00"); }

        if (!camControl.commsEnabled) {
            if (Input.GetKeyDown(KeyCode.Slash)) {
                muzakPlayer.volume = initialVolume;
                if (muzakPlayer.isPlaying && !paused) {
                    muzakPlayer.Pause();
                    paused = true;
                } else if (muzakPlayer.isPlaying && paused) {
                    muzakPlayer.UnPause();
                    paused = false;
                } else if (!muzakPlayer.isPlaying) {
                    muzakPlayer.Play();
                    if (paused) {
                        paused = false;
                    }
                }
            }
            if (Input.GetButtonDown("Toggle BGM")) {
                muzakPlayer.Stop();
                muzakPlayer.volume = initialVolume;
                if (runningTime > 1) {
                    muzakPlayer.Play();
                    if (paused) {
                        muzakPlayer.Pause();
                    }
                } else {
                    if (index < 1) { index = songs.Count - 1; } else { index--; }
                    muzakPlayer.clip = songs[index];
                    if (!paused) {
                        muzakPlayer.Play();
                    }
                }
                runningTime = 0;
            }
            if (Input.GetKeyDown(KeyCode.Period)) {
                muzakPlayer.Stop();
                muzakPlayer.volume = initialVolume;
                if (index > songs.Count - 2) { index = 0; } else { index++; }
                muzakPlayer.clip = songs[index];
                if (!paused) {
                    muzakPlayer.Play();
                }
                runningTime = 0;
            }
            if (Input.GetKeyDown(KeyCode.RightShift)) {
                playlist.enabled = !playlist.enabled;
            }
        }

        if (runningTime > songs[index].length - 5) {
            muzakPlayer.volume -= Time.deltaTime * fadeOutFactor;
        }
        if (runningTime > songs[index].length - 0.1) {
            muzakPlayer.Stop();
            if (index > songs.Count - 2) { index = 0; } 
            else                         { index++; }
            muzakPlayer.clip = songs[index];
            muzakPlayer.volume = initialVolume;
            muzakPlayer.Play();
            paused = false;
            runningTime = 0;
        }
	}

    public void PlayTuneByName(string songName, bool loop) {
        bool foundName = false;

        if (songs.Count > 0) {
            foreach (AudioClip tune in songs) {
                if (!foundName) {
                    if (tune.name == songName) {
                        muzakPlayer.loop = loop;
                        muzakPlayer.clip = tune;
                        muzakPlayer.Play();
                        foundName = true;
                    }
                }
            }
        }
    }

    public void PlaySPRINGDeskOpenSound() {
        PlaySFX(SPRINGDeskOpen);
    }

    public void PlaySPRINGDeskCloseSound() {
        PlaySFX(SPRINGDeskClose);
    }

    public void PlaySPRINGDeskChangeItemSound() {
        PlaySFX(SPRINGDeskChangeItem);
    }

    public void PlaySPRINGDeskNoItemToChangeSound() {
        PlaySFX(SPRINGDeskNoItemToChange);
    }

    public void PlaySPRINGDeskChangeTabSound() {
        PlaySFX(SPRINGDeskChangeTab);
    }

    public void PlayKaChing() {
        PlaySFX(kaChing);
    }

    public void PlayCommsNoSignal() {
        PlaySFX(commsNoSignal);
    }

    public void PlaySFX(AudioClip clip) {

        int chosenChannelIndex = 0;
        
        if (SFXChannels.Count > 0) {
            for (int i = 0; i < SFXChannels.Count; i++) {
                if (!SFXChannels[i].isPlaying) { chosenChannelIndex = i; }
            }

            if (clip != null) {
                SFXChannels[chosenChannelIndex].PlayOneShot(clip);
            }
        }
    }

    public void StopTheMusic() {
        if (muzakPlayer.isPlaying) { muzakPlayer.Stop(); }
    }

    public void AddChannelToOutsideSFXChannels(AudioSource audioFX) {
        if (!outsideSFXChannels.Contains(audioFX)) {
            outsideSFXChannels.Add(audioFX);
        }

        audioFX.volume = mainVolume * 0.01f * SFXVolume * 0.01f;
    }

    public void AddChannelToVoiceChannels(AudioSource audioVoice) {
        if (!voiceChannels.Contains(audioVoice)) {
            voiceChannels.Add(audioVoice);
        }

        audioVoice.volume = mainVolume * 0.01f * voiceVolume * 0.01f;
    }
}
