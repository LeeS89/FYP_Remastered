using UnityEngine;
using UnityEngine.AI;


public partial class EnemyFSMController : ComponentEvents
{
    public bool testRespawn = false;
    public bool _testWaypoint = false;
    #region Updates
    void Update()
    {
        if (_testDeath)
        {
            GameManager.Instance.CharacterDied(CharacterType.Player);
            //_animController.DeadAnimation();
            //ChangeState(_chasing);
            //CarveOnDestinationReached(true);
            _testDeath = false;
        }

        if (testRespawn)
        {
            GameManager.PlayerRespawned();
            //_playerIsDead = false;
            testRespawn = false;
        }

        if (_testWaypoint)
        {
            ChangeWaypoints();
            _testWaypoint = false;
        }

        
    }

    public bool testSeeView = false;
    public UniformZoneGridManager _gridManager;
    private void LateUpdate()
    {

        if (testSeeView)
        {
            Vector3 newPoint = _gridManager.GetRandomPointXStepsFromPlayer(4);
            _enemyEventManager.DestinationUpdated(newPoint);
            //ChangeState(_stationary);
            testSeeView = false;
        }


        /*if (!testSeeView)
        {
            UpdateFieldOfViewCheckFrequency();
        }
        else
        {
            if (_canSeePlayer)
            {
                //_animController.SetAlertStatus(false);
                _enemyEventManager.PlayerSeen(false);
                _canSeePlayer = false;
                _enemyEventManager.ChangeAnimatorLayerWeight(1, 1, 0, 0.5f, false);
                ChangeState(_patrol);
            }
        }*/

        if (!_playerIsDead && _agentIsActive)
        {
            UpdateFieldOfViewCheckFrequency();
        }


        /*if (_testDeath)
        {
            //_animController.LookAround();
            //_agent.enabled = false;
            //_animController.EnemyDied();
            CompareDistances(transform.position, _player.position);
            //_testDeath = false;
        }*/

        if (_currentState != null)
        {
            _currentState.LateUpdateState();
        }

        CheckCurrentPathValidity();


        //MeasurePathToDestination();
        _destinationCheckAction?.Invoke();

        if (_rotatingTowardsTarget)
        {
            //if (_agent.updateRotation)
            //{
            //    _agent.updateRotation = false;
            //}

            RotateTowardsPlayer();
            UpdateAnimatorDirection();
        }
        /* if(_currentState != _stationary && _canSeePlayer)
         {
             if (_agent.updateRotation)
             {
                 _agent.updateRotation = false;
             }

             RotateTowardsPlayer();
             UpdateAnimatorDirection();
         }*/

        if (!_movementChanged) { return; }

        ApplyAnimatorSpeedValues();
    }

    private void CheckCurrentPathValidity()
    {
        if (_agent.hasPath)
        {
            if (_agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                _enemyEventManager.PathInvalid();
                //Debug.LogError("Path is invalid, resetting path.");
            }
        }
    }

    private void MeasurePathToDestination()
    {
        if (!_agent.enabled) { return; }
        //DebugExtension.DebugWireSphere(_agent.destination, Color.green);
        if (_currentState != null && _agent.hasPath && !_agent.pathPending)
        {
            if (CheckIfDestinationIsReached())
            {

                _agent.ResetPath();
                _enemyEventManager.DestinationReached(true);


            }

        }
    }

    /// <summary>
    /// Used when agent enters Stationary state
    /// </summary>
    private void StopImmediately()
    {
        //_agent.SetDestination(LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position));
        if (_agent.hasPath)
        {
            _agent.ResetPath();
        }
        _enemyEventManager.DestinationReached(true);


        _destinationCheckAction = null;
    }


    private void RotateTowardsPlayer()
    {
        Transform player = GameManager.Instance.GetPlayerPosition(PlayerPart.Position);
        if (player == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.001f)
            return;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float dot = Vector3.Dot(forward.normalized, toPlayer.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        const float precisionThreshold = 1f; // degrees

        Quaternion targetRotation = Quaternion.LookRotation(toPlayer);

        if (angle < precisionThreshold)
        {
            // ✅ Close enough — complete the rotation smoothly
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f); // forces it to land exactly, still smooth
            return;
        }

        // ✅ Smoothly rotate toward target
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 5f); // adjust speed as needed
    }

    #endregion
}
