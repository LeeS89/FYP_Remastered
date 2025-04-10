using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChasingState : EnemyState
{
    private bool _isChasing = false;
    //private WaitUntil _waitUntilPlayerHasMoved;
    private WaitUntil _waitUntilPathCheckComplete;
    //private Func<bool> _hasPlayerMoved;
    private int _randomStoppingDistance;
    private bool _canSeePlayer = false;
    private bool _pathCheckComplete = false;
    private bool _shouldUpdateDestination = false;
    private GameObject _owner;
    private NavMeshPath _path;
    private Vector3 _playerPos;

    public ChasingState(EnemyEventManager eventManager, GameObject owner, float walkSpeed, float sprintSpeed) : base(eventManager)
    {
        _owner = owner;
        _path = new NavMeshPath();
        _walkSpeed = walkSpeed;
        _sprintSpeed = sprintSpeed;
        _eventManager.OnDestinationReached += SetDestinationReached;
        //_hasPlayerMoved = CheckIfPlayerHasMoved;
        //_waitUntilPlayerHasMoved = new WaitUntil(_hasPlayerMoved);
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathCheckComplete);
        //_eventManager.OnPlayerSeen += SetPlayerSeen;
    }


    public override void EnterState(AlertStatus alertStatus = AlertStatus.None, float _ = 0)
    {

        GameManager.OnPlayerMoved += SetPlayerMoved;

        _eventManager.OnPlayerSeen += SetPlayerSeen;
        //_eventManager.OnDestinationReached += SetDestinationReached;
        _randomStoppingDistance = Random.Range(5, 11);


        
        if (_coroutine == null)
        {
            _isChasing = true;
            //_eventManager.SpeedChanged(0.9f, 2f);
            _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
            _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);
            _coroutine = CoroutineRunner.Instance.StartCoroutine(ChasePlayerRoutine());
            
        }

    }

    private void SetPlayerSeen(bool seen)
    {
        _canSeePlayer = seen;
    }

    private IEnumerator ChasePlayerRoutine()
    {
        //SetDestinationReached(false);
        /*Vector3 _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);*/

        while (_isChasing)
        {
            //SetDestinationReached(false);

            if (!CheckDestinationReached())
            {
                _eventManager.SpeedChanged(_canSeePlayer ? _walkSpeed : _sprintSpeed, 5f);


                if (CheckIfPlayerHasMoved())
                {
                    _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
                    _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);
                    yield return new WaitForSeconds(0.25f);
                }
                yield return null;
            }
            else
            {
                /// Change to stationary state here
                _eventManager.RequestStationaryState(AlertStatus.Alert, _randomStoppingDistance);
                _isChasing = false;
            }
                

            //while (!CheckIfPlayerHasMoved())
            //{
            //    if (CheckDestinationReached())
            //    {                   
            //        _eventManager.SpeedChanged(0f, 10f);

            //        //SetDestinationReached(false);
            //    }
            //    else
            //    {
            //        _eventManager.SpeedChanged(_canSeePlayer ? _walkSpeed : _sprintSpeed, 5f);

            //    }
            //    yield return null;
            //}

            //while (CheckIfPlayerHasMoved())
            //{
            //    CheckIfDestinationShouldUpdate();

            //    yield return _waitUntilPathCheckComplete;


            //    if (_shouldUpdateDestination)
            //    {
            //        _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
            //        _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);
            //        _eventManager.SpeedChanged(_canSeePlayer ? _walkSpeed : _sprintSpeed, 5f);
            //        _shouldUpdateDestination = false;
            //        SetDestinationReached(false);
            //    }


            //}
            //yield return null;
            /*while (CheckIfPlayerHasMoved())
            {
                _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
                _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);
                _eventManager.SpeedChanged(_canSeePlayer ? _walkSpeed : _sprintSpeed, 5f);
                
                yield return new WaitForSeconds(0.25f);
            }*/





            /* if (CheckDestinationReached())
             {
                 Debug.LogError("Chasing Stopped");
                 //_eventManager.StopImmediately();
                 //_eventManager.DestinationUpdated(_agent.transform.position);

                 _eventManager.SpeedChanged(0f, 2f);
                 SetDestinationReached(false);
             }

             yield return _waitUntilPlayerHasMoved;*/
        }
        
    }

    private void CheckIfDestinationShouldUpdate()
    {
        _pathCheckComplete = false;

        PathCheckRequest request = new PathCheckRequest
        {
            From = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position),//_owner.transform.position,
            To = LineOfSightUtility.GetClosestPointOnNavMesh(GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position),
            Path = _path,
            OnComplete = (distance) =>
            {
                _shouldUpdateDestination = distance > _randomStoppingDistance;
                _pathCheckComplete = true;
            }
        };

        PathDistanceBatcher.RequestCheck(request);
    }

    public override void ExitState()
    {

        _isChasing = false;
        if (_coroutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        GameManager.OnPlayerMoved -= SetPlayerMoved;

        _eventManager.OnPlayerSeen -= SetPlayerSeen;
        //_eventManager.OnDestinationReached -= SetDestinationReached;
        //_canSeePlayer = false;
    }

    public override void OnStateDestroyed()
    {
        ExitState();
        //_hasPlayerMoved = null;
        //_waitUntilPlayerHasMoved = null;
        base.OnStateDestroyed();
    }

}
