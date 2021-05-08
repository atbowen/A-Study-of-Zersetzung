using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DatingScreen : MonoBehaviour
{
    public PanelsBottom commsPanel;
    
    [SerializeField]
    private Camera datingSourceCamera;
    [SerializeField]
    private RenderTexture chatVideoTexture;

    public List<DatingProfile> profiles;
    public DatingProfile emptyProfile;

    private DatingProfile tedsProfile;

    private bool isActivated = false;

    [SerializeField]
    private Text basicInfoTxt, profileTxt, captionTxt, 
                    chatPendingText, chatRequestTxt;
    [SerializeField]
    private string noProfilesText = "0 Profiles Found", incomingChatRequestText = "Will  you  chat  with  ";

    [SerializeField]
    private RawImage dateScreenFrame, profilePage, profileWindowHighlighter, startChatButton, startChatButtonHighlighter, statusIndicator, profilePicImage, 
                        chatPendingWindow, acceptChatButton, rejectChatButton;
    [SerializeField]
    private Texture onlineImage, offlineImage, profileBgd, noProfilePicsTexture, chatButtonImageInactive, chatButtonImageActive;
    [SerializeField]
    private float profileBgdAlphaWhenBrowsing, profileBgdAlphaWhenInspecting, startChatButtonAvailableAlpha, startChatButtonNotAvailableAlpha;
    private RawImage curProfileInspectionImage, curChatButton;

    
    private DatingProfile curProfile;
    private int curProfileIndex, numOfPicsInCurProfile, curProfilePicIndex;
    
    [SerializeField]
    private List<string> chatPendingTextFrames;
    [SerializeField]
    private float defaultChatPendingWaitTime, chatPendingTextFrameTime;
    private float chatPendingWaitTime, chatPendingWaitTimeRef, chatPendingTextRefTime;
    private int curChatPendingTextFrameIndex;

    private enum SessionMode { Inactive, Browsing, Inspecting, BeingContacted, Contacting, Chatting }
    private SessionMode SessionState, PrevSessionState;
    private enum InspectionMode { LookingAtMainPage, LookingAtLikesAndDislikes, LookingAtPictures }
    private InspectionMode InspectionState;

    private CommsController commsControl;

    // Start is called before the first frame update
    void Start()
    {
        commsPanel = commsPanel.GetComponent<PanelsBottom>();
        commsControl = FindObjectOfType<CommsController>();
        tedsProfile = FindObjectOfType<Teddy>().transform.GetComponent<IDCharacter>().stardaterProfile;

        basicInfoTxt.supportRichText = true;
        profileTxt.supportRichText = true;

        EnableScreen(false);

        curProfileIndex = 0;

        isActivated = false;

        curProfileInspectionImage = profilePage;
        curChatButton = acceptChatButton;
        curChatPendingTextFrameIndex = 0;

        SessionState = SessionMode.Inactive;
        PrevSessionState = SessionMode.Inactive;
        InspectionState = InspectionMode.LookingAtMainPage;
    }

    // Update is called once per frame
    void Update()
    {
        if (profiles.Count == 0) {
            curProfile = emptyProfile;
        }
        else {
            curProfile = profiles[curProfileIndex];
        }

        if (curProfile != null) {
            if (curProfile.StatusIsOnline())    { statusIndicator.texture = onlineImage; }
            else                                { statusIndicator.texture = offlineImage; }

            numOfPicsInCurProfile = curProfile.profilePics.Count;
        }

        switch (SessionState) {
            case SessionMode.Inactive:

                datingSourceCamera.enabled = false;

                EnableScreen(false);

                curChatButton = acceptChatButton;
                curProfileInspectionImage = dateScreenFrame;

                curProfileIndex = 0;
                curProfilePicIndex = 0;

                break;
            case SessionMode.Browsing:

                datingSourceCamera.enabled = false;
                profilePicImage.enabled = false;
                profilePage.enabled = true;

                chatPendingWindow.enabled = false;
                startChatButton.enabled = false;
                startChatButtonHighlighter.enabled = false;

                profileTxt.enabled = false;
                basicInfoTxt.enabled = true;
                captionTxt.enabled = false;
                chatPendingText.enabled = false;
                chatRequestTxt.enabled = false;

                profilePage.color = new Color(profilePage.color.r, profilePage.color.g, profilePage.color.b, profileBgdAlphaWhenBrowsing);

                if (profiles.Count > 0 && curProfile != null) {
                    statusIndicator.enabled = true;
                    basicInfoTxt.text = curProfile.GetBasicInfo();

                    if (curProfile.customBgd != null)   { profilePage.texture = curProfile.customBgd; }
                    else                                { profilePage.texture = profileBgd; }
                }
                else {
                    statusIndicator.enabled = false;
                    basicInfoTxt.text = noProfilesText;
                }

                break;
            case SessionMode.Inspecting:

                datingSourceCamera.enabled = false;
                profilePage.enabled = true;
                chatPendingWindow.enabled = false;

                basicInfoTxt.enabled = false;
                chatPendingText.enabled = false;
                chatRequestTxt.enabled = false;

                profilePage.color = new Color(profilePage.color.r, profilePage.color.g, profilePage.color.b, profileBgdAlphaWhenInspecting);

                if (curProfile != null) {

                    startChatButton.enabled = true;
                    startChatButton.texture = chatButtonImageInactive;
                    statusIndicator.enabled = true;

                    if (curProfileInspectionImage == profilePage) {
                        profileWindowHighlighter.enabled = true;
                        startChatButtonHighlighter.enabled = false;
                    }
                    else if (curProfileInspectionImage == startChatButton) {
                        profileWindowHighlighter.enabled = false;
                        startChatButtonHighlighter.enabled = true;
                    }  

                    if (curProfile.StatusIsOnline()) {
                        statusIndicator.texture = onlineImage;
                        startChatButton.color = new Color(startChatButton.color.r, startChatButton.color.g, startChatButton.color.b, startChatButtonAvailableAlpha);
                    }
                    else {
                        statusIndicator.texture = offlineImage;
                        startChatButton.color = new Color(startChatButton.color.r, startChatButton.color.g, startChatButton.color.b, startChatButtonNotAvailableAlpha);
                    }

                    if (curProfile.customBgd != null)   { profilePage.texture = curProfile.customBgd; }
                    else                                { profilePage.texture = profileBgd; }
                }

                switch (InspectionState) {
                    case InspectionMode.LookingAtMainPage:

                        profilePicImage.enabled = false;

                        captionTxt.enabled = false;

                        if (curProfile != null) {
                            profileTxt.text = curProfile.GetCharacteristics();
                            profileTxt.enabled = true;
                        }

                        break;
                    case InspectionMode.LookingAtLikesAndDislikes:

                        profilePicImage.enabled = false;

                        captionTxt.enabled = false;

                        if (curProfile != null) {
                            profileTxt.text = curProfile.GetLikesAndDislikes();
                            profileTxt.enabled = true;
                        }

                        break;
                    case InspectionMode.LookingAtPictures:

                        profilePicImage.enabled = true;
                        profileTxt.enabled = false;

                        if (curProfile != null) {
                            if (curProfile.profilePics.Count > 0) {

                                profilePicImage.rectTransform.sizeDelta = new Vector2(curProfile.profilePics[curProfilePicIndex].width, curProfile.profilePics[curProfilePicIndex].height);
                                profilePicImage.texture = curProfile.profilePics[curProfilePicIndex];

                                captionTxt.enabled = true;
                                captionTxt.text = curProfile.profilePics[curProfilePicIndex].name;
                            }
                        } else {
                            profilePicImage.texture = noProfilePicsTexture;
                            captionTxt.enabled = false;
                        }

                        break;
                }

                break;
            case SessionMode.BeingContacted:

                datingSourceCamera.enabled = false;

                chatPendingWindow.enabled = true;
                acceptChatButton.enabled = true;
                rejectChatButton.enabled = true;

                chatRequestTxt.enabled = true;

                break;
            case SessionMode.Contacting:

                datingSourceCamera.enabled = false;

                if (Time.time - chatPendingWaitTimeRef > chatPendingWaitTime) {

                    commsControl.StartDatingChat(curProfile.member);
                    SessionState = SessionMode.Chatting;

                }
                else {

                    chatPendingWindow.enabled = true;
                    chatPendingText.enabled = true;

                    chatPendingText.text = chatPendingTextFrames[curChatPendingTextFrameIndex];

                    if (Time.time - chatPendingTextRefTime > chatPendingTextFrameTime) {
                        curChatPendingTextFrameIndex = (curChatPendingTextFrameIndex + chatPendingTextFrames.Count + 1) % chatPendingTextFrames.Count;
                        chatPendingTextRefTime = Time.time;
                    }

                }      

                break;
            case SessionMode.Chatting:

                datingSourceCamera.enabled = true;

                profilePage.enabled = true;
                profilePage.texture = chatVideoTexture;

                profilePage.color = new Color(profilePage.color.r, profilePage.color.g, profilePage.color.b, 1);

                profilePicImage.enabled = false;

                profileTxt.enabled = false;
                captionTxt.enabled = false;

                startChatButton.enabled = true;
                startChatButton.texture = chatButtonImageActive;

                chatPendingWindow.enabled = false;
                chatPendingText.enabled = false;

                startChatButtonHighlighter.enabled = false;

                break;
        }
    }

    private void HideStardaterScreens() {
        dateScreenFrame.enabled = false;
        datingSourceCamera.enabled = false;

        profilePage.enabled = false;
        profilePicImage.enabled = false;
        startChatButton.enabled = false;
        acceptChatButton.enabled = false;
        rejectChatButton.enabled = false;
        chatPendingWindow.enabled = false;

        startChatButtonHighlighter.enabled = false;
        profileWindowHighlighter.enabled = false;

        basicInfoTxt.enabled = false;
        profileTxt.enabled = false;
        captionTxt.enabled = false;
        chatPendingText.enabled = false;
        chatRequestTxt.enabled = false;
    }

    public void PressX() {
        if (SessionState == SessionMode.Browsing) {
            SessionState = SessionMode.Inspecting;
            InspectionState = InspectionMode.LookingAtMainPage;
            curProfileInspectionImage = profilePage;

            curProfilePicIndex = 0;
        } else if (SessionState == SessionMode.BeingContacted) {

            if (curChatButton == acceptChatButton) {



            } else if (curChatButton == rejectChatButton) {

            }

        } else if (SessionState == SessionMode.Inspecting) {
            if (curProfileInspectionImage == startChatButton && curProfile.StatusIsOnline()) {

                if (curProfile != null) {
                    if (curProfile.member != null) {
                        PlaceStardaterCamInFrontOfMember(curProfile.member);

                        float tempPendingTime = curProfile.GetWaitTimeForPendingChat();

                        if (tempPendingTime == 0f)  { chatPendingWaitTime = defaultChatPendingWaitTime; }
                        else                        { chatPendingWaitTime = tempPendingTime; }

                        chatPendingTextRefTime = Time.time;
                        chatPendingWaitTimeRef = Time.time;
                        curChatPendingTextFrameIndex = 0;
                    }
                }

                SessionState = SessionMode.Contacting;

                curProfileInspectionImage = profilePage;
            }
        }
    }

    public void PressCircle() {
        if (SessionState == SessionMode.Browsing) {

        } else if (SessionState == SessionMode.Inspecting) {
            SessionState = SessionMode.Browsing;
            InspectionState = InspectionMode.LookingAtMainPage;
        } else if (SessionState == SessionMode.BeingContacted) {
            SessionState = PrevSessionState;
        } else if (SessionState == SessionMode.Chatting) {

            curChatPendingTextFrameIndex = 0;

            SessionState = SessionMode.Inspecting;
        }
    }

    public void PressLeftLS() {
        if (SessionState == SessionMode.Browsing) {

            curProfileIndex = (curProfileIndex - 1 + profiles.Count) % profiles.Count;

        } else if (SessionState == SessionMode.Inspecting) {

            if (curProfileInspectionImage == profilePage)   { if (curProfile.StatusIsOnline()) { curProfileInspectionImage = startChatButton; } }
            else                                            { curProfileInspectionImage = profilePage; }

            if (InspectionState == InspectionMode.LookingAtMainPage) {

            } else if (InspectionState == InspectionMode.LookingAtLikesAndDislikes) {
                
            } else if (InspectionState == InspectionMode.LookingAtPictures) {

            }

        }
        else if (SessionState == SessionMode.BeingContacted) {
            if (curChatButton == acceptChatButton) { curChatButton = rejectChatButton; }
            if (curChatButton == rejectChatButton) { curChatButton = acceptChatButton; }
        }
    }

    public void PressRightLS() {
        if (SessionState == SessionMode.Browsing) {

            curProfileIndex = (curProfileIndex + 1 + profiles.Count) % profiles.Count;

        }
        else if (SessionState == SessionMode.Inspecting) {

            if (curProfileInspectionImage == profilePage)   { if (curProfile.StatusIsOnline()) { curProfileInspectionImage = startChatButton; } }
            else                                            { curProfileInspectionImage = profilePage; }

            if (InspectionState == InspectionMode.LookingAtMainPage) {

            }
            else if (InspectionState == InspectionMode.LookingAtLikesAndDislikes) {

            }
            else if (InspectionState == InspectionMode.LookingAtPictures) {

            }

        }
        else if (SessionState == SessionMode.BeingContacted) {
            if (curChatButton == acceptChatButton) { curChatButton = rejectChatButton; }
            if (curChatButton == rejectChatButton) { curChatButton = acceptChatButton; }
        }
    }

    public void PressUpLS() {

        if (SessionState == SessionMode.Inspecting) {

            if (InspectionState == InspectionMode.LookingAtMainPage) {
                curProfilePicIndex = numOfPicsInCurProfile - 1;
                InspectionState = InspectionMode.LookingAtPictures;
            }
            else if (InspectionState == InspectionMode.LookingAtLikesAndDislikes) {
                InspectionState = InspectionMode.LookingAtMainPage;
            }
            else if (InspectionState == InspectionMode.LookingAtPictures) {
                if (numOfPicsInCurProfile > 1) {
                    if (curProfilePicIndex > 0) {
                        curProfilePicIndex--;
                    }
                    else {
                        InspectionState = InspectionMode.LookingAtLikesAndDislikes;
                    }
                }
            }

        }

    }

    public void PressDownLS() {

        if (SessionState == SessionMode.Inspecting) {

            if (InspectionState == InspectionMode.LookingAtMainPage) {
                InspectionState = InspectionMode.LookingAtLikesAndDislikes;
            }
            else if (InspectionState == InspectionMode.LookingAtLikesAndDislikes) {
                curProfilePicIndex = 0;
                InspectionState = InspectionMode.LookingAtPictures;
            }
            else if (InspectionState == InspectionMode.LookingAtPictures) {
                if (numOfPicsInCurProfile > 1) {
                    if (curProfilePicIndex < (numOfPicsInCurProfile - 1)) {
                        curProfilePicIndex++;
                    }
                    else {
                        InspectionState = InspectionMode.LookingAtMainPage;
                    }
                }
            }

        }

    }

    public void EnableScreen(bool enableYorN) {
        
        if (enableYorN) {
            SessionState = SessionMode.Browsing;

            dateScreenFrame.enabled = true;
            isActivated = true;
        }
        else {
            SessionState = SessionMode.Inactive;

            dateScreenFrame.enabled = false;
            datingSourceCamera.enabled = false;

            profilePage.enabled = false;
            profilePicImage.enabled = false;
            startChatButton.enabled = false;
            chatPendingWindow.enabled = false;
            acceptChatButton.enabled = false;
            rejectChatButton.enabled = false;
            statusIndicator.enabled = false;

            startChatButtonHighlighter.enabled = false;
            profileWindowHighlighter.enabled = false;

            basicInfoTxt.enabled = false;
            profileTxt.enabled = false;
            captionTxt.enabled = false;
            chatPendingText.enabled = false;
            chatRequestTxt.enabled = false;

            isActivated = false;
        }
    }

    public void AddStardaterProfile(DatingProfile profile) {
        if (!profiles.Contains(profile) && profile.member != null) { profiles.Add(profile); }
    }

    public void AddStardaterProfile(IDCharacter charID) {
        if (charID.stardaterProfile != null) {
            charID.stardaterProfile.member = charID;
            if (!profiles.Contains(charID.stardaterProfile)) {
                profiles.Add(charID.stardaterProfile);
            }
        }
    }

    public bool StardaterIsActive() {
        return isActivated;
    }

    public void ContactForChat(DatingProfile sender, DatingProfile receiver) {

        if (sender == tedsProfile) {
            curProfile = sender;
            sender.SetChattingStatusWithTed(true);

            PrevSessionState = SessionState;
            SessionState = SessionMode.BeingContacted;

            chatRequestTxt.text = incomingChatRequestText + sender.GetName() + "?";

        } else {
            sender.SetChattingStatus(true);
            receiver.SetChattingStatus(true);
        }
    }

    public void PlaceStardaterCamInFrontOfMember(IDCharacter charID) {

        datingSourceCamera.transform.SetParent(null);
        datingSourceCamera.transform.SetParent(charID.stardaterCamHolder);

        datingSourceCamera.transform.localPosition = Vector3.zero;
        datingSourceCamera.transform.localRotation = Quaternion.identity;
    }

    public bool TedIsInChat() {
        return (SessionState == SessionMode.Chatting);
    }

    public void TedExitChat() {
        if (SessionState == SessionMode.Chatting) {
            PressCircle();
        }
    }
}
