using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailScreen : MonoBehaviour {
    public RawImage mailTextBgd, mailAttachmentIndicator, mailListBgd, mailListSelection, mailTextScroller, mailTextScrollUp, mailTextScrollDown;
    public RawImage[] mailAttachmentsIcons;                                      // max 5 attachments
    public Text mailSender, mailRecipient, mailText, mailListText;
    public RawImage selectedAttachment, selectedAttachmentHighlight;
    public Texture  mailListSelectedMail, mailListSelectedMailSelected, 
                    mailTextScrollUpUnpressed, mailTextScrollUpPressed, 
                    mailTextScrollDownUnpressed, mailTextScrollDownPressed;

    public List<EMail> emailList;
    public EMail currentEMail;
    public bool showEMail, mailListActive, mailSelected, mailTextIsScrollable;
    public Color mailListCOColor, mailListSubjectColor;

    private enum EncryptionState {Hidden, MailList, Initialize, Hacking, Hacked};
    private EncryptionState Crypto;

    private int numOfOnscreenLinesInMailText;
    private int mailListSelectionHeight, currentEmailListIndex;

    // Start is called before the first frame update
    void Start()
    {
        currentEmailListIndex = 0;
        currentEMail = emailList[currentEmailListIndex];
        currentEMail.isNew = false;

        mailListText.supportRichText = true;

        ShowEMail(false);

        numOfOnscreenLinesInMailText = 13;                      // This is how many lines will fit in the mail text window!
        mailListSelectionHeight = 14;                            // This is the height of the email selector box (and also the height of a line of text)

        Crypto = EncryptionState.Hidden;
    }

    // Update is called once per frame
    void Update()
    {
        switch (Crypto) {
            case EncryptionState.Hidden:
                currentEMail = emailList[currentEmailListIndex];
                ShowAttachments(currentEMail, false);

                if (showEMail) {
                    if (currentEMail.encrypted) { SetBodyToEncrypted(currentEMail); }
                    ShowEMail(true);

                    mailListActive = true;
                    Crypto = EncryptionState.MailList;
                }
                break;
            case EncryptionState.MailList:
                currentEMail = emailList[currentEmailListIndex];
                AssignTextAndBgd(currentEMail);
                ShowAttachments(currentEMail, true);

                if (mailSelected) {
                    mailListActive = false;
                    Crypto = EncryptionState.Initialize;
                }

                if (!showEMail) {
                    mailListActive = false;
                    ShowEMail(false);
                    Crypto = EncryptionState.Hidden;
                }
                break;
            case EncryptionState.Initialize:
                ShowAttachments(currentEMail, true);

                if (!showEMail) {
                    mailListActive = false;
                    ShowEMail(false);
                    SetBodyToOrig(currentEMail);
                    Crypto = EncryptionState.Hidden;
                }
                break;
        }
    }

    private void ShowEMail(bool showOrNo) {
        if (mailText.cachedTextGenerator.lineCount > numOfOnscreenLinesInMailText && showOrNo)  {
            mailTextIsScrollable = true;
            mailTextScroller.enabled = true;
            mailTextScrollUp.enabled = true;
            mailTextScrollDown.enabled = true;
        }
        else {
            mailTextIsScrollable = false;
            mailTextScroller.enabled = false;
            mailTextScrollUp.enabled = false;
            mailTextScrollDown.enabled = false;
        }

        mailTextBgd.enabled = showOrNo;
        mailSender.enabled = showOrNo;
        mailRecipient.enabled = showOrNo;
        mailText.enabled = showOrNo;
        mailListBgd.enabled = showOrNo;
        mailListSelection.enabled = showOrNo;
        mailListText.enabled = showOrNo;
    }

    private void AssignTextAndBgd(EMail email) {
        mailRecipient.text = "REC:  " + email.receiverInfo;
        mailSender.text = "CO:  " + email.senderInfo;
        mailText.text = email.bodyToShow;
        mailTextBgd.texture = email.imageBody;

        mailListText.text = "";
        for (int i = 0; i < emailList.Count; i++) {
            int stringSenderLength = emailList[i].senderInfo.Length;
            int stringBodyLength = emailList[i].bodyToShow.Length;

            string COColor = ColorUtility.ToHtmlStringRGBA(mailListCOColor);
            string subjectColor = ColorUtility.ToHtmlStringRGBA(mailListSubjectColor);

            if (emailList[i].isNew) {
                mailListText.text = mailListText.text + "<color=#" + COColor + "><CO:  " + emailList[i].senderInfo.Substring(0, Mathf.Min(10, stringSenderLength)) + "..</color>\n" + "<color=#" + subjectColor + ">[" +
                                    emailList[i].bodyToShow.Substring(0, Mathf.Min(11, stringBodyLength)) + "..]></color>\n";
            } else {
                mailListText.text = mailListText.text + "<CO:  " + emailList[i].senderInfo.Substring(0, Mathf.Min(10, stringSenderLength)) + "..\n" + "[" +
                                    emailList[i].bodyToShow.Substring(0, Mathf.Min(11, stringBodyLength)) + "..]>\n";
            }
        }
    }

    private void ShowAttachments(EMail email, bool showOrNo) {

        int numOfAttachments = email.attachments.Length;

        if (showOrNo) {
            if (numOfAttachments > 0) {
                for (int i = 0; i < numOfAttachments; i++) {
                    mailAttachmentsIcons[i].enabled = true;
                    mailAttachmentsIcons[i].texture = email.attachments[i].attachmentIcon;                    
                }
                mailAttachmentIndicator.enabled = true;
            } else { mailAttachmentIndicator.enabled = false; }

            if (numOfAttachments < 5) {
                for (int i = numOfAttachments; i < 5; i++) {
                    mailAttachmentsIcons[i].enabled = false;
                }
            }
        } else {
            for (int i = 0; i < mailAttachmentsIcons.Length; i++) {
                mailAttachmentsIcons[i].enabled = false;
            }
            mailAttachmentIndicator.enabled = false;
        }
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
        if (currentEmailListIndex > 0)  { mailListSelection.rectTransform.anchoredPosition += new Vector2(0, mailListSelectionHeight); }
        else                            { mailListSelection.rectTransform.anchoredPosition += new Vector2(0, (-mailListSelectionHeight * (emailList.Count - 1))); }

        currentEmailListIndex = (currentEmailListIndex + emailList.Count - 1) % emailList.Count;
        emailList[currentEmailListIndex].isNew = false;
    }
    
    public void EmailListCycleDown() {
        if (currentEmailListIndex < (emailList.Count - 1))  { mailListSelection.rectTransform.anchoredPosition += new Vector2(0, -mailListSelectionHeight);} 
        else                                                { mailListSelection.rectTransform.anchoredPosition += new Vector2(0, (mailListSelectionHeight * (emailList.Count - 1))); }

        currentEmailListIndex = (currentEmailListIndex + emailList.Count + 1) % emailList.Count;
        emailList[currentEmailListIndex].isNew = false;
    }
}
