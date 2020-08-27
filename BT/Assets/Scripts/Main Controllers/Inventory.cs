using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Transform> items;
    
    // Start is called before the first frame update
    void Start()
    {
        if (items.Count > 0) {
            foreach (Transform item in items) {
                if (item.GetComponent<InventoryItem>() != null) { item.GetComponent<InventoryItem>().AssignOwnerAndHide(this.transform); }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool HasItem(string itemName) {
        bool foundItem;
        foundItem = false;

        foreach(Transform child in transform) {
            if (child.name == itemName) { foundItem = true; }
        }

        return foundItem;
    }

    public Transform GetItemTransformByName(string itemName) {
        foreach(Transform child in transform) {
            if (child.name == itemName) { return child; }
        }

        return null;
    }

    public Transform GetTransformByNameAndRemove(string trName) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].name == trName) {
                Transform itemCopy = items[i];
                items.Remove(items[i]);
                return itemCopy;
            }
        }

        return null;
    }

    public void AddItemToInventory(Transform newItem) {
        if (newItem.GetComponent<InventoryItem>() != null) {
            newItem.GetComponent<InventoryItem>().AssignOwnerAndHide(this.transform);
            items.Add(newItem);
        }
    }
}
