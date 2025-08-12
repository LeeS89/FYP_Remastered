using UnityEngine;

public abstract class FSMControllerBase : ComponentEvents
{
    protected EnemyEventManager _agentEventManager;
    protected AIDestinationRequestData _resourceRequest;

    protected PatrolState _patrol;
    protected StationaryState _stationary;
    
    protected DeathState _deathState;
    protected EnemyState _currentState;

    protected float _patrolPointWaitDelay;
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")]
    protected float _walkSpeed;

    [Header("Chasing State Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")]
    protected float _sprintSpeed;
    protected ChasingState _chasing;

    protected DestinationManager _destinationManager;
    [SerializeField] protected int _maxFlankingSteps = 0;
    [SerializeField] protected GameObject _debugFlankCubes;
    public int AgentZone { get; protected set; } = 0;

    protected GameObject OwningAgent => gameObject;

    protected virtual void OwnerDeathStatusChanged(bool isDead) => OwnerIsDead = isDead;
    public bool OwnerIsDead { get; protected set; }

    protected abstract void ChangeState(EnemyState state, AlertStatus status = AlertStatus.None);

    protected virtual void StationaryStateRequested(AlertStatus alertStatus) { }
  
    protected virtual void OnDestinationRequestComplete(bool success, Vector3 destination, AIDestinationType destType) { }

    protected virtual void CheckIfDestinationReached() { }

    protected virtual void CallForBackup() { }

    public virtual void EnterAlertPhase() { }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _agentEventManager = eventManager as EnemyEventManager;
        _agentEventManager.OnOwnerDeathStatusUpdated += OwnerDeathStatusChanged;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        
        _agentEventManager.OnOwnerDeathStatusUpdated -= OwnerDeathStatusChanged;
        _agentEventManager = null;
        base.UnRegisterLocalEvents(eventManager);
    }

    protected override void RegisterGlobalEvents()
    {
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded += OnSceneComplete;
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
    }

    protected override void UnRegisterGlobalEvents()
    {
        SceneEventAggregator.Instance.UnRegisterAgentAndZone(this, AgentZone);
        BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;
        
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
    }

    protected virtual void SetupFSM()
    {
        _patrol = new PatrolState(OwningAgent, _agentEventManager, _patrolPointWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_agentEventManager, OwningAgent, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_agentEventManager, OwningAgent);
        _deathState = new DeathState(_agentEventManager, OwningAgent);
        _resourceRequest = new AIDestinationRequestData();
        _destinationManager = new DestinationManager(_agentEventManager, _maxFlankingSteps, _debugFlankCubes, transform, OnDestinationRequestComplete);
        AgentZone = _destinationManager.GetCurrentWPZone();
        SceneEventAggregator.Instance.RegisterAgentAndZone(this, AgentZone);
        Debug.LogError("WP ZOne: " + AgentZone);

    }

    protected override void OnSceneComplete()
    {
        if (_currentState != null)
        {
            _currentState.ExitState();
            _currentState = null;
        }
        _patrol.OnStateDestroyed();
        _chasing.OnStateDestroyed();
        _stationary.OnStateDestroyed();
        _deathState.OnStateDestroyed();

        _patrol = null;
        _chasing = null;
        _stationary = null;
        _deathState = null;
        BaseSceneManager._instance.OnSceneEnded -= OnSceneComplete;
    }
}
