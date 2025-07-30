using System.Collections;
using UnityEngine;


public class StationaryState : EnemyState
{
    private int _agentId = 0;
  
    private bool _newStateRequestAlreadySent = false;
   
    private bool _isStationary = false;

    private bool _canFlankTarget = true;

    public StationaryState(EnemyEventManager eventManager, GameObject owner) : base(eventManager, owner)
    {
        _eventManager.OnPursuitConditionChanged += SetCanFlankTarget;
    }
    
   

    public override void EnterState()
    {
        //_eventManager.OnPlayerSeen += SetPlayerSeen;
        Debug.LogError("Entering Stationary State with Alert Status: " + _alertStatus);
        switch (_alertStatus)
        {

            case AlertStatus.None:

                break;
            case AlertStatus.Alert:

                _newStateRequestAlreadySent = false;
                _eventManager.SpeedChanged(0f, 10f);
                //SetPlayerSeen(_canSeePlayer);


                CoroutineRunner.Instance.StartCoroutine(DelayJobStart());
                if (_coroutine == null)
                {
                    _isStationary = true;
                    _coroutine = CoroutineRunner.Instance.StartCoroutine(PlayerVisibilityRoutine());
                }

                _eventManager.RotateTowardsTarget(true);

                break;
            default:
                //_eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                break;
        }
      
    }

    private IEnumerator DelayJobStart()
    {
        yield return new WaitForSeconds(0.5f);
       
        float bufferMultiplier = Random.Range(1.2f, 1.45f);
        _agentId = StationaryChaseManagerJob.Instance.RegisterAgent(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), bufferMultiplier, this);
       
    }

    private void SetCanFlankTarget(bool canFlank)
    {
        _canFlankTarget = canFlank;
    }


    private IEnumerator PlayerVisibilityRoutine()
    {
        while (_isStationary)
        {
            if (_canFlankTarget && !_canSeePlayer)
            {
                yield return new WaitForSeconds(1f);

                if (_canFlankTarget && !_canSeePlayer && !_newStateRequestAlreadySent)
                {
                    _newStateRequestAlreadySent = true;
                    _eventManager.DestinationRequested(AIDestinationType.FlankDestination);
                    _isStationary = false;
                    yield break;
                }
            }

            yield return null;
        }


/*
        if (!_canSeePlayer && !_destinationRequestComplete)
        {

            //_newDestinationAlreadyApplied = true;
            //_eventManager.RequestChasingState(point);
            _eventManager.RequestTargetPursuit(DestinationType.Flank);

            yield return _waitUntilDestinationRequestComplete;
            _destinationRequestComplete = false;

            if (_destinationRequestSuccess)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
            //yield break;
        }*/

       
    }


    /// <summary>
    /// Each time a StationaryChaseManagerJob job runs, it calls this function, passing the result of the distance from the player calculation
    /// and the players current position.
    /// If true is passed, a path check is done to determine if the player is reachable.
    /// </summary>
    /// <param name="canChase"> passes true if the distance between agent and player is greater than distance on entry * bufferMultiplier </param>
    /// <param name="targetPosition"></param>
    public override void SetChaseEligibility(bool canChase, Vector3 targetPosition)
    {

        base.SetChaseEligibility(canChase, targetPosition);

        if (canChase && !_newStateRequestAlreadySent)
        {
            _newStateRequestAlreadySent = true;
            _eventManager.DestinationRequested(AIDestinationType.ChaseDestination);

           /* if (!_queuedForNewFlankPoint)
            {
                _queuedForNewFlankPoint = true;
                _chasePlayerPathRequest.path = _path;
                _chasePlayerPathRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position);
                _chasePlayerPathRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(targetPosition);
                _eventManager.PathRequested(_chasePlayerPathRequest);

                _chasePlayerPathRequest.externalCallback = (success) =>
                {
                    _shouldReChasePlayer = success;
                    _queuedForNewFlankPoint = false;
                };

                if (_shouldReChasePlayer && !_newDestinationAlreadyApplied)
                {
                    _newDestinationAlreadyApplied = true;
                    _eventManager.RequestChasingState();
                }

            }*/

        }

    }

  
  

   
    public override void ExitState()
    {
        //_eventManager.OnDestinationRequestStatus -= SetDestinationRequestStatus;
      
        _eventManager.RotateTowardsTarget(false);
        
       
        _shouldReChasePlayer = false;
        if (_coroutine != null)
        {
            _isStationary = false;
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        StationaryChaseManagerJob.Instance.UnregisterAgent(_agentId);
     
    }

    public override void OnStateDestroyed()
    {
        _eventManager.OnPursuitConditionChanged -= SetCanFlankTarget;
        base.OnStateDestroyed();
       
     
    }

   
}
