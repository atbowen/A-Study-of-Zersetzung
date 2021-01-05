using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Transform owner;

    public List<IDItem> items;

    public Transform itemViewerPosePosition;
    
    // Start is called before the first frame update
    void Start()
    {
        if (owner == null) {
            owner = this.transform.parent;
        }

        if (items.Count > 0) {
            foreach (IDItem item in items) {
                item.AssignOwnerAndHide(this.transform);
                item.ShowOrHideInventoryCopy(false);
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
            if (items[i].transform.name == trName) {
                Transform itemCopy = items[i].transform;
                items.Remove(items[i]);
                return itemCopy;
            }
        }

        return null;
    }

    public void PlaceItemToView(int inventoryIndex) {
        items[inventoryIndex].itemCopyForInventory.SetParent(itemViewerPosePosition);
        items[inventoryIndex].itemCopyForInventory.localPosition = Vector3.zero;
        items[inventoryIndex].itemCopyForInventory.localRotation = Quaternion.identity;
    }

    public void PlaceItemToView(IDItem item) {

        item.itemCopyForInventory.SetParent(null);
        item.itemCopyForInventory.SetParent(itemViewerPosePosition);
        item.itemCopyForInventory.localPosition = Vector3.zero + item.itemCopyViewerOffsetPos;
        item.itemCopyForInventory.localRotation = Quaternion.identity * Quaternion.Euler(item.itemCopyViewerRotAdjustment);
        Vector3 newScale = item.itemCopyForInventory.localScale;
        newScale.Set(item.itemCopyViewerScale.x, item.itemCopyViewerScale.y, item.itemCopyViewerScale.z);
        item.itemCopyForInventory.localScale = newScale;

        item.ShowOrHideInventoryCopy(true);
    }

    public void ReturnInventoryItem(int inventoryIndex) {
        items[inventoryIndex].itemCopyForInventory.SetParent(items[inventoryIndex].transform);
        items[inventoryIndex].itemCopyForInventory.localPosition = Vector3.zero;
    }

    public void ReturnInventoryItem(IDItem item) {

        item.itemCopyForInventory.SetParent(null);
        item.itemCopyForInventory.SetParent(item.transform);
        item.itemCopyForInventory.localPosition = Vector3.zero;

        item.ShowOrHideInventoryCopy(false);
    }

    public void AddItemToInventory(IDItem newItem) {

        newItem.AssignOwnerAndHide(this.transform);
        items.Add(newItem);
    }


}
