using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedItemDestroyer : MonoBehaviour
{
    public List<Transform> thoseMarkedForDeath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collide) {

        Debug.Log("here!");

        if (!thoseMarkedForDeath.Contains(collide.transform)) {
            thoseMarkedForDeath.Add(collide.transform);
            Debug.Log(thoseMarkedForDeath.Count);
        }
    }
}
