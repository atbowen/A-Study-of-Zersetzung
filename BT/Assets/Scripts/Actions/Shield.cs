using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shield : MonoBehaviour {
    public List<AudioClip> partActivationSounds;
    public AudioClip shieldActivationCompletionSound;

    private Teddy ted;
    private ToolSelector toolSelect;
    private MusicPlayer musicBox;
    
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

    public RawImage shieldActivationPopup, littleShieldGuyAvatar;
    public List<Texture> shieldActivationIconFrames, littleShieldGuyActivationFrames;
    [SerializeField]
    private float shieldPopupActivationFrameTime, shieldPopupDeactivationFrameTime;
    private float shieldPopupAnimationRefTime;
    private int shieldPopupAnimationFrameIndex;
    private bool animatingShieldIconBgd, animatingLittleShieldGuyAvatar;


    // Start is called before the first frame update
    void Start()
    {
        musicBox = FindObjectOfType<MusicPlayer>();

        ShieldPart[] shieldPartSetArray = new ShieldPart[] { face, chest, gut, hips, leftUpperArm, leftLowerArm, leftHand,
            rightUpperArm, rightLowerArm, rightHand, leftUpperLeg, leftLowerLeg, rightUpperLeg, rightLowerLeg, leftBoot, rightBoot };
        shieldPartSet = new List<ShieldPart>(shieldPartSetArray);

        shieldPartSetCopy = new List<ShieldPart>();

        activated = false;

        InitializePartsInSet(shieldPartSet);
        DeactivateShieldSet(shieldPartSet);

        refTimeforShieldPartActivations = 0;

        shieldPopupAnimationFrameIndex = 0;

        // Initialize variables for animating the shield popup in the HUD
        shieldActivationPopup.texture = shieldActivationIconFrames[0];
        littleShieldGuyAvatar.texture = littleShieldGuyActivationFrames[0];
        animatingShieldIconBgd = true;
        animatingLittleShieldGuyAvatar = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated) {
            if ((Time.time - refTimeforShieldPartActivations > timeBetweenShieldPartActivations) && shieldPartSet.Count > 0) {
                int randNum = Random.Range(0, shieldPartSet.Count);
                ShieldPart randPart = shieldPartSet[randNum];

                ActivateShieldPart(randPart);
                PlayRandomShieldActivationSound();
                shieldPartSetCopy.Add(randPart);
                shieldPartSet.Remove(randPart);

                refTimeforShieldPartActivations = Time.time;

                if (shieldPartSet.Count == 1) {
                    PlayShieldActivationCompletionSound();
                }
            }

            if (animatingShieldIconBgd) {
                if (Time.time - shieldPopupAnimationRefTime > shieldPopupActivationFrameTime) {
                    if (shieldPopupAnimationFrameIndex < shieldActivationIconFrames.Count - 1) {
                        shieldPopupAnimationFrameIndex++;
                        shieldActivationPopup.texture = shieldActivationIconFrames[shieldPopupAnimationFrameIndex];
                    }
                    else {
                        animatingShieldIconBgd = false;
                        animatingLittleShieldGuyAvatar = true;
                        shieldPopupAnimationFrameIndex = 0;
                    }

                    shieldPopupAnimationRefTime = Time.time;
                }
            } else if (animatingLittleShieldGuyAvatar) {
                if (Time.time - shieldPopupAnimationRefTime > shieldPopupActivationFrameTime) {
                    if (shieldPopupAnimationFrameIndex < littleShieldGuyActivationFrames.Count - 1) {
                        shieldPopupAnimationFrameIndex++;
                        littleShieldGuyAvatar.texture = littleShieldGuyActivationFrames[shieldPopupAnimationFrameIndex];
                    }
                    else {
                        animatingLittleShieldGuyAvatar = false;
                        shieldPopupAnimationFrameIndex = littleShieldGuyActivationFrames.Count - 1;
                    }

                    shieldPopupAnimationRefTime = Time.time;
                }
            }

        } else {
            if ((Time.time - refTimeforShieldPartActivations > timeBetweenShieldPartActivations) && shieldPartSetCopy.Count > 0) {
                int randNum = Random.Range(0, shieldPartSetCopy.Count);
                ShieldPart randPart = shieldPartSetCopy[randNum];

                DeactivateShieldPart(randPart);
                PlayRandomShieldActivationSound();
                shieldPartSet.Add(randPart);
                shieldPartSetCopy.Remove(randPart);

                refTimeforShieldPartActivations = Time.time;
            }

            
            if (animatingLittleShieldGuyAvatar) {
                if (Time.time - shieldPopupAnimationRefTime > shieldPopupDeactivationFrameTime) {
                    if (shieldPopupAnimationFrameIndex > 0) {
                        shieldPopupAnimationFrameIndex--;
                        littleShieldGuyAvatar.texture = littleShieldGuyActivationFrames[shieldPopupAnimationFrameIndex];
                    }
                    else {
                        animatingShieldIconBgd = true;
                        animatingLittleShieldGuyAvatar = false;
                        shieldPopupAnimationFrameIndex = shieldActivationIconFrames.Count - 1;
                    }

                    shieldPopupAnimationRefTime = Time.time;
                }
            } else if (animatingShieldIconBgd) {
                if (Time.time - shieldPopupAnimationRefTime > shieldPopupDeactivationFrameTime) {
                    if (shieldPopupAnimationFrameIndex > 0) {
                        shieldPopupAnimationFrameIndex--;
                        shieldActivationPopup.texture = shieldActivationIconFrames[shieldPopupAnimationFrameIndex];
                    }
                    else {
                        animatingShieldIconBgd = false;
                        shieldPopupAnimationFrameIndex = 0;
                    }

                    shieldPopupAnimationRefTime = Time.time;
                }
            }
        }
    }

    public void ToggleShields() {
        activated = !activated;

        if (activated)  { animatingShieldIconBgd = true; }
        else            { animatingLittleShieldGuyAvatar = true; }

        shieldPopupAnimationRefTime = Time.time;
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

    private void PlayShieldActivationCompletionSound() {
        musicBox.PlaySFX(shieldActivationCompletionSound);
    }

    private void PlayRandomShieldActivationSound() {
        if (partActivationSounds.Count > 0) {
            musicBox.PlaySFX(partActivationSounds[Random.Range(0, partActivationSounds.Count)]);
        }
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
