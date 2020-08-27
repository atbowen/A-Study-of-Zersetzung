using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Accessibility Requirement/Knowledge")]
public class RequiredKnowledge : AccessibilityRequirement
{
    [SerializeField]
    private string knowledgeSummary;
    public string KnowledgeSummary { get => knowledgeSummary; set => knowledgeSummary = value; }
    [TextArea]
    [SerializeField]
    private string knowledgeDetail;
    public string KnowledgeDetail { get => knowledgeDetail; set => knowledgeDetail = value; }

    public List<string> evidenceNames, trajRefSetNames, NMRRefResultsNames;

    public override bool CheckRequirement(Transform actor) {
        ID ident = actor.Find("ID").GetComponent<ID>();
        foreach(RequiredKnowledge fact in ident.knownFacts) {
            if (fact == this)   { return true; }
        }

        return false;
    }
}
