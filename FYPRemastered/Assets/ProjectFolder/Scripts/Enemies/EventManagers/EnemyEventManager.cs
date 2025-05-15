using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    public event Action OnDeathAnimationComplete;

    // Death/Respawn events
    public event Action<bool> OnAgentDeathComplete;
    public event Action<bool> OnAgentRespawn;

    //Patrolling events
    public event Action<WaypointData> OnWaypointsUpdated;

    // Gun Events
    public event Action OnShoot;

    public event Action<bool> OnReload;

    private List<ComponentEvents> _cachedListeners;

    // Chasing Events
    public event Action OnRequestStationaryState;
    public event Action<Vector3?> OnRequestChasingState;


    /// <summary>
    /// Called From Scene Manager
    /// </summary>
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


    /// <summary>
    /// Come back to later -> Will be called on Scene Completed
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void UnbindComponentsToEvents()
    {
        throw new System.NotImplementedException();
    }


    public void WaypointsUpdated(WaypointData wpData)
    {
        OnWaypointsUpdated?.Invoke(wpData);
    }

    /// <summary>
    /// Animation actions other than Locomotion i.e. Melee, Look around etc
    /// </summary>
    /// <param name="action"></param>
    public void AnimationTriggered(AnimationAction action)
    {
        OnAnimationTriggered?.Invoke(action);
    }

    /// <summary>
    /// Updates agent move speed and animation
    /// </summary>
    /// <param name="moveSpeed"></param>
    /// <param name="lerpSpeed"></param>
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

    /// <summary>
    /// Used to switch between aiming and non-aiming layers
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="layerReady"></param>
    public void ChangeAnimatorLayerWeight(int layer, float from, float to, float duration, bool layerReady)
    {
        OnChangeAnimatorLayerWeight?.Invoke(layer, from, to, duration, layerReady);
    }

    /// <summary>
    /// Used to alert the agents weapon to be ready to fire
    /// </summary>
    /// <param name="isReady"></param>
    public void AimingLayerReady(bool isReady)
    {
        OnAimingLayerReady?.Invoke(isReady);
    }

   
    public void Reload(bool isReloading)
    {
        OnReload?.Invoke(isReloading);
    }

    public void DeathComplete(bool status)
    {
        OnAgentDeathComplete?.Invoke(status);
    }

    public void AgentRespawn(bool status)
    {
        OnAgentRespawn?.Invoke(status);
    }

    public void DeathAnimationComplete()
    {
        OnDeathAnimationComplete?.Invoke();
    }

    public void RequestStationaryState()
    {
        OnRequestStationaryState?.Invoke();
    }

    public void RequestChasingState(Vector3? destination = null)
    {
        OnRequestChasingState?.Invoke(destination);
    }
}
