using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
[Obsolete("", true)]
public partial class NPCControllerObsolete : ComponentEvents, IAgentData, INPCBrainContext, INotificationListener
{
    protected EnemyEventManager _eManager;
    //   private bool _isInStateTransition = false;
    protected Action _onLayerToggleComplete;
    protected ZoneId _zoneId = ZoneId.Unknown;

    // Data queried by the FSMManager 
    public ITargetable PrimaryTarget { get; set; }
    public NavMeshAgent Agent { get; protected set; }
    public NavMeshObstacle Obstacle { get; protected set; }
    public NavMeshPath Path { get; protected set; }

    [Header("Time to wait at each point when patrolling")]
    [Range(0.5f, 15f)]
    [SerializeField] protected float _maxWaitAtSeconds;
    [Min(0.5f)]
    [SerializeField] protected float _minWaitAtSeconds;
    public float MaxPatrolPointWaitTime => _maxWaitAtSeconds;
    public float MinPatrolPointWaitTime => _minWaitAtSeconds;
    public float WalkSpeed => _walkSpeed;
    public float SprintSpeed => _sprintSpeed;
    [Header("Agent Speed Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")]
    protected float _walkSpeed = 0.9f;
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")]
    protected float _sprintSpeed = 3.6f;
    // End FSM Data

    protected Action<bool> OnMeleeRangeCheckCallback;

    public bool TestSprint;
    public bool TestWalk;

    [Header("Data used by the Brain component")]
    public StateId CurrentFSMState => _fsmManager?.CurrentStateId ?? StateId.None;
    public CombatOrder CurrentComOrder { get; private set; } = CombatOrder.None;
    public RotationOrder CurrentRotOrder { get; private set; } = RotationOrder.None;
    public FOVResult CurrentFOVState { get; private set; } = FOVResult.None;
    private bool TargetDead() => PrimaryTarget?.IsDead ?? true;
    // End Brain data


    // ITargetable Data - This Gameobjects information for targeting purposes by other NPC's
    // i.e. its LayerMask, Transform, Aim Trigger, etc.
    [Header("The transform of this game object used for targeting purposes")]
    [SerializeField] protected Transform _parentTransform;
    public Transform Transform => _parentTransform != null ? _parentTransform : transform;
    public Vector3 Forward => _parentTransform != null ? _parentTransform.forward : transform.forward;

    [Header("Mask of this Gamobeject used for targeting purposes")]
    [SerializeField] protected LayerMask _layerMask;
    public LayerMask LayerMask => _layerMask;
    [Header("Trigger area on the game object that other NPC's use as target area for aiming")]
    [SerializeField] protected Collider _targetCollider;
    public Collider TargetableCollider { get; protected set; }
    public bool IsStationary { get; private set; } = false;

    public Vector3 Position()
     => _parentTransform == null ? transform.position : _parentTransform.position;
    public Quaternion Rotation()
        => _parentTransform == null ? transform.rotation : _parentTransform.rotation;
    // End ITargetable Data

    // Possibly Redundant
    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();

    public Func<StateId, float> OnRequestAgentStoppingDistance { get; private set; }

    public bool IsDead { get; private set; } = false;

    [Range(5,12)]
    [SerializeField] private int _maxFlankSteps = 5;
    [Min(4)]
    [SerializeField] private int _minFlankSteps = 2;
    public int MaxFlankSteps => _maxFlankSteps;

    public int MinFlankSteps => _minFlankSteps;

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


    [Header("Agent uses random stop distance between min & max during target pursuit")]
    [SerializeField] protected float _minStopdistance = 4f;
    [SerializeField] protected float _maxStopdistance = 12f;
    public float GetAgentStoppingDistance(StateId currentState)
    {
        if (currentState != _fsmManager.CurrentStateId) return 0f;
        return currentState == StateId.Chase ? UnityEngine.Random.Range(_minStopdistance, _maxStopdistance) : 0f;
    }



    // IFSMNotifications - For notifications received by the FSMManager, i.e. No valid destination, target lost, Target within melee/ shot range, etc.
    public void OnNotify(in NPCNotification n)
    {
        if (_fsmManager.IsInStateTransition /*|| n.Id != _fsmManager.CurrentStateId*/) return;
     
        if (!this.TryDecide(n, out var decision)) return;
       
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


    }

