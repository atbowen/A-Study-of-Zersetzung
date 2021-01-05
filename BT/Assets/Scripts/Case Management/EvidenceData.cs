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

    protected bool retrieved = false;
    public virtual bool Retrieved { get => retrieved; set => retrieved = value; }

    public abstract void DetermineMatches();
    public abstract string GetResults();
    public abstract string GetResults(int referenceIndexToShow);
    public abstract string GetNotes();
    public abstract string GetNotes(int referenceIndexToShow);
}
