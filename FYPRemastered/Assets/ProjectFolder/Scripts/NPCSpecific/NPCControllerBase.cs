using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public abstract class NPCControllerBase : ComponentEvents, IFSMOwner
{
  //  protected EnemyEventManager _eEventManager;
    protected FieldOfViewManager _fovhandler;
    [SerializeField] protected FieldOfViewParams _fovParams;
    protected Action<bool> OnVisibilityCallback;
    protected Action<bool> OnAimCheckCallback;
    protected Action<bool> OnMeleeRangeCheckCallback;
    //protected FSMPolicy? _currentPolicy;
   // protected uint _currentPolicyVersion;
    [SerializeField] protected Transform _parentTransform;
    protected Collider _targetableCollider;

    [Header("Agent Speed Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")]
    protected float _walkSpeed = 0.9f;
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")]
    protected float _sprintSpeed = 3.6f;

    [Header("Agent and animation speed values")]
    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;

    public IFSMEvents FSM { get; protected set; } 
    protected IIntentState _state;

    public State CurrentState { get; protected set; }

    public ITargetable PrimaryTarget { get; protected set; }


    public EnemyEventManager OwnerEM { get; protected set; }

    public NavMeshAgent Agent { get; protected set; }

    public NavMeshObstacle Obstacle { get; protected set; }

    public NavMeshPath Path { get; protected set; }

    public bool IsMoving { get; private set; }

    public Collider TargetableCollider { get; protected set; }

    public float WalkSpeed => _walkSpeed;

    public float SprintSpeed => _sprintSpeed;

    [SerializeField] protected Collider _targetCollider;

    protected void UpdateAgentTargetSpeed(float speed, float lerpSpeed)
    {
        _lerpSpeed = lerpSpeed;
        _targetSpeed = speed;
    }

    protected void UpdateAgentSpeed()
    {
        if (Agent == null) return;
        float smoothedSpeed = Mathf.Lerp(Agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        Agent.speed = smoothedSpeed;

        float _currentSpeed = Agent.speed;

        if (Mathf.Approximately(Agent.speed, _targetSpeed)) Agent.speed = _targetSpeed;
    }

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

        SetNavMeshAgentParams();
        OnVisibilityCallback = OnVisibilityGained;
        OnAimCheckCallback = OnAimEnter;
        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;
        OwnerEM.OnSpeedChanged += UpdateAgentTargetSpeed;
        _fovhandler = new FieldOfViewManager(_fovParams, OnVisibilityCallback, OnAimCheckCallback, OnMeleeRangeCheckCallback, new AITraceComponent());

        FSM = new FSMManager(this);
        // _fsmController = new FSMManager();
        FSM.Notification = OnNotification;

        base.RegisterLocalEvents(OwnerEM);
        RegisterGlobalEvents();
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
        OwnerEM.OnSpeedChanged -= UpdateAgentTargetSpeed;
        OnVisibilityCallback = null;
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

    protected abstract void ChangeState(State state, Transform targetPos = null);

    protected abstract void SetAndChaseTarget(Transform targetPosition);

  //  protected abstract void OnPathValidationResult(bool pathBlocked, FSMPolicy policy);

    protected abstract void Engage();

    protected abstract void OnDamageTaken(float remainingHealth);

    protected virtual void Update()
    {
        if (FSM == null) return;
        FSM.Tick?.Invoke(Time.deltaTime);
        IsMoving = !FSM.DestinationReached;
       // _fovhandler?.Tick();
    }

    protected virtual void LateUpdate()
    {
        UpdateAgentSpeed();
        if (OwnerEM == null) return;
        OwnerEM.TickAnimator(Agent.velocity, Agent.transform.forward);
    }

    protected abstract void PolicyResult(in FSMPolicyResult result);

    public abstract void LogUnhandled(IntentStateBase state, StateNotification notification);

    public abstract void OnNotification(in StateNotification n);

    public abstract void SwitchTo(IIntentState next);

   


    /* public void SetIntent(MovementIntent intent) =>
        SwitchTo(intent switch
        {
            MovementIntent.Patrol => PatrolState.Instance,
            MovementIntent.Chase => ChaseState.Instance,
            MovementIntent.Flee => HoldState.Instance,   // plug your FleeState here
            MovementIntent.Hold => HoldState.Instance,
            _ => PatrolState.Instance
        });*/
}
