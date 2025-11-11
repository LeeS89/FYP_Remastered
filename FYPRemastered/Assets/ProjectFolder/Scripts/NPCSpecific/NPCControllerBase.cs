using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public abstract class NPCControllerBase : ComponentEvents, IFSMOwner, IFSMData, IFSMNotifications
{
    [Header("FOV Data")]
    [SerializeField] protected FOVParameters _fovParams;

    // FSM Data
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

    // protected Action<bool> OnAimCheckCallback;
    protected Action<bool> OnMeleeRangeCheckCallback;

   

    public IFSMControl FSM { get; protected set; }
    protected IIntentState _state;

    protected EnemyEventManager _eManager;
   
   
    

    public bool TestSprint;
    public bool TestWalk;


 
    // ITargetable Data - This Gameobjects information for targeting purposes
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
    // End ITargetable Data

    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();


    // IFSMNotifications
    public abstract void Notify(in NotifyOwnerNPC n);
    public void OnAnimationIntent(AnimationCue cue)
        => _eManager.TriggerAnimation(cue);
    // End IFSMNotificationss


    public (Vector3, Vector3?) GetTargetablePositionAndForward()
        => _parentTransform == null ? (transform.position, transform.forward) : (_parentTransform.position, _parentTransform.forward);

    public Vector3 GetPosition()
     => _parentTransform == null ? transform.position : _parentTransform.position;


    private IDestinationResolver _destResolver;
    private IFieldOfViewRunner _fovRunner;

    private Dictionary<StateId, ICandidateProvider> _destinationProviders;

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
       
      //  OnAimCheckCallback = OnAimEnter;
        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;

        _destinationProviders = new()
        {
            [StateId.Patrol] = new WaypointProvider()
        };
        
        _destResolver = new DestinationFinder(_destinationProviders);
        _fovRunner = new NPCFieldOfViewHandler(_fovParams);

        FSM = new FSMManager(data: this, notify: this, resolver: _destResolver, runner: _fovRunner);
      
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
       // OnAimCheckCallback = null;
        OnMeleeRangeCheckCallback = null;
        base.UnRegisterLocalEvents(_eManager);
        UnRegisterGlobalEvents();
    }

    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        SwitchTo(Patrol.Instance);
    }

    protected abstract void OnVisibilityGained(bool seen);

    protected abstract void OnAimEnter(bool aiming);

    protected abstract void OnMeleeRangeEnter(bool targetInRange);


    protected abstract void Engage();

    protected abstract void OnDamageTaken(float remainingHealth);

    protected virtual void Update()
    {
        FSM?.Tick(Time.deltaTime);
        IsMoving = FSM?.IsMoving() ?? false;
    }

    protected virtual void LateUpdate()
    {
        FSM?.LateTick?.Invoke(Time.deltaTime);
        if (_eManager == null) return;
        _eManager.TickAnimator(Agent.velocity, Agent.transform.forward);
    }

   
    public abstract void LogUnhandled(IntentStateBase state, StateNotification notification);

    

    public abstract void SwitchTo(IIntentState next);



    public Quaternion GetRotation()
         => _parentTransform == null ? transform.rotation : _parentTransform.rotation;

    
}
