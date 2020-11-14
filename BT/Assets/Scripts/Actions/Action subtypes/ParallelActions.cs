using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParallelActions {
    public List<Action> actions;

    public bool active, finished;
}
