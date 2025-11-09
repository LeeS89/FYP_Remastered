using System;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public abstract class NPCControllerBase : ComponentEvents, IFSMOwner
{
   // protected FieldOfViewManager _fovhandler;
    [SerializeField] protected FieldOfViewParams _fovParams;
   // protected Action<bool> OnVisibilityCallback;
    protected Action<bool> OnAimCheckCallback;
    protected Action<bool> OnMeleeRangeCheckCallback;

    [SerializeField] protected Transform _parentTransform;
    [SerializeField] protected Collider _targetableCollider;

    [Header("Agent Speed Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")]
    protected float _walkSpeed = 0.9f;
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")]
    protected float _sprintSpeed = 3.6f;

    [Header("Collider used for aiming towards when taking fire")]
    [SerializeField] protected Collider _targetCollider;

    public IFSMEvents FSM { get; protected set; }
    protected IIntentState _state;

    public State CurrentState { get; protected set; }

    [Header("Current Engage Target")]
    public ITargetable PrimaryTarget { get; set; }


    public EnemyEventManager OwnerEM { get; protected set; }
    public NavMeshAgent Agent { get; protected set; }
    public NavMeshObstacle Obstacle { get; protected set; }
    public NavMeshPath Path { get; protected set; }
    public Collider TargetableCollider { get; protected set; }
    public bool IsMoving { get; private set; } = false;

    public bool TestSprint;
    public bool TestWalk;

    public float WalkSpeed => _walkSpeed;

    public float SprintSpeed => _sprintSpeed;

    protected Vector3 _currentDestination;
    protected Vector3? _currentDestinationForward = null;

 
    public uint CurrentStateId { get; set; }

    [Range(0.5f, 15f)]
    [SerializeField] protected float _maxWaitAtSeconds;
    [Min(0.5f)]
    [SerializeField] protected float _minWaitAtSeconds;

    public float MaxWaitTime => _maxWaitAtSeconds;

    public float MinWaitTime => _minWaitAtSeconds;

    public Transform Transform => _parentTransform == null ? transform : _parentTransform;

    public LayerMask LayerMask => throw new NotImplementedException();

    public Transform RootTransform => throw new NotImplementedException();

    public float SprintEnterDist => throw new NotImplementedException();

    public float SprintExitDist => throw new NotImplementedException();

    #region Obsolete region
    public void OnDestinationFound(StateId id, Vector3 destination, NavMeshPath p)
    {
        if (id != _state.Id || destination == Vector3.zero) return;

        float newSpeed;

        switch (id)
        {
            case StateId.Patrol or StateId.Flank or StateId.Chase or StateId.Search:
                newSpeed = WalkSpeed;
                break;
            case StateId.Flee or StateId.Follow or StateId.Cover:
                newSpeed = SprintSpeed;
                break;
            default:
            //    FSM?.DestinationApproval(approved: false, p, destination, id, 0f, 10f);
                return;
        }
        //SetAgentTargetSpeed(newSpeed, 2f);
     //   FSM?.DestinationApproval(approved: true, p, destination, id, newSpeed, 2f);
    }

    public void DestinationReached(StateId reachedInState, bool isStale)
    {

        if (isStale) return;
        if (reachedInState == StateId.Patrol)
            FSM?.LookAroundAndContinue();
        // Future logic here for Flee/ Search states
    }
    #endregion






    public (Vector3, Vector3?) GetTargetablePositionAndForward()
        => _parentTransform == null ? (transform.position, transform.forward) : (_parentTransform.position, _parentTransform.forward);

    public Vector3 GetPosition()
     => _parentTransform == null ? transform.position : _parentTransform.position;



    public override void RegisterLocalEvents(EventManager eventManager)
    {
        OwnerEM = eventManager as EnemyEventManager;

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
       
        OnAimCheckCallback = OnAimEnter;
        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;
        
       

        FSM = new FSMManager(this);
        // _fsmController = new FSMManager();
        //FSM.Notification = OnNotification;

        base.RegisterLocalEvents(OwnerEM);
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
        OnAimCheckCallback = null;
        OnMeleeRangeCheckCallback = null;
        base.UnRegisterLocalEvents(OwnerEM);
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


   // protected abstract void SetAndChaseTarget(Transform targetPosition);

  //  protected abstract void OnPathValidationResult(bool pathBlocked, FSMPolicy policy);

    protected abstract void Engage();

    protected abstract void OnDamageTaken(float remainingHealth);

    protected virtual void Update()
    {
      //   if (FSM == null) return;

        FSM?.Tick(Time.deltaTime);
        IsMoving = FSM?.IsMoving() ?? false;

       // _fovhandler?.Tick();
    }

    protected virtual void LateUpdate()
    {
        FSM?.LateTick?.Invoke(Time.deltaTime);
        if (OwnerEM == null) return;
        OwnerEM.TickAnimator(Agent.velocity, Agent.transform.forward);
    }

   
    public abstract void LogUnhandled(IntentStateBase state, StateNotification notification);

    public abstract void Notify(in NotifyOwnerNPC n);

    public abstract void SwitchTo(IIntentState next);



    public Quaternion GetRotation()
         => _parentTransform == null ? transform.rotation : _parentTransform.rotation;

   
}
