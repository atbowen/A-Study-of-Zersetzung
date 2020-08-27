using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DatingScreen : MonoBehaviour
{
    public PanelsBottom commsPanel;

    private RawImage dateScreen;
    
    // Start is called before the first frame update
    void Start()
    {
        dateScreen = this.GetComponent<RawImage>();
        commsPanel = commsPanel.GetComponent<PanelsBottom>();

        EnableScreen(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableScreen(bool enableYorN) {
        dateScreen.enabled = enableYorN;
        //dateScreen.rectTransform.anchoredPosition = commsPanel.panelTop.rectTransform.anchoredPosition + new Vector2(0, -20);
    }
}
