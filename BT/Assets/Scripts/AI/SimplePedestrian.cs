using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimplePedestrian : MonoBehaviour
{
    [SerializeField]
    private float topWalkSpeed, animationSpeedMultiplier;
    [SerializeField, Range(0.0f, 100.0f)]
    private float percentTopWalkSpeedMin, percentTopWalkSpeedMax;
    [SerializeField]
    private string walkTriggerString;

    private NavMeshAgent agent;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {

        anim.SetTrigger(walkTriggerString);
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.destination != null) {
            //this.transform.LookAt(agent.destination, Vector3.up);

            //this.transform.Translate(0, 0, walkSpeed * Time.deltaTime);
        }
    }

    public void InitializePedestrian() {
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
        float percentTopWalkSpeed = Random.Range(percentTopWalkSpeedMin, percentTopWalkSpeedMax);
        agent.speed = percentTopWalkSpeed / 100.0f * topWalkSpeed;
        anim.speed = percentTopWalkSpeed / 100.0f * animationSpeedMultiplier;
    }

    public void SetDestination(Transform destination) {
        agent.destination = destination.position;
        anim.SetTrigger(walkTriggerString);
    }
}
