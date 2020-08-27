using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonManager : MonoBehaviour
{
    public List<PrisonCell> cells;
    public float cageCaptureTime;

    private Transform prisonCage;
    public SkinnedMeshRenderer cageSkin;
    private Animator cageAnim;
    private Transform cageParent, currentSuspect;

    private Vector3 cageInitialPos;

    private bool capturing;
    private float captureRefTime;
    
    // Start is called before the first frame update
    void Start()
    {
        prisonCage = GameObject.Find("Prison Cage").transform;
        //cageSkin = prisonCage.GetComponent<SkinnedMeshRenderer>();
        cageAnim = prisonCage.GetComponent<Animator>();

        cageParent = prisonCage.parent;
        cageInitialPos = prisonCage.localPosition;
        cageSkin.enabled = false;

        //if (cells.Count > 0) {
        //    foreach (PrisonCell room in cells) {
        //        room.arrivalLocationOffset = room.cell.position + room.arrivalLocationOffset;
        //    }
        //}

        capturing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (capturing) {
            if (Time.time - captureRefTime > cageCaptureTime) {
                EndCapture();
            }
        }
    }

    public void TransportSuspectToOpenCell(Transform suspect) {
        if (suspect.Find("ID").GetComponent<ID>()) {
            ID ident = suspect.Find("ID").GetComponent<ID>();

            suspect.GetComponent<Collider>().enabled = false;
            suspect.GetComponent<Rigidbody>().isKinematic = true;

            if (ident.Arrestable) {
                PrisonCell openCell = RetrieveAvailableCell();
                if (openCell != null) {
                    suspect.parent = null;
                    suspect.position = openCell.arrivalLocation.position;

                    suspect.GetComponent<Collider>().enabled = true;
                    suspect.GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
    }

    private PrisonCell RetrieveAvailableCell() {
        bool foundCell = false;

        foreach (PrisonCell room in cells) {
            if (!foundCell) {
                if (!room.occupied) {
                    foundCell = true;
                    return room;
                }
            }
        }

        return null;
    }

    public void CaptureSuspect(Transform suspect, Vector3 offsetPosition) {
        currentSuspect = suspect;
        capturing = true;
        captureRefTime = Time.time;

        prisonCage.parent = null;
        prisonCage.position = suspect.position + offsetPosition;
        cageSkin.enabled = true;

        cageAnim.SetTrigger("close");
    }

    public void EndCapture() {
        TransportSuspectToOpenCell(currentSuspect);
        cageSkin.enabled = false;
        prisonCage.parent = cageParent;
        prisonCage.position = cageInitialPos;

        cageAnim.SetTrigger("open");
        capturing = false;
    }
}

[System.Serializable]
public class PrisonCell {
    public Transform cell;
    public Transform arrivalLocation;
    public bool occupied;
}
