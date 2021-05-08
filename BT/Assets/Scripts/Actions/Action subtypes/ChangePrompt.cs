using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePrompt: Action
{
    public Prompt newOpeningLine;

    public bool forStardaterChat;

    public override void DoAction() {
        if (GameObject.Find(actorName) != null) {
            Transform actor = GameObject.Find(actorName).transform;

            if (forStardaterChat) {
                if (actor.GetComponent<IDCharacter>() != null) {
                    if (actor.GetComponent<IDCharacter>().stardaterProfile != null) {
                        actor.GetComponent<IDCharacter>().stardaterProfile.SetOpeningLine(newOpeningLine);
                    }
                }
            }
            if (actor.GetComponent<TextAndSpeech>() != null) {
                actor.GetComponent<TextAndSpeech>().openingTextLine = newOpeningLine;
            }
        }
    }
}
