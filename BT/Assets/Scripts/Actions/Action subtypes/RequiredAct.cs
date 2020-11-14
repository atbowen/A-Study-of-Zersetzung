using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequiredAct : AccessibilityRequirement
{
    [SerializeField]
    private Action act;
    public Action Act { get => act; set => act = value; }

    public override bool CheckRequirement(Transform actor) {
        if (act.Finished)   { return true; }
        else                { return false; }
    }
}
