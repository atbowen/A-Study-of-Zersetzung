using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewsTicker : MonoBehaviour
{
    public RawImage tickerBgd;
    public Texture[] tickerBgdTextures;
    public Text tickerText, dateText, conditionsText;

    public int charWidth, spaceShift, visibleStringWidth;
    public float    textShiftDelayPerPixel, tickerBgdSwitchRate, tickerBgdSwitchRateVariance, tickerBgdDirectionSwitchRandDelay,
                    conditionsFadeRate, conditionsFadeDelay, conditionsFadeStep;
    public Color conditionsColor;

    public bool showTicker;

    private TimeAndConditions TandC;
    private int currentPixelFrame, tickerBgdTextureFrameIndex;
    private string tickerTextString, visibleString;
    private float tickerRefTime, tickerBgdRefTime, tickerBgdSwitchRateConst, tickerBgdDirectionSwitchRefTime, conditionsRefTime;
    private bool fadeInOrOut, tickerBgdSwitchForwardOrBack;
    
    // Start is called before the first frame update
    void Start()
    {
        TandC = FindObjectOfType<TimeAndConditions>();

        currentPixelFrame = 0;
        tickerBgdTextureFrameIndex = 0;
        tickerBgdSwitchRateConst = tickerBgdSwitchRate;
        tickerRefTime = 0;
        tickerBgdDirectionSwitchRefTime = 0;
        conditionsRefTime = 0;

        tickerTextString = tickerText.text;
        visibleString = "";
        dateText.text = "";
        conditionsText.text = "";

        fadeInOrOut = false;
        tickerBgdSwitchForwardOrBack = true;

        tickerBgd.enabled = false;
        tickerText.enabled = false;

        showTicker = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (showTicker) {
            tickerBgd.enabled = true;
            tickerText.enabled = true;
            dateText.enabled = true;
            conditionsText.enabled = true;

            if (Time.time - tickerRefTime > textShiftDelayPerPixel) {
                ShiftTextLeftOnePixel();
                tickerRefTime = Time.time;
            }

            if (Time.time - conditionsRefTime > conditionsFadeRate) {
                FadeConditionsText();
                conditionsRefTime = Time.time;
            }

            if (Time.time - tickerBgdRefTime > tickerBgdSwitchRate) {

                if (Time.time - tickerBgdDirectionSwitchRefTime > tickerBgdDirectionSwitchRandDelay) {
                    int randSwitcher = Random.Range(0, 10);
                    if (randSwitcher > 7) { tickerBgdSwitchForwardOrBack = !tickerBgdSwitchForwardOrBack; }
                    tickerBgdDirectionSwitchRefTime = Time.time;
                }

                if (tickerBgdSwitchForwardOrBack)   { TickerBgdFrameSwitchForward(); } 
                else                                { TickerBgdFrameSwitchBackward(); }

                tickerBgdRefTime = Time.time;                
                tickerBgdSwitchRate = Random.Range(tickerBgdSwitchRateConst - tickerBgdSwitchRateVariance, tickerBgdSwitchRateConst + tickerBgdSwitchRateVariance);                
            }
        } else {
            tickerBgd.enabled = false;
            tickerText.enabled = false;
            dateText.enabled = false;
            conditionsText.enabled = false;
        }

        tickerText.text = visibleString;
        tickerBgd.texture = tickerBgdTextures[tickerBgdTextureFrameIndex];
        dateText.text = TandC.GetDay() + "  " + TandC.GetMinute() + "  ";
        conditionsText.text = TandC.GetTemperature() + "c" + TandC.GetHumidity() + "%";
        conditionsText.color = conditionsColor;
    }

    public void ShiftTextLeftOnePixel() {
        if (currentPixelFrame < (spaceShift - 1)) {
            tickerText.rectTransform.anchoredPosition += new Vector2(-1, 0);
            currentPixelFrame++;
        } else {
            tickerText.rectTransform.anchoredPosition += new Vector2((spaceShift - 1), 0);
            currentPixelFrame = 0;

            ShiftTextLeftOneChar();
        }
    }

    public void ShiftTextLeftOneChar() {
        char[] tickerTextChars = tickerTextString.ToCharArray();
        tickerTextString = "";
        visibleString = "";

        char firstChar = tickerTextChars[0];
        char nextChar = tickerTextChars[1];

        if (nextChar == ' ')    { spaceShift = charWidth - 2; } 
        else                    { spaceShift = charWidth; }

        for (int i = 0; i < tickerTextChars.Length - 1; i++) {
            tickerTextString = tickerTextString + tickerTextChars[i + 1].ToString();
        }
        for (int i = 0; i < Mathf.Min(tickerTextChars.Length - 1, visibleStringWidth); i++) {
            visibleString = visibleString + tickerTextChars[i + 1].ToString();
        }

        tickerTextString = tickerTextString + firstChar.ToString();
    }

    private void FadeConditionsText() {
        if (fadeInOrOut) {
            if (conditionsColor.a < 0.9) {
                conditionsColor.a += conditionsFadeStep;
            } else { fadeInOrOut = false; }
        } else {
            if (conditionsColor.a > 0.1) {
                conditionsColor.a -= conditionsFadeStep;
            } else { fadeInOrOut = true; }
        }
    }

    private void TickerBgdFrameSwitchForward() {
        tickerBgdTextureFrameIndex = (tickerBgdTextureFrameIndex + 1) % tickerBgdTextures.Length; 
    }

    private void TickerBgdFrameSwitchBackward() {
        tickerBgdTextureFrameIndex = (tickerBgdTextureFrameIndex + tickerBgdTextures.Length - 1) % tickerBgdTextures.Length;
    }
}
