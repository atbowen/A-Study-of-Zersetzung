using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AccessibilityRequirement : ScriptableObject
{
    //public string actorName;

    //public enum RequirementType { Item, Act, IDPassKey, Knowledge, CaseProgress }
    //public RequirementType Type;

    //public string itemName, IDCode, infoString, clueString;
    //public Action act;
    [SerializeField]
    protected bool canInitiateInvestigation, updatesProject;
    public virtual bool CanInitiateInvestigation { get => canInitiateInvestigation; set => canInitiateInvestigation = value; }
    public virtual bool UpdatesProject { get => updatesProject; set => updatesProject = value; }
    [SerializeField]
    protected string projectName;
    public virtual string ProjectName { get => projectName; set => projectName = value; }

    public abstract bool CheckRequirement(Transform actor);
}
