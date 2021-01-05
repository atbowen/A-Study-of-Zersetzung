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
    public bool makeTextBulge;
    public float crownIconBulgeFactor, crownTextBulgeFactor;

    private bool gotMoney, waitingForCounterFadeAway, fadeCounter;

    public List<Texture> crownCounterAnimationFrames;
    [SerializeField]
    private float crownCounterAnimationFrameChangeTime, delayBeforeCounterFades, counterFadeFactor;
    [SerializeField]
    private float crownCounterLowAlphaThreshold = 0.01f;
    private float crownCounterAnimationFrameRefTime, fadeDelayRefTime;
    private int crownCounterAnimationFrameIndex;
    private float increaseAmountFactor;

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

        gotMoney = false;
        crownCounterAnimationFrameIndex = 0;
        waitingForCounterFadeAway = false;
        fadeCounter = false;
        crownIcon.color = new Color(crownIcon.color.r, crownIcon.color.g, crownIcon.color.b, 0);
        crownText.color = new Color(crownText.color.r, crownText.color.g, crownText.color.b, 0);
    }

    // Update is called once per frame
    void Update() {
        crownText.text = totalCrownDisplayed.ToString();

        if (totalIncreasing) {
            if (Time.time - delayCrownTotalCalculationRef > delayBeforeCrownTotalCalculation) {
                if (actualTotalCrown > totalCrownDisplayed) {
                    if ((actualTotalCrown - totalCrownDisplayed) > increaseAmountFactor * Time.deltaTime) {
                        totalCrownDisplayed += increaseAmountFactor * Time.deltaTime;
                    }
                    else { totalCrownDisplayed = actualTotalCrown; }
                }
            }
        }

        //if (flashingIcon) {

        //    if (Time.time - delayIconFlashTimerRef > delayBeforeIconFlash) {
        //        if (Time.time - delayIconFlashTimerRef + delayBeforeIconFlash < crownIconFlashingTimeOnPickup) {

        //            crownIcon.rectTransform.localScale = new Vector2(crownIconBulgeFactor, crownIconBulgeFactor);

        //            if (iconFlashingOn) {
        //                if (Time.time - flashCrownIconTimerRef > crownIconFlashSpeed) {
        //                    FlashCrownIconOn();
        //                }
        //            }
        //            else {
        //                if (Time.time - flashCrownIconTimerRef > crownIconFlashSpeed) {
        //                    FlashCrownIconOff();
        //                }
        //            }
        //        } else {
        //            StopFlashingCrownIcon();
        //            crownIcon.rectTransform.localScale = new Vector2(1, 1);
        //        }
        //    }
        //}

        if (gotMoney) {

            if (Time.time - crownCounterAnimationFrameRefTime > crownCounterAnimationFrameChangeTime) {
                if (crownCounterAnimationFrameIndex < crownCounterAnimationFrames.Count - 1) {
                    crownCounterAnimationFrameIndex++;
                    crownIcon.texture = crownCounterAnimationFrames[crownCounterAnimationFrameIndex];
                }
                else {
                    gotMoney = false;
                    crownCounterAnimationFrameIndex = 0;

                    waitingForCounterFadeAway = true;
                    fadeDelayRefTime = Time.time;
                }

                crownCounterAnimationFrameRefTime = Time.time;
            }
        }

        if (waitingForCounterFadeAway) {
            if (Time.time - fadeDelayRefTime > delayBeforeCounterFades) {
                waitingForCounterFadeAway = false;

                fadeCounter = true;
            }
        }

        if (fadeCounter) {

            bool iconAndTextAreInvisible = true;

            if (crownIcon.color.a > crownCounterLowAlphaThreshold) {
                crownIcon.color = new Color(crownIcon.color.r, crownIcon.color.g, crownIcon.color.b, crownIcon.color.a - (counterFadeFactor * Time.deltaTime));
                iconAndTextAreInvisible = false;
            }
            else {
                crownIcon.color = new Color(crownIcon.color.r, crownIcon.color.g, crownIcon.color.b, 0);
            }

            if (crownText.color.a > crownCounterLowAlphaThreshold) {
                crownText.color = new Color(crownText.color.r, crownText.color.g, crownText.color.b, crownText.color.a - (counterFadeFactor * Time.deltaTime));
                iconAndTextAreInvisible = false;
            }
            else {
                crownText.color = new Color(crownText.color.r, crownText.color.g, crownText.color.b, 0);
            }

            if (iconAndTextAreInvisible) { fadeCounter = false; }
        }

        if (flashingText) {

            if (Time.time - delayTextFlashTimerRef > delayBeforeTextFlash) {
                if (Time.time - delayTextFlashTimerRef + delayBeforeTextFlash < crownTextFlashingTimeOnPickup) {
                    if (textFlashingOn) {
                        if (Time.time - flashCrownTextTimerRef > crownTextFlashSpeed) {
                            if (makeTextBulge) {
                                crownText.rectTransform.localScale = new Vector2(crownTextBulgeFactor, crownTextBulgeFactor);
                            }
                            FlashCrownTextOn();
                        }
                    }
                    else {
                        if (Time.time - flashCrownTextTimerRef > crownTextFlashSpeed) {
                            if (makeTextBulge) {
                                crownText.rectTransform.localScale = new Vector2(1, 1);
                            }
                            FlashCrownTextOff();
                        }
                    }
                } else {
                    if (makeTextBulge) {
                        crownText.rectTransform.localScale = new Vector2(1, 1);
                    }
                    StopFlashingCrownText();
                }
            }
        }
    }
    
    public void IncreaseTotalAmount(float increaseAmount) {

        musicBox.PlayKaChing();

        actualTotalCrown += increaseAmount;
        increaseAmountFactor = increaseAmount * crownCountIncreaseSpeed;
        flashingIcon = true;
        flashingText = true;
        totalIncreasing = true;
        iconFlashingOn = true;
        textFlashingOn = true;
        delayIconFlashTimerRef = Time.time;
        delayTextFlashTimerRef = Time.time;
        delayCrownTotalCalculationRef = Time.time;

        gotMoney = true;
        crownCounterAnimationFrameRefTime = Time.time;
        crownIcon.color = crownIconColor;
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
