using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/ChangeChannel")]
public class ChangeChannel : Action
{
    public int channelIndex;

    [SerializeField]
    public List<Material> cameraFeeds;

    [SerializeField]
    private string monitorName;
    public string MonitorName { get => monitorName; set => monitorName = value; }

    [SerializeField]
    private float delay;
    public float Delay { get => delay; set => delay = value; }

    public override void DoAction() {
        Transform actorTrans = GameObject.Find(actorName).transform;

        if (cameraFeeds.Count > 1) {
            channelIndex = (channelIndex + 1) % cameraFeeds.Count;
            actorTrans.GetComponent<MeshRenderer>().material = cameraFeeds[channelIndex];
        }

        if (useSound != null && actorTrans.GetComponent<AudioSource>() != null) {
            actorTrans.GetComponent<AudioSource>().PlayOneShot(useSound);
        }
    }
}
