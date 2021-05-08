using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigestScreen : MonoBehaviour
{
    [SerializeField]
    private RawImage inspectorImage, inspectorBgd, peopleListBookmark, placesListBookmark, thingsListBookmark, listSelector;
    [SerializeField]
    private Texture unknownEntryTexture, peopleBookmarkCovered, peopleBookmarkUncovered, placesBookmarkCovered, placesBookmarkUncovered, thingsBookmarkCovered, thingsBookmarkUncovered;
    public List<Texture> inspectorImageFrames;
    [SerializeField]
    private float loadingDelayTime, inspectorOpeningFrameTime, inspectorClosingFrameTime;

    [SerializeField]
    private Text entryListHeader, entryListOnPage, entryListSelectedEntryHighlightCopy, entryHeader, entryDescription;
    [SerializeField]
    private string unknownFieldText, peopleHeaderText, placesHeaderText, thingsHeaderText;
    [SerializeField]
    private Color bodyTextColor, headerTextColor, highlightedTextColor, highlightCopyColor;

    public List<DigestEntry> startingList;    

    private List<DigestEntry> peopleList = new List<DigestEntry>(), placesList = new List<DigestEntry>(), thingsList = new List<DigestEntry>();

    private List<DigestEntry> curList = new List<DigestEntry>();
    private RawImage curBookmark;

    private bool isActive;

    public enum DigestEntryType { Person, Place, Thing }

    private enum DigestInspectionMode { Inactive, Loading, Viewing, Closing }
    private DigestInspectionMode DigestInspectionState, PreviousDigestViewingState;

    private int curInspectorFrameIndex;
    private float loadingDelayRefTime, inspectorFrameRefTime;
    private bool inspectorFinishedTransitioning, listHeaderFinishedTransitioning, listFinishedTransitioning;

    private int curListIndex, curVisibleListIndex;

    private Vector2 listSelectorInitialPos, highlightCopyInitialPos;
    private const float textLineHeight = 7.0f;
    private const int numberOfViewableTextLinesInList = 10;

    // Start is called before the first frame update
    void Start()
    {
        if (startingList.Count > 0) {
            foreach (DigestEntry entry in startingList) {
                if (entry.typeOfDigestEntry == DigestEntryType.Person && !peopleList.Contains(entry)) { peopleList.Add(entry); }
                if (entry.typeOfDigestEntry == DigestEntryType.Place && !placesList.Contains(entry)) { placesList.Add(entry); }
                if (entry.typeOfDigestEntry == DigestEntryType.Thing && !thingsList.Contains(entry)) { thingsList.Add(entry); }
            }
        }

        listSelectorInitialPos = listSelector.rectTransform.anchoredPosition;
        highlightCopyInitialPos = entryListSelectedEntryHighlightCopy.rectTransform.anchoredPosition;

        inspectorBgd.enabled = false;
        inspectorImage.enabled = false;
        listSelector.enabled = false;

        peopleListBookmark.enabled = false;
        placesListBookmark.enabled = false;
        thingsListBookmark.enabled = false;

        entryListHeader.enabled = false;
        entryListOnPage.enabled = false;
        entryListSelectedEntryHighlightCopy.enabled = false;
        entryHeader.enabled = false;
        entryDescription.enabled = false;

        curList = peopleList;
        curListIndex = 0;
        curVisibleListIndex = 0;

        entryListHeader.color = headerTextColor;
        entryListOnPage.color = bodyTextColor;
        entryHeader.color = headerTextColor;
        entryDescription.color = bodyTextColor;
        entryListSelectedEntryHighlightCopy.color = highlightCopyColor;

        entryListHeader.text = peopleHeaderText;
        curBookmark = peopleListBookmark;

        isActive = false;

        DigestInspectionState = DigestInspectionMode.Inactive;
    }

    // Update is called once per frame
    void Update()
    {
        switch (DigestInspectionState) {
            case (DigestInspectionMode.Inactive):
                break;
            case (DigestInspectionMode.Loading):

                if (Time.time - loadingDelayRefTime > loadingDelayTime) {

                    inspectorBgd.enabled = true;

                    inspectorBgd.texture = inspectorImageFrames[curInspectorFrameIndex];

                    if (Time.time - inspectorFrameRefTime > inspectorOpeningFrameTime) {
                        if (curInspectorFrameIndex < inspectorImageFrames.Count - 1) {
                            curInspectorFrameIndex++;
                        }
                        else {
                            DigestInspectionState = DigestInspectionMode.Viewing;

                            inspectorImage.enabled = true;

                            entryListHeader.enabled = true;
                            entryListOnPage.enabled = true;
                            entryHeader.enabled = true;
                            entryDescription.enabled = true;

                            //listSelector.enabled = true;
                            entryListSelectedEntryHighlightCopy.enabled = true;
                            peopleListBookmark.enabled = true;
                            placesListBookmark.enabled = true;
                            thingsListBookmark.enabled = true;

                            curBookmark = peopleListBookmark;

                            entryListHeader.text = peopleHeaderText;

                            peopleListBookmark.texture = peopleBookmarkUncovered;
                            placesListBookmark.texture = placesBookmarkCovered;
                            thingsListBookmark.texture = thingsBookmarkCovered;
                        }

                        inspectorFrameRefTime = Time.time;
                    }
                }

                break;
            case (DigestInspectionMode.Viewing):

                UpdateListAndSelection();

                if (curList[curListIndex].GetPic() != null)         { inspectorImage.texture = curList[curListIndex].GetPic(); }
                else                                                { inspectorImage.texture = unknownEntryTexture; }

                if (curList[curListIndex].GetName() != null)        { entryHeader.text = curList[curListIndex].GetName(); }
                else                                                { entryHeader.text = unknownFieldText; }

                if (curList[curListIndex].GetDescription() != null) { entryDescription.text = curList[curListIndex].GetDescription(); }
                else                                                { entryDescription.text = unknownFieldText; }

                break;
            case (DigestInspectionMode.Closing):

                inspectorBgd.texture = inspectorImageFrames[curInspectorFrameIndex];

                if (Time.time - inspectorFrameRefTime > inspectorClosingFrameTime) {
                    if (curInspectorFrameIndex > 0) {
                        curInspectorFrameIndex--;
                    }
                    else {

                        inspectorBgd.enabled = false;

                        DigestInspectionState = DigestInspectionMode.Inactive;
                    }

                    inspectorFrameRefTime = Time.time;
                }

                break;
        }
    }

    public void ShowDigestWindows(bool yesOrNo) {

        isActive = yesOrNo;

        if (yesOrNo) {
            DigestInspectionState = DigestInspectionMode.Loading;

            loadingDelayRefTime = Time.time;

            curInspectorFrameIndex = 0;
            inspectorFrameRefTime = Time.time;
        }
        else {
            DigestInspectionState = DigestInspectionMode.Closing;

            inspectorImage.enabled = false;
            listSelector.enabled = false;
            peopleListBookmark.enabled = false;
            placesListBookmark.enabled = false;
            thingsListBookmark.enabled = false;

            entryListHeader.enabled = false;
            entryListOnPage.enabled = false;
            entryListSelectedEntryHighlightCopy.enabled = false;
            entryHeader.enabled = false;
            entryDescription.enabled = false;
            
            curInspectorFrameIndex = inspectorImageFrames.Count - 1;
            inspectorFrameRefTime = Time.time;
        }
    }

    public bool IsActive() {
        return isActive;
    }

    public bool IsInInactiveState() {
        return (!isActive && DigestInspectionState == DigestInspectionMode.Inactive);
    }

    public void ForceClose() {

        isActive = false;

        inspectorBgd.enabled = false;
        inspectorImage.enabled = false;
        listSelector.enabled = false;
        peopleListBookmark.enabled = false;
        placesListBookmark.enabled = false;
        thingsListBookmark.enabled = false;

        entryListHeader.enabled = false;
        entryListOnPage.enabled = false;
        entryListSelectedEntryHighlightCopy.enabled = false;
        entryHeader.enabled = false;
        entryDescription.enabled = false;

        DigestInspectionState = DigestInspectionMode.Inactive;

    }

    public void AddIDEntryToList(ID subject) {
        if (subject.digestEntries.Count > 0) {
            foreach (DigestEntry entry in subject.digestEntries) {
                if (entry.typeOfDigestEntry == DigestEntryType.Person && !peopleList.Contains(entry)) {
                    peopleList.Add(entry);
                    AlphabetizeAListByName(peopleList);
                }
                if (entry.typeOfDigestEntry == DigestEntryType.Place && !placesList.Contains(entry)) {
                    placesList.Add(entry);
                    AlphabetizeAListByName(placesList);
                }
                if (entry.typeOfDigestEntry == DigestEntryType.Thing && !thingsList.Contains(entry)) {
                    thingsList.Add(entry);
                    AlphabetizeAListByName(thingsList);
                }
            }
        }
    }

    public void AddEntryToList(DigestEntry entry) {
        if (entry.typeOfDigestEntry == DigestEntryType.Person && !peopleList.Contains(entry)) {
            peopleList.Add(entry);
            AlphabetizeAListByName(peopleList);
        }
        if (entry.typeOfDigestEntry == DigestEntryType.Place && !placesList.Contains(entry)) {
            placesList.Add(entry);
            AlphabetizeAListByName(placesList);
        }
        if (entry.typeOfDigestEntry == DigestEntryType.Thing && !thingsList.Contains(entry)) {
            thingsList.Add(entry);
            AlphabetizeAListByName(thingsList);
        }
    }

    private void AlphabetizeAListByName(List<DigestEntry> entryList) {
        List<string> listOfEntryNames = new List<string>();
        List<DigestEntry> tempEntryList = new List<DigestEntry>();

        if (entryList.Count > 1) {
            foreach (DigestEntry entry in entryList) {
                listOfEntryNames.Add(entry.GetName());
            }

            listOfEntryNames.Sort();

            foreach (string entryName in listOfEntryNames) {
                foreach (DigestEntry entry in entryList) {
                    if (entryName == entry.GetName()) {
                        tempEntryList.Add(entry);
                    }
                }
            }

            entryList.Clear();

            foreach (DigestEntry entry in tempEntryList) {
                entryList.Add(entry);
            }
        }
    }

    private void UpdateListAndSelection() {

        string listText = "";
        string highlightColorHex = ColorUtility.ToHtmlStringRGBA(highlightedTextColor);
        string normalBodyColorHex = ColorUtility.ToHtmlStringRGBA(bodyTextColor);
        string highlightCopyColorHex = ColorUtility.ToHtmlStringRGBA(highlightCopyColor);

        if (curList.Count > numberOfViewableTextLinesInList) {
            for (int i = 0; i < numberOfViewableTextLinesInList; i++) {
                if (i == curVisibleListIndex) {
                    listText += "<color=#" + highlightColorHex + ">" + curList[curListIndex - curVisibleListIndex + i].GetName() + "</color>\n";
                    entryListSelectedEntryHighlightCopy.text = curList[curListIndex - curVisibleListIndex + i].GetName();
                }
                else {
                    listText += "<color=#" + normalBodyColorHex + ">" + curList[curListIndex - curVisibleListIndex + i].GetName() + "</color>\n";
                }
            }
        }
        else {
            for (int i = 0; i < curList.Count; i++) {
                if (i == curVisibleListIndex) {
                    listText += "<color=#" + highlightColorHex + ">" + curList[i].GetName() + "</color>\n";
                    entryListSelectedEntryHighlightCopy.text = curList[i].GetName();
                }
                else {
                    listText += "<color=#" + normalBodyColorHex + ">" + curList[i].GetName() + "</color>\n";
                }
            }
        }

        entryListOnPage.text = listText;

        //listSelector.rectTransform.anchoredPosition = listSelectorInitialPos + new Vector2(0, -textLineHeight * curVisibleListIndex);
        entryListSelectedEntryHighlightCopy.rectTransform.anchoredPosition = highlightCopyInitialPos + new Vector2(0, -textLineHeight * curVisibleListIndex);
    }

    private void ScrollUpList() {
        if (curList.Count > 0) {
            if (curVisibleListIndex > 0) {
                curVisibleListIndex--;
                curListIndex--;
            }
            else {
                if (curListIndex > 0) {
                    curListIndex--;
                }
                else {
                    if (curList.Count > numberOfViewableTextLinesInList) {
                        curVisibleListIndex = numberOfViewableTextLinesInList - 1;
                        curListIndex = curList.Count - 1;
                    }
                    else {
                        curVisibleListIndex = curList.Count - 1;
                        curListIndex = curList.Count - 1;
                    }
                }
            }
        }
    }

    private void ScrollDownList() {
        if (curList.Count > 0) {
            if (curVisibleListIndex < numberOfViewableTextLinesInList - 1) {
                if (curListIndex < curList.Count - 1) {
                    curVisibleListIndex++;
                    curListIndex++;
                }
                else {
                    curVisibleListIndex = 0;
                    curListIndex = 0;
                }
            }
            else {
                if (curListIndex < curList.Count - 1) {
                    curListIndex++;
                }
                else {
                    curVisibleListIndex = 0;
                    curListIndex = 0;
                }
            }
        }
    }

    ///
    /// CONTROLS
    /// 

    public void PressX() {
        if (DigestInspectionState == DigestInspectionMode.Viewing) {

        }
    }

    public void PressCircle() {
        if (DigestInspectionState == DigestInspectionMode.Viewing) {

        }
    }

    public void PressUpLS() {
        if (DigestInspectionState == DigestInspectionMode.Viewing) {
            ScrollUpList();
        }
    }

    public void PressDownLS() {
        if (DigestInspectionState == DigestInspectionMode.Viewing) {
            ScrollDownList();
        }
    }

    public void PressLeftLS() {
        if (DigestInspectionState == DigestInspectionMode.Viewing) {
            if (curList == peopleList) {
                curList = thingsList;
                peopleListBookmark.texture = peopleBookmarkCovered;
                placesListBookmark.texture = placesBookmarkCovered;
                thingsListBookmark.texture = thingsBookmarkUncovered;

                entryListHeader.text = thingsHeaderText;
            }
            else if (curList == placesList) {
                curList = peopleList;
                peopleListBookmark.texture = peopleBookmarkUncovered;
                placesListBookmark.texture = placesBookmarkCovered;
                thingsListBookmark.texture = thingsBookmarkCovered;

                entryListHeader.text = peopleHeaderText;
            }
            else if (curList == thingsList) {
                curList = placesList;
                peopleListBookmark.texture = peopleBookmarkCovered;
                placesListBookmark.texture = placesBookmarkUncovered;
                thingsListBookmark.texture = thingsBookmarkCovered;

                entryListHeader.text = placesHeaderText;
            }

            curListIndex = 0;
            curVisibleListIndex = 0;
            //listSelector.rectTransform.anchoredPosition = listSelectorInitialPos;
        }
    }

    public void PressRightLS() {
        if (DigestInspectionState == DigestInspectionMode.Viewing) {
            if (DigestInspectionState == DigestInspectionMode.Viewing) {
                if (curList == peopleList) {
                    curList = placesList;
                    peopleListBookmark.texture = peopleBookmarkCovered;
                    placesListBookmark.texture = placesBookmarkUncovered;
                    thingsListBookmark.texture = thingsBookmarkCovered;

                    entryListHeader.text = placesHeaderText;
                }
                else if (curList == placesList) {
                    curList = thingsList;
                    peopleListBookmark.texture = peopleBookmarkCovered;
                    placesListBookmark.texture = placesBookmarkCovered;
                    thingsListBookmark.texture = thingsBookmarkUncovered;

                    entryListHeader.text = thingsHeaderText;
                }
                else if (curList == thingsList) {
                    curList = peopleList;
                    peopleListBookmark.texture = peopleBookmarkUncovered;
                    placesListBookmark.texture = placesBookmarkCovered;
                    thingsListBookmark.texture = thingsBookmarkCovered;

                    entryListHeader.text = peopleHeaderText;
                }

                curListIndex = 0;
                curVisibleListIndex = 0;
                //listSelector.rectTransform.anchoredPosition = listSelectorInitialPos;
            }
        }
    }
}
