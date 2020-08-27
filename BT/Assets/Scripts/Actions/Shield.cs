using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    private Teddy ted;
    private ToolSelector toolSelect;
    
    private bool activated;

    private List<ShieldPart> shieldPartSet, shieldPartSetCopy;

    public ShieldPart   face,
                        chest,
                        gut,
                        hips,
                        leftUpperArm,
                        leftLowerArm,
                        leftHand,
                        rightUpperArm,
                        rightLowerArm,
                        rightHand,
                        leftUpperLeg,
                        leftLowerLeg,
                        rightUpperLeg,
                        rightLowerLeg,
                        leftBoot,
                        rightBoot;

    public float timeBetweenShieldPartActivations;
    private float refTimeforShieldPartActivations;


    // Start is called before the first frame update
    void Start()
    {
        ShieldPart[] shieldPartSetArray = new ShieldPart[] { face, chest, gut, hips, leftUpperArm, leftLowerArm, leftHand,
            rightUpperArm, rightLowerArm, rightHand, leftUpperLeg, leftLowerLeg, rightUpperLeg, rightLowerLeg, leftBoot, rightBoot };
        shieldPartSet = new List<ShieldPart>(shieldPartSetArray);

        shieldPartSetCopy = new List<ShieldPart>();

        activated = false;

        InitializePartsInSet(shieldPartSet);
        DeactivateShieldSet(shieldPartSet);

        refTimeforShieldPartActivations = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated) {
            if ((Time.time - refTimeforShieldPartActivations > timeBetweenShieldPartActivations) && shieldPartSet.Count > 0) {
                int randNum = Random.Range(0, shieldPartSet.Count);
                ShieldPart randPart = shieldPartSet[randNum];

                ActivateShieldPart(randPart);
                shieldPartSetCopy.Add(randPart);
                shieldPartSet.Remove(randPart);

                refTimeforShieldPartActivations = Time.time;
            }
        } else {
            if ((Time.time - refTimeforShieldPartActivations > timeBetweenShieldPartActivations) && shieldPartSetCopy.Count > 0) {
                int randNum = Random.Range(0, shieldPartSetCopy.Count);
                ShieldPart randPart = shieldPartSetCopy[randNum];

                DeactivateShieldPart(randPart);
                shieldPartSet.Add(randPart);
                shieldPartSetCopy.Remove(randPart);

                refTimeforShieldPartActivations = Time.time;
            }
        }
    }

    public void ToggleShields() {
        activated = !activated;
        
        //if (activated) { DeactivateShieldSet(shieldPartSet); }
        //else { ActivateShieldSet(shieldPartSet); }
    }

    // This will make a single shield part appear
    private void ActivateShieldPart(ShieldPart part) {
        if (part.part.GetComponent<SkinnedMeshRenderer>() != null) { part.part.GetComponent<SkinnedMeshRenderer>().enabled = true; }
        if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = true; }
        if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = true; }
    }

    // This will make every shield part in the set appear at once
    private void ActivateShieldSet(List<ShieldPart> set) {
        activated = true;
        foreach (ShieldPart part in set) {
            if (part.part.GetComponent<SkinnedMeshRenderer>() != null) { part.part.GetComponent<SkinnedMeshRenderer>().enabled = true; }
            if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = true; }
            if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = true; }
        }
    }

    private void DeactivateShieldPart(ShieldPart part) {
        if (part.part.GetComponent<SkinnedMeshRenderer>() != null) { part.part.GetComponent<SkinnedMeshRenderer>().enabled = false; }
        if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = false; }
        if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = false; }
    }

    private void DeactivateShieldSet(List<ShieldPart> set) {
        activated = false;
        foreach (ShieldPart part in set) {
            if (part.part.GetComponent<SkinnedMeshRenderer>() != null) { part.part.GetComponent<SkinnedMeshRenderer>().enabled = false; }
            if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = false; }
            if (part.part.GetComponent<MeshRenderer>() != null) { part.part.GetComponent<MeshRenderer>().enabled = false; }
        }
    }

    private void InitializePartsInSet(List<ShieldPart> set) {
        foreach (ShieldPart part in set) {
            part.InitializePart();
            part.PlacePart();
        }
    }

    private void CreateTempListOfParts(List<ShieldPart> set) {
        shieldPartSetCopy = set;
    }
}

[System.Serializable]
public class ShieldPart {
    public Transform part;
    public Transform tedBodyPart;
    public Vector3 locationOffset;
    public Collider partCollider;

    private float health;

    private const float healthMax = 100;
    private const float healthMin = 0;

    public void InitializePart() {
        health = healthMax;
    }

    public void PlacePart() {
        part.parent = null;
        part.parent = tedBodyPart;
        //part.localPosition = locationOffset;
    }

    public float CheckHealth() {
        if (health > healthMax) { health = healthMax; }
        else if (health < healthMin) { health = healthMin; }

        return health;
    }
}
