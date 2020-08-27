using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Confusion : MonoBehaviour {

    //public string[,] fearList;                        // [0, ] is low fear priority
                                                        // [1, ] is medium
                                                        // [2, ] is high
                                                        // [3, ] is insane

    private float confusion;
    public GameObject bCam;
    public RawImage confFill;

    private float confMax;

    // Use this for initialization
    void Start() {
        confusion = 0;

        confMax = this.GetComponent<RawImage>().uvRect.height;

        confFill.rectTransform.localScale = new Vector3(1, confusion, 1);
    }

    // Update is called once per frame
    void Update() {
        //confusion = Mathf.Min(1 / ((bCam.transform.position - dave.transform.position).magnitude + 1) * 5, confMax);
        confFill.rectTransform.localScale = new Vector3(1, confusion, 1);

    }

    void fearCheck(string[,] fearDB) {
        for (int i = 0; i < fearDB.GetLength(0) - 1; i++) {
            for (int j = 0; j < fearDB.GetLength(1) - 1; j++) {

            }
        }
    }
}
