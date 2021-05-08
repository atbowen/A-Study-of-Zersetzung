using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DatingProfile
{
    public IDCharacter member;
    public Animator anim, animHead;
    public List<Texture> profilePics;
    public Texture customBgd, mainProfileImage;

    [SerializeField]
    private float avgTimeOnline, avgTimeOffline, timeDelta;
    [SerializeField]
    private float avgChanceToMessageTed, avgChanceToRespond, avgTimeResponding;

    [SerializeField]
    private bool usesVideoInChat;

    [SerializeField]
    private Prompt currentOpeningLine;

    [SerializeField]
    private string profileName, realName, age, location, profession;

    [SerializeField]
    private Color plainTextColor, profileNameColor, headerTextColor;

    private static string unknownProfileName = "New  member";
    private static string unknownRealName = "It's  a  mystery";
    private static string unknownAge = "???";
    private static string unknownLocation = "(Moon)";
    private static string unknownProfession = "(Colonist)";

    [SerializeField]
    private List<string> likes, dislikes;
    private static string noLikes = "Ask";
    private static string noDislikes = "Don't  ask";

    public enum SeekingType { Friends, Casual, ShortTerm, LongTerm }
    public SeekingType LookingFor;

    public bool isOnline = true;
    private float timeToWaitUntilSwitchingOnlineStatus, onlineStatusRefTime;

    private bool isChatting = false, isChattingWithTed = false;

    private string characteristicsText, likesAndDislikesText;

    private void UpdateProfileText() {
        if (member.GetComponent<IDCharacter>() != null) {
            IDCharacter charID = member.GetComponent<IDCharacter>();

            if (realName == "") {
                if (charID.ObjActualName != "") { realName = charID.ObjActualName; }
                else                            { realName = unknownRealName; }
            }

            if (profileName == "") {
                if (charID.ObjAvatarName != "") { profileName = charID.ObjAvatarName; }
                else                            { profileName = unknownProfileName; }
            }

            if (age == "") { age = unknownAge; }
            if (location == "") { location = unknownLocation; }
            if (profession == "") { profession = unknownProfession; }
            
            string headerColorHex = ColorUtility.ToHtmlStringRGB(headerTextColor);
            string plainColorHex = ColorUtility.ToHtmlStringRGB(plainTextColor);

            characteristicsText =   "<color=#" + headerColorHex + ">Name:</color>  <color=#" + plainColorHex + ">" + profileName + "</color>\n" +
                                    "<color=#" + headerColorHex + ">(Conf)</color>  <color=#" + plainColorHex + ">" + realName + "</color>\n" +
                                    "<color=#" + headerColorHex + ">Age:</color>  <color=#" + plainColorHex + ">" + age + "</color>\n" +
                                    "<color=#" + headerColorHex + ">Location:</color>  <color=#" + plainColorHex + ">" + location + "</color>\n" +
                                    "<color=#" + headerColorHex + ">Profession:</color>  <color=#" + plainColorHex + ">" + profession + "</color>\n" +
                                    "<color=#" + headerColorHex + ">Seeking:</color>  <color=#" + plainColorHex + ">" + ConvertSeekingTypeToText(LookingFor) + "</color>";

            string tempLikes = "<color=#" + headerColorHex + ">Likes:</color>\n<color=#" + plainColorHex + ">",
                    tempDislikes = "<color=#" + headerColorHex + ">Dislikes:</color>\n<color=#" + plainColorHex + ">";

            switch (likes.Count) {
                case (0):
                    tempLikes += noLikes + "</color>\n\n";
                    break;
                case (1):
                    tempLikes += likes[0] + "</color>\n\n";
                    break;
                case (2):
                    tempLikes += likes[0] + "\n" + likes[1] + "</color>\n\n";
                    break;
                case (3):
                    tempLikes += likes[0] + "\n" + likes[1] + "\n" + likes[2] + "</color>\n\n";
                    break;
                default:
                    tempLikes += likes[0] + "\n" + likes[1] + "\n" + likes[2] + "</color>\n";
                    break;
            }

            switch (dislikes.Count) {
                case (0):
                    tempDislikes += noDislikes + "</color>";
                    break;
                case (1):
                    tempDislikes += dislikes[0] + "</color>";
                    break;
                case (2):
                    tempDislikes += dislikes[0] + "\n" + dislikes[1] + "</color>";
                    break;
                case (3):
                    tempDislikes += dislikes[0] + "\n" + dislikes[1] + "\n" + dislikes[2] + "</color>";
                    break;
                default:
                    tempDislikes += dislikes[0] + "\n" + dislikes[1] + "\n" + dislikes[2] + "</color>";
                    break;
            }

            likesAndDislikesText = tempLikes + tempDislikes;
        }
    }

    public bool StatusIsOnline() {
        return isOnline;
    }

    public void SetOnlineStatus(bool yesOrNo) {
        isOnline = yesOrNo;
    }

    public bool StatusIsChatting() {
        return isChatting;
    }

    public void SetChattingStatus(bool yesOrNo) {
        isChatting = yesOrNo;
    }

    public bool StatusIsChattingWithTed() {
        return isChattingWithTed;
    }

    public void SetChattingStatusWithTed(bool yesOrNo) {
        isChattingWithTed = yesOrNo;
    }

    public string GetName() {
        UpdateProfileText();

        return profileName + "  (" + realName + ")";
    }

    public string GetBasicInfo() {
        UpdateProfileText();

        string profileNameColorHex = ColorUtility.ToHtmlStringRGB(profileNameColor);
        string plainColorHex = ColorUtility.ToHtmlStringRGB(plainTextColor);

        string basicInfo = "<color=#" + profileNameColorHex + ">" + profileName + "</color>\n" + "<color=#" + plainColorHex + ">" + realName + "</color>";

        return basicInfo;
    }

    public string GetCharacteristics() {
        UpdateProfileText();

        return characteristicsText;
    }

    public string GetLikesAndDislikes() {
        UpdateProfileText();

        return likesAndDislikesText;
    }

    private string ConvertSeekingTypeToText(SeekingType type) {
        switch (type) {
            case SeekingType.Casual:
                return "Casual";
            case SeekingType.Friends:
                return "Friends";
            case SeekingType.LongTerm:
                return "Long-term";
            case SeekingType.ShortTerm:
                return "Short-term";
            default:
                return "Friends";
        }
    }

    public void SetOpeningLine(Prompt line) {
        currentOpeningLine = line;
    }

    public float GetWaitTimeForPendingChat() {

        if (avgTimeResponding == 0f)    { return 0f; }
        else                            { return Random.Range(avgTimeResponding - timeDelta, avgTimeResponding + timeDelta); }
    }
}
