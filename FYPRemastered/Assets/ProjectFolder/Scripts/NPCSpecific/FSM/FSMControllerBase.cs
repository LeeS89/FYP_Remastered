using UnityEngine;
using UnityEngine.AI;

public abstract class FSMControllerBase : ComponentEvents
{
    [Header("AI Components")]
    [SerializeField] protected NavMeshAgent _agent;
    [SerializeField] protected NavMeshObstacle _obstacle;

    [Header("FSM States")]
    protected PatrolState _patrol;
    protected StationaryState _stationary;
    protected DeathState _deathState;
    protected ChasingState _chasing;
    protected EnemyState _currentState;

    [Header("Event Manager & Resource request handler")]
    protected EnemyEventManager _agentEventManager;
 

    [Header("Agent Speed Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")]
    protected float _walkSpeed;
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")]
    protected float _sprintSpeed;

    [Header("")]

    [Header("Patrol State - Random number between 0 and stopAndWaitDelay to wait at each way point")]
    protected float _patrolPointWaitDelay;

    [Header("Target Flanking Params")]
    [SerializeField] protected int _maxFlankingSteps = 0;
    [SerializeField] protected GameObject _debugFlankCubes;

    protected DestinationManager _destinationManager;
    
    public int AgentZone { get; protected set; } = 0;

    public bool TargetInView { get; protected set; } = false;
    protected GameObject OwningAgent => gameObject;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _agentEventManager = eventManager as EnemyEventManager;
        base.RegisterLocalEvents(_agentEventManager);
        _agentEventManager.OnTargetSeen += TargetInViewStatusUpdated;
        _agentEventManager.OnRequestStationaryState += StationaryStateRequested;
        _agentEventManager.OnDestinationReached += CarveOnDestinationReached;
        _agentEventManager.OnAgentDeathComplete += ToggleGameObject;
        _agentEventManager.OnSpeedChanged += UpdateAgentSpeedValues;
        _agentEventManager.OnRotateTowardsTarget += ToggleAgentControlledRotationToTarget;
        RegisterGlobalEvents();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(_agentEventManager);
        _agentEventManager.OnRotateTowardsTarget -= ToggleAgentControlledRotationToTarget;
        _agentEventManager.OnSpeedChanged -= UpdateAgentSpeedValues;
        _agentEventManager.OnAgentDeathComplete -= ToggleGameObject;
        _agentEventManager.OnDestinationReached -= CarveOnDestinationReached;
        _agentEventManager.OnRequestStationaryState -= StationaryStateRequested;
        _agentEventManager.OnTargetSeen -= TargetInViewStatusUpdated;
       
        UnRegisterGlobalEvents();
    }

    protected override void RegisterGlobalEvents()
    {
        base.RegisterGlobalEvents();
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
        GameManager.OnPlayerMoved += EnemyState.SetPlayerMoved;
    }

    protected override void UnRegisterGlobalEvents()
    {
        base.UnRegisterGlobalEvents();
        GameManager.OnPlayerMoved -= EnemyState.SetPlayerMoved;
        SceneEventAggregator.Instance.UnRegisterAgentAndZone(this, AgentZone);
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
    }

    protected virtual void SetupFSM()
    {
        _patrol = new PatrolState(OwningAgent, _agentEventManager, _patrolPointWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_agentEventManager, OwningAgent, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_agentEventManager, OwningAgent);
        _deathState = new DeathState(_agentEventManager, OwningAgent);
        _destinationManager = new DestinationManager(_agentEventManager, _maxFlankingSteps, _debugFlankCubes, transform, OnDestinationRequestComplete);
        AgentZone = _destinationManager.GetCurrentWPZone();
        SceneEventAggregator.Instance.RegisterAgentAndZone(this, AgentZone);
        Debug.LogError("WP ZOne: " + AgentZone);

    }


  
    protected abstract void ChangeState(EnemyState state, AlertStatus status = AlertStatus.None);

    protected virtual void StationaryStateRequested(AlertStatus alertStatus) { }
  
    protected virtual void OnDestinationRequestComplete(bool success, Vector3 destination, AIDestinationType destType) { }

    protected virtual void CheckIfDestinationReached() { }

    protected virtual void CarveOnDestinationReached(bool reached) { }

    protected virtual void TargetInViewStatusUpdated(bool inView) { }

    protected virtual void CallForBackup() { }

    public virtual void EnterAlertPhase() { }

    protected virtual void ToggleGameObject(bool status) => gameObject.SetActive(status);

    protected virtual void ToggleNMObstacle(bool status) { }

    protected virtual void UpdateAgentSpeedValues(float speed, float lerpSpeed) { }

    protected virtual void ToggleAgentControlledRotationToTarget(bool rotate) { }

    public abstract void ResetFSM(EnemyState contextState = null);


    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        if (_currentState != null)
        {
            _currentState.ExitState();
            _currentState = null;
        }
        _patrol?.OnStateDestroyed();
        _chasing?.OnStateDestroyed();
        _stationary?.OnStateDestroyed();
        _deathState?.OnStateDestroyed();

        _patrol = null;
        _chasing = null;
        _stationary = null;
        _deathState = null;
        _agentEventManager = null;
    }
}
