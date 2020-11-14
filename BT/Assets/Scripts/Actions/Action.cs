using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This establishes a unit of ACTION, fundamentally composed of an actor, 
// changes to perform at start and finish, and a list of actions to trigger at finish.

// The unit, PARALLEL ACTION, is a list of actions to trigger simulatneously.

// The unit, ACTION SCENE, is a list of parallel actions to run, where each successive
// parallel action unit is triggered at the conclusion of the last completing current parallel action.

// Note that successive actions triggered by other actions directly using TriggerActions()
// will not be controlled by the Action Scene Coordinator
public abstract class Action : ScriptableObject {

    // Is this Action attached to the actor?
    protected bool actorIsThis;
    public virtual bool ActorIsThis { get => actorIsThis; set => actorIsThis = value; }
    
    // Name of action and actor
    [SerializeField]
    protected string actionName, actorName;
    public virtual string ActionName { get => actionName; set => actionName = value; }
    public virtual string ActorName { get => actorName; set => actorName = value; }

    // Action parameters and statuses
    [SerializeField]
    protected bool active, finished, interruptable, requiresItem, waitForRequirementsToBeMet, flashStatus, disablePhysics, disableControl, enablePhysicsAfterFinished, enableControlAfterFinished;
    public virtual bool Active { get => active; set => active = value; }
    public virtual bool Finished { get => finished; set => finished = value; }
    public virtual bool Interruptable { get => interruptable; set => interruptable = value; }
    public virtual bool RequiresItem { get => requiresItem; set => requiresItem = value; }
    public virtual bool WaitForRequirementsToBeMet { get => waitForRequirementsToBeMet; set => waitForRequirementsToBeMet = value; }
    public virtual bool FlashStatus { get => flashStatus; set => flashStatus = value; }
    public virtual bool DisablePhysics { get => disablePhysics; set => disablePhysics = value; }
    public virtual bool DisableControl { get => disableControl; set => disableControl = value; }
    public virtual bool EnablePhysicsAfterFinished { get => enablePhysicsAfterFinished; set => enablePhysicsAfterFinished = value; }
    public virtual bool EnableControlAfterFinished { get => enableControlAfterFinished; set => enableControlAfterFinished = value; }

    [SerializeField]
    protected float startTime, duration;
    public virtual float StartTime { get => startTime; set => startTime = value; }
    public virtual float Duration { get => duration; set => duration = value; }

    [SerializeField]
    [TextArea]
    protected string description, statusMessageToFlash;
    public virtual string Description { get => description; set => description = value; }
    public virtual string StatusMessageToFlash { get => statusMessageToFlash; set => statusMessageToFlash = value; }

    //[SerializeField]
    //protected List<AccessibilityRequirement> requirements;
    //public virtual List<AccessibilityRequirement> Requirements {
    //    get {
    //        return requirements;
    //    }
    //    set {
    //        foreach(AccessibilityRequirement req in value) {
    //            requirements.Add(req);
    //        }
    //    }
    //}

    [SerializeField]
    public AudioClip useSound;

    [SerializeField]
    public List<AccessibilityRequirement> requirements;

    // Does the action trigger other actions?
    protected bool triggersActions;
    public virtual bool TriggersActions { get => triggersActions; set => triggersActions = value; }
    protected ParallelActions triggeredActionsAtConclusion;
    public virtual ParallelActions TriggeredActionsAtConclusion { get => triggeredActionsAtConclusion; set => triggeredActionsAtConclusion = value; }

    // This is the actual action, to be specified by each inherited classes
    public abstract void DoAction();

    public void TriggerOtherActionsUponCompletion() {
        if (triggersActions && triggeredActionsAtConclusion.actions.Count > 0) {
            foreach (Action act in triggeredActionsAtConclusion.actions) {
                
            }
        }
    }
}