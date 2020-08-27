using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeddyLeftEye : MonoBehaviour {

    public Light leftLight;
    public bool leftEyeActive, leftEyeLock, tedTrack;
    public float leftEyeSpeed;                                                                                      // Factor controlling eye movement speed, all directions
    public float leftEyeLookSens;                                                                                   // Factor controlling eye rotation speed
    public float tiltSpeed;                                                                                         // Factor controlling eye tilt speed
    public bool sixDOF;                                                                                             // Toggle for classic six-degrees-of-freedom controls

    private Teddy ted;                                                                                              // Ted reference needed for lock mode
    private GameObject cam;                                                                                         // Is this eye camera reference needed??                        !!!
    private Rigidbody rigid;                                                                                            

	// Use this for initialization
	void Start () {
        ted = FindObjectOfType<Teddy>();
        cam = GameObject.Find("LeftEyeCam");
        //rigid = this.GetComponent<Rigidbody>();

        leftEyeActive = false;                                                                                      // Left eye starts inactive
        leftEyeLock = true;                                                                                         // Left eye starts locked (really??)
        tedTrack = false;                                                                                           // Ted lock-on initialized to off

        //rigid.velocity = Vector3.zero;
        //rigid.angularVelocity = Vector3.zero;

        sixDOF = false;                                                                                             // Non-6dof on start
        leftLight.enabled = false;                                                                                  // Eye light off on start
    }
	
	// Update is called once per frame
	void Update () {
        if (leftEyeActive) {                                                                                        // If the left eye is active...
            if (!leftEyeLock) {                                                                                     //      If view isn't locked...                                                                               
                if (!sixDOF) {
                    this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x, 
                                                                  this.transform.localEulerAngles.y, 
                                                                  0);                                           //          If not in 6dof mode, zero loc z-rot
                }
         

                float moveX = Input.GetAxis("Horizontal");                                                          //          Side movement (A-D), loc x
                float moveZ = Input.GetAxis("Vertical");                                                            //          Forward-back movement (W-S), loc z
                this.transform.Translate(moveX * leftEyeSpeed * Time.deltaTime,
                                         0, 
                                         moveZ * leftEyeSpeed * Time.deltaTime, Space.Self);                        //          Translation, * leftEyeSpeed factor

                float rotX = -Input.GetAxis("Mouse Y");                                                             //          Vert rotation, loc x, default inverted
                float rotY = Input.GetAxis("Mouse X");                                                              //          Horiz rotation, loc y
                float rotZ = -Input.GetAxis("Tilt");                                                                //          Tilt, loc z (inverted), for 6dof mode
                if (sixDOF) {                                                                                       //          If in 6dof mode...
                    this.transform.Rotate(rotX * leftEyeLookSens, 
                                          rotY * leftEyeLookSens, 
                                          rotZ * tiltSpeed);                                                        //              Rotation, * sens and tilt factors
                } else {                                                                                            //          If not in 6dof mode...
                    this.transform.Rotate(rotX * leftEyeLookSens, 0, 0);                                            //              Rotation, vertical, why separate??              !!!
                    this.transform.Rotate(0, rotY * leftEyeLookSens, 0, Space.World);                               //              Rotation, horizontal, why separate??            !!!
                }
            } else {                                                                                                //      If view is locked...
                if (tedTrack) {
                    this.transform.LookAt(ted.transform.position + new Vector3(0, 55, 0));                          //          Eye rotation follows Ted (origin + 55 y)
                }
            }

            if (Input.GetKeyDown(KeyCode.C)) {                                                                      //      Light toggle (C)
                leftLight.enabled = !leftLight.enabled;
            }
        } 
    }
}
