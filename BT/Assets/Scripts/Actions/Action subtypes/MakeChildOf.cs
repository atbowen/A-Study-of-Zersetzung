using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Make Child of")]
public class MakeChildOf : Action {
    [SerializeField]
    private string parentName;
    public string ParentName { get => parentName; set => parentName = value; }

    [SerializeField]
    private Vector3 positionRelativeToParent, rotationRelativeToParent;
    public Vector3 PositionRelativeToParent { get => positionRelativeToParent; set => positionRelativeToParent = value; }
    public Vector3 RotationRelativeToParent { get => rotationRelativeToParent; set => rotationRelativeToParent = value; }

    public override void DoAction() {
        Transform actor = GameObject.Find(actorName).transform;
        Transform parent = GameObject.Find(parentName).transform;

        actor.parent = null;
        actor.parent = parent;

        actor.localPosition = positionRelativeToParent;
        actor.localRotation = Quaternion.Euler(rotationRelativeToParent);
    }
}
