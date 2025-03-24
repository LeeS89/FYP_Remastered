using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class PatrolState : EnemyState
{
    
    private float _walkSpeed;

    private List<Transform> _wayPoints;
    private NavMeshAgent _agent;

    private WaitUntil _waitUntilDestinationReached;
    private Func<bool> hasReachedDestination;
    private int index = 0;

    private Vector3 _direction;
    private float _distance;
    private float _randomWaitTime;
    private bool _isPatrolling = false;
    

    public PatrolState(EnemyFSMController fsm, List<Transform> wayPoints, NavMeshAgent agent, float randomDelay, float walkSpeed) : base(fsm)
    {
        _walkSpeed = walkSpeed;
        _randomWaitTime = randomDelay;
        _wayPoints = wayPoints;
        _agent = agent;
        hasReachedDestination = CheckDestinationReached;
        _waitUntilDestinationReached = new WaitUntil(hasReachedDestination);
    }

    
    public override void EnterState()
    {
        if (_coroutine == null)
        {
            _isPatrolling = true;
            _coroutine = _fsm.StartCoroutine(TraverseWayPoints());
        }
    }


    IEnumerator TraverseWayPoints()
    {
        while (_isPatrolling)
        {
            index = GetNextDestination();

            _agent.SetDestination(_wayPoints[index].position);
            SpeedChanged(_walkSpeed, 2f);
            //OnSpeedChanged?.Invoke(_walkSpeed, 2f);

            yield return _waitUntilDestinationReached;

            SpeedChanged(0f, 10f);
            //OnSpeedChanged?.Invoke(0.0f, 10f);
            // Calculating the direction vector from the agent to the destination
            Vector3 directionToFace = _wayPoints[index].forward;

            //New rotation based on the direction vector
            Quaternion targetRotation = Quaternion.LookRotation(directionToFace);

            while (Quaternion.Angle(_agent.transform.rotation, targetRotation) > 2.0f)
            {
                // Smoothly rotating the agent towards the target rotation
                _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRotation, Time.deltaTime * 3.0f);
                yield return null;
            }

            AnimationTriggered(AnimationAction.Look);
            

            float _delayTime = Random.Range(0, _randomWaitTime);
            float elapsedTime = 0.0f;
            //Debug.LogError("Delay Time: " + _delayTime);
            while (elapsedTime < _delayTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }


        }
    }

    private int GetNextDestination()
    {
        int newIndex = 0;
        int _nextIndex = UnityEngine.Random.Range(0, _wayPoints.Count - 1);

        if (_wayPoints[_nextIndex].gameObject.activeInHierarchy)
        {
            newIndex = _nextIndex;
            //return _nextIndex;
        }
        else
        {
            for (int i = 0; i < _wayPoints.Count; i++)
            {
                if (_wayPoints[i].gameObject.activeInHierarchy)
                {
                    newIndex = i;
                    break;
                }
            }
        }
        return newIndex;
    }
    private bool CheckDestinationReached()
    {
        
        GetDistanceToDestination();
        return _distance <= (_agent.stoppingDistance + 0.5f);

    }


    private void GetDistanceToDestination()
    {
        if (_wayPoints[index].gameObject.activeInHierarchy)
        {
            _direction = _agent.transform.position - _wayPoints[index].position;
            _distance = _direction.magnitude;
        }
       
    }

    public override void ExitState()
    {
        if(_coroutine != null)
        {
            _isPatrolling = false;
            _fsm.StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateDestroyed()
    {
        ExitState();
        _wayPoints = null;
        _agent = null;
        hasReachedDestination = null;
        _waitUntilDestinationReached = null;
    }
}
