using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemScreen : MonoBehaviour
{
    public RawImage labelListImage;
    public Transform sfxVolumeSlider, musicVolumeSlider, saveSwitchPivot, resumeSwitchPivot, spiderSwitchPivot, shutdownButton;
    public Texture saveGameSelect, resumeGameSelect, sfxVolumeSelect, musicVolumeSelect, spidersEverywhereSelect, shutdownSelect;

    public Transform spiderSwitchLight;
    public Material[] spiderSwitchLitMaterials, spiderSwitchUnlitMaterials;
    [SerializeField]
    private float switchOnRotationY;
    private Quaternion saveSwitchPivotInitialRot, resumeSwitchPivotInitialRot, spiderSwitchPivotInitialRot;

    private bool isActive, spidersOn;

    private enum SystemMode { Selecting, Saving, Resuming, AdjustingSFX, AdjustingMusic, ShuttingDown }
    private SystemMode SystemState;

    private Texture curSelectedOption;

    [SerializeField, Range(0.0f, 1.0f)]
    private float functionLabelFlashingAlphaMin, functionLabelFlashingAlphaMax, functionLabelFullAlpha;
    [SerializeField]
    private float functionLabelFlashFrameFactor, functionLabelFlashFrameTime;
    private float functionLabelFlashRefTime;
    private bool functionLabelAlphaIsIncreasing;
    private Vector3 sfxSliderInitialPos, musicSliderInitialPos;
    [SerializeField]
    private float sliderSpeed, sliderMaxDistanceFromFull, sliderMaxDistanceDelta;
    [SerializeField]
    private float sfxVolumeInitial = 1, musicVolumeInitial = 1;

    // Start is called before the first frame update
    void Start()
    {
        ShowAllTextAndHighlights(false);

        sfxSliderInitialPos = sfxVolumeSlider.localPosition;
        musicSliderInitialPos = musicVolumeSlider.localPosition;

        saveSwitchPivotInitialRot = saveSwitchPivot.rotation;
        resumeSwitchPivotInitialRot = resumeSwitchPivot.rotation;
        spiderSwitchPivotInitialRot = spiderSwitchPivot.rotation;

        functionLabelAlphaIsIncreasing = false;

        SystemState = SystemMode.Selecting;
    }

    // Update is called once per frame
    void Update()
    {

        switch (SystemState) {
            case SystemMode.Selecting:

                labelListImage.texture = curSelectedOption;

                FlashIcon();

                break;
            case SystemMode.Saving:
                break;
            case SystemMode.Resuming:
                break;
            case SystemMode.AdjustingSFX:

                //UpdateSFXVolume();

                break;
            case SystemMode.AdjustingMusic:

                //UpdateMusicVolume();

                break;
            case SystemMode.ShuttingDown:
                break;
        }
    }

    private void ShowAllTextAndHighlights(bool show) {

        if (show) {
            curSelectedOption = saveGameSelect;
            labelListImage.texture = saveGameSelect;

            SystemState = SystemMode.Selecting;
        }
        else {
            SystemState = SystemMode.Selecting;
        }

        labelListImage.enabled = show;
    }

    public bool IsActive() {
        return isActive;
    }

    public bool IsInSelectingMode() {
        return SystemState == SystemMode.Selecting;
    }

    public void ActivateSystemOptions(bool makeActive) {

        isActive = makeActive;

        ShowAllTextAndHighlights(makeActive);
    }

    private void SaveGame() {
        SystemState = SystemMode.Saving;
    }

    private void ResumeGame() {
        SystemState = SystemMode.Resuming;
    }

    private void UpdateSFXVolume() {

        float sfxVolume = (sfxVolumeSlider.position - sfxSliderInitialPos).magnitude / sliderMaxDistanceFromFull;

    }

    private void UpdateMusicVolume() {

        float musicVolume = (musicVolumeSlider.position - musicSliderInitialPos).magnitude / sliderMaxDistanceFromFull;

    }

    private void ToggleSpiders() {
        spidersOn = !spidersOn;

        if (spidersOn) {
            spiderSwitchLight.GetComponent<MeshRenderer>().materials = spiderSwitchLitMaterials;
            spiderSwitchPivot.Rotate(0, 0, switchOnRotationY);
        }
        else {
            spiderSwitchLight.GetComponent<MeshRenderer>().materials = spiderSwitchUnlitMaterials;
            spiderSwitchPivot.rotation = spiderSwitchPivotInitialRot;
        }
    }

    private void ShutDownTheGame() {
        SystemState = SystemMode.ShuttingDown;
    }

    private void FlashIcon() {
        if (Time.time - functionLabelFlashRefTime > functionLabelFlashFrameTime) {
            if (functionLabelAlphaIsIncreasing) {
                if (labelListImage.color.a < functionLabelFlashingAlphaMax) {
                    labelListImage.color = new Color(labelListImage.color.r, labelListImage.color.g, labelListImage.color.b, labelListImage.color.a + functionLabelFlashFrameFactor * Time.deltaTime);
                }
                else {
                    functionLabelAlphaIsIncreasing = false;
                }

                functionLabelFlashRefTime = Time.time;
            }
            else {
                if (labelListImage.color.a > functionLabelFlashingAlphaMin) {
                    labelListImage.color = new Color(labelListImage.color.r, labelListImage.color.g, labelListImage.color.b, labelListImage.color.a - functionLabelFlashFrameFactor * Time.deltaTime);
                }
                else {
                    functionLabelAlphaIsIncreasing = true;
                }

                functionLabelFlashRefTime = Time.time;
            }
        }
    }

    public void PressX() {
        if (SystemState == SystemMode.Selecting) {
            if (curSelectedOption == saveGameSelect) {
                SystemState = SystemMode.Saving;
                saveSwitchPivot.Rotate(0, 0, switchOnRotationY);
            } else if (curSelectedOption == resumeGameSelect) {
                SystemState = SystemMode.Resuming;
                resumeSwitchPivot.Rotate(0, 0, switchOnRotationY);
            } else if (curSelectedOption == sfxVolumeSelect) {
                SystemState = SystemMode.AdjustingSFX;
            } else if (curSelectedOption == musicVolumeSelect) {
                SystemState = SystemMode.AdjustingMusic;
            } else if (curSelectedOption == spidersEverywhereSelect) {
                ToggleSpiders();
            } else if (curSelectedOption == shutdownSelect) {
                SystemState = SystemMode.ShuttingDown;
            }

            labelListImage.color = new Color(labelListImage.color.r, labelListImage.color.g, labelListImage.color.b, functionLabelFullAlpha);

        } else if (SystemState == SystemMode.AdjustingSFX || SystemState == SystemMode.AdjustingMusic) {
            SystemState = SystemMode.Selecting;
        } 
    }

    public void PressCircle() {
        if (SystemState == SystemMode.Saving) {
            SystemState = SystemMode.Selecting;
            saveSwitchPivot.rotation = saveSwitchPivotInitialRot;
        }
        else if (SystemState == SystemMode.Resuming) {
            SystemState = SystemMode.Selecting;
            resumeSwitchPivot.rotation = resumeSwitchPivotInitialRot;
        }
        else if (SystemState == SystemMode.AdjustingSFX || SystemState == SystemMode.AdjustingMusic || SystemState == SystemMode.ShuttingDown) {
            SystemState = SystemMode.Selecting;
        }
    }

    public void PressUpLS() {
        if (SystemState == SystemMode.Selecting) {
            if (curSelectedOption == saveGameSelect || curSelectedOption == resumeGameSelect) {
                curSelectedOption = shutdownSelect;
            }
            else if (curSelectedOption == sfxVolumeSelect) {
                curSelectedOption = saveGameSelect;
            }
            else if (curSelectedOption == musicVolumeSelect) {
                curSelectedOption = sfxVolumeSelect;
            }
            else if (curSelectedOption == spidersEverywhereSelect) {
                curSelectedOption = musicVolumeSelect;
            }
            else if (curSelectedOption == shutdownSelect) {
                curSelectedOption = spidersEverywhereSelect;
            }
        }
    }

    public void PressDownLS() {
        if (SystemState == SystemMode.Selecting) {
            if (curSelectedOption == saveGameSelect || curSelectedOption == resumeGameSelect) {
                curSelectedOption = sfxVolumeSelect;
            }
            else if (curSelectedOption == sfxVolumeSelect) {
                curSelectedOption = musicVolumeSelect;
            }
            else if (curSelectedOption == musicVolumeSelect) {
                curSelectedOption = spidersEverywhereSelect;
            }
            else if (curSelectedOption == spidersEverywhereSelect) {
                curSelectedOption = shutdownSelect;
            }
            else if (curSelectedOption == shutdownSelect) {
                curSelectedOption = saveGameSelect;
            }
        }
    }

    public void PressLeftLS(float intensity) {
        if (SystemState == SystemMode.Selecting) {
            if (curSelectedOption == saveGameSelect) {
                curSelectedOption = resumeGameSelect;
            }
            else if (curSelectedOption == resumeGameSelect) {
                curSelectedOption = saveGameSelect;
            }
        }
        else if (SystemState == SystemMode.AdjustingSFX) {
            if (sfxSliderInitialPos.x - sfxVolumeSlider.localPosition.x < sliderMaxDistanceFromFull) {
                sfxVolumeSlider.Translate(intensity * sliderSpeed * Time.deltaTime, 0, 0, Space.Self);
            }
        }
        else if (SystemState == SystemMode.AdjustingMusic) {
            if (musicSliderInitialPos.x - musicVolumeSlider.localPosition.x < sliderMaxDistanceFromFull) {
                musicVolumeSlider.Translate(intensity * sliderSpeed * Time.deltaTime, 0, 0, Space.Self);
            }
        }
    }

    public void PressRightLS(float intensity) {
        if (SystemState == SystemMode.Selecting) {
            if (curSelectedOption == saveGameSelect) {
                curSelectedOption = resumeGameSelect;
            }
            else if (curSelectedOption == resumeGameSelect) {
                curSelectedOption = saveGameSelect;
            }
        }
        else if (SystemState == SystemMode.AdjustingSFX) {
            if (sfxVolumeSlider.localPosition.x < sfxSliderInitialPos.x) {
                sfxVolumeSlider.Translate(intensity * sliderSpeed * Time.deltaTime, 0, 0, Space.Self);
            }
        }
        else if (SystemState == SystemMode.AdjustingMusic) {
            if (musicVolumeSlider.localPosition.x < musicSliderInitialPos.x) {
                musicVolumeSlider.Translate(intensity * sliderSpeed * Time.deltaTime, 0, 0, Space.Self);
            }
        }
    }
}
