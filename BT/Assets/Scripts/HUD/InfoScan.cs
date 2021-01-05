using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoScan : MonoBehaviour {
    public Text infoMsg, creditChitTxt, notepadPickupTxt, notepadPickupRiskRating;
    public RawImage reticle, targetingReticle, creditChitBgd, notepadPickupBgd;
    public Transform bgdPanels;
    public Texture scanningTexture, reticleView, targetRetView, reticleTraj, targetRetTraj, reticleMatSci, targetRetMatSci,
                reticleImageAn, targetRetImageAn, reticleSecurity, targetRetSecurity;
    public string idleText;
    public float scanTime, panelSizingRate, boundingBoxLineStretchTime;
    public string displayText, displayModeText;
    public Color targetTextColor, unknownTextColor, fieldHeaderColor, DisplayModeColor, DisplayModeOutOfRangeColor,
                boundingBoxNoColor, boundingBoxOutOfRangeColor, boundingBoxFriendColor, boundingBoxNeutralColor, boundingBoxDangerousColor, boundingBoxUsableColor;

    public float dimAdjustmentMultiplier;               // Tweaks method which adjusts bounding box line length to make lines for bigger objects skew shorter; a higher number increases general line length
    public float boundingBoxOutOfRangeAlphaReducer;     // Reduces main/start color of bounding box lines when target is out of range; higher the number, lower the alpha

    private BodyCam bCam;
    private ID currentID;
    //private LineRenderer[] bbRend = new LineRenderer[24];

    [SerializeField]
    private List<LineRenderer> bbRend = new List<LineRenderer>();
    [SerializeField]
    private float bbLineRendererDepth, bbLineRendererThickness, bbLineRendThicknessFOVCoeff, bbLineRendBaseLength;

    private Text scan;
    private Panels panels;
    private bool moneyPanelIsExpanding, moneyPanelIsShrinking,
                 showBoundBoxOfCurrentID;
    private Vector2 moneyPanelSize, moneyPanelTextSize;

    private float boundingBoxLineStretchTimerRef;
    private bool onTarget, hidingReticle;
    
    private CameraMaster camMaster;

	// Use this for initialization
	void Start () {
        scan = this.GetComponent<Text>();
        bCam = FindObjectOfType<BodyCam>();
        camMaster = FindObjectOfType<CameraMaster>();
        panels = bgdPanels.GetComponent<Panels>();

        //for (int i = 0; i < bbRend.Length; i++) {
        //    bbRend[i] = this.transform.Find("TargetInfoBox").Find("Bounding Box Lines").GetChild(i).GetComponent<LineRenderer>();
        //}

        scan.supportRichText = true;
        infoMsg.supportRichText = true;
        creditChitTxt.supportRichText = true;
        notepadPickupTxt.supportRichText = true;

        targetingReticle.enabled = false;

        scan.color = DisplayModeColor;
        scan.enabled = false;

        displayText = displayModeText;
        //idleText = "Scanning...";

        moneyPanelIsExpanding = false;
        moneyPanelIsShrinking = false;

        moneyPanelSize = creditChitBgd.rectTransform.sizeDelta;
        moneyPanelTextSize = creditChitTxt.rectTransform.sizeDelta;
        creditChitBgd.rectTransform.sizeDelta = moneyPanelSize * 0.1f;
        creditChitTxt.rectTransform.sizeDelta = moneyPanelTextSize * 0.1f;

        showBoundBoxOfCurrentID = false;

        boundingBoxLineStretchTimerRef = 0;
        onTarget = false;
    }
	
	// Update is called once per frame
	void Update () {

        scan.text = displayText;

        // Display bounding box corner graphics of current ID target
        if (showBoundBoxOfCurrentID) {
            DisplayBoundingBoxCorners();
        }
        else {
            foreach (LineRenderer rend in bbRend) {
                rend.enabled = false;
            }
        }

        // Control the resizing of money popups
        if (moneyPanelIsExpanding) {
            if (creditChitBgd.rectTransform.sizeDelta.magnitude < moneyPanelSize.magnitude) {
                creditChitBgd.rectTransform.sizeDelta = creditChitBgd.rectTransform.sizeDelta * (1 + panelSizingRate * Time.deltaTime);
                creditChitTxt.rectTransform.sizeDelta = creditChitTxt.rectTransform.sizeDelta * (1 + panelSizingRate * Time.deltaTime);
            }
            else {
                creditChitBgd.rectTransform.sizeDelta = moneyPanelSize;
                creditChitTxt.rectTransform.sizeDelta = moneyPanelTextSize;
                creditChitTxt.enabled = true;
            }
        }

        if (moneyPanelIsShrinking) {
            creditChitTxt.enabled = false;
            creditChitTxt.text = "";
            if (creditChitBgd.rectTransform.sizeDelta.magnitude > (moneyPanelSize.magnitude * 0.1)) {
                creditChitBgd.rectTransform.sizeDelta = creditChitBgd.rectTransform.sizeDelta * 1 / (1 + panelSizingRate * Time.deltaTime);
                creditChitTxt.rectTransform.sizeDelta = creditChitTxt.rectTransform.sizeDelta * 1 / (1 + panelSizingRate * Time.deltaTime);
            }
            else {
                creditChitBgd.rectTransform.sizeDelta = moneyPanelSize * 0.1f;
                creditChitTxt.rectTransform.sizeDelta = moneyPanelTextSize * 0.1f;
                creditChitBgd.enabled = false;
            }
        }
    }

    // Used when the right eye reticle is enabled and is pointing at an object with an ID
    // This the standard popup
    public void EnableInfoPanelWithID(ID ident) {

        SetBBColor(ident);

        notepadPickupBgd.enabled = false;
        notepadPickupTxt.text = "";
        notepadPickupRiskRating.text = "";

        if (ident.ObjName != null && ident.ObjName != "" && ident.ObjDescription != null && ident.ObjStatus != null) {

            // Set currentID to currently targeted ID to make available to other functions
            currentID = ident;

            if (!hidingReticle) {
                if (camMaster.reticleEnabled) {
                    targetingReticle.enabled = true;
                    reticle.enabled = false;
                }
                else {
                    targetingReticle.enabled = false;
                    reticle.enabled = false;
                }
            }

            // Close money popup
            creditChitBgd.enabled = false;
            creditChitTxt.enabled = false;
            creditChitTxt.text = "";
            creditChitBgd.rectTransform.sizeDelta = moneyPanelSize * 0.1f;
            creditChitTxt.rectTransform.sizeDelta = moneyPanelTextSize * 0.1f;

            if (camMaster.reticleEnabled) {

                displayText = ident.ObjName;

                if (!hidingReticle) {
                    DisplayReticleText();
                }

                string hexHeader = ColorUtility.ToHtmlStringRGBA(fieldHeaderColor);
                string hexKnown = ColorUtility.ToHtmlStringRGBA(targetTextColor);
                string hexUnknown = ColorUtility.ToHtmlStringRGBA(unknownTextColor);

                if (ident.KnowActName) { infoMsg.text = "<color=#" + hexHeader + ">Nm-</color>" + ident.ObjActualName + "\n"; }
                else { infoMsg.text = "<color=#" + hexHeader + ">Nm-</color><color=#" + hexUnknown + ">" + ident.UnknownField + "</color>\n"; }

                if (ident.KnowAvatarName) { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Av-</color>" + ident.ObjAvatarName + "\n\n"; }
                else { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Av-</color><color=#" + hexUnknown + ">" + ident.UnknownField + "</color>\n\n"; }

                if (ident.KnowDescription) { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Description</color>\n" + ident.ObjDescription + "\n\n"; }
                else { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Description</color>\n<color=#" + hexUnknown + ">" + ident.UnknownField + "</color>\n\n\n"; }

                if (ident.KnowStatus) { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Status</color>\n<color=#" + hexKnown + ">" + ident.ObjStatus + "</color>"; }
                else { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Status</color>\n<color=#" + hexUnknown + ">" + ident.UnknownField + "</color>"; }

                panels.ShowInfoBgdPanel(infoMsg.cachedTextGenerator.lineCount);

                // Start showing the bounding box!
                if (!onTarget) {
                    boundingBoxLineStretchTimerRef = Time.time;
                    onTarget = true;
                }
                showBoundBoxOfCurrentID = true;
            }
            else {
                // Clear message, hide panels, and no more bounding box
                HideReticleText();

                infoMsg.text = "";
                panels.HideInfoBgdPanel();
                onTarget = false;
                showBoundBoxOfCurrentID = false;
            }

            
        }
    }

    public void EnableMessagePickupWithID(ID ident) {
        SetBBColor(ident);

        moneyPanelIsShrinking = true;
        moneyPanelIsExpanding = false;
        panels.HideInfoBgdPanel();
        infoMsg.text = "";

        IDBurnerMessage targetedID = (IDBurnerMessage)ident;
        currentID = targetedID;

        if (!hidingReticle) {
            if (camMaster.reticleEnabled) {
                targetingReticle.enabled = true;
                reticle.enabled = false;
            }
            else {
                targetingReticle.enabled = false;
                reticle.enabled = false;
            }
        }

        if (camMaster.reticleEnabled) {

            displayText = ident.ObjName;

            if (!hidingReticle) {
                DisplayReticleText();
            }

            notepadPickupTxt.color = fieldHeaderColor;
            notepadPickupRiskRating.color = targetTextColor;

            if (targetedID.messages.Count == 1) {
                notepadPickupTxt.text = targetedID.messages[0].senderInfo;
            } else if (targetedID.messages.Count > 1) {
                notepadPickupTxt.text = targetedID.nameOfCollection;
            }

            notepadPickupBgd.enabled = true;
            notepadPickupRiskRating.text = targetedID.CalculateCumulativeRisk().ToString("F1");

            // Start showing the bounding box!
            if (!onTarget) {
                boundingBoxLineStretchTimerRef = Time.time;
                onTarget = true;
            }
            showBoundBoxOfCurrentID = true;
        }
        else {
            notepadPickupTxt.text = "";
            notepadPickupRiskRating.text = "";
            notepadPickupBgd.enabled = false;
            onTarget = false;
            showBoundBoxOfCurrentID = false;
        }
    }

    // Same functionality as EnableInfoPanelWithID, but for money pickups
    public void EnableCreditPopup(ID ident) {
        SetBBColor(ident);

        panels.HideInfoBgdPanel();
        infoMsg.text = "";
        notepadPickupBgd.enabled = false;
        notepadPickupTxt.text = "";
        notepadPickupRiskRating.text = "";

        IDMoney targetedID = (IDMoney)ident;
        currentID = targetedID;

        if (!hidingReticle) {
            if (camMaster.reticleEnabled) {
                targetingReticle.enabled = true;
                reticle.enabled = false;
            }
            else {
                targetingReticle.enabled = false;
                reticle.enabled = false;
            }
        }

        if (camMaster.reticleEnabled) {

            displayText = ident.ObjName;

            if (!hidingReticle) {
                DisplayReticleText();
            }

            string hexKnown = ColorUtility.ToHtmlStringRGBA(targetTextColor);
            string hexUnknown = ColorUtility.ToHtmlStringRGBA(unknownTextColor);

            creditChitTxt.text = targetedID.amount.ToString();
            if (targetedID.owner != "") { creditChitTxt.text = creditChitTxt.text + "\n" + targetedID.owner; }
            else { creditChitTxt.text = creditChitTxt.text + "\n<color=#" + hexUnknown + ">Private</color>"; }

            if (targetedID.issuer != "") { creditChitTxt.text = creditChitTxt.text + "\n" + targetedID.issuer; }
            else { creditChitTxt.text = creditChitTxt.text + "\n<color=#" + hexUnknown + ">Unknown</color>"; }

            creditChitBgd.enabled = true;
            moneyPanelIsExpanding = true;
            moneyPanelIsShrinking = false;

            if (!onTarget) {
                boundingBoxLineStretchTimerRef = Time.time;
                onTarget = true;
            }
            showBoundBoxOfCurrentID = true;
        }
        else {
            //creditChitTxt.text = "";
            //creditChitBgd.enabled = false;
            HideReticleText();

            moneyPanelIsShrinking = true;
            moneyPanelIsExpanding = false;
            onTarget = false;
            showBoundBoxOfCurrentID = false;
        }
    }

    public void LookingAtUnscannable(ID ident) {

        if (!hidingReticle) {
            if (camMaster.reticleEnabled) {
                targetingReticle.enabled = true;
                reticle.enabled = false;
            }
            else {
                targetingReticle.enabled = false;
                reticle.enabled = false;
            }
        }

        if (camMaster.reticleEnabled) {
            displayText = ident.ObjName;

            if (!hidingReticle) {
                DisplayReticleText();
            }
        }
        else {
            HideReticleText();
        }
    }

    public void LookingAtVaultObject(ID ident, Transform vaulter) {
        IDVaultObject targetedID = (IDVaultObject)ident;
        if (!hidingReticle) {
            if (camMaster.reticleEnabled) {
                targetingReticle.enabled = true;
                reticle.enabled = false;
            }
            else {
                targetingReticle.enabled = false;
                reticle.enabled = false;
            }
        }

        if (camMaster.reticleEnabled) {
            displayText = targetedID.ObjName;

            if (!hidingReticle) {
                DisplayReticleText();
            }
        }
        else {
            HideReticleText();
        }
    }

    public void DisableInfoPanel() {
        scan.enabled = false;
        scan.text = "";

        infoMsg.text = "";
        panels.HideInfoBgdPanel();
        //creditChitTxt.text = "";
        //creditChitBgd.enabled = false;
        moneyPanelIsShrinking = true;
        moneyPanelIsExpanding = false;
        notepadPickupBgd.enabled = false;
        notepadPickupTxt.text = "";
        notepadPickupRiskRating.text = "";
        onTarget = false;
        showBoundBoxOfCurrentID = false;

        if (camMaster.reticleEnabled) {
            reticle.enabled = true;
            targetingReticle.enabled = false;
        }
        else {
            reticle.enabled = false;
            targetingReticle.enabled = false;
        }
    }

    public void DisplayBoundingBoxCorners() {

        //Transform parentOfID = currentID.transform.parent;

        Vector3 boundBoxCenter = Vector3.zero;
        float bbDepth = 0;
        float bbWidth = 0;
        float bbHeight = 0;
        float minDimLength;

        Vector3 corner1, corner2, corner3, corner4, corner5, corner6, corner7, corner8;
        corner1 = Vector3.zero;
        corner2 = Vector3.zero;
        corner3 = Vector3.zero;
        corner4 = Vector3.zero;
        corner5 = Vector3.zero;
        corner6 = Vector3.zero;
        corner7 = Vector3.zero;
        corner8 = Vector3.zero;

        minDimLength = 0;

        bool colliderFound = false;
        bool switchingColliders;

        IDInteractable IDThing = null;

        if (currentID.GetType() == typeof(IDInteractable)) {
            IDThing = (IDInteractable)currentID;

            if (IDThing.switchColliderWithPointerColliders) { switchingColliders = true; }
            else                                            { switchingColliders = false; }
        }
        else {
            switchingColliders = false;
        }

        //float distanceToTarget = 0;

        if (switchingColliders && IDThing != null) {

            if (IDThing.switchColliders.Count > 0) {
                colliderFound = true;

                //foreach (Collider col in IDThing.switchColliders) {
                //    boundBoxCenter += col.bounds.center;
                //}

                //boundBoxCenter = boundBoxCenter / (IDThing.switchColliders.Count);

                Vector3 firstColExtents = IDThing.switchColliders[0].bounds.extents;
                Vector3 firstColCenter = IDThing.switchColliders[0].bounds.center;

                float minX, maxX, minY, maxY, minZ, maxZ;

                minX = Mathf.Min(firstColCenter.x + firstColExtents.x, firstColCenter.x - firstColExtents.x);
                maxX = Mathf.Max(firstColCenter.x + firstColExtents.x, firstColCenter.x - firstColExtents.x);
                minY = Mathf.Min(firstColCenter.y + firstColExtents.y, firstColCenter.y - firstColExtents.y);
                maxY = Mathf.Max(firstColCenter.y + firstColExtents.y, firstColCenter.y - firstColExtents.y);
                minZ = Mathf.Min(firstColCenter.z + firstColExtents.z, firstColCenter.z - firstColExtents.z);
                maxZ = Mathf.Max(firstColCenter.z + firstColExtents.z, firstColCenter.z - firstColExtents.z);

                foreach (Collider col in IDThing.switchColliders) {
                    minX = Mathf.Min(minX, col.bounds.center.x + col.bounds.extents.x, col.bounds.center.x - col.bounds.extents.x);
                    maxX = Mathf.Max(maxX, col.bounds.center.x + col.bounds.extents.x, col.bounds.center.x - col.bounds.extents.x);
                    minY = Mathf.Min(minY, col.bounds.center.y + col.bounds.extents.y, col.bounds.center.y - col.bounds.extents.y);
                    maxY = Mathf.Max(maxY, col.bounds.center.y + col.bounds.extents.y, col.bounds.center.y - col.bounds.extents.y);
                    minZ = Mathf.Min(minZ, col.bounds.center.z + col.bounds.extents.z, col.bounds.center.z - col.bounds.extents.z);
                    maxZ = Mathf.Max(maxZ, col.bounds.center.z + col.bounds.extents.z, col.bounds.center.z - col.bounds.extents.z);
                }

                boundBoxCenter = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

                bbWidth = (Mathf.Abs(firstColCenter.x - boundBoxCenter.x) + firstColExtents.x);
                bbHeight = (Mathf.Abs(firstColCenter.y - boundBoxCenter.y) + firstColExtents.y);
                bbDepth = (Mathf.Abs(firstColCenter.z - boundBoxCenter.z) + firstColExtents.z);

                foreach (Collider col in IDThing.switchColliders) {
                    bbWidth = Mathf.Max(bbWidth, Mathf.Abs(col.bounds.center.x - boundBoxCenter.x) + col.bounds.extents.x);
                    bbHeight = Mathf.Max(bbHeight, Mathf.Abs(col.bounds.center.y - boundBoxCenter.y) + col.bounds.extents.y);
                    bbDepth = Mathf.Max(bbDepth, Mathf.Abs(col.bounds.center.z - boundBoxCenter.z) + col.bounds.extents.z);
                }
            }

            //distanceToTarget = (IDThing.currentSwitchCollider.ClosestPoint(bCam.transform.position) - bCam.transform.position).magnitude;
        } else {

            if (currentID.GetComponent<Collider>() != null) {
                colliderFound = true;
                Collider coll = currentID.GetComponent<Collider>();
                boundBoxCenter = coll.bounds.center;

                bbDepth = coll.bounds.extents.z;
                bbWidth = coll.bounds.extents.x;
                bbHeight = coll.bounds.extents.y;

                //distanceToTarget = (parentOfID.transform.GetComponent<Collider>().ClosestPoint(bCam.transform.position) - bCam.transform.position).magnitude;

            }
            else if (currentID.GetComponent<CapsuleCollider>() != null) {

                colliderFound = true;
                CapsuleCollider coll = currentID.GetComponent<CapsuleCollider>();
                boundBoxCenter = coll.bounds.center;

                bbDepth = coll.bounds.extents.z;
                bbWidth = coll.bounds.extents.x;
                bbHeight = coll.bounds.extents.y;

                //distanceToTarget = (parentOfID.transform.GetComponent<CapsuleCollider>().ClosestPoint(bCam.transform.position) - bCam.transform.position).magnitude;

            }

            //colliderFound = true;

            //Mesh mesh = parentOfID.GetComponent<MeshFilter>().sharedMesh;

            //boundBoxCenter = parentOfID.TransformPoint(mesh.bounds.center);

            //// Meshes imported from blender must be flipped -90 on x (?)
            //bbDepth = mesh.bounds.extents.y;
            //bbWidth = mesh.bounds.extents.x;
            //bbHeight = mesh.bounds.extents.z;

            //Debug.Log(boundBoxCenter + ", " + bbDepth);
        }        

        if (colliderFound) {

            float distanceToTarget = currentID.GetDistanceToActiveIDCollider(bCam.transform.position);

            minDimLength = Mathf.Min(ObjectDimensionAdjuster(bbDepth), ObjectDimensionAdjuster(bbWidth), ObjectDimensionAdjuster(bbHeight));

            corner1 = boundBoxCenter + new Vector3(bbWidth, bbHeight, bbDepth);
            corner2 = boundBoxCenter + new Vector3(-bbWidth, bbHeight, bbDepth);
            corner3 = boundBoxCenter + new Vector3(bbWidth, bbHeight, -bbDepth);
            corner4 = boundBoxCenter + new Vector3(-bbWidth, bbHeight, -bbDepth);
            corner5 = boundBoxCenter + new Vector3(bbWidth, -bbHeight, bbDepth);
            corner6 = boundBoxCenter + new Vector3(-bbWidth, -bbHeight, bbDepth);
            corner7 = boundBoxCenter + new Vector3(bbWidth, -bbHeight, -bbDepth);
            corner8 = boundBoxCenter + new Vector3(-bbWidth, -bbHeight, -bbDepth);

            List<Vector3> corners = new List<Vector3>() { corner1, corner2, corner3, corner4, corner5, corner6, corner7, corner8 };

            Camera cam = bCam.transform.GetComponent<Camera>();

            float minX = cam.WorldToViewportPoint(corner1).x;
            float maxX = cam.WorldToViewportPoint(corner1).x;
            float minY = cam.WorldToViewportPoint(corner1).y;
            float maxY = cam.WorldToViewportPoint(corner1).y;

            foreach (Vector3 corner in corners) {
                minX = Mathf.Min(cam.WorldToViewportPoint(corner).x, minX);
                maxX = Mathf.Max(cam.WorldToViewportPoint(corner).x, maxX);
                minY = Mathf.Min(cam.WorldToViewportPoint(corner).y, minY);
                maxY = Mathf.Max(cam.WorldToViewportPoint(corner).y, maxY);
            }

            float bbBoxWidth = maxX - minX;
            float bbBoxHeight = maxY - minY;

            foreach (LineRenderer rend in bbRend) {
                rend.enabled = true;
                rend.positionCount = 3;
                rend.startWidth = bbLineRendererThickness * bCam.GetFOV() * bbLineRendThicknessFOVCoeff;
                rend.endWidth = bbLineRendererThickness * bCam.GetFOV() * bbLineRendThicknessFOVCoeff;
            }

            float segX = Mathf.Sqrt(bbBoxWidth) * bbLineRendBaseLength;
            float segY = Mathf.Sqrt(bbBoxHeight) * bbLineRendBaseLength;

            bbRend[0].SetPosition(0, cam.ViewportToWorldPoint(new Vector3(minX,         maxY - segY,    bbLineRendererDepth)));
            bbRend[0].SetPosition(1, cam.ViewportToWorldPoint(new Vector3(minX,         maxY,           bbLineRendererDepth)));
            bbRend[0].SetPosition(2, cam.ViewportToWorldPoint(new Vector3(minX + segX,  maxY,           bbLineRendererDepth)));

            bbRend[1].SetPosition(0, cam.ViewportToWorldPoint(new Vector3(maxX - segX,  maxY,           bbLineRendererDepth)));
            bbRend[1].SetPosition(1, cam.ViewportToWorldPoint(new Vector3(maxX,         maxY,           bbLineRendererDepth)));
            bbRend[1].SetPosition(2, cam.ViewportToWorldPoint(new Vector3(maxX,         maxY - segY,    bbLineRendererDepth)));

            bbRend[2].SetPosition(0, cam.ViewportToWorldPoint(new Vector3(maxX,         minY + segY,    bbLineRendererDepth)));
            bbRend[2].SetPosition(1, cam.ViewportToWorldPoint(new Vector3(maxX,         minY,           bbLineRendererDepth)));
            bbRend[2].SetPosition(2, cam.ViewportToWorldPoint(new Vector3(maxX - segX,  minY,           bbLineRendererDepth)));

            bbRend[3].SetPosition(0, cam.ViewportToWorldPoint(new Vector3(minX + segX,  minY,           bbLineRendererDepth)));
            bbRend[3].SetPosition(1, cam.ViewportToWorldPoint(new Vector3(minX,         minY,           bbLineRendererDepth)));
            bbRend[3].SetPosition(2, cam.ViewportToWorldPoint(new Vector3(minX,         minY + segY,    bbLineRendererDepth)));

            //float seg;
            //if (Time.time - boundingBoxLineStretchTimerRef < boundingBoxLineStretchTime) {
            //    seg = (minDimLength * (distanceToTarget / 500) * (bCam.GetFOV() * 0.01f) + 0.5f) * (Time.time - boundingBoxLineStretchTimerRef) / boundingBoxLineStretchTime;
            //}
            //else {
            //    seg = (minDimLength * (distanceToTarget / 500) * (bCam.GetFOV() * 0.01f) + 0.5f);
            //}
            //float segWidth = 0.01f * distanceToTarget * (bCam.GetFOV() * 0.01f) + 0.1f;

            //for (int i = 0; i < bbRend.Length; i++) {
            //    bbRend[i].enabled = true;
            //    bbRend[i].positionCount = 5;
            //    bbRend[i].startWidth = segWidth;
            //    bbRend[i].endWidth = segWidth;
            //}

            //bbRend[0].SetPosition(1, corner1 + new Vector3(-seg, 0, 0));
            //bbRend[1].SetPosition(0, corner1);
            //bbRend[1].SetPosition(1, corner1 + new Vector3(0, -seg, 0));
            //bbRend[2].SetPosition(0, corner1);
            //bbRend[2].SetPosition(1, corner1 + new Vector3(0, 0, -seg));

            //bbRend[3].SetPosition(0, corner2);
            //bbRend[3].SetPosition(1, corner2 + new Vector3(seg, 0, 0));
            //bbRend[4].SetPosition(0, corner2);
            //bbRend[4].SetPosition(1, corner2 + new Vector3(0, -seg, 0));
            //bbRend[5].SetPosition(0, corner2);
            //bbRend[5].SetPosition(1, corner2 + new Vector3(0, 0, -seg));

            //bbRend[6].SetPosition(0, corner3);
            //bbRend[6].SetPosition(1, corner3 + new Vector3(-seg, 0, 0));
            //bbRend[7].SetPosition(0, corner3);
            //bbRend[7].SetPosition(1, corner3 + new Vector3(0, -seg, 0));
            //bbRend[8].SetPosition(0, corner3);
            //bbRend[8].SetPosition(1, corner3 + new Vector3(0, 0, seg));

            //bbRend[9].SetPosition(0, corner4);
            //bbRend[9].SetPosition(1, corner4 + new Vector3(seg, 0, 0));
            //bbRend[10].SetPosition(0, corner4);
            //bbRend[10].SetPosition(1, corner4 + new Vector3(0, -seg, 0));
            //bbRend[11].SetPosition(0, corner4);
            //bbRend[11].SetPosition(1, corner4 + new Vector3(0, 0, seg));

            //bbRend[12].SetPosition(0, corner5);
            //bbRend[12].SetPosition(1, corner5 + new Vector3(-seg, 0, 0));
            //bbRend[13].SetPosition(0, corner5);
            //bbRend[13].SetPosition(1, corner5 + new Vector3(0, seg, 0));
            //bbRend[14].SetPosition(0, corner5);
            //bbRend[14].SetPosition(1, corner5 + new Vector3(0, 0, -seg));

            //bbRend[15].SetPosition(0, corner6);
            //bbRend[15].SetPosition(1, corner6 + new Vector3(seg, 0, 0));
            //bbRend[16].SetPosition(0, corner6);
            //bbRend[16].SetPosition(1, corner6 + new Vector3(0, seg, 0));
            //bbRend[17].SetPosition(0, corner6);
            //bbRend[17].SetPosition(1, corner6 + new Vector3(0, 0, -seg));

            //bbRend[18].SetPosition(0, corner7);
            //bbRend[18].SetPosition(1, corner7 + new Vector3(-seg, 0, 0));
            //bbRend[19].SetPosition(0, corner7);
            //bbRend[19].SetPosition(1, corner7 + new Vector3(0, seg, 0));
            //bbRend[20].SetPosition(0, corner7);
            //bbRend[20].SetPosition(1, corner7 + new Vector3(0, 0, seg));

            //bbRend[21].SetPosition(0, corner8);
            //bbRend[21].SetPosition(1, corner8 + new Vector3(seg, 0, 0));
            //bbRend[22].SetPosition(0, corner8);
            //bbRend[22].SetPosition(1, corner8 + new Vector3(0, seg, 0));
            //bbRend[23].SetPosition(0, corner8);
            //bbRend[23].SetPosition(1, corner8 + new Vector3(0, 0, seg));


        }

    }

    //public void DisplayTextUsingTargetingColor(string text) {
    //    scan.color = targetTextColor;
    //    displayText = text;
    //}

    //public void DisplayTextUsingModeColor(string text) {
    //    scan.color = DisplayModeColor;
    //    displayText = text;
    //}

    public void DisplayReticleText() {
        scan.text = displayText;

        if (currentID != null) {

            float distanceToID;
            Collider col = null;

            if (currentID.GetType() == typeof(IDInteractable)) {
                IDInteractable IDThing = (IDInteractable)currentID;

                if (IDThing.switchColliderWithPointerColliders && IDThing.currentSwitchCollider != null) {
                    col = IDThing.currentSwitchCollider;
                }
                else {
                    col = IDThing.transform.GetComponent<Collider>();
                }
            }
            else { col = currentID.transform.GetComponent<Collider>(); }


            distanceToID = Vector3.Distance(col.ClosestPoint(bCam.transform.position), bCam.transform.position);

            if (distanceToID < currentID.maxDistanceToActivate) { scan.color = DisplayModeColor; }
            else                                                { scan.color = DisplayModeOutOfRangeColor; }
        }

        scan.enabled = true;
    }    

    public void DisplayToolText(string text) {
        scan.color = DisplayModeColor;
        scan.text = text;
    }

    private void SetBBColor(ID IDThing) {

        Color startColor, endColor;
        startColor = boundingBoxOutOfRangeColor;
        endColor = boundingBoxOutOfRangeColor;

        float distance = Vector3.Distance(bCam.transform.position, IDThing.transform.GetComponent<Collider>().ClosestPoint(bCam.transform.position));

        if (IDThing.GetType() == typeof(IDCharacter)) {

            IDCharacter thing = (IDCharacter)IDThing;

            //if (distance <= thing.discernibilityRange) {

            //    if (thing.tedsFavorabilityRatingWith < thing.favorabilityNeutralRangeLow) { startColor = boundingBoxDangerousColor; }
            //    else if (thing.tedsFavorabilityRatingWith > thing.favorabilityNeutralRangeHigh) { startColor = boundingBoxFriendColor; }
            //    else { startColor = boundingBoxNeutralColor; }


            //    if (distance <= thing.maxDistanceToActivate) {
            //        endColor = startColor;
            //    }
            //    else {
            //        endColor = boundingBoxNoColor;
            //    }
            //}
            //else {
            //    startColor = boundingBoxOutOfRangeColor;
            //    endColor = boundingBoxNoColor;
            //}

            if (distance <= thing.discernibilityRange) {

                if (thing.tedsFavorabilityRatingWith < thing.favorabilityNeutralRangeLow) { startColor = boundingBoxDangerousColor; }
                else if (thing.tedsFavorabilityRatingWith > thing.favorabilityNeutralRangeHigh) { startColor = boundingBoxFriendColor; }
                else { startColor = boundingBoxNeutralColor; }

            }
            else {
                startColor = boundingBoxOutOfRangeColor;
            }

            endColor = startColor;

        } else if (IDThing.GetType() == typeof(IDInteractable)) {

            IDInteractable thing = (IDInteractable)IDThing;

            if (thing.switchColliderWithPointerColliders) { distance = Vector3.Distance(thing.currentSwitchCollider.ClosestPoint(bCam.transform.position), bCam.transform.position); }

            startColor = boundingBoxUsableColor;
            endColor = startColor;

            //if (distance <= IDThing.maxDistanceToActivate) {
            //    endColor = startColor;
            //}
            //else {
            //    endColor = boundingBoxNoColor;
            //}
        } else {

            startColor = boundingBoxOutOfRangeColor;
            endColor = startColor;

        }

        if (distance > IDThing.maxDistanceToActivate) { startColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a / boundingBoxOutOfRangeAlphaReducer); }

        endColor = startColor;

        foreach (LineRenderer rend in bbRend) {
            rend.startColor = startColor;
            rend.endColor = endColor;
        }
    }

    // Passed dimension lengths of a bounding box; it skews longer lines shorter
    // Allows for making small objects have bigger/more visible box lines without making bigger ones look ridiculous
    private float ObjectDimensionAdjuster(float dimension) {
        return Mathf.Sqrt(Mathf.Sqrt(dimension)) * dimAdjustmentMultiplier;
    }

    public void ChangeReticleTexture(Texture retTexture, Texture targetingRetTexture) {
        reticle.texture = retTexture;
        targetingReticle.texture = targetingRetTexture;
    }

    public void HideReticleText() {
        scan.text = "";
        scan.enabled = false;
    }

    public void HideReticleAndText(bool doIt) {
        if (doIt) {
            hidingReticle = true;
            reticle.enabled = false;
            targetingReticle.enabled = false;
            scan.enabled = false;
        }
        else hidingReticle = false;
    }
}
