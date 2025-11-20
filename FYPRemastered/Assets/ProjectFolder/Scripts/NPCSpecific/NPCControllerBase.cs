using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public abstract class NPCControllerBase : ComponentEvents, IFSMOwner, IFSMData, IZoneAlertListener
{
    protected EnemyEventManager _eManager;
    private bool _isInStateTransition = false;
    protected Action<AnimationLayer> _onAnimLayerToggleComplete;
    protected Action _onLayerToggleComplete;
    protected bool _aimAnimLayerActive = false;
 //   protected int? _currentPatrolZone = null;
    protected ZoneId _zoneId = ZoneId.Unknown;

    protected void OnAnimLayerToggleComplete(AnimationLayer layer) => _aimAnimLayerActive = true;

    [Header("FOV Data")]
    [SerializeField] protected FOVParameters _fovParams;

    // FSM Data queried by the FSMManager 
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
    public Transform Transform => _parentTransform == null ? transform : _parentTransform;

    [Header("Mask of this Gamobeject used for targeting purposes")]
    [SerializeField] protected LayerMask _layerMask;
    public LayerMask LayerMask => throw new NotImplementedException();
    [Header("Trigger area on the game object that other NPC's use as target area for aiming")]
    [SerializeField] protected Collider _targetCollider;
    public Collider TargetableCollider { get; protected set; }
    public bool IsMoving { get; private set; } = false;
    public (Vector3, Vector3?) GetTargetablePositionAndForward()
    => _parentTransform == null ? (transform.position, transform.forward) : (_parentTransform.position, _parentTransform.forward);

    public Vector3 GetPosition()
     => _parentTransform == null ? transform.position : _parentTransform.position;
    public Quaternion GetRotation()
        => _parentTransform == null ? transform.rotation : _parentTransform.rotation;
    // End ITargetable Data

    // Possibly Redundant
    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();

    [Header("Agent uses random stop distance between min & max during target pursuit")]
    [SerializeField] protected float _minStopdistance = 4f;
    [SerializeField] protected float _maxStopdistance = 12f;
    public float GetAgentStoppingDistance(StateId currentState)
    {
        if (currentState != _state.Id) return 0f;

        return currentState == StateId.Chase ? UnityEngine.Random.Range(_minStopdistance, _maxStopdistance) : 0f;
    }

    // FSMManager Composition
    public IFSMControl FSM { get; protected set; }
    private IPathResolver _pathFinder;
    private ICandidateProvider _destinationResolver;
    private IFieldOfViewRunner _fovRunner;
    private Dictionary<StateId, ICandidateProvider> _destinationProviders;
    protected IIntentState _state;
    // end FSMManager Composition

    // IFSMNotifications - For notifications received by the FSMManager, i.e. No valid destination, target lost, Target within melee/ shot range, etc.
    public virtual void Notify(in NotifyOwnerNPC n)
    {
        if (_isInStateTransition || n.Id != _state.Id) return;
        _state.Handle(this, n);
    }
    public void AnimationIntent(AnimationCue cue) => _eManager.TriggerAnimation(cue);
    // End IFSMNotificationss



    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eManager = eventManager as EnemyEventManager;

        if (_targetCollider == null)
        {
            if (!TryGetComponent<Collider>(out var coll))
            {
                TargetableCollider = gameObject.AddComponent<BoxCollider>();
            }
            else
                TargetableCollider = coll;
        }
        else
        {
            TargetableCollider = _targetCollider;
        }

        SetPrimaryToPlayer();
        SetNavMeshAgentParams();

        _onAnimLayerToggleComplete = OnAnimLayerToggleComplete;
        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;

        _destinationProviders = new()
        {
            [StateId.Patrol] = new WaypointProvider(WaypointRepo.Instance),
            [StateId.Chase] = new TargetPointProvider(PrimaryTarget),
        };
        
        _destinationResolver = new DestinationResolver(_destinationProviders);
        _fovParams.FOVTarget = PrimaryTarget;
        _pathFinder = new PathFinder(_destinationResolver);
        _fovRunner = new NPCFieldOfViewHandler(_fovParams);

        FSM = new FSMManager(data: this, /*notify: this, */resolver: _pathFinder, runner: _fovRunner);
        FSM.Notification = Notify;
        FSM.OnAnimationIntent = AnimationIntent;
       // FSM.OnWaypointZoneReceived = OnWaypointZoneReceived;
        FSM.OnMapDestinationToZone = MapDestinationToZone;

        base.RegisterLocalEvents(_eManager);
        RegisterGlobalEvents();
    }

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

    protected void SetPrimaryToPlayer()
    {
        if(!GameManager.Instance.TryGetPlayer(out var player))
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to retrieve player ref");
#endif
            return;
        }
        PrimaryTarget = player;
        //PrimaryTarget = GameManager.Instance.TryGetPlayer();
    }

    private void SetNavMeshAgentParams()
    {
        string errorMessage = "";
        if (TryGetComponent<NavMeshAgent>(out var agent)) Agent = agent;
        else errorMessage += "Must Provide a NavMeshAgent Component - ";

        if (TryGetComponent<NavMeshObstacle>(out var ob)) Obstacle = ob;
        else errorMessage += "Must Provide a NavMeshObstacle Component - ";

        Path = new NavMeshPath();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        OnMeleeRangeCheckCallback = null;
        base.UnRegisterLocalEvents(_eManager);
        UnRegisterGlobalEvents();
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        FSM.OnMapDestinationToZone = null;
        FSM.Notification = null;
        FSM.OnAnimationIntent = null;
        FSM.OnWaypointZoneReceived = null;
    }

    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        SwitchTo(Patrol.Instance);
        
       /* _currentPatrolZone = FSM?.TryGetPatrolZone();

        if (_currentPatrolZone != null)
        {
            SceneEventAggregator.Instance.RegisterAgentAndZone(this, _currentPatrolZone.Value);
#if UNITY_EDITOR
            Debug.LogError("Successfully got zone: " + _currentPatrolZone);
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to get zone");
#endif
        }*/
    }

    /// <summary>
    /// Handles the event triggered when a waypoint zone is received, updating the patrol zone if necessary.
    /// </summary>
    /// <remarks>If the received zone is different from the current patrol zone and the operation is
    /// successful,  the method updates the patrol zone and registers the agent with the new zone. If the agent was 
    /// previously registered with another zone, it is unregistered from that zone first.</remarks>
    /// <param name="success">Indicates whether the waypoint zone was successfully received. If <see langword="false"/>, no action is taken.</param>
    /// <param name="zone">The identifier of the received waypoint zone. Must be a non-negative integer.</param>
    protected void OnWaypointZoneReceived(bool success, int zone)
    {
/*#if UNITY_EDITOR
        Debug.LogError("Successfully got zone: " + zone);
#endif
        if (_currentPatrolZone == zone || !success || zone < 0) return;
        if(_currentPatrolZone != null) SceneEventAggregator.Instance.UnregisterAgentAndZone(this, _currentPatrolZone.Value);
        _currentPatrolZone = zone;
        SceneEventAggregator.Instance.RegisterAgentAndZone(this, zone);*/

    }

    protected override void DeathStatusUpdated(bool isDead)
    {
        if (OwnerIsDead == isDead) return;
        base.DeathStatusUpdated(isDead);


    }

    protected abstract void OnVisibilityGained(bool seen);

    protected abstract void OnAimEnter(bool aiming);

    protected virtual void OnMeleeRangeEnter(bool targetInRange) { }


    protected void Engage() { }

    protected abstract void OnDamageTaken(float remainingHealth);

    protected virtual void Update()
    {
        if(OwnerIsDead) return;
        FSM?.Tick(Time.deltaTime);
        IsMoving = FSM?.IsMoving() ?? false;
    }

    protected virtual void LateUpdate()
    {
        if (OwnerIsDead) return;
        FSM?.LateTick?.Invoke(Time.deltaTime);
        if (_eManager == null) return;
        _eManager.TickAnimator(Agent.velocity, Agent.transform.forward);
    }

   
    public virtual void SwitchTo(IIntentState next)
    {
        if (next == null || _state == next) return;

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

    private FOVResult _currentResult = FOVResult.TargetNotSeen;
    public void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles)
    {
        Debug.LogError("FOVResult: "+result.ToString());
        if (/*_currentResult == result || */OwnerIsDead) return;
        //Debug.LogError("FOVResult when changed: " + result.ToString());
        _currentResult = result;
        if (_state == Patrol.Instance)
        {
           // Debug.LogError("Moving to Chase state");
            if(_currentResult == FOVResult.TargetSeen) 
            {
                TryBroadcastAlert();
              //  if (!_aimAnimLayerActive) _eManager.ToggleAnimationLayer(AnimationLayer.Aim, _onAnimLayerToggleComplete);
               // SwitchTo(ChaseState.Instance);
            }
            return;
        }
    }

    protected void TryBroadcastAlert()
    {
        if (OwnerIsDead || _zoneId == ZoneId.Unknown) return;
        if(SceneEventAggregator.Instance.AlertAgentsInZone(_zoneId, this))
        {
            //SwitchTo(ChaseState.Instance);
            StartCoroutine(WaitRoutine());
            Debug.LogError("Alert broadcasted to zone: " + _zoneId);
        }
    }

    private IEnumerator WaitRoutine()
    {
        bool done = false;
        _eManager.TogglingAnimationLayer(AnimationLayer.Aim,
            onComplete: () => done = true
            );

        while (!done) yield return null;
        if (OwnerIsDead) yield break;
        Debug.LogError("Moving to Chase state");
        SwitchTo(ChaseState.Instance);
    }


    public void EnterAlertPhase()
    {
        if (OwnerIsDead) return;
        SwitchTo(ChaseState.Instance);
    }
}
