using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChasingState : EnemyState
{
   
    private int _randomStoppingDistance;
   
    private Vector3 _playerPos;

    private enum ChaseType
    {
        ChasePlayer,
        FlankPlayer
    }
    private ChaseType _chaseType = ChaseType.ChasePlayer;

    public ChasingState(EnemyEventManager eventManager, GameObject owner, float walkSpeed, float sprintSpeed) : base(eventManager, owner)
    {
      
        _walkSpeed = walkSpeed;
        _sprintSpeed = sprintSpeed;
        _eventManager.OnDestinationReached += SetDestinationReached;
        //_hasPlayerMoved = CheckIfPlayerHasMoved;
        //_waitUntilPlayerHasMoved = new WaitUntil(_hasPlayerMoved);
        //_waitUntilPathCheckComplete = new WaitUntil(() => _pathCheckComplete);
        //_eventManager.OnPlayerSeen += SetPlayerSeen;
    }


    public override void EnterState(Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {

       // GameManager.OnPlayerMoved += SetPlayerMoved;

        //_eventManager.OnPlayerSeen += SetPlayerSeen;
        //_eventManager.OnDestinationReached += SetDestinationReached;
        _randomStoppingDistance = Random.Range(4, 11);


        
        if (_coroutine == null)
        {
            //_isChasing = true;
            //_eventManager.SpeedChanged(0.9f, 2f);
            if(destination == null)
            {
                _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
                _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);
                _chaseType = ChaseType.ChasePlayer;
                //_coroutine = CoroutineRunner.Instance.StartCoroutine(ChasePlayerRoutine());
            }
            else
            {
                _eventManager.DestinationUpdated(destination.Value);
                _eventManager.RotateTowardsTarget(true);
                _chaseType = ChaseType.FlankPlayer;
            }
            
            _coroutine = CoroutineRunner.Instance.StartCoroutine(ChasePlayerRoutine());

        }

    }


    private IEnumerator ChasePlayerRoutine()
    {
        SetDestinationReached(false);

        while (!CheckDestinationReached())
        {

            _eventManager.SpeedChanged(_canSeePlayer ? _walkSpeed : _sprintSpeed, 5f);

            if(_chaseType == ChaseType.ChasePlayer)
            {
                _eventManager.RotateTowardsTarget(_canSeePlayer);
            }

            if (CheckIfPlayerHasMoved())
            {
                _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
                _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);
                
                yield return new WaitForSeconds(0.15f);
            }
            
            yield return null;

        }
        _eventManager.RequestStationaryState();
       
    }

  
    public override void ExitState()
    {

        if (_coroutine != null)
        {
            _eventManager.RotateTowardsTarget(false);
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        //GameManager.OnPlayerMoved -= SetPlayerMoved;

        //_eventManager.OnPlayerSeen -= SetPlayerSeen;
        //_eventManager.OnDestinationReached -= SetDestinationReached;
        //_canSeePlayer = false;
    }

    public override void OnStateDestroyed()
    {
        ExitState();
        
        base.OnStateDestroyed();
    }

}
