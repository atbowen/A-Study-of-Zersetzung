using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonManager : MonoBehaviour
{
    public List<PrisonCell> cells;
    public string cageFoldTriggerString, cageStayFoldedTriggerString, cageUnfoldTriggerString, cageStayUnfoldedTriggerString;
    public float cageFoldTime, cageStillTime, cageUnfoldTime, coolDownTime;

    [Range(0.0f, 1.0f)]
    public float cageInitialAlpha, cageMaxAlpha;

    public float cageAlphaChangeInterval;

    public Transform prisonCage;
    public SkinnedMeshRenderer cageSkin;

    private bool cageActivated;

    private enum CageState { Inactive, FoldingAtSuspectPosition, FoldedAndHoldingAtSuspectPosition, FoldedAndHoldingAtCellPosition, UnfoldingInCell }
    private CageState stateOfCage;
    
    private Animator cageAnim;
    private Transform cageParent;
    private ID currentSuspect;

    private Vector3 cageInitialPos;

    private float cageStepRefTime;
    
    // Start is called before the first frame update
    void Start()
    {
        cageAnim = prisonCage.GetComponent<Animator>();

        cageParent = prisonCage.parent;
        cageInitialPos = prisonCage.localPosition;
        cageSkin.enabled = false;

        //if (cells.Count > 0) {
        //    foreach (PrisonCell room in cells) {
        //        room.arrivalLocationOffset = room.cell.position + room.arrivalLocationOffset;
        //    }
        //}

        Color lowAlphaCageColor = new Color(cageSkin.material.color.r, cageSkin.material.color.g, cageSkin.material.color.b, cageInitialAlpha);
        cageSkin.material.color = lowAlphaCageColor;

        stateOfCage = CageState.Inactive;
        cageStepRefTime = Time.time;

        cageActivated = false;
    }

    // Update is called once per frame
    void Update()
    {
        // This manages the behavior of the cage when capturing an individual
        switch (stateOfCage) {
            case CageState.Inactive:

                if (cageActivated && currentSuspect != null) {

                    cageActivated = false;

                    prisonCage.SetParent(null);
                    prisonCage.position = currentSuspect.GetSelf().position + currentSuspect.OffsetForPrisonCage;
                    cageSkin.enabled = true;

                    currentSuspect.GetSelf().SetParent(prisonCage);
                    if (currentSuspect.GetSelf().GetComponent<CyclopsAI>() != null) { currentSuspect.GetSelf().GetComponent<CyclopsAI>().GotPinched(); }
                    if (currentSuspect.GetSelf().GetComponent<Collider>() != null) { currentSuspect.GetSelf().GetComponent<Collider>().enabled = false; }
                    if (currentSuspect.GetSelf().GetComponent<Rigidbody>() != null) { currentSuspect.GetSelf().GetComponent<Rigidbody>().isKinematic = true; }

                    cageAnim.SetTrigger(cageFoldTriggerString);

                    stateOfCage = CageState.FoldingAtSuspectPosition;
                    cageStepRefTime = Time.time;
                }
                break;
            case CageState.FoldingAtSuspectPosition:
                if (Time.time - cageStepRefTime > cageFoldTime) {

                    cageAnim.SetTrigger(cageStayFoldedTriggerString);

                    stateOfCage = CageState.FoldedAndHoldingAtSuspectPosition;
                    cageStepRefTime = Time.time;
                }
                break;
            case CageState.FoldedAndHoldingAtSuspectPosition:

                IncreaseCageAlpha();

                if (Time.time - cageStepRefTime > cageStillTime) {
                    TransportSuspectToOpenCell();

                    stateOfCage = CageState.FoldedAndHoldingAtCellPosition;
                    cageStepRefTime = Time.time;
                }
                break;
            case CageState.FoldedAndHoldingAtCellPosition:

                DecreaseCageAlpha();

                if (Time.time - cageStepRefTime > cageStillTime) {
                    cageAnim.SetTrigger(cageUnfoldTriggerString);

                    stateOfCage = CageState.UnfoldingInCell;
                    cageStepRefTime = Time.time;
                }

                break;
            case CageState.UnfoldingInCell:
                if (Time.time - cageStepRefTime > cageUnfoldTime) {
                    cageSkin.enabled = false;
                    currentSuspect.GetSelf().SetParent(null);
                    prisonCage.SetParent(cageParent);
                    prisonCage.position = cageInitialPos;

                    cageAnim.SetTrigger(cageStayUnfoldedTriggerString);

                    stateOfCage = CageState.Inactive;
                    cageStepRefTime = Time.time;
                }
                break;
        }


    }

    public void TransportSuspectToOpenCell() {
        //if (suspect.Find("ID").GetComponent<IDCharacter>()) {
        //    IDCharacter ident = suspect.Find("ID").GetComponent<IDCharacter>();

        //    suspect.GetComponent<Collider>().enabled = false;
        //    suspect.GetComponent<Rigidbody>().isKinematic = true;

        //    if (ident.arrestable) {
        //        PrisonCell openCell = RetrieveAvailableCell();
        //        if (openCell != null) {
        //            suspect.parent = null;
        //            suspect.position = openCell.arrivalLocation.position;

        //            suspect.GetComponent<Collider>().enabled = true;
        //            suspect.GetComponent<Rigidbody>().isKinematic = false;
        //        }
        //    }
        //}

        if (currentSuspect != null) {
            Transform susp = currentSuspect.GetSelf();

            //if (susp.GetComponent<Collider>() != null) { susp.GetComponent<Collider>().enabled = false; }
            //if (susp.GetComponent<Rigidbody>() != null) { susp.GetComponent<Rigidbody>().isKinematic = true; }

            PrisonCell openCell = RetrieveAvailableCell();
            if (openCell != null) {
                prisonCage.position = openCell.arrivalLocation.position;
                susp.SetParent(null);

                if (susp.GetComponent<Collider>() != null) { susp.GetComponent<Collider>().enabled = true; }
                if (susp.GetComponent<Rigidbody>() != null) { susp.GetComponent<Rigidbody>().isKinematic = false; }
            }
        }
    }

    private PrisonCell RetrieveAvailableCell() {

        foreach (PrisonCell room in cells) {
            if (!room.occupied) { return room; }
        }

        return null;
    }

    public void InitiateCapture(ID suspect) {
        if (Time.time - cageStepRefTime > coolDownTime) {
            currentSuspect = suspect;
            cageActivated = true;
        }
    }

    //public void CaptureSuspect() {
    //    cageProcessingStartTime = Time.time;

    //    prisonCage.parent = null;
    //    prisonCage.position = currentSuspect.GetSelf().position + currentSuspect.OffsetForPrisonCage;
    //    cageSkin.enabled = true;

    //    cageAnim.SetTrigger("fold");
    //}

    //public void EndCapture() {
    //    TransportSuspectToOpenCell(currentSuspect.GetSelf());
    //    cageSkin.enabled = false;
    //    prisonCage.parent = cageParent;
    //    prisonCage.position = cageInitialPos;

    //    cageAnim.SetTrigger("unfold");

    //    stateOfCage = CageState.Inactive;
    //    //capturing = false;
    //}

    private void IncreaseCageAlpha() {
        if (cageSkin.material.color.a < cageMaxAlpha) {
            cageSkin.material.color = new Color(cageSkin.material.color.r, cageSkin.material.color.g, cageSkin.material.color.b, cageSkin.material.color.a + (cageAlphaChangeInterval * Time.deltaTime));
        }
    }

    private void DecreaseCageAlpha() {
        if (cageSkin.material.color.a > cageInitialAlpha) {
            cageSkin.material.color = new Color(cageSkin.material.color.r, cageSkin.material.color.g, cageSkin.material.color.b, cageSkin.material.color.a - (cageAlphaChangeInterval * Time.deltaTime));
        }
    }
}

[System.Serializable]
public class PrisonCell {
    public Transform cell;
    public Transform arrivalLocation;
    public bool occupied;
}
