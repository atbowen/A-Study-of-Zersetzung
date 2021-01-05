using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkDesk : MonoBehaviour
{
    public bool deskEnabled;
    public RawImage[] workDeskScreens;

    public string openDeskSound, closeDeskSound, changeItemSound, cantChangeItemSound, useItemSound, switchTabSound;

    private Teddy ted;
    private MailScreen mailManager;
    private NewsTicker tickerFeed;
    private DeskScreen deskScreen;
    private ToolsScreen toolScreen;
    private SystemScreen sysScreen;
    private MusicPlayer musicBox;

    private enum DeskMode { Desk, Tools, Mail, System};
    private DeskMode deskState;
    private const int numOfDeskModes = 4;

    private int currentItemIndex;
    private float itemSelectionTimer;
    private const float itemSelectionDelay = 0.3f;

    private bool joystickCentered;

    // Start is called before the first frame update
    void Start()
    {
        ted = FindObjectOfType<Teddy>();
        mailManager = FindObjectOfType<MailScreen>();
        tickerFeed = FindObjectOfType<NewsTicker>();
        deskScreen = FindObjectOfType<DeskScreen>();
        toolScreen = FindObjectOfType<ToolsScreen>();
        sysScreen = FindObjectOfType<SystemScreen>();
        musicBox = FindObjectOfType<MusicPlayer>();

        HideAllSpringScreens();
        mailManager.showEMail = false;
        
        deskEnabled = false;
        deskState = DeskMode.Desk;

        currentItemIndex = 0;
        itemSelectionTimer = 0;

        joystickCentered = true;
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

            if (Input.GetButtonDown("Right Bumper")) {                                                                              //switch tabs...
                HideAllSpringScreens();                                                                                             //first hide all of the Spring screens 
                deskState = (DeskMode)((((int)deskState) + 1) % numOfDeskModes);                                                    //change the Desk mode
                // If switched to the Desk tab, the text effects on the Status screen will restart, in case the Desk tab is already set to the Status screen
                // Without this, if the Desk tab state is left on the Status Screen and the player switches back to it, the text will remain as before
                if (deskState == DeskMode.Desk) { deskScreen.InitializeStatusTexts(); }
                musicBox.PlaySPRINGDeskChangeTabSound();
            }

            if (Input.GetButtonDown("Left Bumper")) {
                HideAllSpringScreens();
                deskState = (DeskMode)((((int)deskState) - 1 + numOfDeskModes) % numOfDeskModes);
                // If switched to the Desk tab, the text effects on the Status screen will restart, in case the Desk tab is already set to the Status screen
                // Without this, if the Desk tab state is left on the Status Screen and the player switches back to it, the text will remain as before
                if (deskState == DeskMode.Desk) { deskScreen.InitializeStatusTexts(); }
                musicBox.PlaySPRINGDeskChangeTabSound();
            }

            switch (deskState) {
                case DeskMode.Desk:
                    workDeskScreens[0].enabled = true;
                    deskScreen.ActivateDeskScreen();
                    mailManager.showEMail = false;
                    toolScreen.showTools = false;
                    sysScreen.showOptions = false;

                    if ((Time.time - itemSelectionTimer) > itemSelectionDelay) {
                        if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                            deskScreen.PressRightLS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                            deskScreen.PressLeftLS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                            deskScreen.PressUpLS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                            deskScreen.PressDownLS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Right Joystick Horizontal") > 0.1) {
                            deskScreen.PressRightRS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Right Joystick Horizontal") < -0.1) {
                            deskScreen.PressLeftRS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Right Joystick Vertical") > 0.1) {
                            deskScreen.PressUpRS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Right Joystick Vertical") < -0.1) {
                            deskScreen.PressDownRS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("D-Pad Left Right") > 0.01) {
                            deskScreen.PressRightArrow();
                            itemSelectionTimer = Time.time;
                        }
                        else if (Input.GetAxis("D-Pad Left Right") < -0.01) {
                            deskScreen.PressLeftArrow();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetButtonDown("X Button")) {
                            deskScreen.PressX();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetButtonDown("Circle Button")) {
                            deskScreen.PressCircle();
                            itemSelectionTimer = Time.time;
                        }
                    }

                    break;
                case DeskMode.Tools:
                    workDeskScreens[1].enabled = true;
                    deskScreen.DeactivateDeskScreen();
                    mailManager.showEMail = false;
                    toolScreen.showTools = true;
                    sysScreen.showOptions = false;

                    if ((Time.time - itemSelectionTimer) > itemSelectionDelay) {
                        if (Input.GetAxis("Left Joystick Horizontal") > 0.1) {
                            toolScreen.PressRightLS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Horizontal") < -0.1) {
                            toolScreen.PressLeftLS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") > 0.1) {
                            toolScreen.PressUpLS();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Left Joystick Vertical") < -0.1) {
                            toolScreen.PressDownLS();
                            itemSelectionTimer = Time.time;
                        }
                    }

                    if (((Time.time - itemSelectionTimer) > itemSelectionDelay) || toolScreen.CanHoldRightStick()) {
                        
                        if (Input.GetAxis("Right Joystick Horizontal") > 0.1) {
                            toolScreen.PressRightRS(Input.GetAxis("Right Joystick Horizontal"));
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Right Joystick Horizontal") < -0.1) {
                            toolScreen.PressLeftRS(Input.GetAxis("Right Joystick Horizontal"));
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Right Joystick Vertical") > 0.1) {
                            toolScreen.PressUpRS(Input.GetAxis("Right Joystick Vertical"));
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Right Joystick Vertical") < -0.1) {
                            toolScreen.PressDownRS(Input.GetAxis("Right Joystick Vertical"));
                            itemSelectionTimer = Time.time;
                        }
                    }

                    if ((Time.time - itemSelectionTimer) > itemSelectionDelay) {
                        if (Input.GetAxis("D-Pad Up Down") > 0.01 || Input.GetButtonDown("Hallucinations")) {
                            toolScreen.PressUpArrow();
                            itemSelectionTimer = Time.time;
                        }
                        else if (Input.GetAxis("D-Pad Up Down") < -0.01 || Input.GetButtonDown("Reticle")) {
                            toolScreen.PressDownArrow();
                            itemSelectionTimer = Time.time;
                        }
                    }

                    if (((Time.time - itemSelectionTimer) > itemSelectionDelay) || toolScreen.CanHoldTriggers()) {
                        if (Input.GetAxis("Triggers") > 0.1) {
                            toolScreen.PressRightTrigger(Input.GetAxis("Triggers"));
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetAxis("Triggers") < -0.1) {
                            toolScreen.PressLeftTrigger(Input.GetAxis("Triggers"));
                            itemSelectionTimer = Time.time;
                        }
                    }

                    if (Input.GetAxis("Triggers") > -0.05 && Input.GetAxis("Triggers") < 0.05) {
                        toolScreen.ReleaseRightTrigger();
                        toolScreen.ReleaseLeftTrigger();
                    }

                    if (Time.time - itemSelectionTimer > itemSelectionDelay) {
                        if (Input.GetButtonDown("X Button")) {
                            toolScreen.PressX();
                            itemSelectionTimer = Time.time;
                        }
                        if (Input.GetButtonDown("Circle Button")) {
                            toolScreen.PressCircle();
                            itemSelectionTimer = Time.time;
                        }
                    }

                    break;
                case DeskMode.Mail:
                    workDeskScreens[2].enabled = true;
                    deskScreen.DeactivateDeskScreen();
                    mailManager.showEMail = true;
                    toolScreen.showTools = false;
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
                    workDeskScreens[3].enabled = true;
                    deskScreen.DeactivateDeskScreen();
                    mailManager.showEMail = false;
                    toolScreen.showTools = false;
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
        }

        // Set joystickCentered equal to true if the left joystick is centered
        // This will prevent rapid selection
        if (Mathf.Abs(Input.GetAxis("Left Joystick Horizontal")) < 0.1 && Mathf.Abs(Input.GetAxis("Left Joystick Vertical")) < 0.1) { joystickCentered = true; }
    }

    void HideAllSpringScreens() {
        for (int i = 0; i < workDeskScreens.Length; i++) {
            workDeskScreens[i].enabled = false;
        }
        deskScreen.DeactivateDeskScreen();
        mailManager.showEMail = false;
        tickerFeed.showTicker = false;
        toolScreen.showTools = false;
        sysScreen.showOptions = false;
    }

    public void CallDeskOpenSound() {
        musicBox.PlaySPRINGDeskOpenSound();
    }

    public void CallDeskCloseSound() {
        musicBox.PlaySPRINGDeskCloseSound();
    }
}
