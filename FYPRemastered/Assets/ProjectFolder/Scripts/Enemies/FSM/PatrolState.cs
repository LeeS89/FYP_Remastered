using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;



public class PatrolState : EnemyState
{
    private List<Vector3> _wayPoints;
    private List<Vector3> _waypointForwards;

    private WaitUntil _waitUntilDestinationReached;
    private Func<bool> _hasReachedDestination;
    private int index = 0;

    private float _randomWaitTime;
    private bool _isPatrolling = false;
    

    public PatrolState(List<Vector3> wayPoints, List<Vector3> waypointforwards, GameObject owner, EnemyEventManager eventManager, float randomDelay, float walkSpeed) : base(eventManager, owner)
    {
       
        _walkSpeed = walkSpeed;
        _randomWaitTime = randomDelay;
        _wayPoints = wayPoints;
        _waypointForwards = waypointforwards;
        _hasReachedDestination = CheckDestinationReached;
        _waitUntilDestinationReached = new WaitUntil(_hasReachedDestination);
    }

    
    public override void EnterState(Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float _ = 0)
    {
        _eventManager.OnDestinationReached += SetDestinationReached;
        if (_coroutine == null)
        {
            _isPatrolling = true;
            _coroutine = CoroutineRunner.Instance.StartCoroutine(TraverseWayPoints());
        }
    }


    IEnumerator TraverseWayPoints()
    {
        while (_isPatrolling)
        {
            SetDestinationReached(false);
            index = GetNextDestination();
           
            _eventManager.DestinationUpdated(_wayPoints[index]);
            
            _eventManager.SpeedChanged(_walkSpeed, 2f);
            

            yield return _waitUntilDestinationReached;
            Debug.LogError("Reached destination: " + _wayPoints[index]);

            _eventManager.SpeedChanged(0f, 10f);
           
            // Calculating the direction vector from the agent to the destination
            Vector3 directionToFace = _waypointForwards[index];

            //New rotation based on the direction vector
            Quaternion targetRotation = Quaternion.LookRotation(directionToFace);

            while (Quaternion.Angle(_owner.transform.rotation, targetRotation) > 2.0f)
            {
                // Smoothly rotating the agent towards the target rotation
                _owner.transform.rotation = Quaternion.Slerp(_owner.transform.rotation, targetRotation, Time.deltaTime * 2.0f);
                yield return null;
            }

            _eventManager.AnimationTriggered(AnimationAction.Look);
          
            float _delayTime = Random.Range(0, _randomWaitTime);
            float elapsedTime = 0.0f;
           
            while (elapsedTime < _delayTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
        }
    }

    public override void UpdateState() { }
    
   
    private int GetNextDestination()
    {
        int newIndex = 0;

        do
        {
            newIndex = Random.Range(0, _wayPoints.Count);
        }
        while(newIndex == index);

       
        return newIndex;
    }

  /*  private bool CheckDestinationReached()
    {
        return _destinationReached;
        
    }

  */

    public override void ExitState()
    {
        if(_coroutine != null)
        {
            _isPatrolling = false;
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _eventManager.OnDestinationReached -= SetDestinationReached;
    }

   

    public override void OnStateDestroyed()
    {
        ExitState();
        _wayPoints = null;
        _hasReachedDestination = null;
        _waitUntilDestinationReached = null;
        base.OnStateDestroyed();
        
    }

    
}
