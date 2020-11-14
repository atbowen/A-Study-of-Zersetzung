using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IDCharacter : ID
{
    public Texture portrait;
    public Transform warrantMarker;
    public List<Transform> markers;

    public bool arrestable;

    public List<Action> enterCarActions, exitCarActions;
    public Transform head, parentBoneOfHead;

    private Vector3 headInitialPos;
    private Quaternion headInitialRot;

    [Range(0.0f, 100.0f)]
    public float tedsFavorabilityRatingWith, favorabilityNeutralRangeLow, favorabilityNeutralRangeHigh;

    public float discernibilityRange;

    public override void Activate() {
        if (myself.GetComponent<CyclopsAI>() != null && arrestable) {
            //CyclopsAI AI = myself.GetComponent<CyclopsAI>();
            //AI.GotPinched();
            prisonController.InitiateCapture(this);
        }
    }

    public void RecordHeadInitialTransform() {
        headInitialPos = head.localPosition;
        headInitialRot = head.localRotation;
    }

    public Vector3 GetHeadInitialPosition() {
        return headInitialPos;
    }

    public Quaternion GetHeadInitialRotation() {
        return headInitialRot;
    }

    public override void DisplayID() {
        scanner.EnableInfoPanelWithID(this);
    }
}
