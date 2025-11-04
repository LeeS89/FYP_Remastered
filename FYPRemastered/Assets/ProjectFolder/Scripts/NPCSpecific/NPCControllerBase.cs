using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] protected Collider _targetCollider;

    public IFSMEvents FSM { get; protected set; }
    protected IIntentState _state;

    public State CurrentState { get; protected set; }

    public ITargetable PrimaryTarget { get; protected set; }


    public EnemyEventManager OwnerEM { get; protected set; }

    public NavMeshAgent Agent { get; protected set; }

    public NavMeshObstacle Obstacle { get; protected set; }

    public NavMeshPath Path { get; protected set; }

    public bool IsMoving { get; private set; } = false;

    public Collider TargetableCollider { get; protected set; }

    public float WalkSpeed => _walkSpeed;

    public float SprintSpeed => _sprintSpeed;

    protected Vector3 _currentDestination;
    protected Vector3? _currentDestinationForward = null;

   // protected uint _destinationId = 0;

    public uint CurrentStateId { get; set; }

    [Range(0.5f, 15f)]
    [SerializeField] protected float _maxWaitAtSeconds;
    [Min(0.5f)]
    [SerializeField] protected float _minWaitAtSeconds;

    public float MaxWaitTime => _maxWaitAtSeconds;

    public float MinWaitTime => _minWaitAtSeconds;

    public Transform Transform => _parentTransform == null ? transform : _parentTransform;


    #region Timer Region
    protected readonly List<TimerTask> _tasks = new(5);
   // protected readonly List<Timer> _tTasks = new(5);

    protected struct TimerTask
    {
        public float Remaining;
        public Vector3? Destintion;
       // public Vector3? Forward;
       // public Func<Vector3?, Vector3?, bool> Action;
       // public Action<Vector3?> OnTickAction;
        public Action<Vector3?> OnDone;
        
    }
    
   /* protected struct Timer
    {
        
        public bool ActionAlreadyInvoked;
        public float RemainingTime;
        public readonly Vector3? Destination;
        public readonly Vector3? Forward;
        public readonly Func<Vector3?, Vector3?, bool> TimerAction;
        public readonly Action<Vector3?, Vector3?> OnDone;
    }*/

    protected void AddTimer(float seconds, Action<Vector3?> onDone, Vector3? destination = null/*, Vector3? forward = null*/)
    {
        _tasks.Add(new TimerTask
        {
            Remaining = seconds,
            OnDone = onDone,
            Destintion = destination,
            //Forward = forward
        });
    }

    protected void NextFrame(Vector3 destination, Action<Vector3?> onDone)
        => AddTimer(Time.deltaTime + Mathf.Epsilon, onDone, destination);


  /*  protected void UpdateTimers()
    {
        float dt = Time.deltaTime;
        for(int i = 0; i < _tTasks.Count; i++)
        {
            var t = _tTasks[i];

            if (!t.ActionAlreadyInvoked)
            {
                t.ActionAlreadyInvoked = true;

                t.OnDone?.Invoke(t.Destination, t.Forward);

            }
        }
    }*/

    protected void UpdateTicks()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < _tasks.Count; i++)
        {
            var t = _tasks[i];
            t.Remaining -= dt;

            if(t.Remaining <= 0f)
            {
                t.OnDone?.Invoke(t.Destintion);

                int last = _tasks.Count - 1;
                _tasks[i] = _tasks[last];
                _tasks.RemoveAt(last);

                i--;
                continue;
            }

            _tasks[i] = t;
        }
    }

    #endregion

   
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
                SetAgentTargetSpeed(0f, 10f);
                return;
        }
        //SetAgentTargetSpeed(newSpeed, 2f);
        FSM.DestinationApproved(p, destination, id, newSpeed, 2f);
    }

    public void DestinationReached(StateId reachedInState, bool isStale)
    {
       // SetAgentTargetSpeed(0f, 10f);
        //Agent.ResetPath();
       // Agent.enabled = false; /// ReEnable later
       // Obstacle.enabled = true;

        if (isStale) return;
        if (reachedInState == StateId.Patrol)
            FSM?.LookAroundAndContinue();
        // Future logic here for Flee/ Search states
    }


    /*public (float, float) GetSpeedAndLerp(StateId id)
    {
        if (_state == null || id != _state.Id) return (0f, 0f);
        return id switch 
        {
            StateId.Patrol or StateId.Flank or StateId.Chase or StateId.Search => (WalkSpeed, 2f),
            StateId.Flee or StateId.Follow or StateId.Cover => (SprintSpeed, 2f),
            _=> (0f,0f)
            };

    }*/


    protected void SetAgentTargetSpeed(float speed, float lerpSpeed)
      => (_lerpSpeed, _targetSpeed) = (lerpSpeed, speed);
    


  

    IEnumerator DelayEnableRoutine(uint id, Vector3 destination, float newSpeed, float lerp)
    {
        Obstacle.enabled = false;
        yield return null;
      //  SetAgentTargetSpeed(WalkSpeed, 10f);
      //  TrySetDestination(id, destination, newSpeed, lerp);
    }

   

    protected void TrySetDestination(uint id, Vector3 destination, float newSpeed, float lerp)
    {
        SetAgentTargetSpeed(newSpeed, lerp);
        ToggleAgent(setActive: true);
        Agent.SetDestination(destination);
    }

    protected void ToggleAgent(bool setActive)
    {
        if (Agent.enabled == setActive) return;
        Agent.enabled = setActive;
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
      //   if (FSM == null) return;

        FSM?.Tick?.Invoke(Time.deltaTime);
        IsMoving = !FSM?.DestinationReached ?? false;

       // _fovhandler?.Tick();
    }

    protected virtual void LateUpdate()
    {
        //UpdateAgentSpeed();
        if (OwnerEM == null) return;
        OwnerEM.TickAnimator(Agent.velocity, Agent.transform.forward);
    }

   
    public abstract void LogUnhandled(IntentStateBase state, StateNotification notification);

    public abstract void OnNotification(in NotifyOwnerNPC n);

    public abstract void SwitchTo(IIntentState next);



    public Quaternion GetRotation()
         => _parentTransform == null ? transform.rotation : _parentTransform.rotation;

   
}
