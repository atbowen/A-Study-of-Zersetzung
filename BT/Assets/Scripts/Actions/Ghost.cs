using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float fearFactor;
    public List<SkinnedMeshRenderer> objectsToHideSMR;
    public List<MeshRenderer> objectsToHideMR;
    public List<Collider> objectsHavingCollidersToDisable;
    public Color tint;

    public string hallucinationText;

    private BodyCam bCam;
    private Teddy ted;
    private TeddyRightEye rightEye;
    private CameraMaster camMaster;
    private Fear fearMeter;

    // Start is called before the first frame update
    void Start()
    {
        camMaster = FindObjectOfType<CameraMaster>();
        bCam = FindObjectOfType<BodyCam>();
        ted = FindObjectOfType<Teddy>();
        rightEye = FindObjectOfType<TeddyRightEye>();
        fearMeter = FindObjectOfType<Fear>();

        HideMeshesAndColliders();
        SetTint();
    }

    // Update is called once per frame
    void Update()
    {
        if (camMaster.hallucinating) {
            ShowMeshesAndColliders();
        } else {
            HideMeshesAndColliders();
        }
    }

    void ShowMeshesAndColliders() {
        for (int i = 0; i < objectsToHideMR.Count; i++) {
            objectsToHideMR[i].enabled = true;
        }
        for (int i = 0; i < objectsToHideSMR.Count; i++) {
            objectsToHideSMR[i].enabled = true;
        }
        for (int i = 0; i < objectsHavingCollidersToDisable.Count; i++) {
            objectsHavingCollidersToDisable[i].enabled = true;
        }

        SetTint();
    }

    void HideMeshesAndColliders() {
        for (int i = 0; i < objectsToHideMR.Count; i++) {
            objectsToHideMR[i].enabled = false;
        }
        for (int i = 0; i < objectsToHideSMR.Count; i++) {
            objectsToHideSMR[i].enabled = false;
        }
        for (int i = 0; i < objectsHavingCollidersToDisable.Count; i++) {
            objectsHavingCollidersToDisable[i].enabled = false;
        }
    }

    private void SetTint() {
        foreach (SkinnedMeshRenderer skin in objectsToHideSMR) {
            foreach (Material mat in skin.materials) {
                mat.SetColor("_Color", tint);
            }
        }

        foreach (MeshRenderer tf in objectsToHideMR) {
            MeshRenderer skin = tf.GetComponent<MeshRenderer>();
            Color adjustedAlphaTint = new Color(tint.r, tint.g, tint.b, tint.a / 2);
            foreach (Material mat in skin.materials) {
                mat.SetColor("_Color", adjustedAlphaTint);
            }
        }
    }
}
