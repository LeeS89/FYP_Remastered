using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEventManager : EventManager
{
    public event Action<AnimationAction> OnAnimationTriggered;
    public event Action<float, float> OnSpeedChanged;


    private List<ComponentEvents> _cachedListeners;

    public override void BindComponentsToEvents()
    {
        _cachedListeners = new List<ComponentEvents>();

        var childListeners = GetComponentsInChildren<ComponentEvents>(true);
        _cachedListeners.AddRange(childListeners);

        foreach (var listener in _cachedListeners)
        {
            listener.RegisterLocalEvents(this);
        }
    }

    public override void UnbindComponentsToEvents()
    {
        throw new System.NotImplementedException();
    }


    public void AnimationTriggered(AnimationAction action)
    {
        OnAnimationTriggered?.Invoke(action);
    }

    public void SpeedChanged(float moveSpeed, float lerpSpeed)
    {
        OnSpeedChanged?.Invoke(moveSpeed, lerpSpeed);
    }

    
}
