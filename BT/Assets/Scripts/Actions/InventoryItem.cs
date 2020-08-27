﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    public enum ItemCategory { Unique, Quantifiable, Money }
    public ItemCategory TypeOfItem;


    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void AssignOwnerAndHide(Transform owner) {
        if (this.transform.GetComponent<MeshRenderer>() != null) { this.transform.GetComponent<MeshRenderer>().enabled = false; }
        if (this.transform.GetComponent<SkinnedMeshRenderer>() != null) { this.transform.GetComponent<SkinnedMeshRenderer>().enabled = false; }
        if (this.transform.GetComponent<Rigidbody>() != null) { this.transform.GetComponent<Rigidbody>().isKinematic = true; }
        if (this.transform.GetComponent<Collider>() != null) { this.transform.GetComponent<Collider>().enabled = false; }

        this.transform.parent = owner.transform;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
    }
}
