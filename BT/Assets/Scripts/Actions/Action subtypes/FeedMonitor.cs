using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Feed Monitor")]
public class FeedMonitor : Action {
    [SerializeField]
    private Texture cameraFeed;
    public Texture CameraFeed { get => cameraFeed; set => cameraFeed = value; }
    [SerializeField]
    private string monitorName;
    public string MonitorName { get => monitorName; set => monitorName = value; }
    [SerializeField]
    private float delay;
    public float Delay { get => delay; set => delay = value; }

    public override void DoAction() {
        GameObject.Find(actorName).transform.GetComponent<Renderer>().material.mainTexture = cameraFeed;
    }
}
