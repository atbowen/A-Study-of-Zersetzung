using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Animated Voice Line")]
public class AnimatedVoiceLine : Action
{
    [SerializeField]
    private AnimationAudio animatedLine;
    public AnimationAudio AnimatedLine { get => animatedLine; set => animatedLine = value; }

    public List<Action> triggeredActions;

    public override void DoAction() {
        if (GameObject.Find(actorName) != null) {

            Transform actorPerson = GameObject.Find(actorName).transform;

            if (actorPerson.Find("TextAndSpeech") != null) {
                if (actorPerson.Find("TextAndSpeech").GetComponent<TextAndSpeech>() != null) {
                    actorPerson.Find("TextAndSpeech").GetComponent<TextAndSpeech>().PlayClipAndStartAnimatingFace(animatedLine);
                }
            }

            if (triggeredActions.Count > 0) {
                foreach (Action act in triggeredActions) {
                    act.DoAction();
                }
            }
        }
    }
}
