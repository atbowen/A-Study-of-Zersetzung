using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultObject : MonoBehaviour
{
    public List<VaultPosition> vaultPositions;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public VaultPosition GetVaultPositionOfTriggerCollider(Collider col) {
        if (vaultPositions.Count > 0) {
            foreach(VaultPosition vPos in vaultPositions) {
                if (col == vPos.triggerCollider) { return vPos; }
            }
        }

        return null;
    }
}
