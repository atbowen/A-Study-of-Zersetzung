using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Set Object Properties")]
public class SetObjectProperties : Action
{

    public bool clearParent;
    public bool setAnimationStateVariables;

    public bool enableCollider;
    public bool enableRigidbody;
    public bool enableMeshRenderer;
    public bool enableSkinnedMeshRenderer;

    public bool setPositionOffset;
    public Vector3 positionOffset;

    public bool setRotationEulerOffset;
    public Vector3 eulerAngles;

    public bool temporarilyDisableMeshRenderer;
    public bool temporarilyDisableSkinnedMeshRenderer;
    public string nameOfRenderObject;

    private ID actorID;

    public override void DoAction() {
        Transform thing = GameObject.Find(actorName).transform;
        IDCharacter charID = null;

        if (thing.GetComponent<ID>()) {
            actorID = thing.GetComponent<ID>();
        }

        if (actorID) {
            if (actorID.GetType() == typeof(IDCharacter)) {
                charID = (IDCharacter)actorID;
            }
        }

        if (clearParent) { thing.SetParent(null); }

        thing.GetComponent<Collider>().enabled = enableCollider;
        if (charID) {
            if (charID.additionalColliders.Count > 0) {
                foreach (Collider col in charID.additionalColliders) { col.enabled = enableCollider; }
            }
        }
        thing.GetComponent<Rigidbody>().isKinematic = !enableRigidbody;

        if (temporarilyDisableMeshRenderer && thing.Find(nameOfRenderObject).GetComponent<MeshRenderer>()) {
            thing.Find(nameOfRenderObject).GetComponent<MeshRenderer>().enabled = false;
        }

        if (temporarilyDisableSkinnedMeshRenderer && thing.Find(nameOfRenderObject).GetComponent<SkinnedMeshRenderer>()) {
            thing.Find(nameOfRenderObject).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }

        if (setPositionOffset) {
            thing.Translate(positionOffset);
        }

        if (setRotationEulerOffset) {
            thing.Rotate(eulerAngles);
        }

        if (temporarilyDisableMeshRenderer && thing.Find(nameOfRenderObject).GetComponent<MeshRenderer>()) {
            thing.Find(nameOfRenderObject).GetComponent<MeshRenderer>().enabled = enableMeshRenderer;
        }

        if (temporarilyDisableSkinnedMeshRenderer && thing.Find(nameOfRenderObject).GetComponent<SkinnedMeshRenderer>()) {
            thing.Find(nameOfRenderObject).GetComponent<SkinnedMeshRenderer>().enabled = enableSkinnedMeshRenderer;
        }
    }
}
