using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fear : MonoBehaviour {

    //public string[,] fearList;                       // [0, ] is low fear priority
                                                       // [1, ] is medium
                                                       // [2, ] is high
                                                       // [3, ] is insane
    public float adrenaline, adrMax, adrReduce;
    public RawImage adrFill;

    // Use this for initialization
	void Start () {
        //fearList[0, 0] = "Dave";

        adrenaline = 0;

        adrMax = this.GetComponent<RawImage>().uvRect.height;

        adrFill.rectTransform.localScale = new Vector3(1, adrenaline, 1);
	}
	
    // Update is called once per frame
	void Update () {
        if (adrenaline > 0) {
            adrenaline = adrenaline - adrReduce * Time.deltaTime;
        }
        adrFill.rectTransform.localScale = new Vector3(1, adrenaline, 1);
    }
}
