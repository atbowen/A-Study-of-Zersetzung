using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEntrance : MonoBehaviour
{

    private WorkDesk wkDesk;
    public bool canEnter, entering;

    public Transform requiredItem, PositionToFaceWhenEntering;
    
    // Start is called before the first frame update
    void Start()
    {
        //tedsCar = FindObjectOfType<ZXR>();
        wkDesk = FindObjectOfType<WorkDesk>();

        canEnter = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (entering) {
            //tedsCar.tedEntering = true;
            entering = false;
        }
    }

    private void OnTriggerEnter(Collider col) {
        if (col.transform.tag == "Player") {
            //if (wkDesk.inventory.Contains(requiredItem)) {
                Debug.Log("can enter");

                canEnter = true;
            //}
            
        } else if (col.transform.Find("Inventory")) {
            Inventory targetInventory = col.transform.Find("Inventory").GetComponent<Inventory>();

            if (targetInventory.items.Contains(requiredItem)) {
                Debug.Log("can enter");
            }

            canEnter = true;
        }
    }

    private void OnTriggerExit(Collider col) {
        canEnter = false;
    }
}
