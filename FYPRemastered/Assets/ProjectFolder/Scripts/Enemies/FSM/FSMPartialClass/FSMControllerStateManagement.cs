using UnityEngine;
using Random = UnityEngine.Random;

public partial class EnemyFSMController : FSMControllerBase
{
   
    #region FSM Management
    protected override void SetupFSM()
    {
        base.SetupFSM();
        _animController = new EnemyAnimController(_anim, _agentEventManager);
       
    }

   
    protected override void ChangeState(EnemyState state, AlertStatus status = AlertStatus.None)
    {
       
        if(_currentState == state) { return; }

        if (_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;

        _destinationCheckAction = _currentState == _stationary ? StopImmediately : MeasurePathToDestination;

        _currentState.EnterState(status);
 
    }


    protected override void StationaryStateRequested(AlertStatus alertStatus)
    {
        if (_currentState == _stationary)
        {
            return;
        }
        ChangeState(_stationary, alertStatus);
    }

 

    protected override void OnDestinationRequestComplete(bool success, Vector3 destination, AIDestinationType destType)
    {
        if (!success)
        {
            ChangeState(_stationary);
            return;
        }
        
        int stoppingDistance = 0; 

        switch (destType)
        {
            case AIDestinationType.PatrolDestination:
                break;
            case AIDestinationType.ChaseDestination:
                ChangeState(_chasing, AlertStatus.Chasing);
                stoppingDistance = Random.Range(4, 11);
                break;
            case AIDestinationType.FlankDestination:
                ChangeState(_chasing, AlertStatus.Flanking);
                break;
            default:
                ChangeState(_stationary);
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

    public override void ResetFSM(EnemyState contextState = null)
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
                DisableAgentAndObstacle();
                break;
            default: // ResetFSM called when this agent is respawning
                contextState = _patrol;
                if (OwnerIsDead)
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
        ToggleAgent(true);
        //_agent.enabled = true;
        OwnerIsDead = false;
        _agentEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.Combat, 0, 1, 0.5f);
        OwnerDeathStatusChanged(false);
        _agentEventManager.AgentRespawn();
        _alertStatus = AlertStatus.None;
        //AgentIsAlive = true;
    }
    #endregion
}
