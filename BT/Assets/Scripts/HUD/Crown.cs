using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crown : MonoBehaviour
{
    public float startingTotalCrown;
    public RawImage crownIcon;
    public Color crownIconColor, crownIconFlashColor;
    public Text crownText;
    public Color crownTextColor, crownTextFlashColor;
    public float crownCountIncreaseSpeed, delayBeforeCrownTotalCalculation;
    public float crownIconFlashingTimeOnPickup, crownTextFlashingTimeOnPickup, delayBeforeIconFlash, delayBeforeTextFlash;
    public float crownIconFlashSpeed, crownTextFlashSpeed;
    public float crownIconBulgeFactor, crownTextBulgeFactor;

    private MusicPlayer musicBox;
    private float totalCrownDisplayed, actualTotalCrown;
    private bool flashingIcon, flashingText, iconFlashingOn, textFlashingOn;
    private bool totalIncreasing;
    private float flashCrownIconTimerRef, flashCrownTextTimerRef, delayIconFlashTimerRef, delayTextFlashTimerRef, delayCrownTotalCalculationRef;
    private float   iconR, iconG, iconB, iconA,
                    iconFlashR, iconFlashG, iconFlashB, iconFlashA,
                    textR, textG, textB, textA,
                    textFlashR, textFlashG, textFlashB, textFlashA;
    private Color iconFlashOnColor, textFlashOnColor;
    
    // Start is called before the first frame update
    void Start()
    {
        musicBox = FindObjectOfType<MusicPlayer>();

        iconR = crownIconColor.r;
        iconG = crownIconColor.g;
        iconB = crownIconColor.b;
        iconA = crownIconColor.a;
        iconFlashR = crownIconFlashColor.r;
        iconFlashG = crownIconFlashColor.g;
        iconFlashB = crownIconFlashColor.b;
        iconFlashA = crownIconFlashColor.a;

        textR = crownTextColor.r;
        textG = crownTextColor.g;
        textB = crownTextColor.b;
        textA = crownTextColor.a;
        textFlashR = crownTextFlashColor.r;
        textFlashG = crownTextFlashColor.g;
        textFlashB = crownTextFlashColor.b;
        textFlashA = crownTextFlashColor.a;

        iconFlashOnColor = new Color(iconR + iconFlashR, iconG + iconFlashG, iconB + iconFlashB, iconA + iconFlashA);
        textFlashOnColor = new Color(textR + textFlashR, textG + textFlashG, textB + textFlashB, textA + textFlashA);

        crownIcon.enabled = true;
        crownText.enabled = true;

        flashingIcon = false;
        flashingText = false;
        totalIncreasing = false;
        iconFlashingOn = false;
        textFlashingOn = false;

        actualTotalCrown = startingTotalCrown;
        totalCrownDisplayed = startingTotalCrown;
    }

    // Update is called once per frame
    void Update() {
        crownText.text = totalCrownDisplayed.ToString();

        if (totalIncreasing) {
            if (Time.time - delayCrownTotalCalculationRef > delayBeforeCrownTotalCalculation) {
                if (actualTotalCrown > totalCrownDisplayed) {
                    if ((actualTotalCrown - totalCrownDisplayed) > crownCountIncreaseSpeed * Time.deltaTime) {
                        totalCrownDisplayed += crownCountIncreaseSpeed * Time.deltaTime;
                    }
                    else { totalCrownDisplayed = actualTotalCrown; }
                }
            }
        }

        if (flashingIcon) {

            if (Time.time - delayIconFlashTimerRef > delayBeforeIconFlash) {
                if (Time.time - delayIconFlashTimerRef + delayBeforeIconFlash < crownIconFlashingTimeOnPickup) {

                    crownIcon.rectTransform.localScale = new Vector2(crownIconBulgeFactor, crownIconBulgeFactor);

                    if (iconFlashingOn) {
                        if (Time.time - flashCrownIconTimerRef > crownIconFlashSpeed) {
                            FlashCrownIconOn();
                        }
                    }
                    else {
                        if (Time.time - flashCrownIconTimerRef > crownIconFlashSpeed) {
                            FlashCrownIconOff();
                        }
                    }
                } else {
                    StopFlashingCrownIcon();
                    crownIcon.rectTransform.localScale = new Vector2(1, 1);
                }
            }
        }

        if (flashingText) {

            if (Time.time - delayTextFlashTimerRef > delayBeforeTextFlash) {
                if (Time.time - delayTextFlashTimerRef + delayBeforeTextFlash < crownTextFlashingTimeOnPickup) {
                    if (textFlashingOn) {
                        if (Time.time - flashCrownTextTimerRef > crownTextFlashSpeed) {
                            crownText.rectTransform.localScale = new Vector2(crownTextBulgeFactor, crownTextBulgeFactor);
                            FlashCrownTextOn();
                        }
                    }
                    else {
                        if (Time.time - flashCrownTextTimerRef > crownTextFlashSpeed) {
                            crownText.rectTransform.localScale = new Vector2(1, 1);
                            FlashCrownTextOff();
                        }
                    }
                } else {
                    crownText.rectTransform.localScale = new Vector2(1, 1);
                    StopFlashingCrownText();
                }
            }
        }
    }
    
    public void IncreaseTotalAmount(float increaseAmount) {

        musicBox.PlaySFX("got money");

        actualTotalCrown += increaseAmount;
        flashingIcon = true;
        flashingText = true;
        totalIncreasing = true;
        iconFlashingOn = true;
        textFlashingOn = true;
        delayIconFlashTimerRef = Time.time;
        delayTextFlashTimerRef = Time.time;
        delayCrownTotalCalculationRef = Time.time;
    }

    public void FlashCrownIconOn() {
        crownIcon.color = iconFlashOnColor;
        flashCrownIconTimerRef = Time.time;
        iconFlashingOn = false;
    }

    public void FlashCrownIconOff() {
        crownIcon.color = crownIconColor;
        flashCrownIconTimerRef = Time.time;
        iconFlashingOn = true;
    }

    public void StopFlashingCrownIcon() {
        crownIcon.color = crownIconColor;
        flashingIcon = false;
    }

    public void FlashCrownTextOn() {
        crownText.color = textFlashOnColor;
        flashCrownTextTimerRef = Time.time;
        textFlashingOn = false;
    }

    public void FlashCrownTextOff() {
        crownText.color = crownTextColor;
        flashCrownTextTimerRef = Time.time;
        textFlashingOn = true;
    }

    public void StopFlashingCrownText() {
        crownText.color = crownTextColor;
        flashingText = false;
    }
}
