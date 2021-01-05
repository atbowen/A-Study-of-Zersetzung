using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDItem : ID
{
    public enum ItemCategory { Unique, Quantifiable, Money }
    public ItemCategory TypeOfItem;

    // An 18x18 pixel shot of the item, for the Inventory tab in Work Desk - Desk
    public Texture itemPic;

    public bool canBeInHand = true, isInHand = false, canBeReadied = false, isReadied = false, canBeUsed = false, holdOnPickup = false;
    public bool canBump = false;

    public string holdingPoseTriggerString, readyingPoseTriggerString, usingItemAnimationTriggerString;
    public float usingTime;
    public bool requiresUseFreeze;
    public AudioClip pickupSound, equipSound, unequipSound, unreadySound, readySound, usingSound;

    public Transform itemCopyForInventory;

    public List<MeshRenderer> itemMeshRenderers, itemCopyMeshRenderers;
    public List<SkinnedMeshRenderer> itemSkinnedMeshRenderers, itemCopySkinnedMeshRenderers;
    public List<Rigidbody> itemRigids;
    public List<Collider> itemColliders;

    public Vector3 itemCopyViewerOffsetPos, itemCopyViewerRotAdjustment, itemCopyViewerScale;
    public Vector3 itemEquippedPosOffset, itemEquippedRotAdjustment;

    public bool isCombinedItem, destroyCombinationItemsAfterCreation;
    public List<IDItem> consistsOfTheseItems;

    public bool makesCombinationItem;
    public List<IDItem> canCombineToMakeTheseItems;

    public List<Action> triggersActionsOnEquipUse;

    public override void Activate() {

        bCam.InitiateUseActionWithAnimationTrigger("Take item", 1f, false);
        tedInventory.AddItemToInventory(this);
        //wkDesk.AddItemToViewer(myself);
        statusWindow.FlashStatusText(objActualName + "  added  to  inventory.");
    }

    public override void DisplayID() {
        scanner.EnableInfoPanelWithID(this);
    }

    public override void DisplayID(IDCharacter charID) {
        
    }

    public void AssignOwnerAndHide(Transform owner) {
        foreach (MeshRenderer mesh in itemMeshRenderers)                { mesh.enabled = false; }
        foreach (SkinnedMeshRenderer mesh in itemSkinnedMeshRenderers)  { mesh.enabled = false; }
        foreach (Rigidbody rigid in itemRigids)                         { rigid.isKinematic = true; }
        foreach (Collider collide in itemColliders)                     { collide.enabled = false; }

        this.transform.parent = owner.transform;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
    }

    public void ShowOrHideItem(bool showOrNo) {
        foreach (MeshRenderer mesh in itemMeshRenderers) { mesh.enabled = showOrNo; }
    }

    public void ShowOrHideInventoryCopy(bool showOrNo) {
        foreach (MeshRenderer mesh in itemCopyMeshRenderers) { mesh.enabled = showOrNo; }
    }

    public void EquipItem() {

        if (canBeInHand) {
            this.transform.SetParent(bCam.usingHand);
            this.transform.localPosition = Vector3.zero + itemEquippedPosOffset;
            this.transform.localRotation = Quaternion.identity * Quaternion.Euler(itemEquippedRotAdjustment);

            ShowOrHideItem(true);

            isInHand = true;
            bCam.SetCurrentHeldItem(this);

            if (equipSound != null) {
                if (this.transform.GetComponent<AudioSource>() != null) {
                    this.transform.GetComponent<AudioSource>().PlayOneShot(equipSound);
                }
            }
        }
    }

    public void UnequipItem() {

        ShowOrHideItem(false);
        this.transform.SetParent(tedInventory.transform);

        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;

        isInHand = false;
        bCam.SetCurrentHeldItem(null);

        if (unequipSound != null) {
            if (this.transform.GetComponent<AudioSource>() != null) {
                this.transform.GetComponent<AudioSource>().PlayOneShot(unequipSound);
            }
        }
    }

    public void ReadyItem() {
        if (canBeReadied) {
            isReadied = true;
            if (readyingPoseTriggerString != null) {
                //if (this.transform.parent.GetComponent<Inventory>() != null) {
                //    if (this.transform.parent.GetComponent<Inventory>().owner.name == "_Ted") {
                //        Debug.Log("readying!");
                        bCam.TriggerAnimation(readyingPoseTriggerString);
                //    } else if (this.transform.GetComponent<Inventory>().owner.GetComponent<NPCIntelligence>() != null) {

                //    }
                //}
            }

            if (readySound != null) {
                if (this.transform.GetComponent<AudioSource>() != null) {
                    this.transform.GetComponent<AudioSource>().PlayOneShot(readySound);
                }
            }
        }
    }

    public void UnreadyItem() {
        if (isReadied) {
            isReadied = false;

            if (unreadySound != null) {
                if (this.transform.GetComponent<AudioSource>() != null) {
                    this.transform.GetComponent<AudioSource>().PlayOneShot(unreadySound);
                }
            }
        }
    }

    public void UseEquippedItem() {
        if (this.canBeReadied && this.isReadied) {
            if (usingItemAnimationTriggerString != null) {
                bCam.InitiateUseActionWithAnimationTrigger(usingItemAnimationTriggerString, usingTime, requiresUseFreeze);
            }
        }

        if (triggersActionsOnEquipUse.Count > 0) {
            foreach (Action act in triggersActionsOnEquipUse) {
                actCoord.TriggerAction(act);
            }
        }

        if (usingSound != null) {
            if (this.transform.GetComponent<AudioSource>() != null) {
                this.transform.GetComponent<AudioSource>().PlayOneShot(usingSound);
            }
        }
    }
}
