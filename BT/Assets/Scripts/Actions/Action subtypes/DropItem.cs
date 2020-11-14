using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Drop Item")]
public class DropItem : Action {
    [SerializeField]
    private string itemName;
    public string ItemName { get => itemName; set => itemName = value; }
    [SerializeField]
    private Vector3 relativePosFromOrigToRelease;
    public Vector3 RelativePosFromOrigToRelease { get => relativePosFromOrigToRelease; set => relativePosFromOrigToRelease = value; }

    public override void DoAction() {
        Transform actor = GameObject.Find(actorName).transform;

        if (actor.Find("Inventory").GetComponent<Inventory>() != null) {
            Inventory inv = actor.Find("Inventory").GetComponent<Inventory>();
            Transform item = inv.GetTransformByNameAndRemove(itemName);



            if (item != null) {
                item.position = actor.position + relativePosFromOrigToRelease;
                if (item.GetComponent<MeshRenderer>() != null) { item.GetComponent<MeshRenderer>().enabled = true; }
                if (item.GetComponent<SkinnedMeshRenderer>() != null) { item.GetComponent<SkinnedMeshRenderer>().enabled = true; }
                if (item.GetComponent<Rigidbody>() != null) { item.GetComponent<Rigidbody>().isKinematic = false; }
                if (item.GetComponent<Collider>() != null) { item.GetComponent<Collider>().enabled = true; }

                item.parent = null;
            }
        }
    }
}
