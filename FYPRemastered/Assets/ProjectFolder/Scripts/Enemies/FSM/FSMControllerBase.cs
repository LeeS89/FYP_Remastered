using UnityEngine;

public abstract class FSMControllerBase : ComponentEvents
{
    protected EnemyEventManager _agentEventManager;
    protected GameObject _owningAgent;
    protected PatrolState _patrol;
    protected StationaryState _stationary;
    protected ChasingState _chasing;
    protected DeathState _deathState;

    protected float _patrolPointWaitDelay;
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")] 
    protected float _walkSpeed;
    
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")] 
    protected float _sprintSpeed;


    protected abstract void ChangeState(EnemyState state);

    protected virtual void StationaryStateRequested() { }
    protected virtual void ChasingStateRequested() { }
    protected virtual void PatrolStateRequested() { }

    protected virtual void OnDestinationRequestComplete(bool success, Vector3 destination, AIDestinationType destType) { }

    protected virtual void CheckIfDestinationReached() { }

    

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _agentEventManager = eventManager as EnemyEventManager;
    }

    protected override void RegisterGlobalEvents()
    {
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
    }

    protected override void UnRegisterGlobalEvents()
    {
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
    }

    protected virtual void SetupFSM()
    {
        _patrol = new PatrolState(_owningAgent, _agentEventManager, _patrolPointWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_agentEventManager, _owningAgent, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_agentEventManager, _owningAgent);
        _deathState = new DeathState(_agentEventManager, _owningAgent);
    }
}
