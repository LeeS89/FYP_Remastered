using UnityEngine;
using UnityEngine.AI;


public partial class EnemyFSMController : ComponentEvents
{
    public bool testRespawn = false;
    public bool _testWaypoint = false;

    public bool _updateEnaboled = false;
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

        UpdateAgentSpeed();
    }

    public bool _testEnterPatrol = false;
    public bool testSeeView = false;
    public bool _testMelee = false;
    public bool _testRotation = false;

    private void LateUpdate()
    {
        if (!_agentIsActive) { return; }

        if (_testRotation)
        {
            _agent.updateRotation = false;
        }

        /* if (_testMelee)
         {
             if (_agent.pathPending)
             {
                 Debug.LogWarning("Agent path is pending, skipping update.");
             }
         }*/

        if (_testMelee)
        {
            _enemyEventManager.AnimationTriggered(AnimationAction.Melee);
            _testMelee = false;
        }

        if (!_playerIsDead && _agentIsActive)
        {
            UpdateFieldOfViewCheckFrequency();
        }

        if (_gun != null)
        {
            _gun.UpdateGun();
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

            RotateTowardsPlayer();
           // UpdateAnimatorDirection();
        }
       // UpdateAnimatorDirection();
        UpdateAnimator();
        //if (!_movementChanged) { return; }

        //UpdateAgentSpeed();
    }

    private void CheckCurrentPathValidity()
    {
        if (_agent.hasPath)
        {
            if (_agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                //_enemyEventManager.PathInvalid();
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
        //_enemyEventManager.DestinationReached(true);


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
            //_enemyEventManager.FacingTarget(true);
            return;
        }
        /*else
        {
            _enemyEventManager.FacingTarget(false);
        }*/

            // ✅ Smoothly rotate toward target
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f); // adjust speed as needed
    }

    #endregion
}
