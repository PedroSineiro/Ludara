using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EventAction : EventBase
{
    public UnityEvent action;

    public bool fadeBefore = false;
    public bool fadeAfter = false;

    public override IEnumerator Execute(ButtonManager manager)
    {
        if (fadeBefore)
        {
            yield return manager.FadeAndTransition();
        }

        action?.Invoke();

        if (fadeAfter)
        {
            yield return manager.FadeFromBlack();
        }

        manager.NextEvent();
    }
}
