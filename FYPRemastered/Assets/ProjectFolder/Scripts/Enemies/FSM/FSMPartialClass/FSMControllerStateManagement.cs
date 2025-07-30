using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public partial class EnemyFSMController : ComponentEvents
{
    public AIDestinationRequestData _resourceRequest;
    public GameObject _testFlankCubes;
    private WaypointData _wpData;

    //private DestinationRequestData _destinationData;

    #region FSM Management
    private void SetupFSM()
    {
        _resourceRequest = new AIDestinationRequestData();
        _path = new NavMeshPath();
        _destinationManager = new DestinationManager(_enemyEventManager,_maxFlankingSteps, new NavMeshPath(), _testFlankCubes, transform, OnDestinationRequestComplete, _lineOfSightMask, _fovLayerMask);
        //_destinationData = new DestinationRequestData();
        
     
        _animController = new EnemyAnimController(_anim, _enemyEventManager);
        _patrol = new PatrolState(_owningGameObject, _enemyEventManager, _stopAndWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_enemyEventManager, _owningGameObject, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_enemyEventManager, _owningGameObject);
        _deathState = new DeathState(_enemyEventManager, _owningGameObject);
       

        

       // InitializeWaypoints();
        //InitializeWeapon();

       // ChangeState(_patrol);

    }

    private void ChangeWaypoints()
    {
        BlockData temp = _blockData;
        InitializeWaypoints();
        
       // BaseSceneManager._instance.ReturnWaypointBlock(temp);
        
    }

    private void InitializeWaypoints()
    {
        _resourceRequest.flankBlockingMask = _lineOfSightMask;
        _resourceRequest.flankTargetMask = _fovLayerMask;
        _resourceRequest.flankTargetColliders = GameManager.Instance.GetPlayerTargetPoints();
        //_blockData = BaseSceneManager._instance.RequestWaypointBlock();
        _resourceRequest.resourceType = AIResourceType.WaypointBlock;
        _resourceRequest.waypointCallback = (blockData) =>
        {
            if (blockData != null)
            {
                _blockData = blockData;
                SetWayPoints();
            }
            else
            {
                Debug.LogError("No valid waypoint block data found!");
                
            }
        };

        SceneEventAggregator.Instance.RequestResource(_resourceRequest);

      
    }

    
    private void SetWayPoints()
    {
        if (_blockData != null)
        {
            _blockZone = _blockData._blockZone;
            //Debug.LogError("Block Zone for enemy: " + _blockZone);

            _wpData.UpdateData(_blockData._waypointPositions.ToList(), _blockData._waypointForwards.ToList());

            //_destinationManager.LoadWaypointData(_wpData);
            //_enemyEventManager.WaypointsUpdated(_wpData);

            SceneEventAggregator.Instance.RegisterAgentAndZone(this, _blockZone);
            //BaseSceneManager._instance.RegisterAgentAndZone(this, _blockZone);

           
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

        _eventManager.SetupGun(_owningGameObject, _enemyEventManager, _bulletSpawnPoint, 5, playerTransform);
        //_gun = new GunBase(_bulletSpawnPoint, playerTransform, _enemyEventManager, _owningGameObject);
   
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

    private void PursuitTargetRequested(AIDestinationType chaseType)
    {
        switch (chaseType)
        {
            case AIDestinationType.ChaseDestination:
                AttemptTargetChase();
                break;
            case AIDestinationType.FlankDestination:
                AttemptTargetFlank();
                break;
            case AIDestinationType.PatrolDestination:

                AttemptPatrol();

                break;
            default:
                Debug.LogError("Invalid chase type requested: " + chaseType);
                break;
        }
    }

    private bool IsStaleRequest(AIDestinationType expectedType)
    {
        return expectedType != _resourceRequest.destinationType;
    }

    private void AttemptPatrol()
    {
       

        _resourceRequest.destinationType = AIDestinationType.PatrolDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        
        _resourceRequest.path = _path;

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.PatrolDestination)) { return; }
            DestinationRequestResult(success, point, AlertStatus.None);
           
        };
        _destinationManager.RequestNewDestination(_resourceRequest);
    }

    private void AttemptTargetChase()
    {
        ChasingStateRequested();

        _resourceRequest.destinationType = AIDestinationType.ChaseDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        /*Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        _destinationData.end = LineOfSightUtility.GetClosestPointOnNavMesh(playerPos);*/
        _resourceRequest.path = _path;

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.ChaseDestination)) { return; }
            int _randomStoppingDistance = Random.Range(4, 11);
            DestinationRequestResult(success, point, AlertStatus.Chasing, _randomStoppingDistance);
            //StartCoroutine(DestinationRequestResult(success, point, AlertStatus.Chasing, _randomStoppingDistance));
        };
        _destinationManager.RequestNewDestination(_resourceRequest);

    }
    
    private void AttemptTargetFlank()
    {
        // First enter the Chasing state here
        ChasingStateRequested();
        // In chasing, before the wile loop in the coroutine, yield wait until => Destination found


        // Send Destination Request here
        _resourceRequest.destinationType = AIDestinationType.FlankDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        
        _resourceRequest.path = _path;

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.FlankDestination)) { return; }
            DestinationRequestResult(success, point, AlertStatus.Flanking);
            //StartCoroutine(DestinationRequestResult(success, point, AlertStatus.Flanking));
        };
        _destinationManager.RequestNewDestination(_resourceRequest);
        // If request fails => Request Player destination

        // If 2nd request fails => Later implement a fallback 
    }

    private void DestinationRequestResult(bool success, Vector3 destination, AlertStatus status = AlertStatus.None, int stoppingDistance = 0)
    {
        if (success)
        {
          
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
                _enemyEventManager.DestinationApplied();
            };
            _destinationManager.StartCarvingRoutine(_resourceRequest); // Move to extension function
            
           /* yield return new WaitUntil(() => _agent.enabled);
            _agent.SetDestination(destination);
            _enemyEventManager.DestinationApplied();*/
        }
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
            _enemyEventManager.DestinationApplied();
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
        _agentIsActive = true;
    }
    #endregion



    #region Obsolete
    private IEnumerator SetNewDestination(Vector3 destination)
    {
        if (_obstacle.enabled)
        {
            yield return new WaitUntil(() => !_obstacle.enabled);
        }
        if (!_agent.enabled)
        {
            ToggleAgent(true);
        }

        _agent.SetDestination(destination);
    }
    #endregion
}
