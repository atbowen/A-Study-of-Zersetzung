using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAndConditions : MonoBehaviour
{
    public string missionName, sceneName, sceneStartSong;

    private MusicPlayer musicBox;
    private int day, minute, temperature, humidity;
    private const float firstDay = 4762,
                        firstMinute = 97;
    
    // Start is called before the first frame update
    void Start()
    {
        musicBox = FindObjectOfType<MusicPlayer>();

        day = (int)firstDay;
        minute = (int)firstMinute;

        temperature = 26;
        humidity = 36;

        musicBox.PlayTuneByName(sceneStartSong, true);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
    }

    private void UpdateTime() {
        minute  = (int) ((firstMinute + (Time.time / 60)) % 468);
        day     = (int) (firstDay + (Time.time / 60 / 468));
    }

    public int GetDay() {
        return day;
    }

    public int GetMinute() {
        return minute;
    }

    public int GetTemperature() {
        return temperature;
    }

    public int GetHumidity() {
        return humidity;
    }
}
