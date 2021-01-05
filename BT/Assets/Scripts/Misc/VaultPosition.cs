using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VaultPosition {
    public Collider triggerCollider;
    public float minimumYAngleFacingAwayToVault;
    public Transform startingPointReference;
    public Vector3 localOffsetOfVaultingManeuver;
    public string vaultAnimationTriggerString;
    public float vaultAnimationDuration;
    public bool freezeControlDuringVault;

    private List<Transform> potentialVaulters = new List<Transform>();

    public bool CanVaultOn(Transform vaulter) {
        return (Mathf.Abs(Vector3.Angle(vaulter.forward, startingPointReference.forward)) < minimumYAngleFacingAwayToVault);
    }

    public Vector3 GetStartingPositionOfVault(Transform vaulter) {
        Vector3 relPosOfVaulter = startingPointReference.InverseTransformPoint(vaulter.position);

        return startingPointReference.TransformPoint(new Vector3(relPosOfVaulter.x, 0, 0));
    }

    public Vector3 GetEndPositionAfterVaulting(Transform vaulter) {
        Vector3 worldStartPoint = GetStartingPositionOfVault(vaulter);
        //Vector3 localEndPoint = startingPointReference.InverseTransformPoint(worldStartPoint + localOffsetOfVaultingManeuver);
        Transform actualStartPoint = startingPointReference;
        actualStartPoint.position = worldStartPoint;

        return actualStartPoint.TransformPoint(localOffsetOfVaultingManeuver);
    }

    public void SetStartingPositionAndRotationOfVault(Transform vaulter) {
        Vector3 relPosOfVaulter = startingPointReference.InverseTransformPoint(vaulter.position);

        vaulter.position = startingPointReference.TransformPoint(new Vector3(relPosOfVaulter.x, 0, 0));
        vaulter.rotation = startingPointReference.rotation;
    }

    public void SetEndPositionAfterVaulting(Transform vaulter) {
        Vector3 worldStartPoint = GetStartingPositionOfVault(vaulter);
        Vector3 localEndPoint = startingPointReference.InverseTransformPoint(worldStartPoint + startingPointReference.TransformPoint(localOffsetOfVaultingManeuver));

        vaulter.position = startingPointReference.TransformPoint(localEndPoint);
    }
}
