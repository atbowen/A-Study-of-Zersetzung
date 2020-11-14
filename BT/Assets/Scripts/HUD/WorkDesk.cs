using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkDesk : MonoBehaviour
{
    public bool openDesk, closeDesk, deskEnabled;
    public RawImage[] deskScreens;
    public RawImage itemViewScreen, itemLeftArrow, itemRightArrow;
    public Text itemDescription;
    public List<Transform> viewerInventory, viewerPotentialInventory;
    public Transform itemPosePosition;
    public Transform viewerInventoryHolder;

    public string openDeskSound, closeDeskSound, changeItemSound, cantChangeItemSound, useItemSound, switchTabSound;

    private Teddy ted;
    private MailScreen mailManager;
    private NewsTicker tickerFeed;
    private ToolsScreen toolScreen;
    private SystemScreen sysScreen;
    private MusicPlayer musicBox;

    private enum DeskMode { Desk, Tools, Mail, System};
    private DeskMode deskState;
    private const int numOfDeskModes = 4;

    private int currentItemIndex;
    private float itemSelectionTimer;
    private const float itemSelectionDelay = 0.3f;
    

    // Start is called before the first frame update
    void Start()
    {
        ted = FindObjectOfType<Teddy>();
        mailManager = FindObjectOfType<MailScreen>();
        tickerFeed = FindObjectOfType<NewsTicker>();
        toolScreen = FindObjectOfType<ToolsScreen>();
        sysScreen = FindObjectOfType<SystemScreen>();
        musicBox = FindObjectOfType<MusicPlayer>();

        HideAllSpringScreens();
        mailManager.showEMail = false;

        openDesk = false;
        closeDesk = false;
        deskEnabled = false;
        deskState = DeskMode.Desk;

        currentItemIndex = 0;
        itemSelectionTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //if (openDesk) {
        //    musicBox.PlaySFX(openDeskSound);
        //    openDesk = false;
        //}

        //if (closeDesk) {
        //    musicBox.PlaySFX(closeDeskSound);
        //    closeDesk = false;
        //}

        if (deskEnabled) {
            tickerFeed.showTicker = true;

            if ((Time.time - itemSelectionTimer) > (itemSelectionDelay * 0.2)) {
                itemLeftArrow.rectTransform.localScale = new Vector2(1, 1);
                itemRightArrow.rectTransform.localScale = new Vector2(1, 1);
            }

            if (Input.GetButtonDown("Right Bumper")) {                                                                              //switch tabs...
                HideAllSpringScreens();                                                                                             //first hide all of the Spring screens 
                deskState = (DeskMode)((((int)deskState) + 1) % numOfDeskModes);                                                    //change the Desk mode
                musicBox.PlaySPRINGDeskChangeTabSound();

                PlaceItemToView(currentItemIndex);
                itemDescription.text = "Item:\n<color=#ffffff9b>" + viewerInventory[currentItemIndex].name + "</color>";
            }

            if (Input.GetButtonDown("Left Bumper")) {
                HideAllSpringScreens();
                deskState = (DeskMode)((((int)deskState) - 1 + numOfDeskModes) % numOfDeskModes);
                musicBox.PlaySPRINGDeskChangeTabSound();

                PlaceItemToView(currentItemIndex);
                itemDescription.text = "Item:\n<color=#ffffff9b>" + viewerInventory[currentItemIndex].name + "</color>";
            }

            switch (deskState) {
                case DeskMode.Desk:
                    deskScreens[0].enabled = true;
                    ShowItemUI(true);
                    mailManager.showEMail = false;
                    toolScreen.showTools = false;
                    toolScreen.enableToolControls = false;
                    sysScreen.showOptions = false;

                    RotateInventoryItem(75);

                    if ((((Time.time - itemSelectionTimer) > itemSelectionDelay) && Mathf.Abs(Input.GetAxis("D-Pad Left Right")) > 0.1) || Input.GetButtonDown("Reticle")
                        || Input.GetButtonDown("Hallucinations")) {         //switch item

                        if (Input.GetAxis("D-Pad Left Right") > 0.01 || Input.GetButtonDown("Hallucinations")) {
                            CycleItemsRight();
                        } else if (Input.GetAxis("D-Pad Left Right") < -0.01 || Input.GetButtonDown("Reticle")) {
                            CycleItemsLeft();
                        }
                    }

                    break;
                case DeskMode.Tools:
                    deskScreens[1].enabled = true;
                    ShowItemUI(true);
                    mailManager.showEMail = false;
                    toolScreen.showTools = true;
                    toolScreen.enableToolControls = true;
                    sysScreen.showOptions = false;

                    RotateInventoryItem(75);

                    if ((((Time.time - itemSelectionTimer) > itemSelectionDelay) && Mathf.Abs(Input.GetAxis("D-Pad Left Right")) > 0.01) || Input.GetButtonDown("Reticle")
                        || Input.GetButtonDown("Hallucinations")) {         //switch item
                        
                        if (Input.GetAxis("D-Pad Left Right") > 0.01 || Input.GetButtonDown("Hallucinations")) {
                            CycleItemsRight();
                        } else if (Input.GetAxis("D-Pad Left Right") < -0.01 || Input.GetButtonDown("Reticle")) {
                            CycleItemsLeft();
                        }
                    }

                    break;
                case DeskMode.Mail:
                    deskScreens[2].enabled = true;
                    ShowItemUI(false);
                    mailManager.showEMail = true;
                    toolScreen.showTools = false;
                    toolScreen.enableToolControls = false;
                    sysScreen.showOptions = false;

                    if (mailManager.mailListActive) {
                        
                        if ((((Time.time - itemSelectionTimer) > itemSelectionDelay) && Input.GetAxis("D-Pad Up Down") < 0) || Input.GetAxis("Mouse ScrollWheel") < 0) {
                            mailManager.EmailListCycleDown();
                            itemSelectionTimer = Time.time;
                        } else if ((((Time.time - itemSelectionTimer) > itemSelectionDelay) && Input.GetAxis("D-Pad Up Down") > 0) || Input.GetAxis("Mouse ScrollWheel") > 0) {
                            mailManager.EmailListCycleUp();
                            itemSelectionTimer = Time.time;
                        }                        
                    }

                    break;
                case DeskMode.System:
                    deskScreens[3].enabled = true;
                    ShowItemUI(false);
                    mailManager.showEMail = false;
                    toolScreen.showTools = false;
                    toolScreen.enableToolControls = false;
                    sysScreen.showOptions = true;

                    if ((((Time.time - itemSelectionTimer) > itemSelectionDelay) && Input.GetAxis("D-Pad Up Down") < 0) || Input.GetAxis("Mouse ScrollWheel") < 0) {
                        sysScreen.optionsListCycleDown();
                        itemSelectionTimer = Time.time;
                    } else if ((((Time.time - itemSelectionTimer) > itemSelectionDelay) && Input.GetAxis("D-Pad Up Down") > 0) || Input.GetAxis("Mouse ScrollWheel") > 0) {
                        sysScreen.optionsListCycleUp();
                        itemSelectionTimer = Time.time;
                    }
                    break;
            }
        } else {
            HideAllSpringScreens();
            PlaceItemToView(currentItemIndex);
            itemDescription.text = "Item:\n<color=#ffffff9b>" + viewerInventory[currentItemIndex].name + "</color>";            
        }
    }

    void HideAllSpringScreens() {
        for (int i = 0; i < deskScreens.Length; i++) {
            deskScreens[i].enabled = false;
        }

        ShowItemUI(false);
        mailManager.showEMail = false;
        tickerFeed.showTicker = false;
        toolScreen.showTools = false;
        toolScreen.enableToolControls = false;
        sysScreen.showOptions = false;
    }

    private void ShowItemUI(bool showOrNo) {
        itemViewScreen.enabled = showOrNo;
        itemLeftArrow.enabled = showOrNo;
        itemRightArrow.enabled = showOrNo;
        itemDescription.enabled = showOrNo;
    }

    private void PlaceItemToView(int inventoryIndex) {
        viewerInventory[inventoryIndex].parent = null;
        viewerInventory[inventoryIndex].parent = itemPosePosition;
        viewerInventory[inventoryIndex].localPosition = Vector3.zero;
        viewerInventory[inventoryIndex].localRotation = Quaternion.identity;
    }

    private void RotateInventoryItem(float speed) {
        viewerInventory[currentItemIndex].Rotate(0, speed * Time.deltaTime, 0);
    }

    private void ReturnInventoryItem(int inventoryIndex) {
        viewerInventory[inventoryIndex].parent = null;
        viewerInventory[inventoryIndex].parent = viewerInventoryHolder;
        viewerInventory[inventoryIndex].localPosition = Vector3.zero;
    }

    public void AddItemToInventory(Transform newItem) {
        if (newItem.GetComponent<MeshRenderer>()) { newItem.GetComponent<MeshRenderer>().enabled = false; }
        if (newItem.GetComponent<SkinnedMeshRenderer>()) { newItem.GetComponent<SkinnedMeshRenderer>().enabled = false; }
        if (newItem.GetComponent<Rigidbody>()) { newItem.GetComponent<Rigidbody>().isKinematic = true; }
        if (newItem.GetComponent<Collider>()) { newItem.GetComponent<Collider>().enabled = false; }

        newItem.parent = ted.transform;
        newItem.localPosition = Vector3.zero;
        newItem.localRotation = Quaternion.identity;

        AddItemToViewer(newItem);
    }

    public void AddItemToViewer(Transform newItem) {
        if (IsItemInPotentialInventory(newItem.name)) {
            if (!IsItemAlreadyInInventory(newItem.name)) {
                for (int i = 0; i < viewerPotentialInventory.Count; i++) {
                    if (viewerPotentialInventory[i].name == newItem.name) {
                        viewerInventory.Add(viewerPotentialInventory[i]);
                    }
                }
            } else {
                

            }
        }
    }

    private void RemoveViewerItem(string itemName) {
        for (int i = 0;i<viewerInventory.Count; i++) {
            if (viewerInventory[i].name == itemName) {
                viewerInventory.Remove(viewerInventory[i]);
            }
        }
    }

    private bool IsItemAlreadyInInventory(string itemName) {
        for (int i = 0; i < viewerInventory.Count; i++) {
            if (viewerInventory[i].name == itemName) { return true; }
        }

        return false;
    }

    private bool IsItemInPotentialInventory(string itemName) {
        for (int i = 0; i < viewerPotentialInventory.Count; i++) {
            if (viewerPotentialInventory[i].name == itemName) { return true; }
        }

        return false;
    }

    private void CycleItemsRight() {
        itemRightArrow.rectTransform.localScale = new Vector2(0.5f, 0.5f);

        itemSelectionTimer = Time.time;

        if (viewerInventory.Count > 1) {
            ReturnInventoryItem(currentItemIndex);
            currentItemIndex = (currentItemIndex + viewerInventory.Count + 1) % viewerInventory.Count;
            PlaceItemToView(currentItemIndex);
            itemDescription.text = "Item:\n<color=#ffffff9b>" + viewerInventory[currentItemIndex].name + "</color>";
            musicBox.PlaySPRINGDeskChangeItemSound();
        } else {
            musicBox.PlaySPRINGDeskNoItemToChangeSound();
        }
    }

    private void CycleItemsLeft() {
        itemLeftArrow.rectTransform.localScale = new Vector2(0.5f, 0.5f);

        itemSelectionTimer = Time.time;

        if (viewerInventory.Count > 1) {
            ReturnInventoryItem(currentItemIndex);
            currentItemIndex = (currentItemIndex + viewerInventory.Count - 1) % viewerInventory.Count;
            PlaceItemToView(currentItemIndex);
            itemDescription.text = "Item:\n<color=#ffffff9b>" + viewerInventory[currentItemIndex].name + "</color>";
            musicBox.PlaySPRINGDeskChangeItemSound();
        } else {
            musicBox.PlaySPRINGDeskNoItemToChangeSound();
        }
    }

    public void CallDeskOpenSound() {
        musicBox.PlaySPRINGDeskOpenSound();
    }

    public void CallDeskCloseSound() {
        musicBox.PlaySPRINGDeskCloseSound();
    }
}
