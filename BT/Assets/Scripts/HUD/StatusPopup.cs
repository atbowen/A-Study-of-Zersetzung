using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusPopup : MonoBehaviour
{
    public RawImage statusBgd;
    public Text statusText;
    public float showTime, fadeTime, fadeRate;

    private int statusBgdHeight, statusBgdWidth;
    private Vector2 statusBgdPosition;

    private const int fontHeight = 8;

    private bool refreshed;
    private float statusShowTimerRef, statusFadeTimerRef;
    private Color textColor, origTextColor, bgdColor, origBgdColor;
    
    // Start is called before the first frame update
    void Start()
    {
        refreshed = false;

        statusShowTimerRef = 0;
        statusFadeTimerRef = 0;

        textColor = statusText.color;
        origTextColor = statusText.color;
        bgdColor = statusBgd.color;
        origBgdColor = statusBgd.color;

        statusText.supportRichText = true;
        statusBgdPosition = statusBgd.rectTransform.anchoredPosition;

        statusBgd.enabled = false;
        statusText.enabled = false;

        statusBgdHeight = (int) statusBgd.rectTransform.rect.height;
        statusBgdWidth = (int) statusBgd.rectTransform.rect.width;
    }

    // Update is called once per frame
    void Update()
    {
        if (refreshed) {
            statusBgd.enabled = true;
            statusText.enabled = true;

            if (Time.time - statusShowTimerRef > showTime) {

                if (textColor.a > 0.05) {
                    textColor.a -= Time.deltaTime * fadeRate;
                    bgdColor.a -= Time.deltaTime * fadeRate;
                }

                if (Time.time - statusShowTimerRef - statusFadeTimerRef > fadeTime + showTime) {
                    statusBgd.enabled = false;
                    statusText.enabled = false;

                    textColor = origTextColor;
                    bgdColor = origBgdColor;
                    refreshed = false;
                } 
            }
        }

        statusText.color = textColor;
        statusBgd.color = bgdColor;

        SizeImageToTextLines(statusBgd, statusBgdPosition, statusBgdWidth, statusBgdHeight, statusText);
    }

    private void SizeImageToTextLines(RawImage imageToSize, Vector2 origImagePos, int imageWidth, int imageHeight, Text text) {
        int lineCount = text.cachedTextGenerator.lineCount;
        int numberOfChars = text.text.ToString().ToCharArray().Length;
        int numberOfSpaces = 0;

        for (int i = 0; i < text.text.ToString().ToCharArray().Length; i++) {
            if (text.text.ToString().ToCharArray()[i] == ' ') { numberOfSpaces++; }
        }

        numberOfChars -= numberOfSpaces;

        if (lineCount > 1) {
            text.rectTransform.anchoredPosition = Vector2.zero;

            imageToSize.rectTransform.anchoredPosition = origImagePos + new Vector2(0, -Mathf.CeilToInt(imageHeight / 2));
            imageToSize.rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight * 2);
            text.rectTransform.anchoredPosition = Vector2.zero;            
        } else {
            int newImageWidth = (numberOfChars * 5) + (numberOfSpaces * 2);

            text.rectTransform.anchoredPosition = new Vector2(Mathf.CeilToInt((imageWidth - newImageWidth) / 2), 0);

            imageToSize.rectTransform.sizeDelta = new Vector2(newImageWidth, imageHeight);
            imageToSize.rectTransform.anchoredPosition = origImagePos + new Vector2(-Mathf.CeilToInt((imageWidth - newImageWidth) / 2), 0);
            text.rectTransform.anchoredPosition = new Vector2(Mathf.CeilToInt((imageWidth - newImageWidth) / 2), 0);
        }
    }

    public void FlashStatusText(string newText) {
        statusText.text = newText;

        statusBgd.enabled = true;
        statusText.enabled = true;

        refreshed = true;
        statusShowTimerRef = Time.time;
    }
}
