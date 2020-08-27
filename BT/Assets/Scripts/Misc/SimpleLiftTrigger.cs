using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLiftTrigger : MonoBehaviour
{
    private LiftControl lift;

    // Start is called before the first frame update
    void Start()
    {
        lift = this.transform.parent.GetComponent<LiftControl>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.Find("ID") != null) {
            lift.EnteredLiftNowActivate();
            other.transform.parent = lift.transform;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform.Find("ID") != null) {
            lift.ExitedLiftNowStay();
            other.transform.parent = null;
        }
    }
}
