using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChasingState : EnemyState
{
    private bool _isChasing = false;
    private WaitUntil _waitUntilPlayerHasMoved;
    private Func<bool> _hasPlayerMoved;
    private int _randomStoppingDistance;
    private bool _canSeePlayer = false;

    public ChasingState(NavMeshAgent agent, EnemyEventManager eventManager, float walkSpeed, float sprintSpeed) : base(agent, eventManager)
    {
        _walkSpeed = walkSpeed;
        _sprintSpeed = sprintSpeed;
        _hasPlayerMoved = CheckIfPlayerHasMoved;
        _waitUntilPlayerHasMoved = new WaitUntil(_hasPlayerMoved);
        _eventManager.OnPlayerSeen += SetPlayerSeen;
    }
   

    public override void EnterState(AlertStatus alertStatus = AlertStatus.None)
    {
        //_eventManager.OnPlayerSeen += SetPlayerSeen;
        _eventManager.OnDestinationReached += SetDestinationReached;
        //Vector3 _playerPos = GameManager.Instance.GetPlayerPosition().position;
        _randomStoppingDistance = Random.Range(5,11);
        //_eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);
        
        _isChasing = true;
        if (_coroutine == null)
        {
            _eventManager.SpeedChanged(0.9f, 2f);
            _coroutine = CoroutineRunner.Instance.StartCoroutine(ChasePlayerRoutine());
        }
        //_eventManager.SpeedChanged(0.9f, 2f);
    }

    private void SetPlayerSeen(bool seen)
    {
        _canSeePlayer = seen;
    }

    private IEnumerator ChasePlayerRoutine()
    {
        while (_isChasing)
        {
            SetDestinationReached(false);
            Vector3 _playerPos = GameManager.Instance.GetPlayerPosition().position;
            _eventManager.DestinationUpdated(_playerPos, _randomStoppingDistance);

            while (!CheckIfPlayerHasMoved())
            {
                if (CheckDestinationReached())
                {
                    //Debug.LogError("Chasing Reached Dest");
                    _eventManager.SpeedChanged(0f, 10f);
                    //SetDestinationReached(false);
                }
                else
                {
                    if(_canSeePlayer)
                    {
                        _eventManager.SpeedChanged(_walkSpeed, 5f);
                    }
                    else
                    {
                        _eventManager.SpeedChanged(_sprintSpeed, 5f);
                    }
                }
                yield return null;
            }

            yield return null;



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

   
   
    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public override void LateUpdateState()
    {
        
    }

    public override void ExitState()
    {
        
        _isChasing = false;
        if(_coroutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
        //_eventManager.OnPlayerSeen -= SetPlayerSeen;
        _eventManager.OnDestinationReached -= SetDestinationReached;
        _canSeePlayer = false;
    }

    public override void OnStateDestroyed()
    {
        ExitState();
        _hasPlayerMoved = null;
        _waitUntilPlayerHasMoved = null;
        base.OnStateDestroyed();
    }

}
