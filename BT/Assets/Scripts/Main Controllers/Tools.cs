using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tools : MonoBehaviour {

    public GameObject[] laser;
    public RawImage resultBgd;
    public Text result;

    private enum ToolChoice { None, Trajectory, Sample, ImagAn, Shield, Security };

    private enum Traj { Limbo, Initialized, OriginSet };
    private enum Shld { None, Initialized, firstEdgeSet, secondEdgeSet };
    private enum Smpl { None, Aiming, Collecting, Analyzing };
    private enum ImgAn { None, Aiming, Selecting, Viewing, Analyzing };
    private enum Hax { None, Aiming, Interfacing, Typing, HackedOptions };
    
    private ToolChoice tool;

    private Traj laserState;
    private Shld shieldState;
    private Smpl sampleState;
    private ImgAn imageAnalysisState;
    private Hax securityState;

    public AudioClip trajToolSound, fluidToolSound, ImagAnSound,
                shieldSound, hackSound, cancelSound,
                pointSetSound, completeSound, backupSound;

    private BodyCam bc;
    private CameraMaster camControl;
    private AudioSource audioFX;
    private string[] textColor;

    private Vector3[] origin, endpoint, laserLengthRef, laserDir;
    private int laserNumber, laserNumberTotal;
    private bool laserPending;
    private float[] laserLength;

    // Use this for initialization
    void Start () {
        bc = FindObjectOfType<BodyCam>();
        audioFX = this.GetComponent<AudioSource>();
        camControl = FindObjectOfType<CameraMaster>();

        laserNumberTotal = laser.Length;

        origin = new Vector3[laserNumberTotal];
        endpoint = new Vector3[laserNumberTotal];
        laserLengthRef = new Vector3[laserNumberTotal];
        laserDir = new Vector3[laserNumberTotal];
        laserLength = new float[laserNumberTotal];
        textColor = new string[laserNumberTotal];

        resultBgd.enabled = false;
        result.enabled = false;

        for (int i = 0; i < laserNumberTotal; i++) {
            origin[i] = Vector3.zero;
            endpoint[i] = Vector3.zero;
            laserLength[i] = 0;
        }

        foreach (GameObject las in laser) {
            las.GetComponent<MeshRenderer>().enabled = false;
            las.transform.parent = null;
        }

        //textColor[0] = "07FFFAFF";
        //textColor[1] = "FFC21CFF";
        //textColor[2] = "FF05D5FF";
        //textColor[3] = "0905FFFF";

        laserNumber = 0;
        laserPending = false;

        tool = ToolChoice.None;

        laserState = Traj.Limbo;
    }

    // Update is called once per frame
    void Update() {

        if (!camControl.commsEnabled) {
            if (Input.GetKeyDown(KeyCode.R)) {
                resultBgd.enabled = false;
                result.enabled = false;
                //foreach (GameObject clone in cloneLaser) {                                                    // For some reason, this code allows the Traj code to continue to function even after pressing r
                //    clone.GetComponent<MeshRenderer>().enabled = false;
                //}
                tool = ToolChoice.None;
            } else if (Input.GetKeyDown(KeyCode.T)) {
                laserNumber = 0;
                result.text = "Pending measurement...\n\n";
                result.enabled = true;
                resultBgd.enabled = true;
                audioFX.clip = trajToolSound;
                audioFX.Play();
                tool = ToolChoice.Trajectory;
                laserState = Traj.Initialized;
                laserPending = false;
                foreach (GameObject las in laser) {
                    las.GetComponent<MeshRenderer>().enabled = false;
                }
            } else if (Input.GetKeyDown(KeyCode.Y)) {
                result.text = "Collect sample.";
                result.enabled = true;
                resultBgd.enabled = true;
                audioFX.clip = fluidToolSound;
                audioFX.Play();
                tool = ToolChoice.Sample;
            } else if (Input.GetKeyDown(KeyCode.P)) {
                result.text = "Snap picture or search library.";
                result.enabled = true;
                resultBgd.enabled = true;
                audioFX.clip = ImagAnSound;
                audioFX.Play();
                tool = ToolChoice.ImagAn;
            } else if (Input.GetKeyDown(KeyCode.O)) { //leftEye.leftEyeActive && !leftEye.leftEyeLock && 
                result.text = "jlh"; // +---++XLXCtools++v.136++//n//naccount ending in ***6773 will be charged";
                result.enabled = true;
                resultBgd.enabled = true;
                //audioFX.clip = shieldSound;
                //audioFX.Play();
                //leftEyeTool = LeftEyeTools.Shield;
            } else if (Input.GetKeyDown(KeyCode.H)) { //leftEye.leftEyeActive && !leftEye.leftEyeLock && 
                result.text = "xlxc";// +XLXCTOOLS++ver136.2+++//n//nAccount will be charged. interface closed circuit";
                result.enabled = true;
                resultBgd.enabled = true;
                //audioFX.clip = hackSound;
                //audioFX.Play();
                //leftEyeTool = LeftEyeTools.Hack;
            }



            switch(tool) {
                case ToolChoice.None:

                    result.enabled = false;
                    break;

                // TRAJECTORY tool
                case ToolChoice.Trajectory:

                    switch(laserState) {                                                                     // This is when max number of trajectories have been created
                        case Traj.Limbo:
                            if (Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.LeftShift)) {
                                result.text = "Pending measurement...\n\n";
                                for (int i = 0; i <= (laserNumber - 2); i++) {
                                    result.text = result.text + "Trajectory = " + Mathf.Abs(laserLength[i]).ToString("F2") + ", " +
                                                        Vector3.Angle(laserDir[i], laserLengthRef[i]).ToString("F1") + " deg\n";
                                }
                                laserNumber--;
                                audioFX.clip = backupSound;
                                audioFX.Play();
                                laserState = Traj.OriginSet;
                            }
                            break;
                        case Traj.Initialized:
                            RaycastHit hit;
                            if (Physics.Raycast(bc.transform.position, bc.transform.forward, out hit, 25f)) {
                            origin[laserNumber] = hit.point;
                            } else {
                                origin[laserNumber] = bc.transform.forward * 25f + bc.transform.position;
                            }
                            if (laserNumber < laserNumberTotal && !laserPending) {
                                laser[laserNumber].GetComponent<MeshRenderer>().enabled = true;
                                laserPending = true;
                            }
                            laser[laserNumber].transform.position = origin[laserNumber];
                            laser[laserNumber].transform.rotation = Quaternion.LookRotation(bc.transform.forward);
                            laser[laserNumber].transform.localScale = new Vector3(0.4f, 0.4f, 1);

                            if (Input.GetKeyDown(KeyCode.F)) {
                                if (Input.GetKey(KeyCode.LeftShift) && laserNumber >= 1) {
                                    laser[laserNumber].GetComponent<MeshRenderer>().enabled = false;
                                    result.text = "Pending measurement...\n\n";
                                    for (int i = 0; i <= (laserNumber - 2); i++) {
                                        result.text = result.text + "Trajectory = " + Mathf.Abs(laserLength[i]).ToString("F2") + ", " +
                                                                Vector3.Angle(laserDir[i], laserLengthRef[i]).ToString("F1") + " deg\n";
                                    }
                                    laserNumber--;
                                    audioFX.clip = backupSound;
                                    audioFX.Play();
                                    laserState = Traj.OriginSet;
                                } else if (!Input.GetKey(KeyCode.LeftShift)) {
                                    audioFX.clip = pointSetSound;
                                    audioFX.Play();
                                    laserState = Traj.OriginSet;
                                }
                            }
                            break;
                        case Traj.OriginSet:
                            //RaycastHit hit;
                            if (Physics.Raycast(bc.transform.position, bc.transform.forward, out hit, 25f)) {
                                endpoint[laserNumber] = hit.point;
                            } else {
                                endpoint[laserNumber] = bc.transform.forward * 25f + bc.transform.position;
                            }
                            laserDir[laserNumber] = endpoint[laserNumber] - origin[laserNumber];
                            laserLengthRef[laserNumber] = new Vector3(laserDir[laserNumber].x, laserDir[laserNumber].y, 0);
                            laserLength[laserNumber] = laserDir[laserNumber].magnitude;
                            laser[laserNumber].transform.rotation = Quaternion.LookRotation(laserDir[laserNumber]);
                            laser[laserNumber].transform.localScale = new Vector3(0.4f, 0.4f, Mathf.Abs(laserLength[laserNumber]));

                            if (Input.GetKeyDown(KeyCode.F)) {
                                if (Input.GetKey(KeyCode.LeftShift)) {
                                    audioFX.clip = backupSound;
                                    audioFX.Play();
                                    laserState = Traj.Initialized;
                                } else {
                                    result.text = "Pending measurement...\n\n";
                                    for (int i = 0; i <= laserNumber; i++) {
                                        result.text = result.text + "Trajectory = " + Mathf.Abs(laserLength[i]).ToString("F2") + ", " +
                                                            Vector3.Angle(laserDir[i], laserLengthRef[i]).ToString("F1") + " deg\n";
                                    }
                                    laserNumber++;
                                    audioFX.clip = completeSound;
                                    audioFX.Play();
                                    laserPending = false;
                                    if (laserNumber >= laserNumberTotal) {
                                        laserState = Traj.Limbo;
                                    } else {
                                        laserState = Traj.Initialized;
                                    }
                                }
                            }
                            break;
                        
                    }
                    break;

                // SECURITY tool
                case ToolChoice.Security:

                    break;
            }
        }
    }
}
