using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSceneCoordinator : MonoBehaviour
{
    private List<ActionRoutine> routines;

    private List<ActionScene> scenes = new List<ActionScene>();
    private List<Action> singleActs = new List<Action>();

    // If waitForRequirementsToBeMet = true for an action, it is added to the contingentActions list; actions in this list will be rechecked every frame
    private List<Action> contingentActions = new List<Action>();

    private StatusPopup statusMsg;

    private bool animationHold, freezeTedsBody, freezeRightEye, freezeTime;

    private float animationTimer, animationCurrentTime;

    // Start is called before the first frame update
    void Start()
    {
        statusMsg = FindObjectOfType<StatusPopup>();        

        animationHold = false;
    }

    // Update is called once per frame
    void Update()
    {


        // This manages individual actions
        if (singleActs.Count > 0) {
            for (int i = 0; i < singleActs.Count; i++) {
                if (Time.time - singleActs[i].StartTime > singleActs[i].Duration) {
                    //singleActs[i].act.FinalizeAction();
                    singleActs[i].Active = false;
                    GameObject.Find(singleActs[i].ActorName).transform.Find("ID").GetComponent<ID>().preventActionTrigger = false;
                    singleActs.Remove(singleActs[i]);
                }
            }
        }

        // This manages the action scenes -- the scene is added to scenes, the first parallel action set in the scene is run in StartActionScene(), and this loop continues it
        if (scenes.Count > 0) {
            for (int i = 0; i < scenes.Count; i++) {
                ActionScene scene = scenes[i];

                if (scene.parallelActs.Count > 0 && scene.active) {

                    ParallelActions currentParallelActions = scene.activeParallelActs;

                    for (int j = 0; j < currentParallelActions.actions.Count; j++) {
                        Action act = currentParallelActions.actions[j];

                        if (Time.time - act.StartTime > act.Duration) {
                            act.Active = false;
                            act.Finished = true;
                        }
                    }

                    if (AreParallelActionsAllDone(currentParallelActions)) {
                        int index = scene.parallelActs.IndexOf(currentParallelActions);

                        if (index < scene.parallelActs.Count - 1) {
                            currentParallelActions = scene.parallelActs[index + 1];
                            RunAllParallelActions(currentParallelActions);
                        }
                        else {
                            scene.active = false;
                            scenes.Remove(scene);
                        }
                    }
                }
            }
        }

        if (contingentActions.Count > 0) {
            foreach (Action act in contingentActions) {
                if (RequiredItemsCheckForSingleAction(act)) {
                    TriggerAction(act);
                    contingentActions.Remove(act);
                }
            }
        }
    }

    // METHODS
    
    public void TriggerParallelActions(ParallelActions parActions) {
        if (parActions.actions.Count > 0) {
            if (RequiredItemsCheckForParallelActions(parActions)) {
                foreach (Action act in parActions.actions) {
                    SearchAndDoAction(act);
                }
            }
        }
    }

    public void TriggerRoutine(ActionRoutine routine) {
        if (routine.cues.Count > 0) {

        }
    }

    // This starts a simple action--one actor, one action
    public void TriggerAction(Action singleAction) {
        if (GameObject.Find(singleAction.ActorName) != null) {
            Transform actionActor = GameObject.Find(singleAction.ActorName).transform;
            if (actionActor.Find("ID").GetComponent<ID>() != null) {
                if (RequiredItemsCheckForSingleAction(singleAction)) {
                    SearchAndDoAction(singleAction);
                }
            }
        }
    }

    // This adds the action scene to the pool of action scenes running in the update loop
    public void StartActionScene(ActionScene actScene) {

        actScene.active = true;

        // Reset all actions in scene to active = false, finished = false;
        foreach (ParallelActions parActs in actScene.parallelActs) {
            foreach(Action act in parActs.actions) {
                act.Active = false;
                act.Finished = false;
            }
        }

        // Set first set of parallel actions in scene to active/current and run it
        actScene.activeParallelActs = actScene.parallelActs[0];
        RunAllParallelActions(actScene.activeParallelActs);

        scenes.Add(actScene);
    }
    
    private void SearchAndDoAction(Action act) {
        act.DoAction();
        act.Active = true;
        act.StartTime = Time.time;

        //singleActs.Add(act);
        if (act.FlashStatus) { statusMsg.FlashStatusText(act.StatusMessageToFlash); }
    }

    private bool RequiredItemsCheckForParallelActions(ParallelActions parActs) {
        foreach (Action act in parActs.actions) {
            if (!RequiredItemsCheckForSingleAction(act)) { return false; }
        }
        return true;
    }

    private bool RequiredItemsCheckForSingleAction(Action act) {
        if (act.requirements.Count > 0) {
            Transform actor = GameObject.Find(act.ActorName).transform;
            Inventory inv = actor.Find("Inventory").GetComponent<Inventory>();
            ID ident = actor.Find("ID").GetComponent<ID>();
            foreach (AccessibilityRequirement req in act.requirements) {
                if (!req.CheckRequirement(actor)) {
                    return false;
                }
                //switch (req.Type) {
                //    case AccessibilityRequirement.RequirementType.Item:
                //        bool itemFound = false;
                //        foreach (Transform child in inv.transform) {
                //            if (child.name == req.itemName) { itemFound = true; }
                //        }
                //        if (!itemFound) { return false; }
                //        break;
                //    case AccessibilityRequirement.RequirementType.IDPassKey:
                //        bool codeFound = false;
                //        foreach (PassCode accCode in ident.accessCodes) {
                //            if (accCode.code == req.IDCode) { codeFound = true; }
                //        }
                //        if (!codeFound) { return false; }
                //        break;
                //    case AccessibilityRequirement.RequirementType.Knowledge:
                //        bool factFound = false;
                //        foreach (HelpfulKnowledge fact in ident.knownFacts) {
                //            if (fact.knowledgeSummary == req.infoString) { factFound = true; }
                //        }
                //        if (!factFound) { return false; }
                //        break;
                //}
            }
        }
        return true;
    }

    // This returns true if all of the actions running in parallel in the particular miniscene are inactive/completed, returns false if any are active/running
    private bool AreParallelActionsAllDone(ParallelActions parallelActs) {
        bool isActive = false;

        if (parallelActs.actions.Count > 0) {
            for (int i = 0; i < parallelActs.actions.Count; i++) {
                if (parallelActs.actions[i].Active) { isActive = true; }
            }
        }

        return !isActive;
    }

    // Sets all of the parallel actions in a miniscene to active and runs them
    private void RunAllParallelActions(ParallelActions parallelActs) {
        for (int i = 0; i < parallelActs.actions.Count; i++) {
            TriggerAction(parallelActs.actions[i]);
        }
    }

    // OBSOLETE
    // Check for required items by cross-checking item names; for each req item, if it finds a matching name, it's a match; if for any req item
    // it doesn't find a matching name, the action isn't completed
    // Action is initiated, set to active, timer starts, action is added to single action queue, status msg is flashed if applicable
    //private bool RequiredItemsCheckForSingleAction(Action act) {
    //    bool allRequiredItemsFound = true;

    //    if (act.requiresItem && act.requiredItems.Count > 0 && GameObject.Find(act.actorName).transform.Find("Inventory") != null) {
    //        Inventory inv = GameObject.Find(act.actorName).transform.Find("Inventory").GetComponent<Inventory>();

    //        for (int j = 0; j < act.requiredItems.Count; j++) {
    //            bool currentItemHasMatch = false;

    //            for (int k = 0; k < inv.items.Count; k++) {
    //                if (inv.items[k].name == act.requiredItems[j]) { currentItemHasMatch = true; }
    //            }

    //            if (!currentItemHasMatch) { allRequiredItemsFound = false; }
    //        }
    //    }

    //    return allRequiredItemsFound;
    //}
}
