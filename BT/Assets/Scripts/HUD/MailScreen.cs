using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailScreen : MonoBehaviour {
    public RawImage mailTextBgd, mailAttachmentSelector, mailListBgd, mailListSelection, mailTextScroller, mailTextScrollUp, mailTextScrollDown,
                    attachmentImageInspector, attachmentTextInspectorBgd, attachmentCursor;
    public List<RawImage> mailAttachmentsIcons; // max 5 attachments
    public Text mailSender, mailRecipient, mailText, mailListText, attachmentTextInspector, attachmentNameText, attachmentTextOfPOI;
    public Texture imageAttachmentIconTexture, dataAttachmentIconTexture;
    public MeshRenderer messengerMainScreen, messengerAttachmentScreen;
    public Material[] newMessagesScreenMaterial, noNewMessagesScreenMaterial, attachmentDefaultMaterial, blankScreenMaterial;
    public List<Texture> transitionScreenFrames;

    public List<EMail> emailList;
    
    public Color mailListCOColor, mailListSubjectColor;

    private enum MailMode {Inactive, Loading, ViewingMessage, ViewingAttachment, Closing, Hacking, Hacked};
    private MailMode MailState;

    private EMail curEMail;
    private EMailAttachment curAttachment;

    private RawImage curAttachmentIcon;
    private int curAttachmentIndex, curAttachmentPointOfInterestIndex;
    [SerializeField]
    private float attachmentSelectorFlashIncrement;
    [SerializeField, Range(0.0f, 1.0f)]
    private float attachmentSelectorFlashAlphaMin, attachmentSelectorFlashAlphaMax;
    private bool attachmentSelectorIsIncreasingAlpha;
    [SerializeField]
    private float attachmentUnravelingTextTimePerCharForPOI;
    private float attachmentUnravelingTextRefTime;
    private int curAttachmentPointDescriptionCharIndex;
    private List<char> unravelingCharsForPOI = new List<char>();
    private bool attachmentPointDescriptionTextIsUnraveling;
    [SerializeField]
    private float attachmentCursorFlashIncrement;
    [SerializeField, Range(0.0f, 1.0f)]
    private float attachmentCursorFlashAlphaMin, attachmentCursorFlashAlphaMax;
    private bool attachmentCursorIsIncreasingAlpha;

    [SerializeField]
    private float alphaOfMessageWindowsWhenInspectingAttachment;
    private float mailTextInitialAlpha, mailTextBgdInitialAlpha, mailRecipientInitialAlpha, mailSenderInitialAlpha, 
                    mailScrollerInitialAlpha, mailScrollUpInitialAlpha, mailScrollDownInitialAlpha;
    [SerializeField]
    private int textSizeHeight, textBgdVerticalPadding;
    private float attachmentTextWidth, attachmentTextBgdWidth;

    [SerializeField]
    private float loadingDelayTime, transitionScreenFrameTime;
    private int curtransitionScreenFramesIndex;
    private float loadingDelayRefTime, transitionScreenFrameRefTime;
    [SerializeField]
    private int numOfOnscreenLinesInMailText, numOfEMailItemSlotsInList;
    private int mailListSelectionHeight, currentEmailListIndex, currentListVisibleIndex;
    private string curEmailText;
    private int curFirstLineIndex;

    private Vector2 mailListSelectionInitialPosition;

    // Start is called before the first frame update
    void Start()
    {
        currentEmailListIndex = 0;
        currentListVisibleIndex = 0;
        curEMail = emailList[currentEmailListIndex];
        curEMail.isNew = false;

        mailListText.supportRichText = true;
        mailRecipient.supportRichText = true;
        mailSender.supportRichText = true;

        mailListSelectionHeight = (int)Mathf.Floor(mailListSelection.rectTransform.rect.height);
        mailListSelectionInitialPosition = mailListSelection.rectTransform.anchoredPosition;

        mailTextInitialAlpha = mailText.color.a;
        mailTextBgdInitialAlpha = mailTextBgd.color.a;
        mailRecipientInitialAlpha = mailRecipient.color.a;
        mailSenderInitialAlpha = mailSender.color.a;
        mailScrollerInitialAlpha = mailTextScroller.color.a;
        mailScrollUpInitialAlpha = mailTextScrollUp.color.a;
        mailScrollDownInitialAlpha = mailTextScrollDown.color.a;

        attachmentTextWidth = attachmentTextInspector.rectTransform.rect.width;
        attachmentTextBgdWidth = attachmentTextInspectorBgd.rectTransform.rect.width;

        curAttachmentIndex = 0;
        curAttachmentIcon = mailAttachmentsIcons[curAttachmentIndex];
        curAttachmentPointOfInterestIndex = 0;
        attachmentPointDescriptionTextIsUnraveling = false;
        curAttachmentPointDescriptionCharIndex = 0;

        attachmentSelectorIsIncreasingAlpha = false;

        curFirstLineIndex = 0;
        DetermineVisibleTextOfCurrentMessage();

        ShowEMail(false);

        MailState = MailMode.Inactive;
    }

    // Update is called once per frame
    void Update()
    {
        curEMail = emailList[currentEmailListIndex];

        switch (MailState) {

            case MailMode.Inactive:

                CheckForNewMail();                 

                break;
            case MailMode.Loading:

                mailListBgd.texture = transitionScreenFrames[curtransitionScreenFramesIndex];

                if (Time.time - loadingDelayRefTime > loadingDelayTime) {

                    mailListBgd.enabled = true;
                    messengerMainScreen.materials = blankScreenMaterial;
                    messengerAttachmentScreen.materials = blankScreenMaterial;

                    if (Time.time - transitionScreenFrameRefTime > transitionScreenFrameTime) {
                        if (curtransitionScreenFramesIndex < transitionScreenFrames.Count - 1) {
                            curtransitionScreenFramesIndex++;
                            transitionScreenFrameRefTime = Time.time;
                        }
                        else {
                            
                            ShowEMail(true);

                            DetermineVisibleTextOfCurrentMessage();

                            MailState = MailMode.ViewingMessage;
                        }
                    }
                }

                break;
            case MailMode.ViewingMessage:

                if (curEMail.attachments.Count > 0) {

                    curAttachmentIcon = mailAttachmentsIcons[curAttachmentIndex];
                    curAttachment = curEMail.attachments[curAttachmentIndex];

                    mailAttachmentSelector.enabled = true;
                    mailAttachmentSelector.rectTransform.anchoredPosition = curAttachmentIcon.rectTransform.anchoredPosition;
                    FlashAttachmentIcon();

                    attachmentNameText.text = curAttachment.attachmentShortDescription;
                    attachmentNameText.enabled = true;
                }
                else {
                    mailAttachmentSelector.enabled = false;

                    attachmentNameText.text = "";
                    attachmentNameText.enabled = false;
                }

                curEMail = emailList[currentEmailListIndex];
                AssignTextAndBgd(curEMail, currentEmailListIndex, currentListVisibleIndex);
                ShowAttachments(curEMail, true);

                break;
            case MailMode.ViewingAttachment:

                if (curAttachment != null) {
                    if (curAttachment.pointsOfInterest.Count > 0) {
                        attachmentCursor.enabled = true;
                        attachmentTextOfPOI.enabled = true;

                        attachmentCursor.rectTransform.anchoredPosition = 
                                attachmentImageInspector.rectTransform.anchoredPosition + curAttachment.pointsOfInterest[curAttachmentPointOfInterestIndex].posRelToImageCenter;

                        FlashAttachmentCursor();

                        if (attachmentPointDescriptionTextIsUnraveling) {
                            if (Time.time - attachmentUnravelingTextRefTime > attachmentUnravelingTextTimePerCharForPOI) {

                                if (curAttachmentPointDescriptionCharIndex < unravelingCharsForPOI.Count) {
                                    
                                    attachmentTextOfPOI.text += unravelingCharsForPOI[curAttachmentPointDescriptionCharIndex];
                                    curAttachmentPointDescriptionCharIndex++;

                                    attachmentUnravelingTextRefTime = Time.time;
                                }
                                else {
                                    attachmentPointDescriptionTextIsUnraveling = false;
                                }
                            }
                        }
                    }
                }
                else {
                    attachmentCursor.enabled = false;
                    attachmentTextOfPOI.enabled = false;
                }

                break;
            case MailMode.Closing:

                mailListBgd.texture = transitionScreenFrames[curtransitionScreenFramesIndex];

                if (Time.time - transitionScreenFrameRefTime > transitionScreenFrameTime) {
                    if (curtransitionScreenFramesIndex > 0) {
                        curtransitionScreenFramesIndex--;
                        transitionScreenFrameRefTime = Time.time;
                    }
                    else {

                        ShowEMail(false);

                        MailState = MailMode.Inactive;
                    }
                }
                
                break;
        }

    }

    private void ShowEMail(bool showOrNo) {

        if (showOrNo) {

            currentEmailListIndex = 0;
            curEMail = emailList[currentEmailListIndex];
            currentListVisibleIndex = 0;

            mailListSelection.rectTransform.anchoredPosition = mailListSelectionInitialPosition + new Vector2(0, (-mailListSelectionHeight * currentListVisibleIndex));

            curFirstLineIndex = 0;
            curAttachmentIndex = 0;
            curAttachmentPointOfInterestIndex = 0;

        }
        else {
            ShowAttachments(curEMail, false);

            mailTextScroller.enabled = false;
            mailTextScrollUp.enabled = false;
            mailTextScrollDown.enabled = false;

            attachmentImageInspector.enabled = false;
            attachmentTextInspector.enabled = false;
            attachmentTextInspectorBgd.enabled = false;
            attachmentNameText.enabled = false;
            attachmentTextOfPOI.enabled = false;
            attachmentCursor.enabled = false;
        }

        mailTextBgd.enabled = showOrNo;
        mailSender.enabled = showOrNo;
        mailRecipient.enabled = showOrNo;
        mailText.enabled = showOrNo;
        mailListBgd.enabled = showOrNo;
        mailListSelection.enabled = showOrNo;
        mailListText.enabled = showOrNo;
    }

    public void MakeActive(bool yesOrNo) {

        //ShowEMail(yesOrNo);

        if (yesOrNo) {

            loadingDelayRefTime = Time.time;
            curtransitionScreenFramesIndex = 0;
            transitionScreenFrameRefTime = Time.time;

            MailState = MailMode.Loading;
        }
        else {

            mailListText.enabled = false;
            mailListSelection.enabled = false;

            attachmentImageInspector.enabled = false;
            attachmentTextInspector.enabled = false;
            attachmentTextInspectorBgd.enabled = false;
            attachmentNameText.enabled = false;
            attachmentTextOfPOI.enabled = false;
            attachmentCursor.enabled = false;

            DimMessageWindows(false);

            curtransitionScreenFramesIndex = transitionScreenFrames.Count - 1;
            transitionScreenFrameRefTime = Time.time;
            MailState = MailMode.Closing;
        }
    }

    public void ForceClose() {
        ShowEMail(false);
        MailState = MailMode.Inactive;
    }

    public bool IsActive() {
        return MailState != MailMode.Inactive;
    }

    public bool IsInInactiveState() {
        return MailState == MailMode.Inactive;
    }

    public bool CanBeExited() {
        return MailState == MailMode.ViewingMessage;
    }

    private void AssignTextAndBgd(EMail email, int curMailIndex, int curListIndex) {

        //string rcp = ColorUtility.ToHtmlStringRGBA(new Color(mailRecipient.color.r, mailRecipient.color.g, mailRecipient.color.b, mailRecipient.color.a * 0.7f));
        //string sen = ColorUtility.ToHtmlStringRGBA(new Color(mailSender.color.r, mailSender.color.g, mailSender.color.b, mailSender.color.a * 0.7f));

        //mailRecipient.text = "<color=#" + rcp + ">Rcp:  </color>" + email.receiverInfo;
        //mailSender.text = "<color=#" + sen + ">C/O:  </color>" + email.senderInfo;
        mailRecipient.text = "Rcp:  " + email.receiverInfo;
        mailSender.text = "C/O:  " + email.senderInfo;
        mailText.text = curEmailText;
        mailTextBgd.texture = email.imageBody;

        mailListText.text = "";

        if (emailList.Count < numOfEMailItemSlotsInList) {

            for (int i = 0; i < emailList.Count; i++) {
                int stringSenderLength = emailList[i].senderInfo.Length;
                int stringBodyLength = emailList[i].bodyToShow.Length;

                string COColor = ColorUtility.ToHtmlStringRGBA(mailListCOColor);
                string subjectColor = ColorUtility.ToHtmlStringRGBA(mailListSubjectColor);

                if (emailList[i].isNew) {
                    mailListText.text = mailListText.text + "<color=#" + COColor + ">" + emailList[i].senderInfo.Substring(0, Mathf.Min(8, stringSenderLength)) + "</color>\n                           (!)\n";
                }
                else {
                    mailListText.text = mailListText.text + "" + emailList[i].senderInfo.Substring(0, Mathf.Min(8, stringSenderLength)) + "\n\n";
                }
            }

        }
        else {

            for (int i = 0; i < numOfEMailItemSlotsInList; i++) {

                int offset = i + (currentEmailListIndex - currentListVisibleIndex);

                int stringSenderLength = emailList[offset].senderInfo.Length;
                int stringBodyLength = emailList[offset].bodyToShow.Length;

                string COColor = ColorUtility.ToHtmlStringRGBA(mailListCOColor);
                string subjectColor = ColorUtility.ToHtmlStringRGBA(mailListSubjectColor);

                if (emailList[offset].isNew) {
                    mailListText.text = mailListText.text + "<color=#" + COColor + ">" + emailList[offset].senderInfo.Substring(0, Mathf.Min(8, stringSenderLength)) + "</color>\n                           (!)\n";
                }
                else {
                    mailListText.text = mailListText.text + "" + emailList[offset].senderInfo.Substring(0, Mathf.Min(8, stringSenderLength)) + "\n\n";
                }
            }

        }
    }

    private void ShowAttachments(EMail email, bool showOrNo) {

        int numOfAttachments = email.attachments.Count;

        if (showOrNo) {
            if (numOfAttachments > 0) {
                for (int i = 0; i < numOfAttachments; i++) {

                    mailAttachmentsIcons[i].enabled = true;

                    switch (email.attachments[i].TypeOfAttachment) {
                        case EMailAttachment.AttachmentType.Image:
                            mailAttachmentsIcons[i].texture = imageAttachmentIconTexture;
                            break;
                        case EMailAttachment.AttachmentType.Data:
                            mailAttachmentsIcons[i].texture = dataAttachmentIconTexture;
                            break;
                    }                    
                }
                mailAttachmentSelector.enabled = true;
            }
            else {
                mailAttachmentSelector.enabled = false;
            }

            if (numOfAttachments < 5) {
                for (int i = numOfAttachments; i < 5; i++) {
                    mailAttachmentsIcons[i].enabled = false;
                }
            }
        }
        else {
            for (int i = 0; i < mailAttachmentsIcons.Count; i++) {
                mailAttachmentsIcons[i].enabled = false;
            }
            mailAttachmentSelector.enabled = false;
        }
    }

    private void CheckForNewMail() {

        bool foundNewMessage = false;

        if (emailList.Count > 0) {
            foreach (EMail mail in emailList) {
                if (mail.isNew) { foundNewMessage = true; }
            }
        }

        messengerAttachmentScreen.materials = attachmentDefaultMaterial;

        if (foundNewMessage)    { messengerMainScreen.materials = newMessagesScreenMaterial; }
        else                    { messengerMainScreen.materials = noNewMessagesScreenMaterial; }
    }

    public void CreateEncryptedMessageWithCode(EMail email) {

        char[] letters = email.body.ToCharArray();
        email.bodyEncrypted = "";

        if (email.code.Count > 0) {
            for (int i = 0; i < letters.Length; i++) {
                if (email.code.ContainsKey(letters[i])) {
                    letters[i] = email.code[letters[i]];
                }

                email.bodyEncrypted = email.bodyEncrypted + letters[i];
            }
        }
    }

    private bool HitDangerKey(EMail email, char keyPressed) {
        char[] keyToLower = keyPressed.ToString().ToLower().ToCharArray();
        keyPressed = keyToLower[0];

        for (int i = 0; i < email.dangerKeys.Length; i++) {
            if (email.dangerKeys[i] == keyPressed) {
                return true;
            }
        }

        return false;
    }

    public void HackCharacter(EMail email, char character) {
        if (HitDangerKey(email, character)) {
            email.hackHealth--;
        }

        char[] letters = email.body.ToCharArray();

        for (int i = 0; i < letters.Length; i++) {
            if (letters[i] == email.code[character]) {
                letters[i] = character;
            }
        }
    }

    private void CheckHackHealth(EMail email) {
        if (email.hackHealth < 1) {
            CreateEncryptedMessageWithCode(email);
        }
    }

    public void AddEMail(EMail email) {
        emailList.Add(email);
    }

    public void SetBodyToOrig(EMail email) {
        email.bodyToShow = email.body;
    }

    public void SetBodyToEncrypted(EMail email) {
        email.bodyToShow = email.bodyEncrypted;
    }

    public void EmailListCycleUp() {

        if (emailList.Count > 0) {
            if (currentListVisibleIndex == 0) {
                if (currentEmailListIndex == 0) {
                    if (numOfEMailItemSlotsInList > emailList.Count) {
                        currentListVisibleIndex = emailList.Count - 1;
                        currentEmailListIndex = emailList.Count - 1;
                        mailListSelection.rectTransform.anchoredPosition = mailListSelectionInitialPosition + new Vector2(0, (-mailListSelectionHeight * currentListVisibleIndex));
                    }
                    else {
                        currentListVisibleIndex = numOfEMailItemSlotsInList - 1;
                        currentEmailListIndex = emailList.Count - 1;
                        mailListSelection.rectTransform.anchoredPosition = mailListSelectionInitialPosition + new Vector2(0, (-mailListSelectionHeight * (numOfEMailItemSlotsInList - 1)));
                    }
                }
                else {
                    currentEmailListIndex--;
                }
            }
            else {
                currentListVisibleIndex--;
                currentEmailListIndex--;
                mailListSelection.rectTransform.anchoredPosition = mailListSelectionInitialPosition + new Vector2(0, (-mailListSelectionHeight * currentListVisibleIndex));
            }

            curEMail = emailList[currentEmailListIndex];
            curEMail.isNew = false;

            curFirstLineIndex = 0;
            DetermineVisibleTextOfCurrentMessage();

            curAttachmentIndex = 0;
            curAttachmentPointOfInterestIndex = 0;
            InitializeTextOfCurrentAttachmentCurrentPOI();
        }
    }

    public void EmailListCycleDown() {

        if (emailList.Count > 0) {
            if (currentListVisibleIndex == numOfEMailItemSlotsInList - 1) {
                if (currentEmailListIndex == emailList.Count - 1) {
                    currentListVisibleIndex = 0;
                    currentEmailListIndex = 0;
                    mailListSelection.rectTransform.anchoredPosition = mailListSelectionInitialPosition;
                }
                else {
                    currentEmailListIndex++;
                }
            }
            else {
                if (currentEmailListIndex == emailList.Count - 1) {
                    currentListVisibleIndex = 0;
                    currentEmailListIndex = 0;
                    mailListSelection.rectTransform.anchoredPosition = mailListSelectionInitialPosition;
                }
                else {
                    currentListVisibleIndex++;
                    currentEmailListIndex++;
                    mailListSelection.rectTransform.anchoredPosition = mailListSelectionInitialPosition + new Vector2(0, (-mailListSelectionHeight * currentListVisibleIndex));
                }
            }

            curEMail = emailList[currentEmailListIndex];
            curEMail.isNew = false;

            curFirstLineIndex = 0;
            DetermineVisibleTextOfCurrentMessage();

            curAttachmentIndex = 0;

            curAttachmentPointOfInterestIndex = 0;
            InitializeTextOfCurrentAttachmentCurrentPOI();
        }
    }

    private void DetermineVisibleTextOfCurrentMessage() {

        string[] messageLines = curEMail.bodyToShow.Split('\n');

        if (messageLines.Length > numOfOnscreenLinesInMailText) {

            string mailBody = "";

            for (int i = 0; i < numOfOnscreenLinesInMailText; i++) {
                mailBody += messageLines[curFirstLineIndex + i] + "\n";
            }

            curEmailText = mailBody;

            mailTextScroller.enabled = true;

            if (curFirstLineIndex > 0)  { mailTextScrollUp.enabled = true; }
            else                        { mailTextScrollUp.enabled = false; }

            if (curFirstLineIndex < messageLines.Length - numOfOnscreenLinesInMailText) { mailTextScrollDown.enabled = true; }
            else                                                                        { mailTextScrollDown.enabled = false; }

        }
        else {
            curEmailText = curEMail.bodyToShow;

            mailTextScroller.enabled = false;
            mailTextScrollUp.enabled = false;
            mailTextScrollDown.enabled = false;
        }
    }

    private void ScrollUpMessage() {
        string[] messageLines = curEMail.bodyToShow.Split('\n');

        if (messageLines.Length > numOfOnscreenLinesInMailText) {
            if (curFirstLineIndex > 0) {

                Debug.Log("here! up!");

                curFirstLineIndex--;
            }
        }
    }

    private void ScrollDownMessage() {
        string[] messageLines = curEMail.bodyToShow.Split('\n');

        if (messageLines.Length > numOfOnscreenLinesInMailText) {
            if (curFirstLineIndex < messageLines.Length - numOfOnscreenLinesInMailText) {

                Debug.Log("here! down!");

                curFirstLineIndex++;
            }
        }
    }

    private void FlashAttachmentIcon() {
        if (attachmentSelectorIsIncreasingAlpha) {
            if (mailAttachmentSelector.color.a < attachmentSelectorFlashAlphaMax) {
                mailAttachmentSelector.color = new Color(mailAttachmentSelector.color.r, mailAttachmentSelector.color.g, mailAttachmentSelector.color.b,
                                                           mailAttachmentSelector.color.a + (attachmentSelectorFlashIncrement * Time.deltaTime));
            }
            else {
                attachmentSelectorIsIncreasingAlpha = false;
            }
        }
        else {
            if (mailAttachmentSelector.color.a > attachmentSelectorFlashAlphaMin) {
                mailAttachmentSelector.color = new Color(mailAttachmentSelector.color.r, mailAttachmentSelector.color.g, mailAttachmentSelector.color.b,
                                                           mailAttachmentSelector.color.a - (attachmentSelectorFlashIncrement * Time.deltaTime));
            }
            else {
                attachmentSelectorIsIncreasingAlpha = true;
            }
        }
    }

    private void InspectAttachment(bool yesOrNo) {

        if (curAttachment != null) {

            DimMessageWindows(yesOrNo);

            if (yesOrNo) {

                mailAttachmentSelector.color = new Color(mailAttachmentSelector.color.r, mailAttachmentSelector.color.g, mailAttachmentSelector.color.b, attachmentSelectorFlashAlphaMax);

                switch (curAttachment.TypeOfAttachment) {

                    case EMailAttachment.AttachmentType.Image:

                        if (curAttachment.attachmentPicture != null) {

                            attachmentImageInspector.rectTransform.sizeDelta = new Vector2(curAttachment.attachmentPicture.width, curAttachment.attachmentPicture.height);
                            attachmentImageInspector.texture = curAttachment.attachmentPicture;
                            attachmentImageInspector.enabled = true;
                        
                        }

                        break;
                    case EMailAttachment.AttachmentType.Data:

                        if (curAttachment.attachmentText != "") {

                            string[] messageLines = curAttachment.attachmentText.Split('\n');

                            attachmentTextInspector.rectTransform.sizeDelta = new Vector2(attachmentTextWidth, textSizeHeight * messageLines.Length);
                            attachmentTextInspector.text = curAttachment.attachmentText;
                            attachmentTextInspector.enabled = true;
                            attachmentTextInspectorBgd.rectTransform.sizeDelta = new Vector2(attachmentTextBgdWidth, textSizeHeight * messageLines.Length + textBgdVerticalPadding);
                            attachmentTextInspectorBgd.enabled = true;

                        }

                        break;
                }
            }
            else {

                DimMessageWindows(false);
                attachmentImageInspector.enabled = false;
                attachmentTextInspector.enabled = false;
                attachmentTextInspectorBgd.enabled = false;

            }
        }
    }

    private void DimMessageWindows(bool yesOrNo) {
        if (yesOrNo) {
            mailText.color = new Color(mailText.color.r, mailText.color.g, mailText.color.b, alphaOfMessageWindowsWhenInspectingAttachment);
            mailTextBgd.color = new Color(mailTextBgd.color.r, mailTextBgd.color.g, mailTextBgd.color.b, alphaOfMessageWindowsWhenInspectingAttachment);
            mailRecipient.color = new Color(mailRecipient.color.r, mailRecipient.color.g, mailRecipient.color.b, alphaOfMessageWindowsWhenInspectingAttachment);
            mailSender.color = new Color(mailSender.color.r, mailSender.color.g, mailSender.color.b, alphaOfMessageWindowsWhenInspectingAttachment);
            mailTextScroller.color = new Color(mailTextScroller.color.r, mailTextScroller.color.g, mailTextScroller.color.b, alphaOfMessageWindowsWhenInspectingAttachment);
            mailTextScrollUp.color = new Color(mailTextScrollUp.color.r, mailTextScrollUp.color.g, mailTextScrollUp.color.b, alphaOfMessageWindowsWhenInspectingAttachment);
            mailTextScrollDown.color = new Color(mailTextScrollDown.color.r, mailTextScrollDown.color.g, mailTextScrollDown.color.b, alphaOfMessageWindowsWhenInspectingAttachment);
        }
        else {
            mailText.color = new Color(mailText.color.r, mailText.color.g, mailText.color.b, mailTextInitialAlpha);
            mailTextBgd.color = new Color(mailTextBgd.color.r, mailTextBgd.color.g, mailTextBgd.color.b, mailTextBgdInitialAlpha);
            mailRecipient.color = new Color(mailRecipient.color.r, mailRecipient.color.g, mailRecipient.color.b, mailRecipientInitialAlpha);
            mailSender.color = new Color(mailSender.color.r, mailSender.color.g, mailSender.color.b, mailSenderInitialAlpha);
            mailTextScroller.color = new Color(mailTextScroller.color.r, mailTextScroller.color.g, mailTextScroller.color.b, mailScrollerInitialAlpha);
            mailTextScrollUp.color = new Color(mailTextScrollUp.color.r, mailTextScrollUp.color.g, mailTextScrollUp.color.b, mailScrollUpInitialAlpha);
            mailTextScrollDown.color = new Color(mailTextScrollDown.color.r, mailTextScrollDown.color.g, mailTextScrollDown.color.b, mailScrollDownInitialAlpha);
        }
    }

    private void InitializeTextOfCurrentAttachmentCurrentPOI() {

        attachmentTextOfPOI.text = "";

        if (curAttachment != null) {
            if (curAttachment.pointsOfInterest.Count > 0) {

                unravelingCharsForPOI.Clear();
                unravelingCharsForPOI.Add('>');
                unravelingCharsForPOI.Add(' ');
                unravelingCharsForPOI.Add(' ');

                char[] temp = curAttachment.pointsOfInterest[curAttachmentPointOfInterestIndex].pointDescription.ToCharArray();

                for (int i = 0; i < temp.Length; i++) {
                    unravelingCharsForPOI.Add(temp[i]);
                }
            }

            attachmentPointDescriptionTextIsUnraveling = true;
            curAttachmentPointDescriptionCharIndex = 0;
            attachmentUnravelingTextRefTime = Time.time;
        }
    }

    private void FlashAttachmentCursor() {
        if (attachmentCursorIsIncreasingAlpha) {
            if (attachmentCursor.color.a < attachmentCursorFlashAlphaMax) {
                attachmentCursor.color = new Color(attachmentCursor.color.r, attachmentCursor.color.g, attachmentCursor.color.b, 
                                                        attachmentCursor.color.a + attachmentCursorFlashIncrement * Time.deltaTime);
            }
            else {
                attachmentCursorIsIncreasingAlpha = false;
            }
        }
        else {
            if (attachmentCursor.color.a > attachmentCursorFlashAlphaMin) {
                attachmentCursor.color = new Color(attachmentCursor.color.r, attachmentCursor.color.g, attachmentCursor.color.b,
                                                        attachmentCursor.color.a - attachmentCursorFlashIncrement * Time.deltaTime);
            }
            else {
                attachmentCursorIsIncreasingAlpha = true;
            }
        }
    }

    ///
    /// CONTROLS
    ///

    public void PressX() {
        if (MailState == MailMode.ViewingMessage && curEMail.attachments.Count > 0 && curAttachment != null) {

            InspectAttachment(true);

            curAttachmentPointOfInterestIndex = 0;
            InitializeTextOfCurrentAttachmentCurrentPOI();

            MailState = MailMode.ViewingAttachment;
        }
        else if (MailState == MailMode.ViewingAttachment) {

            InspectAttachment(false);
            attachmentCursor.enabled = false;
            attachmentTextOfPOI.enabled = false;

            MailState = MailMode.ViewingMessage;
        }
    }

    public void PressCircle() {
        if (MailState == MailMode.ViewingAttachment) {

            InspectAttachment(false);
            attachmentCursor.enabled = false;
            attachmentTextOfPOI.enabled = false;

            MailState = MailMode.ViewingMessage;
        }
    }

    public void PressUpLS() {
        if (MailState == MailMode.ViewingMessage) {
            if (emailList.Count > 0) {
                EmailListCycleUp();
            }
        }
    }

    public void PressDownLS() {
        if (MailState == MailMode.ViewingMessage) {
            if (emailList.Count > 0) {
                EmailListCycleDown();
            }
        }
    }

    public void PressLeftLS() {

    }

    public void PressRightLS() {

    }

    public void PressUpRS() {
        if (MailState == MailMode.ViewingMessage) {
            ScrollUpMessage();
            DetermineVisibleTextOfCurrentMessage();
        }
        else if (MailState == MailMode.ViewingAttachment) {
            if (curAttachment != null) {
                if (curAttachment.pointsOfInterest.Count > 1) {
                    curAttachmentPointOfInterestIndex = (curAttachmentPointOfInterestIndex + 1) % curAttachment.pointsOfInterest.Count;
                    InitializeTextOfCurrentAttachmentCurrentPOI();
                }
            }
        }
    }

    public void PressDownRS() {
        if (MailState == MailMode.ViewingMessage) {
            ScrollDownMessage();
            DetermineVisibleTextOfCurrentMessage();
        }
        else if (MailState == MailMode.ViewingAttachment) {
            if (curAttachment != null) {
                if (curAttachment.pointsOfInterest.Count > 1) {
                    curAttachmentPointOfInterestIndex = (curAttachmentPointOfInterestIndex - 1 + curAttachment.pointsOfInterest.Count) % curAttachment.pointsOfInterest.Count;
                    InitializeTextOfCurrentAttachmentCurrentPOI();
                }
            }
        }
    }

    public void PressLeftRS() {
        if (MailState == MailMode.ViewingMessage) {
            if (curEMail.attachments.Count > 0) {
                curAttachmentIndex = (curAttachmentIndex + 1) % curEMail.attachments.Count;
            }
        }
        else if (MailState == MailMode.ViewingAttachment) {
            if (curAttachment != null) {
                if (curAttachment.pointsOfInterest.Count > 1) {
                    curAttachmentPointOfInterestIndex = (curAttachmentPointOfInterestIndex - 1 + curAttachment.pointsOfInterest.Count) % curAttachment.pointsOfInterest.Count;
                    InitializeTextOfCurrentAttachmentCurrentPOI();
                }
            }
        }
    }

    public void PressRightRS() {
        if (MailState == MailMode.ViewingMessage) {
            if (curEMail.attachments.Count > 0) {
                curAttachmentIndex = (curAttachmentIndex - 1 + curEMail.attachments.Count) % curEMail.attachments.Count;
            }
        }
        else if (MailState == MailMode.ViewingAttachment) {
            if (curAttachment != null) {
                if (curAttachment.pointsOfInterest.Count > 1) {
                    curAttachmentPointOfInterestIndex = (curAttachmentPointOfInterestIndex + 1) % curAttachment.pointsOfInterest.Count;
                    InitializeTextOfCurrentAttachmentCurrentPOI();
                }
            }
        }
    }
}
