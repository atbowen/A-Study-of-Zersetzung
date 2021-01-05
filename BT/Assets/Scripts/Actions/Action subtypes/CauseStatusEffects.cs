using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Cause Status Effects")]
public class CauseStatusEffects : Action
{
    public bool increasesSapLevel;

    [Range(0.0f, 100.0f)]
    public float sapLevelIncrease;

    public override void DoAction() {
        if (increasesSapLevel) { FindObjectOfType<SapPoisonLevel>().TakeHit(sapLevelIncrease); }
    }
}
