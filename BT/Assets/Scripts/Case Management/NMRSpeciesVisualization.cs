using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NMRSpeciesVisualization
{
    private float shift;
    private bool isMobile, isPhage, isConsuming, isBeingConsumed, isDead;
    private float movementSpeedMin, movementSpeedMax;
    private Vector2 curVelocity, stageCenter, stageLimits;
    private float detectionRange;
    private List<float> tastyPeakShifts = new List<float>();

    private RawImage speciesVisImage;
    private NMRSpeciesVisualization curTarget;
    private List<NMRSpeciesVisualization> potentialVictims = new List<NMRSpeciesVisualization>();
    private List<Texture> movementFrames = new List<Texture>(), phageFrames = new List<Texture>(), destructionFrames = new List<Texture>();
    private int movementFrameIndex, phageFrameIndex, destructionFrameIndex;
    private float movementFrameRefTime, phageFrameRefTime, destructionFrameRefTime;

    [SerializeField]
    private float frameTime = 1f, tastyPeakShiftDelta = 1, consumptionRange = 2, imageHalfDiameter = 3f;

    public NMRSpeciesVisualization(float pkSh, RawImage img, Vector2 centerPos, Vector2 limits, float moveSpeedMin, float moveSpeedMax, bool mobilize, 
                                    List<Texture> movementSlides, List<Texture> deathSlides) {

        shift = pkSh;
        isBeingConsumed = false;
        isDead = false;

        speciesVisImage = img;
        movementFrames = movementSlides;
        destructionFrames = deathSlides;

        isConsuming = false;

        if (mobilize) {
            isMobile = true;
            movementSpeedMin = moveSpeedMin;
            movementSpeedMax = moveSpeedMax;

            SetRandomVelocity();

            movementFrameIndex = 0;
            speciesVisImage.texture = movementFrames[movementFrameIndex];
            movementFrameRefTime = Time.time;
        }
        else {
            isMobile = false;
            movementFrameIndex = 0;
            speciesVisImage.texture = movementFrames[movementFrameIndex];
        }

        stageCenter = centerPos;
        stageLimits = limits;
        speciesVisImage.enabled = true;
    }

    public NMRSpeciesVisualization(float pkSh, RawImage img, Vector2 centerPos, Vector2 limits, float moveSpeedMin, float moveSpeedMax, bool mobilize, float targetRange, List<float> ediblePeaks,
                                    List<Texture> movementSlides, List<Texture> eatSlides, List<Texture> deathSlides) {

        shift = pkSh;
        isBeingConsumed = false;
        isDead = false;

        speciesVisImage = img;
        movementFrames = movementSlides;
        destructionFrames = deathSlides;

        isPhage = true;
        isConsuming = false;
        phageFrames = eatSlides;
        detectionRange = targetRange;
        tastyPeakShifts = ediblePeaks;
        phageFrameIndex = 0;
        phageFrameRefTime = Time.time;

        if (mobilize) {
            isMobile = true;
            movementSpeedMin = moveSpeedMin;
            movementSpeedMax = moveSpeedMax;

            SetRandomVelocity();

            movementFrameIndex = 0;
            speciesVisImage.texture = movementFrames[movementFrameIndex];
            movementFrameRefTime = Time.time;
        }
        else {
            isMobile = false;
            movementFrameIndex = 0;
            speciesVisImage.texture = movementFrames[movementFrameIndex];
        }

        stageCenter = centerPos;
        stageLimits = limits;
        speciesVisImage.enabled = true;
    }

    public void InitializeVictimList(List<NMRSpeciesVisualization> vics) {

        potentialVictims.Clear();
        foreach (NMRSpeciesVisualization spec in vics) {

            if (!spec.IsDestroyed()) {
                bool foundMatch = false;

                foreach (float sh in tastyPeakShifts) {
                    if (!foundMatch && (spec.GetShift() < (sh + tastyPeakShiftDelta)) && (spec.GetShift() > (sh - tastyPeakShiftDelta))) {
                        potentialVictims.Add(spec);
                        foundMatch = true;
                    }
                }
            }
        }
    }

    public void UpdatePositionAndFrames() {

        if (!isDead) {
            if (isMobile && !isConsuming && !isBeingConsumed) {

                if (isPhage && curTarget != null) {
                    // If it's a phage and has a target, move to target at maximum speed
                    if ((curTarget.GetImage().rectTransform.anchoredPosition - speciesVisImage.rectTransform.anchoredPosition).magnitude < consumptionRange) {

                        curVelocity = Vector2.zero;
                        curTarget.MakeBeingConsumed();

                        isConsuming = true;
                        phageFrameIndex = 0;
                        phageFrameRefTime = Time.time;
                    }
                    else {
                        curVelocity = (curTarget.GetImage().rectTransform.anchoredPosition - speciesVisImage.rectTransform.anchoredPosition).normalized * movementSpeedMax;
                    }
                }

                if (Time.time - movementFrameRefTime > frameTime) {
                    movementFrameIndex = (movementFrameIndex + 1) % movementFrames.Count;
                    speciesVisImage.texture = movementFrames[movementFrameIndex];
                    movementFrameRefTime = Time.time;
                }

                float relX = speciesVisImage.rectTransform.anchoredPosition.x;
                float relY = speciesVisImage.rectTransform.anchoredPosition.y;

                if (Mathf.Abs(speciesVisImage.rectTransform.anchoredPosition.x) > (stageLimits.x + imageHalfDiameter)) {
                    //speciesVisImage.rectTransform.anchoredPosition = new Vector2(-relX, speciesVisImage.rectTransform.anchoredPosition.y);
                    speciesVisImage.enabled = false;
                    isDead = true;
                }
                if (Mathf.Abs(speciesVisImage.rectTransform.anchoredPosition.y) > (stageLimits.y + imageHalfDiameter)) {
                    //speciesVisImage.rectTransform.anchoredPosition = new Vector2(speciesVisImage.rectTransform.anchoredPosition.x, -relY);
                    speciesVisImage.enabled = false;
                    isDead = true;
                }
            }

            if (isPhage) {

                if (isConsuming) {

                    if (curTarget != null) {
                        if (curTarget.IsDestroyed()) {
                            isConsuming = false;
                            curTarget = null;

                            if (isMobile) {
                                SetRandomVelocity();
                                movementFrameIndex = 0;
                                speciesVisImage.texture = movementFrames[movementFrameIndex];
                                movementFrameRefTime = Time.time;
                            }
                        }
                        else {
                            if (Time.time - phageFrameRefTime > frameTime) {
                                phageFrameIndex = (phageFrameIndex + 1) % phageFrames.Count;
                                speciesVisImage.texture = phageFrames[phageFrameIndex];
                                phageFrameRefTime = Time.time;
                            }
                        }
                    }
                }
                else {

                    if (curTarget == null && potentialVictims.Count > 0) {

                        NMRSpeciesVisualization closestVic = potentialVictims[0];

                        foreach (NMRSpeciesVisualization spec in potentialVictims) {

                            float closestVicDist = (closestVic.GetImage().rectTransform.anchoredPosition - speciesVisImage.rectTransform.anchoredPosition).magnitude;
                            float tempDist = (spec.GetImage().rectTransform.anchoredPosition - speciesVisImage.rectTransform.anchoredPosition).magnitude;

                            if (tempDist < closestVicDist) { closestVic = spec; }
                        }

                        if ((closestVic.GetImage().rectTransform.anchoredPosition - speciesVisImage.rectTransform.anchoredPosition).magnitude < detectionRange) {
                            curTarget = closestVic;
                        }
                    }
                }

            }

            if (isBeingConsumed) {
                if (Time.time - destructionFrameRefTime > frameTime) {
                    if (destructionFrameIndex < destructionFrames.Count - 1) {
                        destructionFrameIndex++;
                        speciesVisImage.texture = destructionFrames[destructionFrameIndex];
                        destructionFrameRefTime = Time.time;
                    }
                    else {
                        isBeingConsumed = false;
                        isDead = true;
                        isPhage = false;
                    }
                }
            }

            speciesVisImage.rectTransform.anchoredPosition += curVelocity * Time.deltaTime;
        }
    }

    private void SetRandomVelocity() {
        int plusOrMinusOneX;
        if (Random.Range(0, 2) == 1) { plusOrMinusOneX = 1; }
        else plusOrMinusOneX = -1;

        int plusOrMinusOneY;
        if (Random.Range(0, 2) == 1) { plusOrMinusOneY = 1; }
        else { plusOrMinusOneY = -1; }

        curVelocity = new Vector2(Random.Range(movementSpeedMin, movementSpeedMax) * plusOrMinusOneX, Random.Range(movementSpeedMin, movementSpeedMax) * plusOrMinusOneY);
    }

    public Vector2 GetPosition() {
        return speciesVisImage.rectTransform.anchoredPosition;
    }

    public void MakeDead() {
        isDead = true;
    }

    public void MakeBeingConsumed() {
        if (!isBeingConsumed) {
            isBeingConsumed = true;
            if (isMobile) {
                isMobile = false;
                curVelocity = Vector2.zero;
            }

            if (curTarget != null) {
                curTarget.MakeNotBeingConsumed();
            }

            destructionFrameIndex = 0;
            speciesVisImage.texture = destructionFrames[destructionFrameIndex];
            destructionFrameRefTime = Time.time;
        }
    }

    public void MakeNotBeingConsumed() {
        isBeingConsumed = false;
    }

    public float GetShift() {
        return shift;
    }

    public NMRSpeciesVisualization GetTarget() {
        return curTarget;
    }

    public RawImage GetImage() {
        return speciesVisImage;
    }

    public bool IsDestroyed() {
        return isDead;
    }
}
