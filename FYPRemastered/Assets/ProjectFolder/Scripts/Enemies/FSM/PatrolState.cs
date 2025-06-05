using System.Collections;
using System;
using UnityEngine;
using Random = UnityEngine.Random;



public class PatrolState : EnemyState
{

    private WaitUntil _waitUntilDestinationReached;
    private Func<bool> _hasReachedDestination;
   
    private float _randomWaitTime;
    private bool _isPatrolling = false;
   
    private Vector3 _patrolPointForward;


    public PatrolState(GameObject owner, EnemyEventManager eventManager, float randomDelay, float walkSpeed) : base(eventManager, owner)
    {
       
        _walkSpeed = walkSpeed;
        _randomWaitTime = randomDelay;
        _hasReachedDestination = CheckDestinationReached;
        _waitUntilDestinationReached = new WaitUntil(_hasReachedDestination);
    }

    
    public override void EnterState(/*Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float _ = 0*/)
    {
        _eventManager.OnRotateAtPatrolPoint += SetPatrolPointForwardVector;
      
        if (_coroutine == null)
        {
            _isPatrolling = true;
            _coroutine = CoroutineRunner.Instance.StartCoroutine(TraverseWayPointsNew());
        }
    }

  

    IEnumerator TraverseWayPointsNew()
    {

        //yield return new WaitForSeconds(3f);
       
        while (_isPatrolling)
        {
            _eventManager.DestinationReached(false);
            //SetDestinationReached(false);
         
            _eventManager.RequestTargetPursuit(DestinationType.Patrol);
            Debug.LogError("Patrol State: Requesting Patrol Destination");
            yield return _waitUntilDestinationApplied;
            _destinationApplied = false;

            _eventManager.SpeedChanged(_walkSpeed, 2f);


            yield return _waitUntilDestinationReached;


            _eventManager.SpeedChanged(0f, 10f);

            // Calculating the direction vector from the agent to the destination
            Vector3 directionToFace = _patrolPointForward;

            //New rotation based on the direction vector
            Quaternion targetRotation = Quaternion.LookRotation(directionToFace);

            while (Quaternion.Angle(_owner.transform.rotation, targetRotation) > 2.0f + Mathf.Epsilon)
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

    private void SetPatrolPointForwardVector(Vector3 fwd)
    {
        _patrolPointForward = fwd;
    }


    public override void ExitState()
    {
        if(_coroutine != null)
        {
            _isPatrolling = false;
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _eventManager.OnRotateAtPatrolPoint -= SetPatrolPointForwardVector;
       
    }

   

    public override void OnStateDestroyed()
    {
        ExitState();
       
        _hasReachedDestination = null;
        _waitUntilDestinationReached = null;
  
        base.OnStateDestroyed();
        
    }


    #region Save for later use
  /*  private void UpdateWaypoints()
    {

       // _wayPoints = _updatedWP;
      //  _waypointForwards = _updatedForwards;

        //_updatedWP = null;
        //_updatedForwards = null;


    }*/

   /* private void WaypointsUpdatePending(WaypointData wpData)
    {
        if (wpData.Equals(default(WaypointData)))
        {
            Debug.LogWarning("WaypointData is in its default state, and might not be initialized properly.");
            return;
        }

        //_updatedWP = wpData._waypointPositions;
        //_updatedForwards = wpData._waypointForwards;
        //_wpPendingUpdate = true;

    }*/
    #endregion

}
