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

    public List<Collider> additionalColliders;
    
    public DatingProfile stardaterProfile;
    public Transform stardaterCamHolder;
    public bool makeProfileAvailable;

    private Vector3 headInitialPos;
    private Quaternion headInitialRot;

    private List<IDVaultObject> currentPotentialVaultObjects = new List<IDVaultObject>();

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

    public override void DisplayID(IDCharacter charID) {
        
    }

    public List<IDVaultObject> GetPotentialVaultObjects() {
        return currentPotentialVaultObjects;
    }

    // Particularly useful for recognizing VaultObjects
    private void OnTriggerEnter(Collider other) {
        if (other.transform.parent != null) {
            if (other.transform.parent.GetComponent<IDVaultObject>()) {
                IDVaultObject vObj = other.transform.parent.GetComponent<IDVaultObject>();

                if (!currentPotentialVaultObjects.Contains(vObj)) { currentPotentialVaultObjects.Add(vObj); }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform.parent != null) {
            if (other.transform.parent.GetComponent<IDVaultObject>()) {
                IDVaultObject vObj = other.transform.parent.GetComponent<IDVaultObject>();

                if (currentPotentialVaultObjects.Contains(vObj)) { currentPotentialVaultObjects.Remove(vObj); }
            }
        }
    }
}
