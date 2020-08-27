using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLight : MonoBehaviour
{
    public simpleLight[] lights;
    public bool lightOn;
    //public float flickerRandOn, flickerRandOff, flashOn, flashOff, intensityLow, intensityHigh;

    //private float flickerTimerOn, flickerTimerOff, flickerTimerRef, flashTimerRef;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < lights.Length; i++) {
            //lights[i].lightOrigIntensity = lights[i].lightObject.intensity;
            lights[i].flickerTimerRef = 0;
            lights[i].flashTimerRef = 0;
            lights[i].flickerTimerOn = Random.Range(0, lights[i].flickerRandOn);
            lights[i].flickerTimerOff = Random.Range(0, lights[i].flickerRandOff) + lights[i].flickerTimerOn;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lightOn) {
            for (int i = 0; i < lights.Length; i++) {
                if (lights[i].flicker) {
                    if (Time.time - lights[i].flickerTimerRef < lights[i].flickerTimerOn) {
                        lights[i].lightObject.intensity = lights[i].intensityHigh;
                    } else if (Time.time - lights[i].flickerTimerRef > lights[i].flickerTimerOn && Time.time - lights[i].flickerTimerRef < lights[i].flickerTimerOff) {
                        lights[i].lightObject.intensity = lights[i].intensityLow;
                    } else if (Time.time - lights[i].flickerTimerRef > lights[i].flickerTimerOff) {
                        lights[i].flickerTimerRef = Time.time;
                        lights[i].flickerTimerOn = Random.Range(0, lights[i].flickerRandOn);
                        lights[i].flickerTimerOff = Random.Range(0, lights[i].flickerRandOff) + lights[i].flickerTimerOn;
                    }
                } else if (lights[i].flash) {
                    if (Time.time - lights[i].flashTimerRef < lights[i].flashOn) {
                        lights[i].lightObject.intensity = lights[i].intensityHigh;
                    } else if (Time.time - lights[i].flashTimerRef > lights[i].flashOn && Time.time - lights[i].flashTimerRef < (lights[i].flashOn + lights[i].flashOff)) {
                        lights[i].lightObject.intensity = lights[i].intensityLow;
                    } else if (Time.time - lights[i].flashTimerRef > (lights[i].flashOn + lights[i].flashOff)) {
                        lights[i].flashTimerRef = Time.time;
                    }
                } else {
                    lights[i].lightObject.intensity = lights[i].intensityHigh;
                }
            }
        } else {
            for (int i = 0; i < lights.Length; i++) {
                lights[i].lightObject.intensity = lights[i].lightOrigIntensity;
            }
        }
    }
}
