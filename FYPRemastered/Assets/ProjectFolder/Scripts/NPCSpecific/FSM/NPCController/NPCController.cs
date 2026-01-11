using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AgentEventManager))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public partial class NPCController : TargetableInit<ISceneAIServices, AgentEventManager>, IAgentData, INPCBrainContext, INotificationListener
{ // Remove IAgentData
    
    //   private bool _isInStateTransition = false;
    protected Action _onLayerToggleComplete;
    protected ZoneId _zoneId = ZoneId.Unknown;

    // Data queried by the FSMManager 
    private ITargetable _primaryTarget;// { get; set; }
    public NavMeshAgent Agent { get; protected set; }
    public NavMeshObstacle Obstacle { get; protected set; }
    public NavMeshPath Path { get; protected set; }

    // End FSM Data


    public bool TestSprint;
    public bool TestWalk;

    [Header("Data used by the Brain component")]
    public StateId CurrentFsmState => _fsmManager?.CurrentStateId ?? StateId.None;
    public CombatOrder CurrentComOrder { get; private set; } = CombatOrder.None;
    public RotationOrder CurrentRotOrder { get; private set; } = RotationOrder.None;
    public FOVResult CurrentFovState { get; private set; } = FOVResult.None;
    private bool TargetDead() => _primaryTarget?.IsDead ?? true;
    // End Brain data
/*

    // ITargetable Data - This Gameobjects information for targeting purposes by other NPC's
    // i.e. its LayerMask, Transform, Aim Trigger, etc.
    [Header("The transform of this game object used for targeting purposes")]
    [SerializeField] protected Transform _rootTransform;
    public Transform Transform => _rootTransform != null ? _rootTransform : transform;
    public Vector3 Forward => _rootTransform != null ? _rootTransform.forward : transform.forward;*/

  /*  [Header("Mask of this Gamobeject used for targeting purposes")]
    [SerializeField] protected LayerMask _layerMask;
    public LayerMask LayerMask => _layerMask;*/
/*    [Header("Trigger area on the game object that other NPC's use as target area for aiming")]
    [SerializeField] protected Collider _targetCollider;
    public Collider TargetableCollider { get; protected set; }
    public bool IsStationary { get; private set; } = false;*/
/*
    public Vector3 Position()
     => _rootTransform == null ? transform.position : _rootTransform.position;
    public Quaternion Rotation()
        => _rootTransform == null ? transform.rotation : _rootTransform.rotation;*/
    // End ITargetable Data

    // Possibly Redundant
    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();



    [Header(@"How many consecutive FOV results required to ""See ""or ""Lose ""the target")]
    [SerializeField] private uint _requiredSeenStreak = 3;
    [SerializeField] private uint _requiredNotSeenStreak = 5;
    //  private bool _isTargetVisible = false;
    private uint _currentSeenStreak = 0;
    private uint _currentNotSeenStreak = 0;
    private Action<FOVResult> OnStableFOVResult;
    //  private Action OnTargetSeen;
    // private Action OnTargetLost;
    private bool _aimingAtTarget = false;


    // IFSMNotifications - For notifications received by the FSMManager, i.e. No valid destination, target lost, Target within melee/ shot range, etc.
    public void OnNotify(in NPCNotification n)
    {
        if (_fsmManager.IsInStateTransition /*|| n.Id != _fsmManager.CurrentStateId*/) return;

      //  if (n.Kind == NotificationKind.FOVUpdate) Debug.LogError("FOV Result: "+n.FOVResult.ToString());

        this.Decide(in n);

        /*if (!this.TryDecide(n, out var decision)) return;

        if (decision.BroadcastZoneAlert)
            TryBroadcastAlert(decision.NextIntent);

        if (decision.RotationOrder != RotationOrder.None)
            CurrentRotOrder = decision.RotationOrder;

        if (decision.CombatOrder != CombatOrder.None)
            UpdateCombatOrder(decision.CombatOrder);

        if (decision.NewFOVStatus != FOVResult.None)
            ApplyFOVStatusUpdate(decision.NewFOVStatus);

        if (decision.NextIntent != StateId.None)
            _fsmManager.SwitchTo(decision.NextIntent);
*/

    }

    private void ResetAll()
    {
        UpdateCombatOrder(CombatOrder.None);
        UpdateCurrentFovStatus(FOVResult.None);
        RotateToTarget(rotate: false);
    }

    public void TriggerDeath()
    {
        if (IsDead) return;
        ResetAll();

        OnDeath();
    }

    public bool IsRotatingToTarget() => _fsmManager?.RotatingToTarget ?? false;


    public void UpdateCombatOrder(CombatOrder order)
    {
        if (order == CurrentComOrder) return;
        CancelCurrentCombatOrder();
        CurrentComOrder = order;

        if (IsDead || TargetDead()) return;
        // Apply Order/ Start order
        if (CurrentComOrder == CombatOrder.FireAtWill)
        {
            if (!_animationControl?.IsAnimationLayerActive(AnimationLayer.Aim) ?? true)
                StartCoroutine(WaitForAnimLayerFadeRoutine(AnimationLayer.Aim, true));
            if (!_aimingAtTarget) { _aimingAtTarget = true; _animationControl?.IkLookAtTarget(look: true); }
        }
        //
    }



    private void CancelCurrentCombatOrder()
    {

    }

    private void ApplyFOVStatusUpdate(FOVResult result) => CurrentFovState = result;

    public void AnimationIntent(AnimationCue cue) => _animationControl?.PlayClip(cue);

    public void RotateToTarget(bool rotate) => _fsmManager?.RotateToTarget(rotate);
   

    // End IFSMNotificationss


    /// <summary>
    /// Maps the specified destination to a zone and updates the agent's current zone accordingly.
    /// </summary>
    /// <remarks>This method determines the zone corresponding to the given destination and updates the
    /// agent's current zone if it differs from the previously assigned zone. If no zone is found for the destination,
    /// the agent is assigned to a default zone.  The method also handles the registration and unregistration of the
    /// agent with the appropriate zone using the <see cref="SceneEventAggregator"/>. If the zone changes, the agent is
    /// unregistered from the previous zone and registered with the new one.  This method is intended to be used within
    /// the agent's internal state management and should not be called directly in most cases.</remarks>
    /// <param name="destination">The destination position in world coordinates to map to a zone.</param>
    protected void MapDestinationToZone(Vector3 destination)
    {
        ZoneId id;
        bool found = this.GetZoneId(destination, out id);
        if (found)
        {
            if (id == _zoneId || id == ZoneId.Unknown) return;
            else
            {
                SceneEventAggregator.Instance.UnregisterAgentAndZone(this, _zoneId);
                _zoneId = id;
                SceneEventAggregator.Instance.RegisterAgentAndZone(this, _zoneId);
                Debug.LogError("Zone ID on start: " + _zoneId.ToString());
                _fsmManager.OnMapDestinationToZone = null;
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("No Zone ID found on start");
#endif
            SceneEventAggregator.Instance.UnregisterAgentAndZone(this, _zoneId);
            _zoneId = ZoneId.ZoneA;
            SceneEventAggregator.Instance.RegisterAgentAndZone(this, _zoneId);
            _fsmManager.OnMapDestinationToZone = null;
        }
    }

    
    protected void DeathStatusUpdated(bool isDead)
    {
        if (IsDead == isDead) return;
        //   base.DeathStatusUpdated(isDead);


    }

    protected virtual void OnVisibilityGained(bool seen) { }

    protected virtual void OnAimEnter(bool aiming) { }

   
    protected void Engage() { }

    public override bool IsMoving() => !_fsmManager?.HasReachedDestination() ?? false;


    protected virtual void OnDamageTaken(float remainingHealth) { }

    protected virtual void Update()
    {
        if (IsDead) return;
        _fsmManager?.Tick(Time.deltaTime);
       // IsStationary = _fsmManager?.IsStationary() ?? true;
        _fovRunner?.Tick(Time.deltaTime);

        if (_testStateCheck)
            Debug.LogError("Currentstate: "+_fsmManager?.CurrentStateId.ToString());
    }

    public bool _testStateCheck = false;

    protected virtual void LateUpdate()
    {
        if (IsDead) return;
        TryRotateAndAimAtTargetNew();
        //this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: CanRotateTowardsTarget());
        _fsmManager?.LateTick(Time.deltaTime);
        if (_eManager == null) return;

        if (Agent == null) return;
        _animationControl?.Tick(Agent.velocity, Agent.transform.forward);
    }



    public void LogUnhandled(IntentStateBaseObsolete state, in NPCNotification notification)
    {
        var Kind = notification.Kind;
        Debug.LogError("Notification Kind from unhandled: " + Kind.ToString());
    }


    public void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles)
    {
        //Debug.LogError("FOVResult: "+result.ToString());
        if (IsDead) return;
        result.CalculateFOVResultStreakNew(
            ref _currentSeenStreak,
            ref _currentNotSeenStreak,
            _requiredSeenStreak,
            _requiredNotSeenStreak,
            onResultStable: OnStableFOVResult
            );
    }



    private void StableFOVResultConfirmed(FOVResult result)
    {
        if (IsDead) return;
        Debug.LogError("Stable FOVResult: " + result.ToString());
        ApplyFOVStatusUpdate(result);
        var n = NPCNotification.FOVUpdate(/*_fsmManager.CurrentStateId,*/ CurrentFovState, false);
        OnNotify(n);
    }

    private void TargetSeen()
    {
        if (IsDead) return;
        Debug.LogError("FOVResult: Target Seen");
        ApplyFOVStatusUpdate(FOVResult.TargetSeen);
        //  _eManager.AimTowardsTarget(aim: true);
        var n = NPCNotification.FOVUpdate(/*_fsmManager.CurrentStateId, */CurrentFovState, false);
        OnNotify(n);

        /* if (_fsmManager.CurrentStateId == StateId.Patrol)
         {
             TryBroadcastAlert();
             return;
         }*/
    }

    private void TargetLost()
    {
        if (IsDead) return;
        Debug.LogError("FOVResult: Target Lost");
        CurrentFovState = FOVResult.TargetNotSeen;
        //   _eManager.AimTowardsTarget(aim: false);
    }

    private void TryRotateAndAimAtTarget()
    {
        if (IsDead || _fsmManager == null) return;
        if (_fsmManager.CurrentStateId == StateId.Chase || _fsmManager.CurrentStateId == StateId.Follow)
            if (IsMoving() || CurrentFovState == FOVResult.TargetSeen)
            {
                this.RotateTowardsTarget(_primaryTarget?.Transform, rotate: true);
                if (!_aimingAtTarget) { _aimingAtTarget = true; _animationControl?.IkLookAtTarget(look: true); }
            }
            else
            {
                this.RotateTowardsTarget(_primaryTarget?.Transform, rotate: false);
                if (_aimingAtTarget) { _aimingAtTarget = false; _animationControl?.IkLookAtTarget(look: false); }
            }
    }
    private void TryRotateAndAimAtTargetNew()
    {
        if (IsDead || TargetDead()) return;

        bool rotate = CurrentRotOrder == RotationOrder.RotateTowardsTarget;
        this.RotateTowardsTarget(_primaryTarget.Transform, rotate);

        /* if (_combatOrder != CombatOrder.None)
         {
             this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: true);
             if (!_aimingAtTarget) { _aimingAtTarget = true; _eManager.AimAtTarget(aim: true); }
         }
         else
         {
             this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: false);
             if (_aimingAtTarget) { _aimingAtTarget = false; _eManager.AimAtTarget(aim: false); }
         }*/
    }

    

    protected void TryBroadcastAlert(StateId nextIntent = StateId.None)
    {
        if (IsDead) return;
        if (SceneEventAggregator.Instance.AlertAgentsInZone(_zoneId, this))
        {
            //SwitchTo(ChaseState.Instance);
            //   EnterAlertPhase(nextIntent);
            // StartCoroutine(WaitRoutine());
            Debug.LogError("Alert broadcasted to zone: " + _zoneId);
        }
    }

    private IEnumerator WaitForAnimLayerFadeRoutine(AnimationLayer layer, bool activate, Action OnDone = null)
    {
        if (_animationControl == null) { OnDone?.Invoke(); yield break; }
        _animationControl.ToggleAnimationLayer(layer, activate);

        while (_animationControl.IsAnimationLayerActive(layer) != activate)
            yield return null;

        OnDone?.Invoke();
    }

    [Obsolete]
    private IEnumerator WaitAndSwitchStateRoutine(AnimationLayer layer, StateId nextIntent = StateId.None)
    {
        bool done = false;
        _eManager.TogglingAnimationLayer(layer,
            onComplete: () => done = true
            );

        while (!done)
        {
            if (IsDead) yield break;
            yield return null;
        }
        if (IsDead) yield break;
        Debug.LogError("Moving to Chase state");
        _fsmManager?.SwitchTo(nextIntent);
    }


    public void EnterAlertPhase(StateId nextIntent)
    {
        if (IsDead) return;
        StartCoroutine(WaitAndSwitchStateRoutine(AnimationLayer.Aim, nextIntent)); // change to new
                                                                                   // SwitchTo(ChaseState.Instance);
    }


    public void TryBroadcastAlert()
    {
        if (IsDead || _alertService == null) return;
        _alertService.TryAlertAgentsInZone(_zoneId, this);
    }

    public void SwitchState(StateId intentState)
    {
        if (_fsmManager == null) return;
        if (intentState == StateId.None || intentState == _fsmManager.CurrentStateId) return;
        _fsmManager.SwitchTo(intentState);
    }

    public void UpdateCurrentFovStatus(FOVResult newStatus) => CurrentFovState = newStatus;
    // Sets Sweep Frequency
    public void UpdateFovAlertPhase(AlertPhase newPhase) => _fovRunner?.SetAlertPhase(newPhase);


    public void UpdateRotationOrder(RotationOrder newOrder)
    {
        throw new NotImplementedException();
    }

    
}

public delegate void Notification(in NPCNotification n);