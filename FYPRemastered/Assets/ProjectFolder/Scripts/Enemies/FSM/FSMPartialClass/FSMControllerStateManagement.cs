using UnityEngine;
using Random = UnityEngine.Random;

public partial class EnemyFSMController : ComponentEvents
{
    public AIDestinationRequestData _resourceRequest;
    public GameObject _testFlankCubes;
    
    #region FSM Management
    private void SetupFSM()
    {
        _resourceRequest = new AIDestinationRequestData();
        
        _destinationManager = new DestinationManager(_agentEventManager,_maxFlankingSteps, _testFlankCubes, transform, OnDestinationRequestComplete);
       
        _animController = new EnemyAnimController(_anim, _agentEventManager);
        _patrol = new PatrolState(_owningGameObject, _agentEventManager, _patrolPointWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_agentEventManager, _owningGameObject, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_agentEventManager, _owningGameObject);
        _deathState = new DeathState(_agentEventManager, _owningGameObject);
       

        
        _blockZone = _destinationManager.GetCurrentWPZone();
        Debug.LogError("WP ZOne: "+_blockZone);
        SceneEventAggregator.Instance.RegisterAgentAndZone(this, _blockZone);
        // InitializeWaypoints();
        //InitializeWeapon();

        // ChangeState(_patrol);

    }

   

  

   

    public void ChangeState(EnemyState state)
    {
       
        if (_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;

        _destinationCheckAction = _currentState == _stationary ? StopImmediately : MeasurePathToDestination;



        _currentState.EnterState();
       
        //Debug.LogError("Current State: " + _currentState.GetType().Name);
    }

    public void ChangeStates(EnemyState state)
    {
        if(_currentState == state) { return; }

        switch (state)
        {
            case PatrolState patrol:
                break;
            case StationaryState stationary:
                /*if(_currentState == _chasing)
                {
                    stationary.Alertstatus = AlertStatus.Alert;
                }*/
                break;
            case ChasingState chasingState:
                break;
            case DeathState deathState:
                break;
            default:
                break;
        }

        if (_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;

        _destinationCheckAction = _currentState == _stationary ? StopImmediately : MeasurePathToDestination;



        _currentState.EnterState();

        Debug.LogError("Current State: " + _currentState.GetType().Name);
    }



    private void StationaryStateRequested(AlertStatus alertStatus)
    {
        if (_currentState == _stationary)
        {
            return;
        }
        AlertStatusUpdated(alertStatus);
        ChangeState(_stationary);
    }

    private void ChasingStateRequested()
    {
        if (_currentState == _chasing)
        {
            return;
        }
        
        ChangeState(_chasing);
    }

    private void PatrolStateRequested()
    {
        if (_currentState == _patrol)
        {
            return;
        }

        ChangeState(_patrol);

    }

    

   

   

    private void OnDestinationRequestComplete(bool success, Vector3 destination, AIDestinationType destType)
    {
        if (!success) { return; }

        AlertStatus status = AlertStatus.None;
        int stoppingDistance = 0; 

        switch (destType)
        {
            case AIDestinationType.PatrolDestination:
                break;
            case AIDestinationType.ChaseDestination:
                
                ChasingStateRequested();
                stoppingDistance = Random.Range(4, 11);
                status = AlertStatus.Chasing;
                break;
            case AIDestinationType.FlankDestination:
                ChasingStateRequested();
                status = AlertStatus.Flanking;
                break;
            default:
                StationaryStateRequested(AlertStatus.None);
                return;
        }
        _resourceRequest.carvingCallback = () =>
        {
            if (_obstacle.enabled)
            {
                _obstacle.enabled = false;
            }
        };

        _resourceRequest.agentActiveCallback = () =>
        {
            if (!_agent.enabled)
            {
                ToggleAgent(true);

            }
            AlertStatusUpdated(status);
            _agent.stoppingDistance = stoppingDistance;
            _agent.SetDestination(destination);
            _agentEventManager.DestinationApplied();
        };
        _destinationManager.StartCarvingRoutine(_resourceRequest);


    }
  

    private bool CheckIfDestinationIsReached()
    {
        //Debug.LogError("Remainingdistance: "+_agent.remainingDistance);
        return _agent.remainingDistance <= (_agent.stoppingDistance + 0.25f);
    }

    public void ResetFSM(EnemyState contextState = null)
    {
        if (TargetInView)
        {
            TargetInViewStatusUpdated(false);
            //Debug.LogError("Target in view reset to false");
            // UpdateFieldOfViewResults(false);

        }

        _agentEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.Alert, 1, 0, 0.5f, true);

        switch (contextState)
        {
            case PatrolState: // ResetFSM called when the player dies, or this agent is respawning

                break;
            case DeathState: // ResetFSM called when this agent dies
                DisableAgent();
                break;
            default: // ResetFSM called when this agent is respawning
                contextState = _patrol;
                if (!AgentIsAlive)
                {
                    HandleAgentRespawn();
                }
                break;
        }

        Debug.LogError("Resetting FSM to state: " + contextState.GetType().Name);

        if (_currentState != contextState)
        {
            ChangeState(contextState);
        }
    }

    private void HandleAgentRespawn()
    {
        ToggleGameObject(true);
        _agent.enabled = true;
        AgentIsAlive = true;
    }
    #endregion
}
