using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    public bool doorTriggeredToOpen, entered;

    // Start is called before the first frame update
    void Start()
    {
        doorTriggeredToOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.CompareTag("Player")) {
            doorTriggeredToOpen = true;
        }
        
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform.CompareTag("Player")) {
            doorTriggeredToOpen = false;
        }
        
    }
}
