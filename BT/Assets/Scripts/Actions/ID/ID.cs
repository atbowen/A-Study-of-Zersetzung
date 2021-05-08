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
    protected bool addedToEvidencePool;
    public virtual bool AddedToEvidencePool { get => addedToEvidencePool; set => addedToEvidencePool = value; }

    [SerializeField]
    protected bool addToDigestOnSight, addToDigestOnInteraction, isPersonForDigest, isPlaceForDigest, isThingForDigest;
    public virtual bool AddToDigestOnSight { get => addToDigestOnSight; set => addToDigestOnSight = value; }
    public virtual bool AddToDigestOnInteraction { get => addToDigestOnInteraction; set => addToDigestOnInteraction = value; }

    [SerializeField]
    protected string relevantProjectName, relevantCrimeSceneName;
    public virtual string RelevantProjectName { get => relevantProjectName; set => relevantProjectName = value; }
    public virtual string RelevantCrimeSceneName { get => relevantCrimeSceneName; set => relevantCrimeSceneName = value; }

    [SerializeField]
    protected Vector3 offsetForPrisonCage;
    public Vector3 OffsetForPrisonCage { get => offsetForPrisonCage; set => offsetForPrisonCage = value; }

    public CrimeSceneObject.ObjClass evidenceClass;

    // Access info
    public bool hacked, preventActionTrigger;

    // Acquired info
    public List<RequiredPassCode> codes;
    public List<RequiredKnowledge> knownFacts;
    public List<DigestEntry> digestEntries;

    // Meshes for Momentus trajectory analysis integration
    public List<MeshRenderer> meshRends;
    public List<SkinnedMeshRenderer> skinnedMeshRends;

    // Interactable parameters
    public float maxDistanceToActivate;

    //Components
    protected Animator anim;
    protected Rigidbody rigid;
    protected Collider coll;
    protected BodyCam bCam;
    protected CameraMaster camMaster;
    protected Teddy ted;
    protected TeddyRightEye rightEye;
    protected TeddyLeftEye leftEye;
    protected Inventory tedInventory;
    protected WorkDesk wkDesk;
    protected InfoScan scanner;
    protected StatusPopup statusWindow;
    protected MailScreen mailManager;
    protected DatingScreen dateScreen;
    protected PrisonManager prisonController;
    protected ActionSceneCoordinator actCoord;
    protected ProjectHandler projHandler;
    protected Crown tedFunds;
    protected Disposal trashCan;

    protected Transform myself;

    // Use this for initialization
    void Start () {
        anim = this.GetComponentInParent<Animator>();
        rigid = this.GetComponentInParent<Rigidbody>();
        coll = this.GetComponentInParent<Collider>();
        bCam = FindObjectOfType<BodyCam>();
        camMaster = FindObjectOfType<CameraMaster>();
        ted = FindObjectOfType<Teddy>();
        rightEye = FindObjectOfType<TeddyRightEye>();
        leftEye = FindObjectOfType<TeddyLeftEye>();
        tedInventory = FindObjectOfType<Teddy>().transform.Find("Inventory").GetComponent<Inventory>();
        wkDesk = FindObjectOfType<WorkDesk>();
        scanner = FindObjectOfType<InfoScan>();
        statusWindow = FindObjectOfType<StatusPopup>();
        mailManager = FindObjectOfType<MailScreen>();
        dateScreen = FindObjectOfType<DatingScreen>();
        prisonController = FindObjectOfType<PrisonManager>();
        actCoord = FindObjectOfType<ActionSceneCoordinator>();
        projHandler = FindObjectOfType<ProjectHandler>();
        tedFunds = FindObjectOfType<Crown>();
        trashCan = FindObjectOfType<Disposal>();

        myself = this.transform;

        if (unknownField == null || unknownField == string.Empty) {
            unknownField = "???";
        }

        //if (acts.Count > 0) {
        //    foreach (Action act in acts) {                
        //        act.Initialize();                
        //    }
        //}

        preventActionTrigger = false;


        // If this is a Character and they have a Stardater profile, send the profile to the Dating Screen manager
        if (this.GetType() == typeof(IDCharacter)) {
            IDCharacter charID = (IDCharacter)this;

            if (charID.makeProfileAvailable && charID.stardaterProfile != null) {
                dateScreen.AddStardaterProfile(charID.stardaterProfile);
            }
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        
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

    public Collider GetActiveCollider() {
        if (this.GetType() == typeof(IDInteractable)) {
            IDInteractable IDThing = (IDInteractable)this;

            if (IDThing.switchColliderWithPointerColliders && IDThing.currentSwitchCollider != null) {
                return IDThing.currentSwitchCollider;
            }
            else {
                return this.transform.GetComponent<Collider>();
            }
        }
        else {
            return this.transform.GetComponent<Collider>();
        }
    }

    public float GetDistanceToActiveIDCollider(Vector3 sourcePosition) {
        Collider colliderOfID =  GetActiveCollider();

        return Vector3.Distance(colliderOfID.ClosestPoint(sourcePosition), sourcePosition);
    }

    public void ClearSwitchColliders() {
        if (this.GetType() == typeof(IDInteractable)) {
            IDInteractable IDThing = (IDInteractable)this;

            IDThing.switchColliderWithPointerColliders = false;
            IDThing.switchColliders.Clear();
        }
    }

    public Transform GetSelf() {
        return myself;
    }

    // This will "use" the ID object, whether it's picking up an item, flipping a switch, arresting somebody, arresting an object(!), opening a car door, etc.
    public abstract void Activate();

    public abstract void DisplayID();
    public abstract void DisplayID(IDCharacter charID);
}
