using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChasingState : EnemyState
{
    DestinationRequestData _chasePlayerPathRequest;
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
        _chasePlayerPathRequest = new DestinationRequestData();
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
               // bool resultReceived = false;
               // bool isValid = false;

                //CheckNewPath(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), LineOfSightUtility.GetClosestPointOnNavMesh(_playerPos), _path, out isValid, out resultReceived);
               /* _chasePlayerPathRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position);
                _chasePlayerPathRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(_playerPos);
                _chasePlayerPathRequest.path = _path;
                _chasePlayerPathRequest.callback = (success) =>
                {
                    isValid = success;
                    resultReceived = true;
                };
*/
                //_eventManager.PathRequested(_chasePlayerPathRequest);

                //yield return new WaitUntil(() => resultReceived);

                //if (isValid)
                //{
                    _eventManager.DestinationUpdated(LineOfSightUtility.GetClosestPointOnNavMesh(_playerPos), _randomStoppingDistance);
               // }
                yield return new WaitForSeconds(0.15f);
            }
            
            yield return null;

        }
        _eventManager.RequestStationaryState();
       
    }

    private void CheckNewPath(Vector3 from, Vector3 to, NavMeshPath path, out bool valid, out bool result)
    {
        // Initialize the out parameters to ensure they are assigned before leaving the method
        valid = false;
        result = false;

        // Create local variables to capture the lambda results
        bool localValid = false;
        bool localResult = false;

        _chasePlayerPathRequest.start = from;
        _chasePlayerPathRequest.end = to;
        _chasePlayerPathRequest.path = path;
        _chasePlayerPathRequest.externalCallback = (success) =>
        {
            localValid = success;
            localResult = true;
        };

        // Simulate the callback execution (this part depends on how the callback is triggered in your system)
        _eventManager.PathRequested(_chasePlayerPathRequest);

        // Assign the local variables to the out parameters
        valid = localValid;
        result = localResult;
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
