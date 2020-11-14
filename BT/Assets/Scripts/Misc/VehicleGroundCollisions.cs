using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleGroundCollisions : MonoBehaviour
{

    public AudioSource bumpChannel, continuousSoundsChannel;
    public List<AudioClip> smallBumpSounds, bigBumpSounds, screechingSounds, 
                            syntheticSounds, grassSounds, gravelSounds, metalSounds, woodSounds, glassSounds, concreteSounds, fabricSounds;
    public bool makeBumpSounds;
    public float requiredVelocityChangeForSmallBump, additionalVelocityChangeRequiredForBigBump;
    public float groundSoundThresholdLow, groundSoundThresholdHigh;

    private WheelCollider col;

    private float smallBumpDefaultVolume, bigBumpDefaultVolume, continuousSoundsDefaultVolume;
    private bool currentlyPlayingContinuousSound = false;
    private TerrainType.TypeOfSurface currentGroundSurface;

    private float previousVerticalPosition, previousVerticalVelocity;

    // Start is called before the first frame update
    void Start() {
        col = this.GetComponent<WheelCollider>();
        currentGroundSurface = TerrainType.TypeOfSurface.Synthetic;

        continuousSoundsChannel.loop = true;
        continuousSoundsChannel.volume = 0;

        previousVerticalPosition = 0;
        previousVerticalVelocity = 0;
    }

    // Update is called once per frame
    void Update() {

        WheelHit hit;

        if (col.GetGroundHit(out hit)) {
            CheckCollisions(hit);
        }

    }

    public void PlaySmallBump() {
        if (bumpChannel && smallBumpSounds.Count > 0) {
            bumpChannel.PlayOneShot(smallBumpSounds[Random.Range(0, smallBumpSounds.Count)]);
        }
    }

    public void PlaySmallBump(float volumeLevel) {
        if (bumpChannel && smallBumpSounds.Count > 0) {
            bumpChannel.volume = volumeLevel;
            bumpChannel.PlayOneShot(smallBumpSounds[Random.Range(0, smallBumpSounds.Count)]);
        }
    }

    public void PlayBigBump() {
        if (bumpChannel && bigBumpSounds.Count > 0) {
            bumpChannel.volume = bigBumpDefaultVolume;
            bumpChannel.PlayOneShot(bigBumpSounds[Random.Range(0, bigBumpSounds.Count)]);
        }
    }

    public void PlayBigBump(float volumeLevel) {
        if (bumpChannel && bigBumpSounds.Count > 0) {
            bumpChannel.volume = volumeLevel;
            bumpChannel.PlayOneShot(bigBumpSounds[Random.Range(0, bigBumpSounds.Count)]);
        }
    }

    public void PlayContinuousSound(TerrainType.TypeOfSurface terrain) {
        AudioClip clip = null;

        if (continuousSoundsChannel) {
            switch (terrain) {
                case TerrainType.TypeOfSurface.Synthetic:
                    clip = PickRandomSoundFromList(syntheticSounds);
                    break;
                case TerrainType.TypeOfSurface.Grass:
                    clip = PickRandomSoundFromList(glassSounds);
                    break;
                case TerrainType.TypeOfSurface.Gravel:
                    clip = PickRandomSoundFromList(gravelSounds);
                    break;
                case TerrainType.TypeOfSurface.Metal:
                    clip = PickRandomSoundFromList(metalSounds);
                    break;
                case TerrainType.TypeOfSurface.Wood:
                    clip = PickRandomSoundFromList(woodSounds);
                    break;
                case TerrainType.TypeOfSurface.Glass:
                    clip = PickRandomSoundFromList(glassSounds);
                    break;
                case TerrainType.TypeOfSurface.Concrete:
                    clip = PickRandomSoundFromList(concreteSounds);
                    break;
                case TerrainType.TypeOfSurface.Fabric:
                    clip = PickRandomSoundFromList(fabricSounds);
                    break;
            }
        }

        if (clip) {
            continuousSoundsChannel.clip = clip;
            continuousSoundsChannel.Play();
        }
        else {
            if (syntheticSounds.Count > 0) {
                continuousSoundsChannel.clip = syntheticSounds[Random.Range(0, syntheticSounds.Count)];
                continuousSoundsChannel.Play();
            }
        }
    }

    public AudioClip PickRandomSoundFromList(List<AudioClip> clipList) {
        if (clipList.Count > 0) { return (clipList[Random.Range(0, clipList.Count)]); }
        else return null;
    }

    public void SetVolumeLevels(float defaultVolumeForAll) {
        smallBumpDefaultVolume = defaultVolumeForAll;
        bigBumpDefaultVolume = defaultVolumeForAll;
        continuousSoundsDefaultVolume = defaultVolumeForAll;
    }

    public void SetVolumeLevels(float defaultVolumeForAll, float boostFactor) {
        smallBumpDefaultVolume = defaultVolumeForAll;
        bigBumpDefaultVolume = defaultVolumeForAll * boostFactor;
        continuousSoundsDefaultVolume = defaultVolumeForAll;
    }

    public void SetVolumeLevels(float smallBumpVolume, float bigBumpVolume, float continuousSoundsVolume) {
        smallBumpDefaultVolume = smallBumpVolume;
        bigBumpDefaultVolume = bigBumpVolume;
        continuousSoundsDefaultVolume = continuousSoundsVolume;
    }

    public void AdjustContinuousChannelVolumeByVehicleSpeed(float speed) {

        float volumeCalculated;

        if (speed < groundSoundThresholdLow) { volumeCalculated = 0; }
        else if (speed > groundSoundThresholdHigh) { volumeCalculated = continuousSoundsDefaultVolume; }
        else { volumeCalculated = (speed / groundSoundThresholdHigh) * continuousSoundsDefaultVolume; }

        continuousSoundsChannel.volume = volumeCalculated;
    }

    public void CheckCollisions(WheelHit coll) {
        if (coll.collider.transform.GetComponent<TerrainType>()) {
            TerrainType terr = coll.collider.transform.GetComponent<TerrainType>();

            if (makeBumpSounds) {
                //if (coll.force > (requiredForceForSmallBump + additionalForceRequiredForBigBump)) {
                //    PlayBigBump();
                //    Debug.Log("BUMP");
                //}
                //else {
                //    if (coll.force > requiredForceForSmallBump) {
                //        PlaySmallBump();
                //        Debug.Log("bump");
                //    }
                //}
                float verticalDisplacement = this.transform.position.y - previousVerticalPosition;
                float newVerticalVelocity = verticalDisplacement / Time.deltaTime;

                if (newVerticalVelocity - previousVerticalVelocity > requiredVelocityChangeForSmallBump + additionalVelocityChangeRequiredForBigBump) {
                    PlayBigBump();
                    Debug.Log("BUMP");
                }
                else {
                    if (newVerticalVelocity - previousVerticalVelocity > requiredVelocityChangeForSmallBump) {
                        PlaySmallBump();
                        Debug.Log("bump");
                    }
                }

                previousVerticalPosition = this.transform.position.y;
                previousVerticalVelocity = newVerticalVelocity;
            }

            if (currentlyPlayingContinuousSound) {
                if (terr.SurfaceType != currentGroundSurface) {
                    PlayContinuousSound(terr.SurfaceType);
                }
            }
            else {
                if (terr.SurfaceType == currentGroundSurface) {
                    PlayContinuousSound(terr.SurfaceType);
                    currentlyPlayingContinuousSound = true;
                }
            }

            currentGroundSurface = terr.SurfaceType;
        }
    }

    //private void OnCollisionStay(Collision collision) {
    //    if (collision.transform.GetComponent<TerrainType>()) {
    //        TerrainType terr = collision.transform.GetComponent<TerrainType>();

    //        if (collision.impulse.magnitude > (requiredForceForSmallBump + additionalForceRequiredForBigBump)) {
    //            PlayBigBump();
    //        }
    //        else {
    //            if (collision.impulse.magnitude > requiredForceForSmallBump) {
    //                PlaySmallBump();
    //            }
    //        }

    //        if (currentlyPlayingContinuousSound) {
    //            if (terr.SurfaceType != currentGroundSurface) {
    //                PlayContinuousSound(terr.SurfaceType);
    //            }
    //        }
    //        else {
    //            if (terr.SurfaceType == currentGroundSurface) {
    //                PlayContinuousSound(terr.SurfaceType);
    //            }
    //        }

    //        currentGroundSurface = terr.SurfaceType;
    //    }
    //}

    //private void OnCollisionEnter(Collision collision) {
    //    if (collision.transform.GetComponent<TerrainType>()) {
    //        TerrainType terr = collision.transform.GetComponent<TerrainType>();

    //        if (collision.impulse.magnitude > (requiredForceForSmallBump + additionalForceRequiredForBigBump)) {
    //            PlayBigBump();
    //        }
    //        else {
    //            if (collision.impulse.magnitude > requiredForceForSmallBump) {
    //                PlaySmallBump();
    //            }
    //        }

    //        if (currentlyPlayingContinuousSound) {
    //            if (terr.SurfaceType != currentGroundSurface) {
    //                PlayContinuousSound(terr.SurfaceType);
    //            }
    //        }
    //        else {
    //            if (terr.SurfaceType == currentGroundSurface) {
    //                PlayContinuousSound(terr.SurfaceType);
    //            }
    //        }

    //        currentGroundSurface = terr.SurfaceType;
    //    }
    //}
}
