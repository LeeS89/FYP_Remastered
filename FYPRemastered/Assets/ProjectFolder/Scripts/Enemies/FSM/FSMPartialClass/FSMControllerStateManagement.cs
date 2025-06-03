using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public partial class EnemyFSMController : ComponentEvents
{
    private WaypointData _wpData;
    private DestinationRequestData _destinationData;

    #region FSM Management
    private void SetupFSM()
    {
        _gridManager = MoonSceneManager._instance.GetGridManager();

        _destinationData = new DestinationRequestData();
        _path = new NavMeshPath();
        _fovCheckFrequency = _patrolFOVCheckFrequency;
        _fov = new TraceComponent(1);
        _fovTraceResults = new Collider[1];
        _animController = new EnemyAnimController(_anim, _enemyEventManager);
        _patrol = new PatrolState(_owningGameObject, _enemyEventManager, _stopAndWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_enemyEventManager, _owningGameObject, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_enemyEventManager, _owningGameObject);
        _deathState = new DeathState(_enemyEventManager, _owningGameObject);
        _destinationManager = new DestinationManager(_enemyEventManager, _gridManager, _maxFlankingSteps);

        

        InitializeWaypoints();
        InitializeWeapon();

       // ChangeState(_patrol);

    }

    private void ChangeWaypoints()
    {
        BlockData temp = _blockData;
        InitializeWaypoints();
        
        BaseSceneManager._instance.ReturnWaypointBlock(temp);
        
    }

    private void InitializeWaypoints()
    {

        _blockData = BaseSceneManager._instance.RequestWaypointBlock();

        if (_blockData != null)
        {
            _blockZone = _blockData._blockZone;
            //Debug.LogError("Block Zone for enemy: " + _blockZone);

            _wpData.UpdateData(_blockData._waypointPositions.ToList(), _blockData._waypointForwards.ToList());

            _destinationManager.LoadWaypointData(_wpData);
            //_enemyEventManager.WaypointsUpdated(_wpData);

            BaseSceneManager._instance.RegisterAgentAndZone(this, _blockZone);
        }
    }


    private void InitializeWeapon()
    {
        Transform playerTransform = GameManager.Instance.GetPlayerPosition(PlayerPart.DefenceCollider);
        if (playerTransform == null)
        {
            Debug.LogError("Player transform not found!");
            return;
        }
        _gun = new Gun(_bulletSpawnPoint, playerTransform, _enemyEventManager, _owningGameObject);
   
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
       
        Debug.LogError("Current State: " + _currentState.GetType().Name);
    }



    private void StationaryStateRequested(AlertStatus alertStatus/* = AlertStatus.None*/)
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

    private void PursuitTargetRequested(DestinationType chaseType)
    {
        switch (chaseType)
        {
            case DestinationType.Chase:
                AttemptTargetChase();
                break;
            case DestinationType.Flank:
                AttemptTargetFlank();
                break;
            case DestinationType.Patrol:

                AttemptPatrol();

                break;
            default:
                Debug.LogError("Invalid chase type requested: " + chaseType);
                break;
        }
    }

    private void AttemptPatrol()
    {
       // PatrolStateRequested();

        _destinationData.destinationType = DestinationType.Patrol;
        _destinationData.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        //_destinationData.end = LineOfSightUtility.GetClosestPointOnNavMesh(position.Value);
        _destinationData.path = _path;

        _destinationData.externalCallback = (success, point) =>
        {
            DestinationRequestResult(success, point, AlertStatus.None);
        };
        _destinationManager.RequestNewDestination(_destinationData);
    }

    private void AttemptTargetChase()
    {
        ChasingStateRequested();

        _destinationData.destinationType = DestinationType.Chase;
        _destinationData.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        /*Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        _destinationData.end = LineOfSightUtility.GetClosestPointOnNavMesh(playerPos);*/
        _destinationData.path = _path;

        _destinationData.externalCallback = (success, point) =>
        {
            int _randomStoppingDistance = Random.Range(4, 11);
            DestinationRequestResult(success, point, AlertStatus.Chasing, _randomStoppingDistance);
        };
        _destinationManager.RequestNewDestination(_destinationData);

    }
    
    private void AttemptTargetFlank()
    {
        // First enter the Chasing state here
        ChasingStateRequested();
        // In chasing, before the wile loop in the coroutine, yield wait until => Destination found
        
        
        // Send Destination Request here
        _destinationData.destinationType = DestinationType.Flank;
        _destinationData.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        
        _destinationData.path = _path;

        _destinationData.externalCallback = (success, point) =>
        {
            DestinationRequestResult(success, point, AlertStatus.Flanking);
        };
        _destinationManager.RequestNewDestination(_destinationData);
        // If request fails => Request Player destination

        // If 2nd request fails => Later implement a fallback 
    }

    private void DestinationRequestResult(bool success, Vector3 destination, AlertStatus status = AlertStatus.None, int stoppingDistance = 0)
    {
        if (success)
        {
            _destinationData.carvingCallback = () =>
            {
                if (_obstacle.enabled)
                {
                    _obstacle.enabled = false;
                }
            };

            _destinationData.agentActiveCallback = () =>
            {
                if (!_agent.enabled)
                {
                    ToggleAgent(true);
                    
                }
                AlertStatusUpdated(status);
                _agent.stoppingDistance = stoppingDistance;
                _agent.SetDestination(destination);
                _enemyEventManager.DestinationApplied();
            };
            _destinationManager.StartCarvingRoutine(_destinationData);
        }
    }

    private bool CheckIfDestinationIsReached()
    {
        //Debug.LogError("Remainingdistance: "+_agent.remainingDistance);
        return _agent.remainingDistance <= (_agent.stoppingDistance + 0.25f);
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
