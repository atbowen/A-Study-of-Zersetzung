using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelector : MonoBehaviour
{
    public bool toolSelectorOpen;
    public RawImage securityIcon, trajectoryIcon, materialsAnalysisIcon, imageAnalysisIcon, shieldIcon, mirageIcon, iconHighlight;
    public RawImage[] iconActive;

    public RawImage securityCamOverlay;

    private InfoScan scanner;
    private Shield shield;
    private FakeTedAI fakeTed;

    private RawImage[] icons;
    private RawImage highlightedIcon;

    private bool resetScannerDisplayModeTextTrigger;
    private bool securityStatus, trajectoryStatus, materialsAnalysisStatus, imageAnalysisStatus, shieldStatus, mirageStatus;

    private enum ToolHighlight { security, trajectory, materialsAnalysis, imageAnalysis, shield, mirage, none };
    private ToolHighlight ToolSelect;

    private float toolSelectionWheelDelta = 0.1f;
    
    // Start is called before the first frame update
    void Start()
    {
        scanner = FindObjectOfType<InfoScan>();
        shield = FindObjectOfType<Shield>();
        fakeTed = FindObjectOfType<FakeTedAI>();

        icons = new RawImage[] { securityIcon, trajectoryIcon, materialsAnalysisIcon, imageAnalysisIcon, shieldIcon, mirageIcon };
        securityStatus = false;
        trajectoryStatus = false;
        materialsAnalysisStatus = false;
        imageAnalysisStatus = false;
        shieldStatus = false;
        mirageStatus = false;

        iconHighlight.enabled = false;
        resetScannerDisplayModeTextTrigger = true;

        highlightedIcon = null;

        ToolSelect = ToolHighlight.none;

        securityCamOverlay.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (toolSelectorOpen) {
            ShowToolIcons(true);
            resetScannerDisplayModeTextTrigger = true;
        } else {
            ShowToolIcons(false);
            ClearHighlight();
        }
    }

    private void ShowToolIcons(bool shouldTheyAppear) {        
        foreach(RawImage icon in icons) {
            icon.enabled = shouldTheyAppear;    
            if (icon.transform.Find("Icon Active").GetComponent<RawImage>()) {
                RawImage image = icon.transform.Find("Icon Active").GetComponent<RawImage>();

                if (shouldTheyAppear) {
                    if (GetStatusFromIcon(icon)) { image.enabled = true; } else { image.enabled = false; }
                } else { image.enabled = false; }
            }
        }
    }

    public void IconPointer(float x, float y) {
        if (x <= toolSelectionWheelDelta && x >= -toolSelectionWheelDelta) {
            //ClearHighlight();
        } else if (x > toolSelectionWheelDelta && (y <= toolSelectionWheelDelta && y >= -toolSelectionWheelDelta)) {
            HighlightIcon(imageAnalysisIcon);
            ToolSelect = ToolHighlight.imageAnalysis;
        } else if (x < -toolSelectionWheelDelta && (y <= toolSelectionWheelDelta && y>= -toolSelectionWheelDelta)) {
            HighlightIcon(securityIcon);
            ToolSelect = ToolHighlight.security;
        } else if (x > 0 && y != 0) {
            if (Mathf.Abs(y) / x > 0.4) {
                if (y > 0) {
                    HighlightIcon(materialsAnalysisIcon);
                    ToolSelect = ToolHighlight.materialsAnalysis;
                } 
                else {
                    HighlightIcon(mirageIcon);
                    ToolSelect = ToolHighlight.mirage;
                }
            } else {
                HighlightIcon(imageAnalysisIcon);
                ToolSelect = ToolHighlight.imageAnalysis;
            }
        } else if (x < 0 && y != 0) {
            if (Mathf.Abs(y) / Mathf.Abs(x) > 0.4) {
                if (y > 0) {
                    HighlightIcon(trajectoryIcon);
                    ToolSelect = ToolHighlight.trajectory;
                } 
                else {
                    HighlightIcon(shieldIcon);
                    ToolSelect = ToolHighlight.shield;
                }
            } else {
                HighlightIcon(securityIcon);
                ToolSelect = ToolHighlight.security;
            }
        } else {
            //ClearHighlight();
        }
    }

    private void HighlightIcon(RawImage icon) {
        iconHighlight.transform.position = icon.transform.position + new Vector3(0, 0, -1);
        iconHighlight.enabled = true;
        scanner.DisplayToolText(icon.name);

        highlightedIcon = icon;
    }

    private void ClearHighlight() {
        iconHighlight.enabled = false;
        if (resetScannerDisplayModeTextTrigger) {
            scanner.DisplayToolText(scanner.displayModeText);
            resetScannerDisplayModeTextTrigger = false;
        }
    }

    private void ToggleIconActivationStatus(RawImage icon) {
        if (icon.transform.Find("Icon Active").GetComponent<RawImage>() != null) {
            RawImage image = icon.transform.Find("Icon Active").GetComponent<RawImage>();
            image.enabled = !image.enabled;
        }
    }

    private bool GetStatusFromIcon(RawImage icon) {
        if (icon == securityIcon) { return securityStatus; } 
        else if (icon == trajectoryIcon) { return trajectoryStatus; } 
        else if (icon == materialsAnalysisIcon) { return materialsAnalysisStatus; } 
        else if (icon == imageAnalysisIcon) { return imageAnalysisStatus; } 
        else if (icon == shieldIcon) { return shieldStatus; } 
        else if (icon == mirageIcon) { return mirageStatus; } 
        else return false;
    }

    public void EnableTool() {
        switch (ToolSelect) {
            case ToolHighlight.none:
                scanner.ChangeReticleTexture(scanner.reticleView, scanner.targetRetView);
                break;
            case ToolHighlight.security:
                securityStatus = !securityStatus;
                securityCamOverlay.enabled = !securityCamOverlay.enabled;
                scanner.ChangeReticleTexture(scanner.reticleSecurity, scanner.targetRetSecurity);
                break;
            case ToolHighlight.imageAnalysis:
                imageAnalysisStatus = !imageAnalysisStatus;
                break;
            case ToolHighlight.materialsAnalysis:
                materialsAnalysisStatus = !materialsAnalysisStatus;
                break;
            case ToolHighlight.mirage:
                fakeTed.ToggleFakeTedAI();
                mirageStatus = !mirageStatus;
                break;
            case ToolHighlight.shield:
                shield.ToggleShields();
                shieldStatus = !shieldStatus;
                break;
            case ToolHighlight.trajectory:
                trajectoryStatus = !trajectoryStatus;
                break;
        }

        ToggleIconActivationStatus(highlightedIcon);
    }
}
