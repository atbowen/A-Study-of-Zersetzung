using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EvidenceData : ScriptableObject
{
    [SerializeField]
    protected string sampleName, notes;
    public virtual string SampleName { get => sampleName; set => sampleName = value; }
    public virtual string Notes { get => notes; set => notes = value; }

    protected ProjectHandler projHandler;

    public abstract void SetProjectHandlerMode();
    public abstract void UpdateResultsAndNotes();
}
