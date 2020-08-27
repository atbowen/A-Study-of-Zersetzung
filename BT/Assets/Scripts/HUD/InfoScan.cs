using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoScan : MonoBehaviour {
    public Text infoMsg;
    public RawImage reticle, targetingReticle;
    public Transform bgdPanels;
    public Texture scanningTexture, reticleView, targetRetView, reticleTraj, targetRetTraj, reticleMatSci, targetRetMatSci,
                reticleImageAn, targetRetImageAn, reticleSecurity, targetRetSecurity;
    public string idleText;
    public float scanTime;
    public string displayText, displayModeText;
    public Color targetTextColor, unknownTextColor, fieldHeaderColor, DisplayModeColor;

    private Text scan;
    private Panels panels;

    private CameraMaster camMaster;

	// Use this for initialization
	void Start () {
        scan = this.GetComponent<Text>();
        camMaster = FindObjectOfType<CameraMaster>();
        panels = bgdPanels.GetComponent<Panels>();

        scan.supportRichText = true;
        infoMsg.supportRichText = true;

        targetingReticle.enabled = false;

        scan.color = DisplayModeColor;

        displayText = displayModeText;
        //idleText = "Scanning...";
	}
	
	// Update is called once per frame
	void Update () {

        scan.text = displayText;
        scan.enabled = false;
	}

    //public void ScanObject(GameObject target) {
    //    if (target.name.Contains("Da")) {
    //        scan.text = target.name;
    //    } else {
    //        scan.text = idleText;
    //    }
    //}

    public void EnableInfoPanelWithID(ID ident) {
        if (ident.ObjName != null && ident.ObjName != "" && ident.ObjDescription != null && ident.ObjStatus != null) {

            if (camMaster.reticleEnabled) {
                targetingReticle.enabled = true;
                reticle.enabled = false;
            }
            else {
                targetingReticle.enabled = false;
                reticle.enabled = false;
            }

            if (targetingReticle.enabled) {

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
            }
            else {
                infoMsg.text = "";
                panels.HideInfoBgdPanel();
            }
        }
    }

    public void DisableInfoPanel() {
        infoMsg.text = "";
        panels.HideInfoBgdPanel();

        if (camMaster.reticleEnabled) {
            reticle.enabled = true;
            targetingReticle.enabled = false;
        }
        else {
            reticle.enabled = false;
            targetingReticle.enabled = false;
        }
    }

    public void DisplayBlurb(string objName, string objActName, string objAvName, string objDescrip, string unknown,
                            string objStatus, bool knowNm, bool knowActNm, bool knowAvName, bool knowDescrip, bool KnowStatus) {
        if (objName != null && objName != "" && objDescrip != null && objStatus != null) {
            
            if (camMaster.reticleEnabled) {
                targetingReticle.enabled = true;
                reticle.enabled = false;
            } else {
                targetingReticle.enabled = false;
                reticle.enabled = false;
            }

            if (targetingReticle.enabled) {

                string hexHeader = ColorUtility.ToHtmlStringRGBA(fieldHeaderColor);
                string hexKnown = ColorUtility.ToHtmlStringRGBA(targetTextColor);
                string hexUnknown = ColorUtility.ToHtmlStringRGBA(unknownTextColor);

                if (knowActNm) { infoMsg.text = "<color=#" + hexHeader + ">Nm-</color>" + objActName + "\n"; } else { infoMsg.text = "<color=#" + hexHeader + ">Nm-</color><color=#" + hexUnknown + ">" + unknown + "</color>\n"; }
                if (knowAvName) { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Av-</color>" + objAvName + "\n\n"; } else { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Av-</color><color=#" + hexUnknown + ">" + unknown + "</color>\n\n"; }
                if (knowDescrip) { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Description</color>\n" + objDescrip + "\n\n"; } else { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Description</color>\n<color=#" + hexUnknown + ">" + unknown + "</color>\n\n\n"; }
                if (KnowStatus) { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Status</color>\n<color=#" + hexKnown + ">" + objStatus + "</color>"; } else { infoMsg.text = infoMsg.text + "<color=#" + hexHeader + ">Status</color>\n<color=#" + hexUnknown + ">" + unknown + "</color>"; }

                panels.ShowInfoBgdPanel(infoMsg.cachedTextGenerator.lineCount);
            } else {
                infoMsg.text = "";
                panels.HideInfoBgdPanel();
            }
        } else {
            infoMsg.text = "";
            panels.HideInfoBgdPanel();

            if (camMaster.reticleEnabled) {
                reticle.enabled = true;
                targetingReticle.enabled = false;
            } else {
                reticle.enabled = false;
                targetingReticle.enabled = false;
            }
        }
    }

    public void DisplayTextUsingTargetingColor(string text) {
        scan.color = targetTextColor;
        displayText = text;
    }

    public void DisplayTextUsingModeColor(string text) {
        scan.color = DisplayModeColor;
        displayText = text;
    }

    public void ChangeReticleTexture(Texture retTexture, Texture targetingRetTexture) {
        reticle.texture = retTexture;
        targetingReticle.texture = targetingRetTexture;
    }

    public void HideReticle() {

    }
}
