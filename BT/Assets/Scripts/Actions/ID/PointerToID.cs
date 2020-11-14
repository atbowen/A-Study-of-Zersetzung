using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerToID : MonoBehaviour
{
    public ID desiredID;

    public bool makeDesiredActivateThisCollider;

    public List<Collider> otherCollidersToActivate;
}