    private void UpdateCombatOrder(CombatOrder order)
    {
        if (order == CurrentComOrder) return;
        CancelCurrentCombatOrder();
        CurrentComOrder = order;
        
        if (OwnerIsDead || TargetDead()) return;
        // Apply Order/ Start order
        if(CurrentComOrder == CombatOrder.FireAtWill)
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

    private void ApplyFOVStatusUpdate(FOVResult result) => CurrentFOVState = result; 

    public void AnimationIntent(AnimationCue cue) => _animationControl?.PlayClip(cue);
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

   
    protected override void DeathStatusUpdated(bool isDead)
    {
        if (OwnerIsDead == isDead) return;
        base.DeathStatusUpdated(isDead);


    }

    protected virtual void OnVisibilityGained(bool seen) { }

    protected virtual void OnAimEnter(bool aiming) { }

    protected virtual void OnMeleeRangeEnter(bool targetInRange) { }


    protected void Engage() { }

    protected virtual void OnDamageTaken(float remainingHealth) { }

    protected virtual void Update()
    {
        if(OwnerIsDead) return;
        _fsmManager?.Tick(Time.deltaTime);
        IsStationary = _fsmManager?.IsStationary() ?? true;
    }

    protected virtual void LateUpdate()
    {
        if (OwnerIsDead) return;
        TryRotateAndAimAtTargetNew();
        //this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: CanRotateTowardsTarget());
        _fsmManager?.LateTick(Time.deltaTime);
        if (_eManager == null) return;
        _animationControl?.Tick(Agent.velocity, Agent.transform.forward);
    }


    [Obsolete]
    public void SwitchTo(IIntentState next)
    {
       /* if (next == null || _state == next || OwnerIsDead) return;
        
        _isInStateTransition = true;
        _state?.Exit(this);
        _state = next;
        _state?.Enter(this);
        _isInStateTransition = false;*/
    }

    public void LogUnhandled(IntentStateBase state, in NPCNotification notification)
    {
        var Kind = notification.Kind;
        Debug.LogError("Notification Kind from unhandled: "+ Kind.ToString());
    }

   
    public void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles)
    {
        //Debug.LogError("FOVResult: "+result.ToString());
        if (OwnerIsDead) return;
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
        if (OwnerIsDead) return;
        Debug.LogError("Stable FOVResult: "+result.ToString());
        ApplyFOVStatusUpdate(result);
        var n = NPCNotification.FOVUpdate(/*_fsmManager.CurrentStateId,*/ CurrentFOVState, false);
        OnNotify(n);
    }

    private void TargetSeen()
    {
        if (OwnerIsDead) return;
        Debug.LogError("FOVResult: Target Seen");
        ApplyFOVStatusUpdate(FOVResult.TargetSeen);
        //  _eManager.AimTowardsTarget(aim: true);
        var n = NPCNotification.FOVUpdate(/*_fsmManager.CurrentStateId, */CurrentFOVState, false);
        OnNotify(n);

       /* if (_fsmManager.CurrentStateId == StateId.Patrol)
        {
            TryBroadcastAlert();
            return;
        }*/
    }

    private void TargetLost()
    {
        if (OwnerIsDead) return;
        Debug.LogError("FOVResult: Target Lost");
        CurrentFOVState = FOVResult.TargetNotSeen;
     //   _eManager.AimTowardsTarget(aim: false);
    }

    private void TryRotateAndAimAtTarget()
    {
        if (OwnerIsDead || _fsmManager == null) return;
        if (_fsmManager.CurrentStateId == StateId.Chase || _fsmManager.CurrentStateId == StateId.Follow)
            if (IsStationary || CurrentFOVState == FOVResult.TargetSeen)
            {
                this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: true);
                if (!_aimingAtTarget) { _aimingAtTarget = true; _animationControl?.IkLookAtTarget(look: true); }
            }
            else
            {
                this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: false);
                if (_aimingAtTarget) { _aimingAtTarget = false; _animationControl?.IkLookAtTarget(look: false); }
            }
    }
    private void TryRotateAndAimAtTargetNew()
    {
        if (OwnerIsDead || TargetDead()) return;
  
        bool rotate = CurrentRotOrder == RotationOrder.RotateTowardsTarget;
        this.RotateTowardsTarget(PrimaryTarget.Transform, rotate);

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
        if (OwnerIsDead) return;
        if(SceneEventAggregator.Instance.AlertAgentsInZone(_zoneId, this))
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
            if (OwnerIsDead) yield break;
            yield return null;
        }
        if (OwnerIsDead) yield break;
        Debug.LogError("Moving to Chase state");
        _fsmManager?.SwitchTo(nextIntent);
    }


    public void EnterAlertPhase(StateId nextIntent)
    {
        if (OwnerIsDead) return;
        StartCoroutine(WaitAndSwitchStateRoutine(AnimationLayer.Aim, nextIntent)); // change to new
       // SwitchTo(ChaseState.Instance);
    }

    public void TryBroadcastAlert()
    {
        throw new NotImplementedException();
    }

    public void SwitchState(StateId intentState)
    {
        throw new NotImplementedException();
    }

    void INPCBrainContext.UpdateCombatOrder(CombatOrder newOrder)
    {
        UpdateCombatOrder(newOrder);
    }

    public void UpdateFovStatus(FOVResult newStatus)
    {
        throw new NotImplementedException();
    }

    public void UpdateRotationOrder(RotationOrder newOrder)
    {
        throw new NotImplementedException();
    }
}
























