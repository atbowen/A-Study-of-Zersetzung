using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panels : MonoBehaviour
{
    public RawImage panelTop, panelSpacer, panelBottom;
    private int panelSpacerHeight;

    // Start is called before the first frame update
    void Start()
    {
        panelSpacerHeight = panelSpacer.texture.height;
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
        int panelMiddleHeight = panelSpacerHeight * numOfLines;        

        if (numOfLines > 1) {
            //panelSpacer.enabled = true;
            panelSpacer.rectTransform.anchoredPosition = panelTop.rectTransform.anchoredPosition -
                                                        new Vector2(0, ((panelMiddleHeight / 2) + (panelTopHeight / 2) + 1));

            panelSpacer.rectTransform.sizeDelta = new Vector2(panelSpacer.texture.width, panelMiddleHeight);

            panelBottom.rectTransform.anchoredPosition = panelTop.rectTransform.anchoredPosition -
                                                            new Vector2(0, ((panelMiddleHeight) + (panelTopHeight / 2) + (panelBottomHeight / 2) + 1));
        } else {
            panelSpacer.enabled = false;
            panelBottom.rectTransform.anchoredPosition = panelTop.rectTransform.anchoredPosition -
                                                            new Vector2(0, ((panelTopHeight / 2) + (panelBottomHeight / 2) + 1));
        }
    }
}
