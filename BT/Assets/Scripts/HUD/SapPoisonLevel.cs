using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SapPoisonLevel : MonoBehaviour
{
    [SerializeField]
    private const float fullPoisonValue = 100.0f;
    [SerializeField]
    private float minPoisonValueToHallucinate = 5.0f;

    [SerializeField, Range(0.0f, fullPoisonValue)]
    private float initialPoisonValue;

    public RawImage poisonBar;

    private float currentPoisonValue, referencePoisonValue, pendingLevelAdjustment;
    [SerializeField]
    private float levelIncreaseFactor, levelDecreaseFactor;
    [SerializeField, Range(0.0f, 1.0f)]
    private float updatedPoisonValueEquilibriumDelta;           // If the current poison value is being adjusted, how close to the projected final value does the current value need to be to stop adjusting?
    private int poisonBarFullWidth;
    private bool levelAdjustmentIsNew;

    // Start is called before the first frame update
    void Start()
    {
        poisonBarFullWidth = (int)poisonBar.rectTransform.sizeDelta.x;

        currentPoisonValue = initialPoisonValue;

        pendingLevelAdjustment = 0.0f;
        levelAdjustmentIsNew = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (pendingLevelAdjustment != 0.0f) {
            if (levelAdjustmentIsNew) { referencePoisonValue = currentPoisonValue; }

            levelAdjustmentIsNew = false;

            if ((Mathf.Abs(currentPoisonValue - (referencePoisonValue + pendingLevelAdjustment)) > updatedPoisonValueEquilibriumDelta)) {
                if (((referencePoisonValue + pendingLevelAdjustment) - currentPoisonValue > 0) && (currentPoisonValue < 100.0f))       { currentPoisonValue += levelIncreaseFactor * Time.deltaTime; }
                else if (((referencePoisonValue + pendingLevelAdjustment) - currentPoisonValue < 0) && (currentPoisonValue > 0.0f))   { currentPoisonValue -= levelDecreaseFactor * Time.deltaTime; }
            }
            else {
                pendingLevelAdjustment = 0.0f;
                levelAdjustmentIsNew = true;
            }            
        }

        float width = (int)((currentPoisonValue / fullPoisonValue) * poisonBarFullWidth);
        poisonBar.rectTransform.sizeDelta = new Vector2(width, poisonBar.rectTransform.sizeDelta.y);
    }

    public bool IsSapToxicityLevelHighEnoughToInduceHallucination() {
        if (currentPoisonValue > minPoisonValueToHallucinate)   { return true; }
        else                                                    { return false; }
    }

    public void TakeHit(float dosage) {
        //currentPoisonValue = Mathf.Min(currentPoisonValue + dosage, fullPoisonValue);
        pendingLevelAdjustment += dosage;
    }

    public void Detox(float reduction) {
        //currentPoisonValue = Mathf.Max(currentPoisonValue - reduction, 0.0f);
        pendingLevelAdjustment -= reduction;
    }
}