[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public partial class NPCControllerNew : ComponentInit<ISceneAIServices, AgentEventManager>, IAgentData, INPCBrainContext, INotificationListener
{
    //protected EnemyEventManager _eManager;
    //   private bool _isInStateTransition = false;
    protected Action _onLayerToggleComplete;
    protected ZoneId _zoneId = ZoneId.Unknown;

    // Data queried by the FSMManager 
    public ITargetable PrimaryTarget { get; set; }
    public NavMeshAgent Agent { get; protected set; }
    public NavMeshObstacle Obstacle { get; protected set; }
    public NavMeshPath Path { get; protected set; }

    [Header("Time to wait at each point when patrolling")]
    [Range(0.5f, 15f)]
    [SerializeField] protected float _maxWaitAtSeconds;
    [Min(0.5f)]
    [SerializeField] protected float _minWaitAtSeconds;
    public float MaxPatrolPointWaitTime => _maxWaitAtSeconds;
    public float MinPatrolPointWaitTime => _minWaitAtSeconds;
    public float WalkSpeed => _walkSpeed;
    public float SprintSpeed => _sprintSpeed;
    [Header("Agent Speed Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")]
    protected float _walkSpeed = 0.9f;
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")]
    protected float _sprintSpeed = 3.6f;
    public Func<StateId, float> OnRequestAgentStoppingDistance { get; private set; }
    // End FSM Data

    protected Action<bool> OnMeleeRangeCheckCallback;

    public bool TestSprint;
    public bool TestWalk;

    [Header("Data used by the Brain component")]
    public StateId CurrentFSMState => _fsmManager?.CurrentStateId ?? StateId.None;
    public CombatOrder CurrentComOrder { get; private set; } = CombatOrder.None;
    public RotationOrder CurrentRotOrder { get; private set; } = RotationOrder.None;
    public FOVResult CurrentFOVState { get; private set; } = FOVResult.None;
    private bool TargetDead() => PrimaryTarget?.IsDead ?? true;
    // End Brain data


    // ITargetable Data - This Gameobjects information for targeting purposes by other NPC's
    // i.e. its LayerMask, Transform, Aim Trigger, etc.
    [Header("The transform of this game object used for targeting purposes")]
    [SerializeField] protected Transform _parentTransform;
    public Transform Transform => _parentTransform != null ? _parentTransform : transform;
    public Vector3 Forward => _parentTransform != null ? _parentTransform.forward : transform.forward;

    [Header("Mask of this Gamobeject used for targeting purposes")]
    [SerializeField] protected LayerMask _layerMask;
    public LayerMask LayerMask => _layerMask;
    [Header("Trigger area on the game object that other NPC's use as target area for aiming")]
    [SerializeField] protected Collider _targetCollider;
    public Collider TargetableCollider { get; protected set; }
    public bool IsStationary { get; private set; } = false;

    public Vector3 Position()
     => _parentTransform == null ? transform.position : _parentTransform.position;
    public Quaternion Rotation()
        => _parentTransform == null ? transform.rotation : _parentTransform.rotation;
    // End ITargetable Data

    // Possibly Redundant
    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();

    

 

    [Range(5,12)]
    [SerializeField] private int _maxFlankSteps = 5;
    [Min(4)]
    [SerializeField] private int _minFlankSteps = 2;
    public int MaxFlankSteps => _maxFlankSteps;

    public int MinFlankSteps => _minFlankSteps;

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


    [Header("Agent uses random stop distance between min & max during target pursuit")]
    [SerializeField] protected float _minStopdistance = 4f;
    [SerializeField] protected float _maxStopdistance = 12f;
    public float GetAgentStoppingDistance(StateId currentState)
    {
        if (currentState != _fsmManager.CurrentStateId) return 0f;
        return currentState == StateId.Chase ? UnityEngine.Random.Range(_minStopdistance, _maxStopdistance) : 0f;
    }



    // IFSMNotifications - For notifications received by the FSMManager, i.e. No valid destination, target lost, Target within melee/ shot range, etc.
    public void OnNotify(in NPCNotification n)
    {
        if (_fsmManager.IsInStateTransition /*|| n.Id != _fsmManager.CurrentStateId*/) return;
     
        if (!this.TryDecide(n, out var decision)) return;
       
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


    }

    private void UpdateCombatOrder(CombatOrder order)
    {
        if (order == CurrentComOrder) return;
        CancelCurrentCombatOrder();
        CurrentComOrder = order;
        
        if (IsDead || TargetDead()) return;
        // Apply Order/ Start order
        if(CurrentComOrder == CombatOrder.FireAtWill)
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

    private void ApplyFOVStatusUpdate(FOVResult result) => CurrentFOVState = result; 

    public void AnimationIntent(AnimationCue cue) => _animationControl?.PlayClip(cue);
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
        ZoneId id = ZoneId.Unknown;
        bool found = true;//this.GetZoneId(destination, out id);
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

    protected virtual void OnMeleeRangeEnter(bool targetInRange) { }


    protected void Engage() { }

    protected virtual void OnDamageTaken(float remainingHealth) { }

    protected virtual void Update()
    {
        if(IsDead) return;
        _fsmManager?.Tick(Time.deltaTime);
        IsStationary = _fsmManager?.IsStationary() ?? true;
    }

    protected virtual void LateUpdate()
    {
        if (IsDead) return;
        TryRotateAndAimAtTargetNew();
        //this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: CanRotateTowardsTarget());
        _fsmManager?.LateTick(Time.deltaTime);
        if (_eManager == null) return;
        _animationControl?.Tick(Agent.velocity, Agent.transform.forward);
    }


    [Obsolete]
    public void SwitchTo(IIntentState next)
    {
       /* if (next == null || _state == next || OwnerIsDead) return;
        
        _isInStateTransition = true;
        _state?.Exit(this);
        _state = next;
        _state?.Enter(this);
        _isInStateTransition = false;*/
    }

    public void LogUnhandled(IntentStateBase state, in NPCNotification notification)
    {
        var Kind = notification.Kind;
        Debug.LogError("Notification Kind from unhandled: "+ Kind.ToString());
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
        Debug.LogError("Stable FOVResult: "+result.ToString());
        ApplyFOVStatusUpdate(result);
        var n = NPCNotification.FOVUpdate(/*_fsmManager.CurrentStateId,*/ CurrentFOVState, false);
        OnNotify(n);
    }

    private void TargetSeen()
    {
        if (IsDead) return;
        Debug.LogError("FOVResult: Target Seen");
        ApplyFOVStatusUpdate(FOVResult.TargetSeen);
        //  _eManager.AimTowardsTarget(aim: true);
        var n = NPCNotification.FOVUpdate(/*_fsmManager.CurrentStateId, */CurrentFOVState, false);
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
        CurrentFOVState = FOVResult.TargetNotSeen;
     //   _eManager.AimTowardsTarget(aim: false);
    }

    private void TryRotateAndAimAtTarget()
    {
        if (IsDead || _fsmManager == null) return;
        if (_fsmManager.CurrentStateId == StateId.Chase || _fsmManager.CurrentStateId == StateId.Follow)
            if (IsStationary || CurrentFOVState == FOVResult.TargetSeen)
            {
                this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: true);
                if (!_aimingAtTarget) { _aimingAtTarget = true; _animationControl?.IkLookAtTarget(look: true); }
            }
            else
            {
                this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: false);
                if (_aimingAtTarget) { _aimingAtTarget = false; _animationControl?.IkLookAtTarget(look: false); }
            }
    }
    private void TryRotateAndAimAtTargetNew()
    {
        if (IsDead || TargetDead()) return;
  
        bool rotate = CurrentRotOrder == RotationOrder.RotateTowardsTarget;
        this.RotateTowardsTarget(PrimaryTarget.Transform, rotate);

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
        if(SceneEventAggregator.Instance.AlertAgentsInZone(_zoneId, this))
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
        throw new NotImplementedException();
    }

    public void SwitchState(StateId intentState)
    {
        throw new NotImplementedException();
    }

    void INPCBrainContext.UpdateCombatOrder(CombatOrder newOrder)
    {
        UpdateCombatOrder(newOrder);
    }

    public void UpdateFovStatus(FOVResult newStatus)
    {
        throw new NotImplementedException();
    }

    public void UpdateRotationOrder(RotationOrder newOrder)
    {
        throw new NotImplementedException();
    }
}
