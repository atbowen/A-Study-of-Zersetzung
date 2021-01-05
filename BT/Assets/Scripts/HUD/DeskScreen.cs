using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeskScreen : MonoBehaviour {

    public RawImage projectsBgd, inventoryBgd, statusBgd,
                    projectListBgd, projListFrame, projListOverlay, projListSelectorBar, currentProjectImage, firstBPImageBgd, firstBPImage, secondBPImageBgd, secondBPImage, currentProjectBulletPointSelectorBox,
                    equipButton, itemInHandHighlighter, combineItemButton, itemSelectorBox, itemActionSelectorBox, firstItemCombineBox, currentInvItemInspector,
                    eyeInspector, hostInspector;
    public List<RawImage> itemIcons;
    public Texture emptySlotTexture;
    public Camera inventoryItemCam, eyeInspectorCam, hostInspectorCam;
    public Text projectListTxt, currentProjectTitleTxt, currentProjectNotesTxt, currentProjectBulletPointsTxt,
                currentInvItemNameTxt, currentItemInvDescriptionTxt,
                eyeStatusTxt, eyeStatusHeaderTxt, hostStatusTxt, hostStatusHeaderTxt;

    private bool deskScreenActivated;

    private enum DeskScreenTabMode { InventoryTab, ProjectTab, StatusTab }
    private DeskScreenTabMode DeskScreenTabState;

    private enum InventoryMode { Selecting, SelectingCombinedItem, Combining }
    private InventoryMode InventoryState;

    private ProjectHandler projHandler;
    [SerializeField]
    private Inventory tedInventory;

    [SerializeField, Range(0.0f, 1.0f)]
    private float projBulletPointSelectorFrameAlphaChange, itemSelectorFrameAlphaChange, itemActionSelectorFrameAlphaChange, iconAlphaMin, iconAlphaMax;
    [SerializeField]
    private float iconFlashFrameTime;
    private float flashingIconRefTime;
    private bool flashingIconIsIncreasingAlpha;

    // Manage row n column selection
    private RawImage[,] itemSlots = new RawImage[4, 4];
    private IDItem[,] inventoryItems = new IDItem[4, 4];
    private int curItemRow, curItemCol, combiningFirstItemRow, combiningFirstItemCol, numOfItems;
    private RawImage curItemIcon, currentItemActionIcon;
    private IDItem prevItem, curItem, currentlyEquippedItem, curFirstCombinationItem;

    private bool needToUpdateItemToView;

    // Status screen effects
    [SerializeField]
    private Transform hostModel, eyeModel;
    [SerializeField]
    private float hostModelRotationSpeed, eyeModelRotationSpeed;
    [SerializeField]
    private string hostNameTitleSpecies, hostHeightWeightBloodType, hostHealthSapLevelConnectionStrength, hostExtraEffects,
                    eyeNameVersionConnection, eyeExtraEffects;
    [SerializeField]
    private string hostNameHeader, hostHeightHeader, hostHealthHeader, hostEffectsHeader,
                    eyeNameHeader, eyeEffectsHeader;
    [SerializeField]
    private Color statusTextHeaderColor, statusTextResultsColor;
    [SerializeField]
    private float hostNameDisplayDelay, hostHeightDisplayDelay, hostHealthDisplayDelay, hostEffectsDisplayDelay,
                    eyeNameDisplayDelay, eyeEffectsDisplayDelay;    
    [SerializeField]
    private string statusTextRandomCharString;
    [SerializeField]
    private float charUnravelTimeMin, charUnravelTimeMax;
    [SerializeField, Range(0.0f, 1.0f)]
    private float chanceToDisplayRandomChar, chanceToResetString;
    private string curHostNameString, curHostHeightString, curHostHealthString, curHostEffectsString,
                    curEyeNameString, curEyeEffectsString;
    private bool justDisplayedRandomHostNameChar, justDisplayedRandomHostHeightChar, justDisplayedRandomHostHealthChar, justDisplayedRandomHostEffectsChar,
                    justDisplayedRandomEyeNameChar, justDisplayedRandomEyeEffectsChar;
    //private bool mustResetHostNameString, mustResetHostHeightString, mustResetHostHealthString, mustResetHostEffectsString,
    //                mustResetEyeNameString, mustResetEyeEffectsString;
    private float hostNameCharUnravelTime, hostHeightCharUnravelTime, hostHealthCharUnravelTime, hostEffectsCharUnravelTime,
                    eyeNameCharUnravelTime, eyeEffectsCharUnravelTime;
    private float hostNameUnravelFrameRefTime, hostHeightUnravelFrameRefTime, hostHealthUnravelFrameRefTime, hostEffectsUnravelFrameRefTime,
                    eyeNameUnravelFrameRefTime, eyeEffectsUnravelFrameRefTime;
    private int hostNameCharIndex, hostHeightCharIndex, hostHealthCharIndex, hostEffectsCharIndex,
                    eyeNameCharIndex, eyeEffectsCharIndex;


    // Start is called before the first frame update
    void Start() {

        projHandler = FindObjectOfType<ProjectHandler>();

        DeskScreenTabState = DeskScreenTabMode.ProjectTab;
        InventoryState = InventoryMode.Selecting;

        // Populate the inventory icon array
        int index = 0;

        for (int i = 0; i < itemSlots.GetLength(0); i++) {
            for (int j = 0; j < itemSlots.GetLength(1); j++) {
                itemSlots[i, j] = itemIcons[index];
                index++;
            }
        }

        numOfItems = tedInventory.items.Count;

        currentItemActionIcon = equipButton;
        curItemRow = 0;
        curItemCol = 0;

        needToUpdateItemToView = true;

        hostStatusTxt.supportRichText = true;
        hostStatusHeaderTxt.supportRichText = true;
        eyeStatusTxt.supportRichText = true;
        eyeStatusHeaderTxt.supportRichText = true;
    }

    // Update is called once per frame
    void Update() {

        numOfItems = tedInventory.items.Count;
        GetTedInventory();

        if (deskScreenActivated) {
            switch (DeskScreenTabState) {
                case DeskScreenTabMode.ProjectTab:

                    ShowProjectWindows(true);
                    FlashIcon(projListSelectorBar, projBulletPointSelectorFrameAlphaChange);

                    break;
                case DeskScreenTabMode.InventoryTab:

                    ShowInventoryWindows(true);

                    //numOfItems = tedInventory.items.Count;

                    if (numOfItems > 0) {

                        HighLightCurrentlyEquippedItem();

                        curItem = inventoryItems[curItemRow, curItemCol];
                        curItemIcon = itemSlots[curItemRow, curItemCol];

                        if (curItem != prevItem && prevItem != null) {
                            tedInventory.ReturnInventoryItem(prevItem);
                        }

                        prevItem = curItem;

                        itemSelectorBox.rectTransform.anchoredPosition = curItemIcon.rectTransform.anchoredPosition;

                        currentInvItemNameTxt.enabled = true;
                        currentItemInvDescriptionTxt.enabled = true;

                        currentInvItemNameTxt.text = curItem.ObjActualName;
                        currentItemInvDescriptionTxt.text = curItem.ObjDescription;

                        inventoryItemCam.enabled = true;
                        currentInvItemInspector.enabled = true;

                        if (needToUpdateItemToView) {
                            tedInventory.PlaceItemToView(curItem);
                            needToUpdateItemToView = false;
                        }

                        RotateModel(curItem.itemCopyForInventory, hostModelRotationSpeed);
                    }
                    else {

                        currentInvItemNameTxt.enabled = true;
                        currentItemInvDescriptionTxt.enabled = true;

                        currentInvItemNameTxt.text = "";
                        currentItemInvDescriptionTxt.text = "";

                        inventoryItemCam.enabled = false;
                        currentInvItemInspector.enabled = false;
                    }

                    switch (InventoryState) {
                        case InventoryMode.Selecting:

                            itemActionSelectorBox.rectTransform.anchoredPosition = currentItemActionIcon.rectTransform.anchoredPosition;
                            firstItemCombineBox.enabled = false;

                            if (numOfItems > 0) {
                                itemSelectorBox.enabled = true;
                                //FlashIcon(itemActionSelectorBox, itemActionSelectorFrameAlphaChange);
                                FlashIcon(itemSelectorBox, itemSelectorFrameAlphaChange);
                            }
                            else {
                                itemSelectorBox.enabled = false;
                            }

                            break;
                        case InventoryMode.SelectingCombinedItem:

                            if (numOfItems > 1) {
                                itemSelectorBox.enabled = true;
                                firstItemCombineBox.enabled = true;
                                FlashIcon(itemSelectorBox, itemActionSelectorFrameAlphaChange);
                                //itemActionSelectorBox.color = new Color(itemActionSelectorBox.color.r, itemActionSelectorBox.color.g, itemActionSelectorBox.color.b, iconAlphaMax);
                            }

                            break;
                        case InventoryMode.Combining:



                            break;
                    }

                    break;
                case DeskScreenTabMode.StatusTab:

                    ShowStatusWindows(true);
                    RotateModel(hostModel, hostModelRotationSpeed);
                    RotateModel(eyeModel, eyeModelRotationSpeed);

                    string headerColorHex = ColorUtility.ToHtmlStringRGBA(statusTextHeaderColor);
                    string resultsColorHex = ColorUtility.ToHtmlStringRGBA(statusTextResultsColor);

                    hostStatusTxt.text = "<color=#" + headerColorHex + ">" + hostNameHeader + "</color>  <color=#" + resultsColorHex + ">" + curHostNameString + "</color>\n\n" +
                                            "<color=#" + headerColorHex + ">" + hostHeightHeader + "</color>  <color=#" + resultsColorHex + ">" + curHostHeightString + "</color>\n\n" +
                                            "<color=#" + headerColorHex + ">" + hostHealthHeader + "</color>  <color=#" + resultsColorHex + ">" + curHostHealthString + "</color>\n\n" +
                                            "<color=#" + headerColorHex + ">" + hostEffectsHeader + "</color>  <color=#" + resultsColorHex + ">" + curHostEffectsString + "</color>";
                    eyeStatusTxt.text = "<color=#" + resultsColorHex + ">" + curEyeNameString + "</color>  <color=#" + headerColorHex + ">" + eyeNameHeader + "</color>\n\n" +
                                            "<color=#" + resultsColorHex + ">" + curEyeEffectsString + "</color>  <color=#" + headerColorHex + ">" + eyeEffectsHeader + "</color>";

                    //hostStatusHeaderTxt.text = hostNameHeader + "\n\n" + hostHeightHeader + "\n\n" + hostHealthHeader + "\n\n" + hostEffectsHeader;
                    //hostStatusTxt.text = curHostNameString + "\n\n" + curHostHeightString + "\n\n" + curHostHealthString + "\n\n" + curHostEffectsString;

                    //eyeStatusHeaderTxt.text = eyeNameHeader + "\n\n" + eyeEffectsHeader;
                    //eyeStatusTxt.text = curEyeNameString + "\n\n" + curEyeEffectsString;

                    if ((Time.time - hostNameUnravelFrameRefTime > hostNameCharUnravelTime) && (hostNameCharIndex < hostNameTitleSpecies.ToCharArray().Length)) {
                        
                        if (justDisplayedRandomHostNameChar) { curHostNameString = curHostNameString.Remove(curHostNameString.Length - 1, 1); }

                        bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                        if (displayRandomChar) {
                            curHostNameString = curHostNameString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length)];
                            justDisplayedRandomHostNameChar = true;
                        }
                        else {
                            curHostNameString = curHostNameString + hostNameTitleSpecies.ToCharArray()[hostNameCharIndex];
                            justDisplayedRandomHostNameChar = false;
                            hostNameCharIndex++;
                        }

                        //if (Random.Range(0.0f, 1.0f) < chanceToResetString) {
                        //    curHostNameString = "";
                        //    hostNameCharIndex = 0;
                        //}

                        hostNameUnravelFrameRefTime = Time.time;
                    }
                    if ((Time.time - hostHeightUnravelFrameRefTime > hostHeightCharUnravelTime) && (hostHeightCharIndex < hostHeightWeightBloodType.ToCharArray().Length)) {

                        if (justDisplayedRandomHostHeightChar) { curHostHeightString = curHostHeightString.Remove(curHostHeightString.Length - 1, 1); }

                        bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                        if (displayRandomChar) {
                            curHostHeightString = curHostHeightString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length)];
                            justDisplayedRandomHostHeightChar = true;
                        }
                        else {
                            curHostHeightString = curHostHeightString + hostHeightWeightBloodType.ToCharArray()[hostHeightCharIndex];
                            justDisplayedRandomHostHeightChar = false;
                            hostHeightCharIndex++;
                        }

                        //if (Random.Range(0.0f, 1.0f) < chanceToResetString) {
                        //    curHostHeightString = "";
                        //    hostHeightCharIndex = 0;
                        //}

                        hostHeightUnravelFrameRefTime = Time.time;
                    }
                    if ((Time.time - hostHealthUnravelFrameRefTime > hostHealthCharUnravelTime) && (hostHealthCharIndex < hostHealthSapLevelConnectionStrength.ToCharArray().Length)) {

                        if (justDisplayedRandomHostHealthChar) { curHostHealthString = curHostHealthString.Remove(curHostHealthString.Length - 1, 1); }

                        bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                        if (displayRandomChar) {
                            curHostHealthString = curHostHealthString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)];
                            justDisplayedRandomHostHealthChar = true;
                        }
                        else {
                            curHostHealthString = curHostHealthString + hostHealthSapLevelConnectionStrength.ToCharArray()[hostHealthCharIndex];
                            justDisplayedRandomHostHealthChar = false;
                            hostHealthCharIndex++;
                        }

                        //if (Random.Range(0.0f, 1.0f) < chanceToResetString) {
                        //    curHostHealthString = "";
                        //    hostHealthCharIndex = 0;
                        //}

                        hostHealthUnravelFrameRefTime = Time.time;
                    }

                    if ((Time.time - hostEffectsUnravelFrameRefTime > hostEffectsCharUnravelTime) && (hostEffectsCharIndex < hostExtraEffects.ToCharArray().Length)) {

                        if (justDisplayedRandomHostEffectsChar) { curHostEffectsString = curHostEffectsString.Remove(curHostEffectsString.Length - 1, 1); }

                        bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                        if (displayRandomChar) {
                            curHostEffectsString = curHostEffectsString + statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)];
                            justDisplayedRandomHostEffectsChar = true;
                        }
                        else {
                            curHostEffectsString = curHostEffectsString + hostExtraEffects.ToCharArray()[hostEffectsCharIndex];
                            justDisplayedRandomHostEffectsChar = false;
                            hostEffectsCharIndex++;
                        }

                        //if (Random.Range(0.0f, 1.0f) < chanceToResetString) {
                        //    curHostEffectsString = "";
                        //    hostEffectsCharIndex = 0;
                        //}

                        hostEffectsUnravelFrameRefTime = Time.time;
                    }
                    if ((Time.time - eyeNameUnravelFrameRefTime > eyeNameCharUnravelTime) && (eyeNameCharIndex > -1)) {

                        if (justDisplayedRandomEyeNameChar) { curEyeNameString = curEyeNameString.Remove(0, 1); }

                        bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                        if (displayRandomChar) {
                            curEyeNameString = statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)] + curEyeNameString;
                            justDisplayedRandomEyeNameChar = true;
                        }
                        else {
                            curEyeNameString = eyeNameVersionConnection.ToCharArray()[eyeNameCharIndex] + curEyeNameString;
                            justDisplayedRandomEyeNameChar = false;
                            eyeNameCharIndex--;
                        }

                        //if (Random.Range(0.0f, 1.0f) < chanceToResetString) {
                        //    curEyeNameString = "";
                        //    eyeNameCharIndex = eyeNameVersionConnection.ToCharArray().Length - 1;
                        //}

                        eyeNameUnravelFrameRefTime = Time.time;
                    }
                    if ((Time.time - eyeEffectsUnravelFrameRefTime > eyeEffectsCharUnravelTime) && (eyeEffectsCharIndex > -1)) {

                        if (justDisplayedRandomEyeEffectsChar) { curEyeEffectsString = curEyeEffectsString.Remove(0, 1); }

                        bool displayRandomChar = (Random.Range(0.0f, 1.0f) < chanceToDisplayRandomChar);

                        if (displayRandomChar) {
                            curEyeEffectsString = statusTextRandomCharString.ToCharArray()[Random.Range(0, statusTextRandomCharString.ToCharArray().Length - 1)] + curEyeEffectsString;
                            justDisplayedRandomEyeEffectsChar = true;
                        }
                        else {
                            curEyeEffectsString = eyeExtraEffects.ToCharArray()[eyeEffectsCharIndex] + curEyeEffectsString;
                            justDisplayedRandomEyeEffectsChar = false;
                            eyeEffectsCharIndex--;
                        }

                        //if (Random.Range(0.0f, 1.0f) < chanceToResetString) {
                        //    curEyeEffectsString = "";
                        //    eyeEffectsCharIndex = eyeExtraEffects.ToCharArray().Length - 1;
                        //}

                        eyeEffectsUnravelFrameRefTime = Time.time;
                    }

                    break;
            }
        }
    }

    // Activates the switch-case block in Update which operates the Desk screen tabs
    public void ActivateDeskScreen() {
        deskScreenActivated = true;
    }

    // Deactivates the switch-case block in Update, hides all of the Desk screen tabs
    public void DeactivateDeskScreen() {
        deskScreenActivated = false;

        HideAllDeskScreens();
    }

    public void HideAllDeskScreens() {

        ShowProjectWindows(false);
        ShowInventoryWindows(false);
        ShowStatusWindows(false);
    }

    private void ShowProjectWindows(bool show) {
        if (show) {
            projectsBgd.enabled = true;
            projectListBgd.enabled = true;
            projListFrame.enabled = true;
            projListOverlay.enabled = true;
            projListSelectorBar.enabled = true;
            currentProjectImage.enabled = true;

            projectListTxt.enabled = true;
            currentProjectTitleTxt.enabled = true;
            currentProjectNotesTxt.enabled = true;
            currentProjectBulletPointsTxt.enabled = true;

            ShowInventoryWindows(false);
            ShowStatusWindows(false);
        }
        else {
            projectsBgd.enabled = false;
            projectListBgd.enabled = false;
            projListFrame.enabled = false;
            projListOverlay.enabled = false;
            projListSelectorBar.enabled = false;
            currentProjectImage.enabled = false;
            firstBPImageBgd.enabled = false;
            firstBPImage.enabled = false;
            secondBPImageBgd.enabled = false;
            secondBPImage.enabled = false;
            currentProjectBulletPointSelectorBox.enabled = false;

            projectListTxt.enabled = false;
            currentProjectTitleTxt.enabled = false;
            currentProjectNotesTxt.enabled = false;
            currentProjectBulletPointsTxt.enabled = false;
        }
    }

    private void ShowInventoryWindows(bool show) {
        if (show) {
            inventoryBgd.enabled = true;
            equipButton.enabled = true;
            combineItemButton.enabled = true;
            itemActionSelectorBox.enabled = true;
            foreach (RawImage img in itemIcons) {
                img.enabled = true;
            }

            ShowProjectWindows(false);
            ShowStatusWindows(false);
        }
        else {
            inventoryBgd.enabled = false;
            equipButton.enabled = false;
            combineItemButton.enabled = false;
            itemInHandHighlighter.enabled = false;
            combineItemButton.enabled = false;
            itemSelectorBox.enabled = false;
            itemActionSelectorBox.enabled = false;
            firstItemCombineBox.enabled = false;
            foreach (RawImage img in itemIcons) {
                img.enabled = false;
            }

            inventoryItemCam.enabled = false;
            currentInvItemInspector.enabled = false;

            currentInvItemNameTxt.enabled = false;
            currentItemInvDescriptionTxt.enabled = false;
        }
    }

    private void ShowStatusWindows(bool show) {

        statusBgd.enabled = show;

        eyeInspector.enabled = show;
        hostInspector.enabled = show;

        eyeInspectorCam.enabled = show;
        hostInspectorCam.enabled = show;

        eyeStatusTxt.enabled = show;
        eyeStatusHeaderTxt.enabled = show;
        hostStatusTxt.enabled = show;
        hostStatusHeaderTxt.enabled = show;

        if (show) {
            ShowProjectWindows(false);
            ShowInventoryWindows(false);
        }
        
        if (!show) {
            eyeStatusTxt.text = "";
            hostStatusTxt.text = "";
        }
    }

    private void GetTedInventory() {

        int sourceIndex = 0;

        for (int i = 0; i < inventoryItems.GetLength(0); i++) {
            for (int j = 0; j < inventoryItems.GetLength(1); j++) {
                if (sourceIndex < numOfItems) {
                    if (tedInventory.items[sourceIndex] != null) {
                        inventoryItems[i, j] = tedInventory.items[sourceIndex];
                        itemSlots[i, j].texture = tedInventory.items[sourceIndex].itemPic;
                    }
                    else {
                        inventoryItems[i, j] = null;
                        itemSlots[i, j].texture = emptySlotTexture;
                    }
                }
                else {
                    inventoryItems[i, j] = null;
                    itemSlots[i, j].texture = emptySlotTexture;
                }

                sourceIndex++;
            }
        }
    }

    public void PressX() {
        if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {

                if (tedInventory.items.Count > 0) {

                    if (currentItemActionIcon == equipButton) {
                        // Equip item if possible, or unequip
                        if (!curItem.isInHand) {
                            if (curItem.canBeInHand) {
                                if (currentlyEquippedItem != null) { currentlyEquippedItem.UnequipItem(); }
                                curItem.EquipItem();
                                currentlyEquippedItem = curItem;
                                //itemInHandHighlighter.enabled = true;
                                //itemInHandHighlighter.rectTransform.anchoredPosition = itemSlots[curItemRow, curItemCol].rectTransform.anchoredPosition;
                            }
                        }
                        else if (curItem.isInHand) {
                            curItem.UnequipItem();
                            currentlyEquippedItem = null;
                            //itemInHandHighlighter.enabled = false;
                        }
                    } else if (currentItemActionIcon == combineItemButton) {
                        if (curItem.makesCombinationItem && (numOfItems > 1)) {
                            curFirstCombinationItem = curItem;
                            firstItemCombineBox.rectTransform.anchoredPosition = curItemIcon.rectTransform.anchoredPosition;
                            int[] nextRowAndCol = GetRowAndColumnOfNextItem();
                            curItemRow = nextRowAndCol[0];
                            curItemCol = nextRowAndCol[1];

                            InventoryState = InventoryMode.SelectingCombinedItem;

                            needToUpdateItemToView = true;
                        }
                    }
                }

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

                IDItem firstItem = null, secondItem = null, combinedItem = null;

                if (curFirstCombinationItem.canCombineToMakeTheseItems.Count > 0) {
                    foreach (IDItem item in curFirstCombinationItem.canCombineToMakeTheseItems) {
                        if (item.isCombinedItem && item.consistsOfTheseItems.Contains(curItem)) {
                            combinedItem = item;
                            firstItem = curFirstCombinationItem;
                            secondItem = curItem;
                        }
                    }

                    if (firstItem && secondItem && combinedItem) {
                        tedInventory.items.Remove(firstItem);
                        tedInventory.items.Remove(secondItem);
                        tedInventory.items.Add(combinedItem);
                        numOfItems--;

                        curItem = tedInventory.items[numOfItems - 2];
                        int[] nextRowAndCol = GetRowAndColumnOfNextItem();
                        curItemRow = nextRowAndCol[0];
                        curItemCol = nextRowAndCol[1];

                        InventoryState = InventoryMode.Selecting;

                        needToUpdateItemToView = true;
                    }
                }

            }

        }
    }

    public void PressCircle() {
        if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {
                if (curItem != null) {
                    if (curItem.isInHand) {
                        curItem.UnequipItem();
                        currentlyEquippedItem = null;
                        //itemInHandHighlighter.enabled = false;
                    }
                }
            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {
                InventoryState = InventoryMode.Selecting;
            }

        }
    }

    public void PressUpLS() {
        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {



        } else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            needToUpdateItemToView = true;

            if (InventoryState == InventoryMode.Selecting) {

                bool foundOccupiedSlot = false;

                for (int i=1; i < inventoryItems.GetLength(0); i++) {

                    int nextIndex = (curItemRow - i + inventoryItems.GetLength(0)) % inventoryItems.GetLength(0);

                    if (!foundOccupiedSlot && inventoryItems[nextIndex, curItemCol] != null) {
                        curItemRow = nextIndex;
                        foundOccupiedSlot = true;
                    }
                }

            } else if (InventoryState == InventoryMode.SelectingCombinedItem) {
                bool foundOccupiedSlot = false;

                for (int i = 1; i < inventoryItems.GetLength(0); i++) {

                    int nextIndex = (curItemRow - i + inventoryItems.GetLength(0)) % inventoryItems.GetLength(0);

                    if (!foundOccupiedSlot && inventoryItems[nextIndex, curItemCol] != null) {
                        if (inventoryItems[nextIndex, curItemCol] != curFirstCombinationItem) {
                            curItemRow = nextIndex;
                            foundOccupiedSlot = true;
                        }
                    }
                }
            }

        } else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {



        }
    }

    public void PressDownLS() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {



        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            needToUpdateItemToView = true;

            if (InventoryState == InventoryMode.Selecting) {

                bool foundOccupiedSlot = false;

                for (int i = 1; i < inventoryItems.GetLength(0); i++) {

                    int nextIndex = (curItemRow + i + inventoryItems.GetLength(0)) % inventoryItems.GetLength(0);

                    if (!foundOccupiedSlot && inventoryItems[nextIndex, curItemCol] != null) {
                        curItemRow = nextIndex;
                        foundOccupiedSlot = true;
                    }
                }

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

                bool foundOccupiedSlot = false;

                for (int i = 1; i < inventoryItems.GetLength(0); i++) {

                    int nextIndex = (curItemRow + i + inventoryItems.GetLength(0)) % inventoryItems.GetLength(0);

                    if (!foundOccupiedSlot && inventoryItems[nextIndex, curItemCol] != null) {
                        if (inventoryItems[nextIndex, curItemCol] != curFirstCombinationItem) {
                            curItemRow = nextIndex;
                            foundOccupiedSlot = true;
                        }
                    }
                }

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {



        }

    }

    public void PressLeftLS() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {



        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            needToUpdateItemToView = true;

            if (InventoryState == InventoryMode.Selecting) {

                bool foundOccupiedSlot = false;

                for (int i = 1; i < inventoryItems.GetLength(1); i++) {

                    int nextIndex = (curItemCol - i + inventoryItems.GetLength(1)) % inventoryItems.GetLength(1);

                    if (!foundOccupiedSlot && inventoryItems[curItemRow, nextIndex] != null) {
                        curItemCol = nextIndex;
                        foundOccupiedSlot = true;
                    }
                }

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

                bool foundOccupiedSlot = false;

                for (int i = 1; i < inventoryItems.GetLength(1); i++) {

                    int nextIndex = (curItemCol - i + inventoryItems.GetLength(1)) % inventoryItems.GetLength(1);

                    if (!foundOccupiedSlot && inventoryItems[curItemRow, nextIndex] != null) {
                        if (inventoryItems[curItemRow, nextIndex] != curFirstCombinationItem) {
                            curItemCol = nextIndex;
                            foundOccupiedSlot = true;
                        }
                    }
                }

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {



        }

    }

    public void PressRightLS() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {



        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            needToUpdateItemToView = true;

            if (InventoryState == InventoryMode.Selecting) {

                bool foundOccupiedSlot = false;

                for (int i = 1; i < inventoryItems.GetLength(1); i++) {

                    int nextIndex = (curItemCol + i + inventoryItems.GetLength(1)) % inventoryItems.GetLength(1);

                    if (!foundOccupiedSlot && inventoryItems[curItemRow, nextIndex] != null) {
                        curItemCol = nextIndex;
                        foundOccupiedSlot = true;
                    }
                }

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

                bool foundOccupiedSlot = false;

                for (int i = 1; i < inventoryItems.GetLength(1); i++) {

                    int nextIndex = (curItemCol + i + inventoryItems.GetLength(1)) % inventoryItems.GetLength(1);

                    if (!foundOccupiedSlot && inventoryItems[curItemRow, nextIndex] != null) {
                        if (inventoryItems[curItemRow, nextIndex] != curFirstCombinationItem) {
                            curItemCol = nextIndex;
                            foundOccupiedSlot = true;
                        }
                    }
                }

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {



        }

    }

    public void PressUpRS() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {

        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {

        }

    }

    public void PressDownRS() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {

        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {

        }

    }

    public void PressLeftRS() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {

        } else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {

                if (currentItemActionIcon == equipButton)               { currentItemActionIcon = combineItemButton; }
                else if (currentItemActionIcon == combineItemButton)    { currentItemActionIcon = equipButton; }

            } else if (InventoryState == InventoryMode.SelectingCombinedItem) {

            }

        } else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {

        }

    }

    public void PressRightRS() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {

        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {

                if (currentItemActionIcon == equipButton) { currentItemActionIcon = combineItemButton; }
                else if (currentItemActionIcon == combineItemButton) { currentItemActionIcon = equipButton; }

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {

        }

    }

    public void PressUpArrow() {

    }

    public void PressDownArrow() {

    }

    public void PressLeftArrow() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {

            DeskScreenTabState = DeskScreenTabMode.StatusTab;
            InitializeStatusTexts();
        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {

                DeskScreenTabState = DeskScreenTabMode.ProjectTab;

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {

            DeskScreenTabState = DeskScreenTabMode.InventoryTab;

        }

    }

    public void PressRightArrow() {

        if (DeskScreenTabState == DeskScreenTabMode.ProjectTab) {

            DeskScreenTabState = DeskScreenTabMode.InventoryTab;

        }
        else if (DeskScreenTabState == DeskScreenTabMode.InventoryTab) {

            if (InventoryState == InventoryMode.Selecting) {

                DeskScreenTabState = DeskScreenTabMode.StatusTab;
                InitializeStatusTexts();

            }
            else if (InventoryState == InventoryMode.SelectingCombinedItem) {

            }

        }
        else if (DeskScreenTabState == DeskScreenTabMode.StatusTab) {

            DeskScreenTabState = DeskScreenTabMode.ProjectTab;

        }

    }

    // If an item is currently tagged as equipped, place the highlighter box on its slot; otherwise, disable the box
    private void HighLightCurrentlyEquippedItem() {
        if (currentlyEquippedItem != null) {
            for (int i=0; i < inventoryItems.GetLength(0); i++) {
                for (int j = 0; j < inventoryItems.GetLength(1); j++) {
                    if (inventoryItems[i, j] == currentlyEquippedItem) {
                        itemInHandHighlighter.rectTransform.anchoredPosition = itemSlots[i, j].rectTransform.anchoredPosition;
                        itemInHandHighlighter.enabled = true;
                    }
                }
            }
        }
        else {
            itemInHandHighlighter.enabled = false;
        }
    }

    private int[] GetRowAndColumnOfNextItem() {

        int[] rowAndCol = new int[2] { 0, 1};

        bool foundItem = false;
        int index = 0;

        if (numOfItems > 1) {
            for (int i = 0; i < numOfItems; i++) {
                if (tedInventory.items[i] == curItem) {
                    foundItem = true;
                    index = i;
                }
            }
        }
        if (foundItem) {
            index = (index + 1) % numOfItems;

            for (int i = 0; i < inventoryItems.GetLength(0); i++) {
                for (int j = 0; j < inventoryItems.GetLength(1); j++) {
                    if (inventoryItems[i,j] == tedInventory.items[index]) {
                        rowAndCol[0] = i;
                        rowAndCol[1] = j; 
                    }
                }
            }
        }

        return rowAndCol;
    }

    // Called every frame, this controls the flashing of the selection boxes
    private void FlashIcon(RawImage icon, float speedFactor) {
        if (flashingIconIsIncreasingAlpha) {
            if (Time.time - flashingIconRefTime > iconFlashFrameTime) {
                if (icon.color.a < iconAlphaMax) {
                    icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, icon.color.a + (speedFactor * Time.deltaTime));
                }
                else {
                    flashingIconIsIncreasingAlpha = false;
                }

                flashingIconRefTime = Time.time;
            }
        }
        else {
            if (Time.time - flashingIconRefTime > iconFlashFrameTime) {
                if (icon.color.a > iconAlphaMin) {
                    icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, icon.color.a - (speedFactor * Time.deltaTime));
                }
                else {
                    flashingIconIsIncreasingAlpha = true;
                }

                flashingIconRefTime = Time.time;
            }
        }
    }

    public void RotateModel(Transform model, float speed) {
        model.Rotate(0, speed * Time.deltaTime, 0, Space.World);
    }

    public void InitializeStatusTexts() {

        curHostNameString = "";
        curHostHeightString = "";
        curHostHealthString = "";
        curHostEffectsString = "";

        curEyeNameString = "";
        curEyeEffectsString = "";

        hostNameCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        hostHeightCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        hostHealthCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        hostEffectsCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);

        eyeNameCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);
        eyeEffectsCharUnravelTime = Random.Range(charUnravelTimeMin, charUnravelTimeMax);

        hostNameUnravelFrameRefTime = Time.time;
        hostHeightUnravelFrameRefTime = Time.time;
        hostHealthUnravelFrameRefTime = Time.time;
        hostEffectsUnravelFrameRefTime = Time.time;

        eyeNameUnravelFrameRefTime = Time.time;
        eyeEffectsUnravelFrameRefTime = Time.time;

        hostNameCharIndex = 0;
        hostHeightCharIndex = 0;
        hostHealthCharIndex = 0;
        hostEffectsCharIndex = 0;

        eyeNameCharIndex = eyeNameVersionConnection.ToCharArray().Length - 1;
        eyeEffectsCharIndex = eyeExtraEffects.ToCharArray().Length - 1;

        justDisplayedRandomHostNameChar = false;
        justDisplayedRandomHostHeightChar = false;
        justDisplayedRandomHostHealthChar = false;
        justDisplayedRandomHostEffectsChar = false;

        justDisplayedRandomEyeNameChar = false;
        justDisplayedRandomEyeEffectsChar = false;
    }
}
