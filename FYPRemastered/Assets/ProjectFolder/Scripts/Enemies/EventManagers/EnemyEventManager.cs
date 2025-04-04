using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEventManager : EventManager
{
    // Nav mesh agent events
    public event Action<bool> OnDestinationReached;
    public event Action<Vector3, int> OnDestinationUpdated;  
    public event Action<bool> OnPlayerSeen;

    // Animation events
    public event Action<AnimationAction> OnAnimationTriggered;
    public event Action<float, float> OnSpeedChanged;
    public event Action<int, float, float, float, bool> OnChangeAnimatorLayerWeight;
    public event Action<bool> OnAimingLayerReady;

    // Gun Events
    public event Action OnShoot;

    public event Action<bool> OnReload;

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

    public void DestinationReached(bool reached)
    {
        OnDestinationReached?.Invoke(reached);
    }

    public void DestinationUpdated(Vector3 newDestination, int stoppingDistance = 0)
    {
        OnDestinationUpdated?.Invoke(newDestination, stoppingDistance);
    }

    public void PlayerSeen(bool seen)
    {
        OnPlayerSeen?.Invoke(seen);
    }

    public void Shoot()
    {
        OnShoot?.Invoke();
    }

    public void ChangeAnimatorLayerWeight(int layer, float from, float to, float duration, bool layerReady)
    {
        OnChangeAnimatorLayerWeight?.Invoke(layer, from, to, duration, layerReady);
    }

    public void AimingLayerReady(bool isReady)
    {
        OnAimingLayerReady?.Invoke(isReady);
    }

   
    public void Reload(bool isReloading)
    {
        OnReload?.Invoke(isReloading);
    }
}
