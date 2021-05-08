using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeskInventory : MonoBehaviour
{
    public RawImage equipButton, itemInHandHighlighter, combineItemButton, itemSelectorBox, itemActionSelectorBox, firstItemCombineBox,
                    currentInvItemInspector, itemInspectorFrame, itemSlotBgd, itemPopupBgd;

    public List<RawImage> itemIcons;
    public Texture emptySlotTexture;
    public Camera inventoryItemCam;
    public Text currentInvItemNameTxt, currentItemInvDescriptionTxt;

    private bool isActive;

    [SerializeField]
    private Inventory tedInventory;

    private enum InventoryMode { Selecting, SelectingCombinedItem, Combining }
    private InventoryMode InventoryState;

    [SerializeField]
    private float modelRotationSpeed;
    [SerializeField, Range(0.0f, 100.0f)]
    private float itemSelectorFrameAlphaChange, itemActionSelectorFrameAlphaChange;
    [SerializeField, Range(0.0f, 1.0f)]
    private float itemSelectorAlphaMin, itemSelectorAlphaMax;
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

    // Start is called before the first frame update
    void Start()
    {
        InventoryState = InventoryMode.Selecting;

        // Populate the inventory icon array
        int index = 0;

        for (int i = 0; i < itemSlots.GetLength(0); i++) {
            for (int j = 0; j < itemSlots.GetLength(1); j++) {
                itemSlots[i, j] = itemIcons[index];
                index++;
            }
        }

        isActive = false;

        numOfItems = tedInventory.items.Count;

        currentItemActionIcon = equipButton;
        curItemRow = 0;
        curItemCol = 0;

        needToUpdateItemToView = true;
    }

    // Update is called once per frame
    void Update()
    {
        numOfItems = tedInventory.items.Count;
        GetTedInventory();

        if (isActive) {

            //numOfItems = tedInventory.items.Count;

            if (numOfItems > 0) {

                HighLightCurrentlyEquippedItem();

                curItem = inventoryItems[curItemRow, curItemCol];
                curItemIcon = itemSlots[curItemRow, curItemCol];

                Debug.Log(curItem);

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
                itemInspectorFrame.enabled = true;

                if (needToUpdateItemToView) {
                    tedInventory.PlaceItemToView(curItem);
                    needToUpdateItemToView = false;
                }

                RotateModel(curItem.itemCopyForInventory, modelRotationSpeed);
            }
            else {

                currentInvItemNameTxt.enabled = true;
                currentItemInvDescriptionTxt.enabled = true;

                currentInvItemNameTxt.text = "";
                currentItemInvDescriptionTxt.text = "";

                inventoryItemCam.enabled = false;
                currentInvItemInspector.enabled = false;
                itemInspectorFrame.enabled = false;
            }

            switch (InventoryState) {
                case InventoryMode.Selecting:

                    itemActionSelectorBox.rectTransform.anchoredPosition = currentItemActionIcon.rectTransform.anchoredPosition;
                    firstItemCombineBox.enabled = false;

                    if (numOfItems > 0) {
                        itemSelectorBox.enabled = true;
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
        }
    }

    private void ShowInventoryWindows(bool show) {
        if (show) {
            equipButton.enabled = true;
            combineItemButton.enabled = true;
            itemActionSelectorBox.enabled = true;
            itemPopupBgd.enabled = true;
            itemSlotBgd.enabled = true;

            foreach (RawImage img in itemIcons) {
                img.enabled = true;
            }
        }
        else {
            equipButton.enabled = false;
            combineItemButton.enabled = false;
            itemInHandHighlighter.enabled = false;
            combineItemButton.enabled = false;
            itemSelectorBox.enabled = false;
            itemActionSelectorBox.enabled = false;
            firstItemCombineBox.enabled = false;
            itemPopupBgd.enabled = false;
            itemSlotBgd.enabled = false;

            foreach (RawImage img in itemIcons) {
                img.enabled = false;
            }

            inventoryItemCam.enabled = false;
            currentInvItemInspector.enabled = false;
            itemInspectorFrame.enabled = false;

            currentInvItemNameTxt.enabled = false;
            currentItemInvDescriptionTxt.enabled = false;
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

    // If an item is currently tagged as equipped, place the highlighter box on its slot; otherwise, disable the box
    private void HighLightCurrentlyEquippedItem() {
        if (currentlyEquippedItem != null) {
            for (int i = 0; i < inventoryItems.GetLength(0); i++) {
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

        int[] rowAndCol = new int[2] { 0, 1 };

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
                    if (inventoryItems[i, j] == tedInventory.items[index]) {
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
                if (icon.color.a < itemSelectorAlphaMax) {
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
                if (icon.color.a > itemSelectorAlphaMin) {
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

    public bool IsActiveAndSelecting() {
        return (isActive && (InventoryState == InventoryMode.Selecting));
    }

    public void MakeActive(bool yesOrNo) {
        if (yesOrNo) {
            isActive = true;
            ShowInventoryWindows(true);
            InventoryState = InventoryMode.Selecting;
        }
        else {
            isActive = false;
            ShowInventoryWindows(false);
        }
    }


    //
    // CONTROLS/INPUT
    //

    public void PressX() {

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
                }
                else if (currentItemActionIcon == combineItemButton) {
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

    public void PressCircle() {

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

    public void PressUpLS() {

        needToUpdateItemToView = true;

        if (InventoryState == InventoryMode.Selecting) {

            bool foundOccupiedSlot = false;

            for (int i = 1; i < inventoryItems.GetLength(0); i++) {

                int nextIndex = (curItemRow - i + inventoryItems.GetLength(0)) % inventoryItems.GetLength(0);

                if (!foundOccupiedSlot && inventoryItems[nextIndex, curItemCol] != null) {
                    curItemRow = nextIndex;
                    foundOccupiedSlot = true;
                }
            }

        }
        else if (InventoryState == InventoryMode.SelectingCombinedItem) {
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
    }

    public void PressDownLS() {

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

    public void PressLeftLS() {

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

    public void PressRightLS() {

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

    public void PressUpRS() {

        if (InventoryState == InventoryMode.Selecting) {

        }
        else if (InventoryState == InventoryMode.SelectingCombinedItem) {

        }

    }

    public void PressDownRS() {

        if (InventoryState == InventoryMode.Selecting) {

        }
        else if (InventoryState == InventoryMode.SelectingCombinedItem) {

        }

    }

    public void PressLeftRS() {

        if (InventoryState == InventoryMode.Selecting) {

            if (currentItemActionIcon == equipButton) { currentItemActionIcon = combineItemButton; }
            else if (currentItemActionIcon == combineItemButton) { currentItemActionIcon = equipButton; }

        }
        else if (InventoryState == InventoryMode.SelectingCombinedItem) {

        }

    }

    public void PressRightRS() {

        if (InventoryState == InventoryMode.Selecting) {

            if (currentItemActionIcon == equipButton) { currentItemActionIcon = combineItemButton; }
            else if (currentItemActionIcon == combineItemButton) { currentItemActionIcon = equipButton; }

        }
        else if (InventoryState == InventoryMode.SelectingCombinedItem) {

        }

    }
}
