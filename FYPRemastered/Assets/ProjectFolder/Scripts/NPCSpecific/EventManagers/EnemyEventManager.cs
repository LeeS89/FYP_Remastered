using System;

using UnityEngine;

[Obsolete]
public class EnemyEventManager : EventManager
{
    #region To be made obsolete + re enable later
      public Action<AnimationCue> OnAnimationTriggered;
    /// <summary>
    /// Animation actions other than Locomotion i.e. Melee, Look around etc
    /// </summary>
    /// <param name="action"></param>
     public void TriggerAnimation(AnimationCue action) => OnAnimationTriggered?.Invoke(action);

    ////// NEW TODAY (DECEMBER 6th)
      public Action<AnimationLayer, bool, Action> OnTogglingAnimationLayerNew;
    public void TogglingAnimationLayerNew(AnimationLayer layer, bool activate, Action onComplete = null)
        => OnTogglingAnimationLayerNew?.Invoke(layer, activate, onComplete);
    ///// END DECEMBER 6th

     public Func<AnimationLayer, bool> OnGetLayerActiveState;

     public bool IsLayerActive(AnimationLayer layer) => OnGetLayerActiveState?.Invoke(layer) ?? true;

      public Action<Transform> OnSetLookTarget;
      public void SetLookTarget(Transform target) => OnSetLookTarget?.Invoke(target);

     public Action<bool> OnAimTowardsTarget;
      public void AimAtTarget(bool aim) => OnAimTowardsTarget?.Invoke(aim);

    public Action<Vector3, Vector3> OnTickAnimator;

    public void TickAnimator(Vector3 velocity, Vector3 forward) => OnTickAnimator?.Invoke(velocity, forward);
    #endregion


    // Nav mesh agent events
    public event Action<bool> OnDestinationReached;
    //public event Action<Vector3, int> OnDestinationUpdated;  
    public Action<bool> OnTargetSeen;
    
    public event Action<bool> OnRotateTowardsTarget;
    public event Action OnPathInvalid;
    //public event Action<AIDestinationRequestData> OnPathRequested;

    // Animation events
    
    public event Action<float, float> OnSpeedChanged;
    public Action<AnimationLayer, float, float, float, bool> OnChangeAnimatorLayerWeight;
    public Action<AnimationLayer, Action<AnimationLayer>> OnToggleAnimationLayer;
    
    public event Action<bool> OnAimingLayerReady;
    public event Action OnDeathAnimationComplete;

    // Death/Respawn events
    public event Action<bool> OnAgentDeathComplete;
    public event Action OnAgentRespawn;

    //Patrolling events
    //public event Action<WaypointData> OnWaypointsUpdated;

   

    //public event Action<bool> OnReload;

    public event Action<bool> OnFacingTarget;

    public event Action<bool> OnMelee;
    public event Action OnMeleeAttackPerformed;

  //  private List<ComponentEvents> _cachedListeners;

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
    /*public override void BindComponentsToEvents()
    {
        _cachedListeners = new List<ComponentEvents>();

        var childListeners = GetComponentsInChildren<ComponentEvents>(true);
        _cachedListeners.AddRange(childListeners);

        foreach (var listener in _cachedListeners)
        {
            listener.RegisterLocalEvents(this);
        }
    }*/


    /// <summary>
    /// Come back to later -> Will be called on Scene Completed
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
   /* public override void UnbindComponentsToEvents()
    {
        throw new System.NotImplementedException();
    }*/

    public void FieldOfViewCallback(bool seen, bool inShootingangle) // Obsolete
    {
        OnFieldOfViewCallback(seen, inShootingangle);
    }

    public void PursuitConditionChanged(bool permission) // Obsolete
    {
        OnPursuitConditionChanged?.Invoke(permission);
    }


  


    /// <summary>
    /// Updates agent move speed and animation
    /// </summary>
    /// <param name="moveSpeed"></param>
    /// <param name="lerpSpeed"></param>
    public void SpeedChanged(float moveSpeed, float lerpSpeed) => OnSpeedChanged?.Invoke(moveSpeed, lerpSpeed); // Obsolete


    public void PathInvalid() // Obsolete
    {
        OnPathInvalid?.Invoke();
    }

   
    public void DestinationReached(bool reached) // Obsolete
    {
        OnDestinationReached?.Invoke(reached);
    }

   

    public void TargetSeen(bool seen) => OnTargetSeen?.Invoke(seen); // Obsolete

  

   


    public void FacingTarget(bool facingTarget) // Obsolete
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
        => OnChangeAnimatorLayerWeight?.Invoke(layer, from, to, duration, layerReady);


    public void ToggleAnimationLayer(AnimationLayer layer, Action<AnimationLayer> onLayerToggled)
        => OnToggleAnimationLayer?.Invoke(layer, onLayerToggled);


    public Action<AnimationLayer, Action> OnTogglingAnimationLayer;
    public void TogglingAnimationLayer(AnimationLayer layer, Action onComplete = null)
        => OnTogglingAnimationLayer?.Invoke(layer, onComplete);

 

   

    /// <summary>
    /// Used to alert the agents weapon to be ready to fire
    /// </summary>
    /// <param name="isReady"></param>
    public void AimingLayerReady(bool isReady) // Obsolete
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

    public void AgentRespawn()
    {
        OnAgentRespawn?.Invoke();
    }

    public void DeathAnimationComplete()
    {
        OnDeathAnimationComplete?.Invoke();
    }

    public void RequestStationaryState(AlertStatus status) // Obsolete
    {
        OnRequestStationaryState?.Invoke(status);
    }


    public void DestinationApplied() // Obsolete
    {
        OnDestinationApplied?.Invoke();
    }

    public void AlertStatusChanged(AlertStatus status)// Obsolete
    {
        OnAlertStatusChanged?.Invoke(status);
    }

    public void RotateAtPatrolPoint(Vector3 point) // Obsolete
    {
        OnRotateAtPatrolPoint?.Invoke(point);
    }

    public void DestinationRequested(AIDestinationType type) // Obsolete
    {
        OnDestinationRequested?.Invoke(type);
    }

    public void DestinationRequestStatus(bool complete, bool success) // Obsolete
    {
        OnDestinationRequestStatus?.Invoke(complete, success);
    }

    public void RotateTowardsTarget(bool rotate)
    {
        OnRotateTowardsTarget?.Invoke(rotate);
    }

    public event Action<bool> OnReloading;

    public void Reloading(bool isReloading) => OnReloading?.Invoke(isReloading);

    public event Action<AnimationCue> OnReload;

    public void Reload(AnimationCue cue) => OnReload?.Invoke(cue);

    public event Action<AnimationCue> OnProcessAnimCue;
    public void SendAnimCue(AnimationCue cue) => OnProcessAnimCue?.Invoke(cue);


    /// NEW FSM SETUP with NPC controller

    public Func<State, Transform, AlertStatus, StateChangeResult> OnChangeState;

    public StateChangeResult ChangeState(State newState, Transform targetPos = null, AlertStatus status = AlertStatus.None) => OnChangeState?.Invoke(newState, targetPos, status) ?? StateChangeResult.Success;

    public Action<Transform> OnUpdateChaseTarget;

    public void UpdateChaseTarget(Transform target) => OnUpdateChaseTarget?.Invoke(target);


   
}














