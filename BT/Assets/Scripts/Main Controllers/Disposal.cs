using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disposal : MonoBehaviour
{
    private List<TrashItem> itemsToDisposeOf = new List<TrashItem>();
    private TrashItem tempItem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (itemsToDisposeOf.Count > 0) {
            foreach (TrashItem trashThing in itemsToDisposeOf) {
                if (Time.time - trashThing.timeSubmitted > trashThing.delayUntilDisposal) {
                    tempItem = trashThing;
                }
            }
            if (tempItem != null) {
                itemsToDisposeOf.Remove(tempItem);
                Destroy(tempItem.item);
                tempItem = null;
            }
        }
    }

    public void DestroyItemAfterTimeDelay(GameObject obj, float delay) {
        itemsToDisposeOf.Add(new TrashItem(obj, Time.time, delay));
    }
}

public class TrashItem {
    public GameObject item;
    public float timeSubmitted, delayUntilDisposal;

    public TrashItem(GameObject thing, float startTime, float delayTime) {
        item = thing;
        timeSubmitted = startTime;
        delayUntilDisposal = delayTime;
    }
}
