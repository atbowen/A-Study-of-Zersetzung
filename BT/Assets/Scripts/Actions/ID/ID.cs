using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class ID : MonoBehaviour {

    // Identifiers
    [SerializeField]
    protected string objName, objActualName, objAvatarName, objDescription, objStatus;
    public virtual string ObjName { get => objName; set => objName = value; }
    public virtual string ObjActualName { get => objActualName; set => objActualName = value; }
    public virtual string ObjAvatarName { get => objAvatarName; set => objAvatarName = value; }
    public virtual string ObjDescription { get => objDescription; set => objDescription = value; }
    public virtual string ObjStatus { get => objStatus; set => objStatus = value; }

    [SerializeField]
    protected string unknownField;
    public virtual string UnknownField { get => unknownField; set => unknownField = value; }

    [SerializeField]
    protected bool knowName, knowActName, knowAvatarName, knowDescription, knowStatus;
    public virtual bool KnowName { get => knowName; set => knowName = value; }
    public virtual bool KnowActName { get => knowActName; set => knowActName = value; }
    public virtual bool KnowAvatarName { get => knowAvatarName; set => knowAvatarName = value; }
    public virtual bool KnowDescription { get => knowDescription; set => knowDescription = value; }
    public virtual bool KnowStatus { get => knowStatus; set => knowStatus = value; }

    [SerializeField]
    protected bool arrestable, addedToEvidencePool;
    public virtual bool Arrestable { get => arrestable; set => arrestable = value; }
    public virtual bool AddedToEvidencePool { get => addedToEvidencePool; set => addedToEvidencePool = value; }

    [SerializeField]
    protected string relevantProjectName, relevantCrimeSceneName;
    public virtual string RelevantProjectName { get => relevantProjectName; set => relevantProjectName = value; }
    public virtual string RelevantCrimeSceneName { get => relevantCrimeSceneName; set => relevantCrimeSceneName = value; }

    public CrimeSceneObject.ObjClass evidenceClass;

    // Access info
    public bool hacked, preventActionTrigger;

    // Acquired info
    public List<RequiredPassCode> codes;
    public List<RequiredKnowledge> knownFacts;

    // State info
    public enum IdentificationType { Character, Date, Vehicle, Item, Money, Interactable, Evidence};
    [SerializeField]
    public IdentificationType IDType;

    // Interactable parameters
    public Collider interactionCollider;
    public string triggerStringToActivate, triggerStringToDeactivate;
    public bool isThereALightToTurnOn, isThereAnOnOffLightOnTheActivationSwitch, ActivationSwitchLightOnOffInversion;
    public Transform objectWithLightSwitch, vehicleEntrance;
    public Light activationPointSwitchLightOnOff;
    public float maxDistanceToActivate;

    //public List<ActionTrigger> actionTrigs;
    [SerializeField]
    public List<Action> acts;

    //Components
    protected Animator anim;
    protected Rigidbody rigid;
    protected Collider coll;
    protected BodyCam bCam;
    protected CameraMaster camMaster;
    protected TeddyHead ted;
    protected TeddyRightEye rightEye;
    protected TeddyLeftEye leftEye;
    protected Inventory tedInventory;
    protected WorkDesk wkDesk;
    protected StatusPopup statusWindow;
    protected PrisonManager prisonController;
    protected ActionSceneCoordinator actCoord;
    protected ProjectHandler projHandler;
    protected Crown tedFunds;
    protected Disposal trashCan;

    protected Transform myself;

    private bool buttonPressed, isOn;

    // Use this for initialization
    void Start () {
        anim = this.GetComponentInParent<Animator>();
        rigid = this.GetComponentInParent<Rigidbody>();
        coll = this.GetComponentInParent<Collider>();
        bCam = FindObjectOfType<BodyCam>();
        camMaster = FindObjectOfType<CameraMaster>();
        ted = FindObjectOfType<TeddyHead>();
        rightEye = FindObjectOfType<TeddyRightEye>();
        leftEye = FindObjectOfType<TeddyLeftEye>();
        tedInventory = FindObjectOfType<Teddy>().transform.Find("Inventory").GetComponent<Inventory>();
        wkDesk = FindObjectOfType<WorkDesk>();
        statusWindow = FindObjectOfType<StatusPopup>();
        prisonController = FindObjectOfType<PrisonManager>();
        actCoord = FindObjectOfType<ActionSceneCoordinator>();
        projHandler = FindObjectOfType<ProjectHandler>();
        tedFunds = FindObjectOfType<Crown>();
        trashCan = FindObjectOfType<Disposal>();

        myself = this.transform.parent;

        buttonPressed = false;

        if (unknownField == null || unknownField == string.Empty) {
            unknownField = "???";
        }

        //if (acts.Count > 0) {
        //    foreach (Action act in acts) {                
        //        act.Initialize();                
        //    }
        //}

        preventActionTrigger = false;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        
    }

    public void TriggerInteraction() {
        if (buttonPressed) {
            anim = this.transform.parent.GetComponent<Animator>();
            isOn = !isOn;

            if (!isOn) {
                anim.SetTrigger(triggerStringToDeactivate);
                if (isThereALightToTurnOn && objectWithLightSwitch != null) {
                    objectWithLightSwitch.GetComponent<SwitchLight>().lightOn = false;
                }
                if (isThereAnOnOffLightOnTheActivationSwitch && activationPointSwitchLightOnOff != null) {
                    if (!ActivationSwitchLightOnOffInversion) {
                        activationPointSwitchLightOnOff.intensity = 0;
                    }
                    else {
                        activationPointSwitchLightOnOff.intensity = 1;
                    }
                }
            }
            else {
                anim.SetTrigger(triggerStringToActivate);
                if (isThereALightToTurnOn && objectWithLightSwitch != null) {
                    objectWithLightSwitch.GetComponent<SwitchLight>().lightOn = true;
                }
                if (isThereAnOnOffLightOnTheActivationSwitch && activationPointSwitchLightOnOff != null) {
                    if (!ActivationSwitchLightOnOffInversion) {
                        activationPointSwitchLightOnOff.intensity = 1;
                    }
                    else {
                        activationPointSwitchLightOnOff.intensity = 0;
                    }
                }
            }

            buttonPressed = false;
        }
    }


    public void AddToEvidencePool() {
        if (!addedToEvidencePool && relevantProjectName != "" && relevantCrimeSceneName != "") {
            projHandler.ImportCrimeSceneElementIntoPool(myself, evidenceClass, relevantCrimeSceneName);
            addedToEvidencePool = true;
        }
    }

    public bool HasBeenAddedToEvidencePool() {
        return addedToEvidencePool;
    }

    // This will "use" the ID object, whether it's picking up an item, flipping a switch, arresting somebody, arresting an object(!), opening a car door, etc.
    public abstract void Activate();
}
