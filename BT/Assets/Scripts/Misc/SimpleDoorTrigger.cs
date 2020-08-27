using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDoorTrigger : MonoBehaviour
{
    private SlidingDoor door;
    
    // Start is called before the first frame update
    void Start()
    {
        door = this.transform.parent.GetComponent<SlidingDoor>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.Find("ID") != null) { door.ActivateDoor(); }
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform.Find("ID") != null) { door.ActivateDoor(); }
    }
}
