using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public partial class NPCController : ComponentEvents, IFSMOwner, IFSMData, IZoneAlertListener
{
    protected EnemyEventManager _eManager;
    private bool _isInStateTransition = false;
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
    public bool IsMoving { get; private set; } = false;

    public Vector3 Position()
     => _parentTransform == null ? transform.position : _parentTransform.position;
    public Quaternion Rotation()
        => _parentTransform == null ? transform.rotation : _parentTransform.rotation;
    // End ITargetable Data

    // Possibly Redundant
    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();

    [Header(@"How many consecutive FOV results required to ""See ""or ""Lose ""the target")]
    [SerializeField] private uint _requiredSeenStreak = 3;
    [SerializeField] private uint _requiredNotSeenStreak = 5;
    private bool _isTargetVisible = false;
    private uint _currentSeenStreak = 0;
    private uint _currentNotSeenStreak = 0;
    private Action OnTargetSeen;
    private Action OnTargetLost;
    private bool _aimingAtTarget = false;


    [Header("Agent uses random stop distance between min & max during target pursuit")]
    [SerializeField] protected float _minStopdistance = 4f;
    [SerializeField] protected float _maxStopdistance = 12f;
    public float GetAgentStoppingDistance(StateId currentState)
    {
        if (currentState != _state.Id) return 0f;

        return currentState == StateId.Chase ? UnityEngine.Random.Range(_minStopdistance, _maxStopdistance) : 0f;
    }

    

    // IFSMNotifications - For notifications received by the FSMManager, i.e. No valid destination, target lost, Target within melee/ shot range, etc.
    public virtual void Notify(in NotifyOwnerNPC n)
    {
        if (_isInStateTransition || n.Id != _state.Id) return;
        _state.Handle(this, n);
    }
    public void AnimationIntent(AnimationCue cue) => _eManager.TriggerAnimation(cue);
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
                FSM.OnMapDestinationToZone = null;
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
            FSM.OnMapDestinationToZone = null;
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
        FSM?.Tick(Time.deltaTime);
        IsMoving = FSM?.IsMoving() ?? false;
    }

    protected virtual void LateUpdate()
    {
        if (OwnerIsDead) return;
        TryRotateAndAimTowardsTarget();
        //this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: CanRotateTowardsTarget());
        FSM?.OnLateTick?.Invoke(Time.deltaTime);
        if (_eManager == null) return;
        _eManager.TickAnimator(Agent.velocity, Agent.transform.forward);
    }

   
    public virtual void SwitchTo(IIntentState next)
    {
        if (next == null || _state == next || OwnerIsDead) return;
        
        _isInStateTransition = true;
        _state?.Exit(this);
        _state = next;
        _state?.Enter(this);
        _isInStateTransition = false;
    }

    public void LogUnhandled(IntentStateBase state, in NotifyOwnerNPC notification)
    {
        var Kind = notification.Kind;
        Debug.LogError("Notification Kind from unhandled: "+ Kind.ToString());
    }

    private FOVResult _currentFOVResult = FOVResult.TargetNotSeen;
    public void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles)
    {
        //Debug.LogError("FOVResult: "+result.ToString());
        if (OwnerIsDead) return;
        result.CalculateFOVResultStreak(
            ref _isTargetVisible,
            ref _currentSeenStreak,
            ref _currentNotSeenStreak,
            _requiredSeenStreak,
            _requiredNotSeenStreak,
            onSeenStable: OnTargetSeen,
            onNotSeenStable: OnTargetLost
            );
    }

    private void TargetSeen()
    {
        if (OwnerIsDead) return;
        Debug.LogError("FOVResult: Target Seen");
        _currentFOVResult = FOVResult.TargetSeen;
      //  _eManager.AimTowardsTarget(aim: true);
        if (_state == Patrol.Instance)
        {
            TryBroadcastAlert();
            return;
        }
    }

    private void TargetLost()
    {
        if (OwnerIsDead) return;
        Debug.LogError("FOVResult: Target Lost");
        _currentFOVResult = FOVResult.TargetNotSeen;
     //   _eManager.AimTowardsTarget(aim: false);
    }

    private void TryRotateAndAimTowardsTarget()
    {
        if (OwnerIsDead) return;
        if (_state == ChaseState.Instance || _state == FollowGroup.Instance)
            if (!IsMoving || _currentFOVResult == FOVResult.TargetSeen)
            {
                this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: true);
                if (!_aimingAtTarget) { _aimingAtTarget = true; _eManager.AimTowardsTarget(aim: true); }
            }
            else
            {
                this.RotateTowardsTarget(PrimaryTarget?.Transform, rotate: false);
                if (_aimingAtTarget) { _aimingAtTarget = false; _eManager.AimTowardsTarget(aim: false); }
            }
    }

    protected void TryBroadcastAlert()
    {
        if (OwnerIsDead) return;
        if(SceneEventAggregator.Instance.AlertAgentsInZone(_zoneId, this))
        {
            //SwitchTo(ChaseState.Instance);
            EnterAlertPhase();
           // StartCoroutine(WaitRoutine());
            Debug.LogError("Alert broadcasted to zone: " + _zoneId);
        }
    }

    private IEnumerator WaitAndSwitchStateRoutine(AnimationLayer layer, IIntentState newState = null)
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
        SwitchTo(newState);
    }


    public void EnterAlertPhase()
    {
        if (OwnerIsDead) return;
        StartCoroutine(WaitAndSwitchStateRoutine(AnimationLayer.Aim, ChaseState.Instance));
       // SwitchTo(ChaseState.Instance);
    }
}
