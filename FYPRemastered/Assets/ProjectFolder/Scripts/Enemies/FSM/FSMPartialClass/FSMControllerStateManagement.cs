using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public partial class EnemyFSMController : ComponentEvents
{
    private WaypointData _wpData;

    #region FSM Management
    private void SetupFSM()
    {

        _path = new NavMeshPath();
        _fovCheckFrequency = _patrolFOVCheckFrequency;
        _fov = new TraceComponent(1);
        _fovTraceResults = new Collider[1];
        _animController = new EnemyAnimController(_anim, _enemyEventManager);
        _patrol = new PatrolState(_owningGameObject, _enemyEventManager, _stopAndWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_enemyEventManager, _owningGameObject, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_enemyEventManager, _owningGameObject, _path, _maxFlankingSteps);
        _deathState = new DeathState(_enemyEventManager, _owningGameObject);

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

            _enemyEventManager.WaypointsUpdated(_wpData);
            
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
        //GameManager._onPlayerMovedinternal += EnemyState.SetPlayerMoved;
    }

    public void ChangeState(EnemyState state, Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {
        if (_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;

        _destinationCheckAction = _currentState == _stationary ? StopImmediately : MeasurePathToDestination;



        _currentState.EnterState(destination, alertStatus, stoppingDistance);
        _enemyEventManager.PlayerSeen(_canSeePlayer);
        //_currentState.EnterState(destination);

       // Debug.LogError("Current State: " + _currentState.GetType().Name);
    }

    private void StationaryStateRequested(/*AlertStatus alertStatus*/)
    {
        if (_currentState == _stationary)
        {
            return;
        }

        ChangeState(_stationary, null, _alertStatus);
    }

    private void ChasingStateRequested(Vector3? destination = null)
    {
        if (_currentState == _chasing)
        {
            return;
        }
        
        ChangeState(_chasing, destination);
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
