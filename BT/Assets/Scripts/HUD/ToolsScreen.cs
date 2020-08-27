using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolsScreen : MonoBehaviour
{
    public RawImage cachedResultsListBgd, resultsBgd, notesBgd, coordinatesBgd, imageWindow,
                    packetryButton, momentusButton, ARPAMatButton, canvasSystemsButton, loadButton, addButton, deleteButton, selectorBox;
    public Text cachedResultsListTxt, resultsTxt, notesTxt, coordinatesTxt;
    public Texture[] selectorBoxFrames;
    public bool showTools, enableToolControls;
    public float selectorBoxTimer;

    private enum ToolsTabMode { Selecting, Hax, Trajectory, MatSci, ImageAnalysis, Loading, Adding, Deleting, NoTool, Scrambled};
    private ToolsTabMode Mode;
    private int selectorBoxCurrentFrame;
    private float selectorBoxTimerCurTime;
    private bool selectorBoxLightening;

    // Start is called before the first frame update
    void Start()
    {
        showTools = false;
        Mode = ToolsTabMode.Selecting;

        SelectingATool(false);
        selectorBoxCurrentFrame = 0;
        selectorBoxTimerCurTime = 0;
        selectorBoxLightening = false;
    }

    // Update is called once per frame
    void Update()
    {
        ShowAllWindows(showTools);


        switch (Mode) {
            case ToolsTabMode.Selecting:
                SelectingATool(showTools);
                selectorBox.texture = selectorBoxFrames[selectorBoxCurrentFrame];
                break;
            case ToolsTabMode.Hax:
                
                break;
            case ToolsTabMode.Trajectory:
                
                break;
            case ToolsTabMode.MatSci:
                
                break;
            case ToolsTabMode.ImageAnalysis:
                
                break;
            case ToolsTabMode.Loading:
                
                break;
            case ToolsTabMode.Adding:
                
                break;
            case ToolsTabMode.Deleting:
                
                break;
            case ToolsTabMode.NoTool:
                
                break;
            case ToolsTabMode.Scrambled:
                
                break;
        }
    }

    private void ShowAllWindows(bool showOrNo) {
        cachedResultsListBgd.enabled = showOrNo;
        resultsBgd.enabled = showOrNo;
        notesBgd.enabled = showOrNo;
        coordinatesBgd.enabled = showOrNo;
        imageWindow.enabled = showOrNo;
        packetryButton.enabled = showOrNo;
        momentusButton.enabled = showOrNo;
        ARPAMatButton.enabled = showOrNo;
        canvasSystemsButton.enabled = showOrNo;
        loadButton.enabled = showOrNo;
        addButton.enabled = showOrNo;
        deleteButton.enabled = showOrNo;

        cachedResultsListTxt.enabled = showOrNo;
        resultsTxt.enabled = showOrNo;
        notesTxt.enabled = showOrNo;
        coordinatesTxt.enabled = showOrNo;

        if (showOrNo == false) { SelectingATool(false); }
    }

    private void SelectingATool(bool isSelecting) {
        selectorBox.enabled = isSelecting;

        if (isSelecting) {
            if (Time.time - selectorBoxTimerCurTime > selectorBoxTimer) {
                if (selectorBoxLightening) {
                    if (selectorBoxCurrentFrame > 0) { selectorBoxCurrentFrame -= 1; } 
                    else { selectorBoxLightening = !selectorBoxLightening; }
                } else {
                    if (selectorBoxCurrentFrame < (selectorBoxFrames.Length - 1)) { selectorBoxCurrentFrame += 1; } 
                    else { selectorBoxLightening = !selectorBoxLightening; }
                }
                selectorBoxTimerCurTime = Time.time;
            }
        }
    }

    public void PressedX() {

    }

    public void PressedCircle() {

    }

    public void PressedUp() {

    }

    public void PressedDown() {

    }

    public void PressedLeft() {

    }

    public void PressedRight() {

    }

    public void HoldingLeftTrigger() {

    }

    public void HildingRightTrigger() {

    }
}
