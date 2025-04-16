using Unity.VisualScripting;
using UnityEngine;

public partial class EnemyFSMController : ComponentEvents
{
    #region FSM Management
    private void SetupFSM()
    {
        _fovCheckFrequency = _patrolFOVCheckFrequency;
        _fov = new TraceComponent(1);
        _fovTraceResults = new Collider[1];
        _animController = new EnemyAnimController(_anim, _enemyEventManager);
        _patrol = new PatrolState(_wayPoints, _owningGameObject, _enemyEventManager, _stopAndWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_enemyEventManager, _owningGameObject, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_enemyEventManager, _owningGameObject);
        _deathState = new DeathState(_enemyEventManager);

        Transform playerTransform = GameManager.Instance.GetPlayerPosition(PlayerPart.DefenceCollider);
        _gun = new Gun(_bulletSpawnPoint, playerTransform, _enemyEventManager, _owningGameObject);
        GameManager._onPlayerMovedinternal += EnemyState.SetPlayerMoved;

        ChangeState(_patrol); // uncomment later

    }


    public void ChangeState(EnemyState state, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {
        if (_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;


        _destinationCheckAction = _currentState == _stationary ? StopImmediately : MeasurePathToDestination;

        _currentState.EnterState(AlertStatus.Alert, stoppingDistance);
        //Debug.LogError("Current State: " + _currentState.GetType().Name);
    }

    private void StationaryStateRequested(AlertStatus alertStatus, float stoppingDistance)
    {
        ChangeState(_stationary, alertStatus, stoppingDistance);
    }

    private void ChasingStateRequested()
    {
        ChangeState(_chasing);
    }

    private bool CheckIfDestinationIsReached()
    {
        //Debug.LogError("Remainingdistance: "+_agent.remainingDistance);
        return _agent.remainingDistance <= (_agent.stoppingDistance + 0.2f);
    }

    public void ResetFSM(EnemyState contextState = null)
    {
        if (_canSeePlayer)
        {
            UpdateFieldOfViewResults(false);

        }

        switch (contextState)
        {
            case PatrolState: // ResetFSM called when the player dies, or this agent is respawning

                break;
            case DeathState: // ResetFSM called when this agent dies
                DisableAgent();
                break;
            default: // ResetFSM called when this agent is respawning
                contextState = _patrol;
                if (!_agentIsActive)
                {
                    HandleAgentRespawn();
                }
                break;
        }
        if (_currentState != contextState)
        {
            ChangeState(contextState);
        }
    }

    private void HandleAgentRespawn()
    {
        ToggleGameObject(true);
        _agent.enabled = true;
        _agentIsActive = true;
    }
    #endregion
}
