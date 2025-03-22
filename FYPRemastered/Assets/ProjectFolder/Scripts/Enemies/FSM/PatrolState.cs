using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PatrolState : EnemyState
{
    private List<Transform> _wayPoints;
    private NavMeshAgent _agent;

    private WaitUntil _waitUntilDestinationReached;
    private Func<bool> hasReachedDestination;
    private int index = 0;

    private Vector3 _direction;
    private float distance;

    public PatrolState(EnemyFSMController fsm, List<Transform> wayPoints, NavMeshAgent agent) : base(fsm)
    {
        _wayPoints = wayPoints;
        _agent = agent;
        hasReachedDestination = CheckDestinationReached;
        _waitUntilDestinationReached = new WaitUntil(hasReachedDestination);
    }

    
    public override void EnterState()
    {
        _fsm.StartCoroutine(TraverseWayPoints());
    }


    IEnumerator TraverseWayPoints()
    {
        while (true)
        {
            index = GetNextDestination();

            _agent.SetDestination(_wayPoints[index].position);

            yield return _waitUntilDestinationReached;
            //_walk = false;

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
        return distance <= (_agent.stoppingDistance + 0.5f);

    }


    private void GetDistanceToDestination()
    {
        if (_wayPoints[index].gameObject.activeInHierarchy)
        {
            _direction = _agent.transform.position - _wayPoints[index].position;
            distance = _direction.magnitude;
        }
       
    }

    public override void ExitState()
    {
        throw new System.NotImplementedException();
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
