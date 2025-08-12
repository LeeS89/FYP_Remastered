using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static EnemyAnimController;

public class EnemyEventManager : EventManager
{
    // Nav mesh agent events
    public event Action<bool> OnDestinationReached;
    //public event Action<Vector3, int> OnDestinationUpdated;  
    public event Action<bool> OnTargetSeen;
    public event Action<bool> OnRotateTowardsTarget;
    public event Action OnPathInvalid;
    //public event Action<AIDestinationRequestData> OnPathRequested;

    // Animation events
    public event Action<AnimationAction> OnAnimationTriggered;
    public event Action<float, float> OnSpeedChanged;
    public event Action<AnimationLayer, float, float, float, bool> OnChangeAnimatorLayerWeight;
    public event Action<bool> OnAimingLayerReady;
    public event Action OnDeathAnimationComplete;

    // Death/Respawn events
    public event Action<bool> OnAgentDeathComplete;
    public event Action<bool> OnAgentRespawn;

    //Patrolling events
    //public event Action<WaypointData> OnWaypointsUpdated;

   

    //public event Action<bool> OnReload;

    public event Action<bool> OnFacingTarget;

    public event Action<bool> OnMelee;
    public event Action OnMeleeAttackPerformed;

    private List<ComponentEvents> _cachedListeners;

    // Chasing Events
    public event Action<AlertStatus> OnRequestStationaryState;
   // public event Action OnRequestChasingState;
    public event Action<AIDestinationType> OnDestinationRequested;
    public event Action<bool> OnPendingNewDestination;
    public event Action<bool, bool> OnDestinationRequestStatus;

    public event Action<AlertStatus> OnAlertStatusChanged;
    public event Action OnDestinationApplied;
    public event Action<Vector3> OnRotateAtPatrolPoint;


    public event Action<bool> OnPursuitConditionChanged;

    public event Action<bool, bool> OnFieldOfViewCallback;
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

    public void FieldOfViewCallback(bool seen, bool inShootingangle)
    {
        OnFieldOfViewCallback(seen, inShootingangle);
    }

    public void PursuitConditionChanged(bool permission)
    {
        OnPursuitConditionChanged?.Invoke(permission);
    }

    /* public void WaypointsUpdated(WaypointData wpData)
     {
         OnWaypointsUpdated?.Invoke(wpData);
     }*/

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

    public void PathInvalid()
    {
        OnPathInvalid?.Invoke();
    }

   
    public void DestinationReached(bool reached)
    {
        OnDestinationReached?.Invoke(reached);
    }

   

    public void TargetSeen(bool seen)
    {
        OnTargetSeen?.Invoke(seen);
    }

    public void FacingTarget(bool facingTarget)
    {
        OnFacingTarget?.Invoke(facingTarget);
    }
  

    /// <summary>
    /// Used to switch between aiming and non-aiming layers
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="layerReady"></param>
    public void ChangeAnimatorLayerWeight(AnimationLayer layer, float from, float to, float duration, bool layerReady = false)
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

   
   /* public void Reload(bool isReloading)
    {
        OnReload?.Invoke(isReloading);
    }*/

    public void MeleeTriggered(bool isMelee)
    {
        OnMelee?.Invoke(isMelee);
    }

    public void MeleeAttackPerformed()
    {
        OnMeleeAttackPerformed?.Invoke();
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

    public void RequestStationaryState(AlertStatus status)
    {
        OnRequestStationaryState?.Invoke(status);
    }

   /* public void RequestChasingState()
    {
        OnRequestChasingState?.Invoke();
    }*/

    public void PendingNewDestination(bool pending)
    {
        OnPendingNewDestination?.Invoke(pending);
    }

    public void DestinationApplied()
    {
        OnDestinationApplied?.Invoke();
    }

    public void AlertStatusChanged(AlertStatus status)
    {
        OnAlertStatusChanged?.Invoke(status);
    }

    public void RotateAtPatrolPoint(Vector3 point)
    {
        OnRotateAtPatrolPoint?.Invoke(point);
    }

    public void DestinationRequested(AIDestinationType type)
    {
        OnDestinationRequested?.Invoke(type);
    }

    public void DestinationRequestStatus(bool complete, bool success)
    {
        OnDestinationRequestStatus?.Invoke(complete, success);
    }

    public void RotateTowardsTarget(bool rotate)
    {
        OnRotateTowardsTarget?.Invoke(rotate);
    }
}
