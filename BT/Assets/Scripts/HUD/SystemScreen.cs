using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemScreen : MonoBehaviour
{
    public Text bootDown, bootDownButton, channelOpen, channelOpenToggle, display, displayOptions;
    public RawImage selectorBox;
    public bool showOptions;

    private int currentOptionIndex;
    private const int numOfOptions = 3;
    private Text[] options = new Text[numOfOptions];

    // Start is called before the first frame update
    void Start()
    {
        options[0] = bootDownButton;
        options[1] = channelOpenToggle;
        options[2] = displayOptions;

        currentOptionIndex = 0;

        showOptions = false;

        ShowAllTextAndHighlights(showOptions);
    }

    // Update is called once per frame
    void Update()
    {
        ShowAllTextAndHighlights(showOptions);

        selectorBox.rectTransform.anchoredPosition = new Vector2(selectorBox.rectTransform.anchoredPosition.x, options[currentOptionIndex].rectTransform.anchoredPosition.y + 1);
    }

    private void ShowAllTextAndHighlights(bool show) {
        bootDown.enabled = show;
        bootDownButton.enabled = show;
        channelOpen.enabled = show;
        channelOpenToggle.enabled = show;
        display.enabled = show;
        displayOptions.enabled = show;
        selectorBox.enabled = show;
    }

    public void optionsListCycleUp() {
        currentOptionIndex = (currentOptionIndex + numOfOptions - 1) % numOfOptions;
    }

    public void optionsListCycleDown() {
        currentOptionIndex = (currentOptionIndex + numOfOptions + 1) % numOfOptions;
    }
}
