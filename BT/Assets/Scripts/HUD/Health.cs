using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField]
    private const float fullHealthValue = 100.0f;

    [SerializeField, Range(0.0f, fullHealthValue)]
    private float initialHealth;

    public RawImage healthBar;

    private float currentHealth, pendingHealthAdjustment, referenceHealth;    
    [SerializeField]
    private float healthIncreaseFactor, healthDecreaseFactor;
    [SerializeField, Range(0.0f, 1.0f)]
    private float updatedHealthValueEquilibriumDelta;               // If the current health is being adjusted, how close to the projected final value does the current value need to be to stop adjusting?
    private int healthBarFullWidth;
    private bool levelAdjustmentIsNew;

    // Start is called before the first frame update
    void Start()
    {
        healthBarFullWidth = (int)healthBar.rectTransform.sizeDelta.x;

        currentHealth = initialHealth;

        pendingHealthAdjustment = 0.0f;
        levelAdjustmentIsNew = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (pendingHealthAdjustment != 0.0f) {
            if (levelAdjustmentIsNew) { referenceHealth = currentHealth; }

            levelAdjustmentIsNew = false;

            if ((Mathf.Abs(currentHealth - (referenceHealth + pendingHealthAdjustment)) > updatedHealthValueEquilibriumDelta)) {
                if (((referenceHealth + pendingHealthAdjustment) - currentHealth > 0) && (currentHealth < 100.0f)) { currentHealth += healthIncreaseFactor * Time.deltaTime; }
                else if (((referenceHealth + pendingHealthAdjustment) - currentHealth < 0) && (currentHealth > 0.0f)) { currentHealth -= healthIncreaseFactor * Time.deltaTime; }
            }
            else {
                pendingHealthAdjustment = 0.0f;
                levelAdjustmentIsNew = true;
            }
        }

        int width = (int)((currentHealth / fullHealthValue) * healthBarFullWidth);
        healthBar.rectTransform.sizeDelta = new Vector2(width, healthBar.rectTransform.sizeDelta.y);
    }

    public void TakeDamage(float damageAmount) {
        //currentHealth = Mathf.Max(currentHealth - damageAmount, 100.0f);
        pendingHealthAdjustment -= damageAmount;
    }

    public void Heal(float recovery) {
        //currentHealth = Mathf.Max(currentHealth + recovery, 0.0f);
        pendingHealthAdjustment += recovery;
    }
}
