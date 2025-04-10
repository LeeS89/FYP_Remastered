using UnityEngine;

public partial class EnemyFSMController : ComponentEvents
{
    public bool testRespawn = false;
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
    }

    public bool testSeeView = false;
    private void LateUpdate()
    {

        if (testSeeView)
        {
            ChangeState(_stationary);
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
        //MeasurePathToDestination();
        _destinationCheckAction?.Invoke();

        if (!_movementChanged) { return; }

        UpdateAnimatorSpeed();
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
        _agent.SetDestination(_agent.transform.position);
        _agent.ResetPath();
        _enemyEventManager.DestinationReached(true);


        _destinationCheckAction = null;
    }


    #endregion
}
