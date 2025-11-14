using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public abstract class NPCControllerBase : ComponentEvents, IFSMOwner, IFSMData
{
    protected EnemyEventManager _eManager;
    private bool _isInStateTransition = false;

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
    private IDestinationResolver _destinationResolver;
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


        base.RegisterLocalEvents(_eManager);
        RegisterGlobalEvents();
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

    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        SwitchTo(Patrol.Instance);
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
        throw new NotImplementedException();
    }

    private FOVResult _currentResult = FOVResult.TargetNotSeen;
    public void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles)
    {
        Debug.LogError("FOVResult: "+result.ToString());
        if (_currentResult == result) return;
        Debug.LogError("FOVResult when changed: " + result.ToString());
        _currentResult = result;
        if (_state == Patrol.Instance)
        {
            Debug.LogError("Moving to Chase state");
            if(_currentResult == FOVResult.TargetSeen)
                SwitchTo(ChaseState.Instance);
        }
    }

    
}
