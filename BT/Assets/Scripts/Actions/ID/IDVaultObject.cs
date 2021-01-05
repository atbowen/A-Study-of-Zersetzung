using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDVaultObject : ID
{
    public List<VaultPosition> vaultPositions;

    public override void Activate() {

    }

    public override void DisplayID() {
    }

    public override void DisplayID(IDCharacter charID) {
        bool lookingAtVaultSurface = false;
        bool foundVaultableSurface = false;

        foreach (VaultPosition vPos in vaultPositions) {
            if (vPos.CanVaultOn(charID.transform)) { lookingAtVaultSurface = true; }
        }

        foundVaultableSurface = (charID.GetPotentialVaultObjects().Contains(this));

        if (lookingAtVaultSurface && foundVaultableSurface) { scanner.LookingAtUnscannable(this); }
    }

    public VaultPosition GetVaultPositionOfTriggerCollider(Collider col) {
        if (vaultPositions.Count > 0) {
            foreach (VaultPosition vPos in vaultPositions) {
                if (col == vPos.triggerCollider) { return vPos; }
            }
        }

        return null;
    }
}
