using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequiredItem : AccessibilityRequirement
{
    [SerializeField]
    private string itemName;
    public string ItemName { get => itemName; set => itemName = value; }

    public override bool CheckRequirement(Transform actor) {
        Inventory inv = actor.Find("Inventory").GetComponent<Inventory>();
        if (inv.HasItem(ItemName))  { return true; }
        else                        { return false; }
    }
}
