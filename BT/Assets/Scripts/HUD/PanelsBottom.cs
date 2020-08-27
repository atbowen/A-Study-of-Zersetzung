using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelsBottom : MonoBehaviour
{
    public RawImage panelTop, panelSpacer, panelBottom;
    public List<RawImage> imagesToAlignWithTopEdge;
    public Vector2 imageAlignmentOffset;

    private int panelSpacerHeight, panelSpacerWidth;

    // Start is called before the first frame update
    void Start()
    {
        panelSpacerHeight = panelSpacer.texture.height;
        panelSpacerWidth = (int)panelSpacer.rectTransform.rect.width;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowInfoBgdPanel(int numOfLines) {
        panelTop.enabled = true;
        panelSpacer.enabled = true;
        panelBottom.enabled = true;

        SizePanelToText(numOfLines);
    }

    public void HideInfoBgdPanel() {
        panelTop.enabled = false;
        panelSpacer.enabled = false;
        panelBottom.enabled = false;
    }

    public void SizePanelToText(int numOfLines) {
        int panelTopHeight = panelTop.texture.height;
        int panelBottomHeight = panelBottom.texture.height;
        int panelMiddleHeight = panelSpacerHeight * numOfLines + (panelSpacerHeight - 7);

        if (numOfLines > 0) {
            //panelSpacer.enabled = true;
            panelSpacer.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                        new Vector2(0, ((panelMiddleHeight / 2) + (panelBottomHeight / 2) + 1));

            panelSpacer.rectTransform.sizeDelta = new Vector2(panelSpacerWidth, panelMiddleHeight);

            panelTop.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
                                                            new Vector2(0, ((panelMiddleHeight) + (panelTopHeight / 2) + (panelBottomHeight / 2) + 1));
        }
        //else {
        //    panelSpacer.enabled = false;
        //    panelTop.rectTransform.anchoredPosition = panelBottom.rectTransform.anchoredPosition +
        //                                                    new Vector2(0, ((panelTopHeight / 2) + (panelBottomHeight / 2) + 1));
        //}

        AlignImagesWithTop();
    }

    private void AlignImagesWithTop() {
        if (imagesToAlignWithTopEdge.Count > 0) {
            foreach (RawImage image in imagesToAlignWithTopEdge) {
                image.rectTransform.anchoredPosition = panelTop.rectTransform.anchoredPosition + imageAlignmentOffset;
            }
        }
    }
}
