using Npc.Internal;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AgentEventManager))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public partial class NPCController : TargetableInit<ISceneAIServices, AgentEventManager>, INpcBody, /*IAgentData, */INPCBrainContext, INotificationListener, ICoroutineHost, ITickableGroup
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
    public StateId CurrentFsmState => _fsmManager?.CurrentState ?? StateId.None;
    public CombatOrder CurrentComOrder { get; private set; } = CombatOrder.None;
   // public RotationOrder CurrentRotOrder { get; private set; } = RotationOrder.None;
    public FOVResult CurrentFovState { get; private set; } = FOVResult.None;
    private bool TargetDead() => _primaryTarget?.IsDead ?? true;
    // End Brain data

    // Possibly Redundant
    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();

    public ITargetable Owner => this;

    [Header(@"How many consecutive FOV results required to ""See ""or ""Lose ""the target")]
    [SerializeField] private uint _requiredSeenStreak = 3;
    [SerializeField] private uint _requiredNotSeenStreak = 5;
    //  private bool _isTargetVisible = false;
    private uint _currentSeenStreak = 0;
    private uint _currentNotSeenStreak = 0;
    private Action<FOVResult> OnStableFOVResult;

    //private readonly BufferedInbox _inbox = new();
    private HashSet<ITickable> _tickables = new(5);

    // IFSMNotifications - For notifications received by the FSMManager, i.e. No valid destination, target lost, Target within melee/ shot range, etc.
    public void OnNotifies(in NpcNotification n)
    {
        if (_fsmManager.IsInStateTransition /*|| n.Id != _fsmManager.CurrentStateId*/) return;

      //  if (n.Kind == NotificationKind.FOVUpdate) Debug.LogError("FOV Result: "+n.FOVResult.ToString());

       // _inbox.Enqueue(n);
        this.Decide(in n);

    }

    public void Register(ITickable tickable) => _tickables.Add(tickable);

    public void Unregister(ITickable tickable) => _tickables.Remove(tickable);
   

    private void ResetAll()
    {
        UpdateCombatOrder(CombatOrder.None);
        UpdateCurrentFovStatus(FOVResult.None);
        OverrideRotation(RotationOverride.None);
        // RotateToTarget(rotate: false);
    }

    public void TriggerDeath()
    {
        if (IsDead) return;
        ResetAll();

        OnDeath();
    }

    public void OverrideSpeed(SpeedOverride speedOverride) => _fsmManager?.OverrideSpeed(speedOverride);

    public bool _testOverrideRot = false;
    public void OverrideRotation(RotationOverride rotOverride)
    {
        currentRotOverride = rotOverride;
        _fsmManager?.OverrideRotation(rotOverride);
    }

    public RotationOverride currentRotOverride = RotationOverride.None;

   

    public bool _testComOrder = false;
    
    public void UpdateCombatOrder(CombatOrder order)
    {
        if (_testComOrder)
        {
            Debug.LogError("CurrentOrder is: "+CurrentComOrder.ToString()+", and new Order is: "+order.ToString());
        }
        if (order == CurrentComOrder) return;
        CancelCurrentCombatOrder();
        CurrentComOrder = order;

        if (IsDead || TargetDead()) return;
        // Apply Order/ Start order
        if (CurrentComOrder == CombatOrder.FireAtWill)
        {
          //  Debug.LogError("Aiming at target - Fring at will");

            if (!_animationControl?.IsAnimationLayerActive(AnimationLayer.Aim) ?? true)
                StartCoroutine(WaitForAnimLayerFadeRoutine(AnimationLayer.Aim, true));

            _animationControl?.SetLookAt(true, _primaryTarget?.TargetableCollider.transform);
            //if (!_aimingAtTarget) { _aimingAtTarget = true; _animationControl?.IkLookAtTarget(look: true); }
        }
        //
    }



    private void CancelCurrentCombatOrder()
    {
        if (CurrentComOrder == CombatOrder.FireAtWill)
        {
            _animationControl?.SetLookAt(false);
          //  Debug.LogError("Stop Aiming at target - Hold fire or None order");
        }
    }

    private void ApplyFOVStatusUpdate(FOVResult result) => CurrentFovState = result;

    public void SendAnimationIntent(AnimationCue cue) => _animationControl?.PlayClip(cue);

    // public void RotateToTarget(bool rotate) => _fsmManager?.RotateToTarget(rotate);

    private bool _zoneRegistered = false;
    // End IFSMNotificationss


    /// <summary>
    /// Maps the specified destination to a zone and updates the agent's current zone accordingly.
    /// </summary>
    /// <remarks>This method determines the zone corresponding to the given destination and updates the
    /// agent's current zone if it differs from the previously assigned zone. If no zone is found for the destination,
    /// the agent is assigned to a default zone.  The method also handles the registration and unregistration of the
    /// agent with the appropriate zone using the <see cref="SceneEventAggregatorObsolete"/>. If the zone changes, the agent is
    /// unregistered from the previous zone and registered with the new one.  This method is intended to be used within
    /// the agent's internal state management and should not be called directly in most cases.</remarks>
    /// <param name="destination">The destination position in world coordinates to map to a zone.</param>
    public void MapDestinationToZone(Vector3 destination)
    {
        if (_zoneRegistered) return;

        ZoneId id;
        bool found = this.GetZoneId(destination, out id);
        if (found)
        {
            if (id == _zoneId || id == ZoneId.Unknown) return;
            else
            {
                _zoneRegistered = _alertService?.TryRegisterAgentAndZone(this, id) ?? true;
                _zoneId = id;
#if UNITY_EDITOR
                Debug.LogError("Zone ID on start: " + _zoneId.ToString());
#endif
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("No Zone ID found on start");
#endif
            _zoneId = ZoneId.ZoneA;
            _zoneRegistered = _alertService?.TryRegisterAgentAndZone(this, _zoneId) ?? true;
            
        }
        /*if (found)
        {
            if (id == _zoneId || id == ZoneId.Unknown) return;
            else
            {
                SceneEventAggregator.Instance.UnregisterAgentAndZone(this, _zoneId);
                _zoneId = id;
                SceneEventAggregator.Instance.RegisterAgentAndZone(this, _zoneId);
                Debug.LogError("Zone ID on start: " + _zoneId.ToString());
               // _fsmManager.OnMapDestinationToZone = null;
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
            //_fsmManager.OnMapDestinationToZone = null;
        }*/
    }

    
    protected void DeathStatusUpdated(bool isDead)
    {
        if (IsDead == isDead) return;
        //   base.DeathStatusUpdated(isDead);


    }


   
    protected void Engage() { }

    public override bool IsMoving() => !_fsmManager?.HasReachedDestination() ?? false;


    protected virtual void OnDamageTaken(float remainingHealth) { }

    public bool _showDistance = false;
    public bool ShowFOV = false;
    public bool _testAgentStop = false;
    public bool _testAgentStop2 = false;

    protected virtual void Update()
    {
        if (ShowFOV)
        {
            Debug.LogError("Current FOV: " + CurrentFovState.ToString());
        }
        if (_testOverrideRot)
        {
            OnNotifies(NpcNotification.PathNotifications.DestinationReached());
            _testOverrideRot = false;
        }

        if (_testAgentStop)
        {
            _testAgentStop2 = true;

            FovRunner r = _fovRunner as FovRunner;
            //r._testDistancePrint = true;
            Agent.ResetPath();
            Agent.enabled = false;
            _testAgentStop = false;
        }

    /*    if(_fsmManager != null)
        _fsmManager.TestPrint = _showDistance;*/
       /* if (_showDistance)
        {
            float distance = Vector3.Distance(transform.position, _primaryTarget.Position());
            Debug.LogError("Distance to target: " + distance.ToString("F2"));
        }*/

        if (IsDead) return;
        _fovRunner?.Tick(Time.deltaTime);

        if (_testAgentStop2) { return; }
        //  _fsmManager?.Tick(Time.deltaTime);
        foreach (var t in _tickables)
            t.Tick(Time.deltaTime);
      
        if (_testStateCheck)
            Debug.LogError("Currentstate: "+_fsmManager?.CurrentState.ToString());
    }

    public bool _testStateCheck = false;

    protected virtual void LateUpdate()
    {
        if (IsDead) return;

        //_inbox.Flush(this.Decide);
       
        if(!_testAgentStop2)
        _fsmManager?.LateTick(Time.deltaTime);
        
        if (Agent == null) return;
        _animationControl?.Tick(Agent.velocity, Agent.transform.forward);
    }



    public void LogUnhandled(IntentStateBaseObsolete state, in NpcNotification notification)
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
        var n = NpcNotification.FovNotifications.FOVUpdate(CurrentFovState);
        OnNotifies(n);
    }


    protected void TryBroadcastAlert(StateId nextIntent = StateId.None)
    {
        if (IsDead) return;
        if (SceneEventAggregatorObsolete.Instance.AlertAgentsInZone(_zoneId, this))
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
        if (intentState == StateId.None || intentState == _fsmManager.CurrentState) return;
        _fsmManager.SwitchTo(intentState);
    }

    public bool _testFovResult = false;

    public void UpdateCurrentFovStatus(FOVResult newStatus)
    {
        if (_testFovResult)
        {
            Debug.LogError("New FOV Status is: " + newStatus.ToString());
        }
        CurrentFovState = newStatus;
    }
    // Sets Sweep Frequency
    public void UpdateAlertPhase(AlertPhase newPhase) => _fovDeps?.SetAlertPhase(newPhase);

   
}

