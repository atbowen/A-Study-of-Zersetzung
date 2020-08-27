using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Accessibility Requirement/PassCode")]
public class RequiredPassCode : AccessibilityRequirement
{
    [SerializeField]
    private Texture badge;
    public Texture Badge { get => badge; set => badge = value; }
    [SerializeField]
    private string passCodeType, passCodeName, code;
    public string PassCodeType { get => passCodeType; set => passCodeType = value; }
    public string PassCodeName { get => passCodeName; set => passCodeName = value; }
    public string Code { get => code; set => code = value; }
    [SerializeField]
    [TextArea]
    private string additionalInfo;
    public string AdditionalInfo { get => additionalInfo; set => additionalInfo = value; }

    public override bool CheckRequirement(Transform actor) {
        ID ident = actor.GetComponent<ID>();

        foreach(RequiredPassCode passkey in ident.codes) {
            if (passkey == this) { return true; }
        }

        return false;
    }
}
