using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.AI;

public class StationaryState : EnemyState
{
    private int _agentId = 0;
  
    
    private int _maxSteps = 0;
    private bool _isInState = false;

    private UniformZoneGridManager _gridManager;
    private DestinationRequestData _chasePlayerPathRequest;
    private DestinationRequestData _flankePlayerPathRequest;

    private bool _queuedForNewFlankPoint = false;
    private bool _newStateRequestAlreadySent = false;
    //private bool _initialSeenCheck = true;
    List<Vector3> _newPoints;
    List<int> _stepsToTry;
    private bool _isStationary = false;  

    public StationaryState(EnemyEventManager eventManager, GameObject owner, NavMeshPath path, int maxSteps) : base(eventManager, path, owner) 
    { 
        _maxSteps = maxSteps;
        
        _gridManager = GameObject.FindFirstObjectByType<UniformZoneGridManager>();
        _chasePlayerPathRequest = new DestinationRequestData();
        _flankePlayerPathRequest = new DestinationRequestData();
        _stepsToTry = new List<int>();
        _newPoints = new List<Vector3>();
    }
   
/*
    public override void EnterState(Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {
        //_eventManager.OnPlayerSeen += SetPlayerSeen;
        switch (alertStatus)
        {
            case AlertStatus.None:

                break;
            case AlertStatus.Alert:

                //_eventManager.OnDestinationRequestStatus += SetDestinationRequestStatus; //Delete
                //_initialSeenCheck = true; // Delete
                //_queuedForNewFlankPoint = false;
                _newStateRequestAlreadySent = false;
                _eventManager.SpeedChanged(0f, 10f);
                //SetPlayerSeen(_canSeePlayer);


                CoroutineRunner.Instance.StartCoroutine(DelayJobStart());
                if(_coroutine == null)
                {
                    _isStationary = true;
                    _coroutine = CoroutineRunner.Instance.StartCoroutine(PlayervisibilityRoutine());
                }
              
                _eventManager.RotateTowardsTarget(true);

                break;
            default:
                _eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                break;
        }
        _owner.GetComponent<NavMeshAgent>().updateRotation = false;
       
    }*/

    public override void EnterState()
    {
        //_eventManager.OnPlayerSeen += SetPlayerSeen;
        Debug.LogError("Entering Stationary State with Alert Status: " + _alertStatus);
        switch (_alertStatus)
        {

            case AlertStatus.None:

                break;
            case AlertStatus.Alert:

                //_eventManager.OnDestinationRequestStatus += SetDestinationRequestStatus; //Delete
                //_initialSeenCheck = true; // Delete
                //_queuedForNewFlankPoint = false;
                _newStateRequestAlreadySent = false;
                _eventManager.SpeedChanged(0f, 10f);
                //SetPlayerSeen(_canSeePlayer);


                CoroutineRunner.Instance.StartCoroutine(DelayJobStart());
                if (_coroutine == null)
                {
                    _isStationary = true;
                    _coroutine = CoroutineRunner.Instance.StartCoroutine(PlayervisibilityRoutine());
                }

                _eventManager.RotateTowardsTarget(true);

                break;
            default:
                //_eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                break;
        }
        //_owner.GetComponent<NavMeshAgent>().updateRotation = false;

    }

    private IEnumerator DelayJobStart()
    {
        yield return new WaitForSeconds(0.5f);
        //_isInState = true;
        //SetPlayerSeen(_canSeePlayer);
        float bufferMultiplier = Random.Range(1.2f, 1.45f);
        _agentId = StationaryChaseManagerJob.Instance.RegisterAgent(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), bufferMultiplier, this);
       
    }

   

    private IEnumerator AttemptFlankRoutine()
    {
        GetStepsToTry();

        foreach (int step in _stepsToTry)
        {

            _newPoints = _gridManager.GetCandidatePointsAtStep(step);

            if (_newPoints.Count == 0) { continue; }

            _newPoints = _newPoints.OrderBy(p => Random.value).ToList();

            foreach (var point in _newPoints)
            {

                bool resultReceived = false;
                bool isValid = false;
                _flankePlayerPathRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position);
                _flankePlayerPathRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);
                _flankePlayerPathRequest.path = _path;
                _flankePlayerPathRequest.externalCallback = (success, point) =>
                {
                    isValid = success;
                    resultReceived = true;
                };

                _eventManager.PathRequested(_flankePlayerPathRequest);

                yield return new WaitUntil(() => resultReceived);

                if (!isValid) continue; 

                if (_newStateRequestAlreadySent)
                {

                    yield break;
                }
                else
                {
                    if (!_canSeePlayer)
                    {

                        _newStateRequestAlreadySent = true;
                        _eventManager.RequestChasingState();
                        //_eventManager.RequestTargetPursuit(DestinationType.Flank);
                        yield break;
                    }
                }

            }

        }
        _stepsToTry.Clear();
        _coroutine = null;


    }

    private IEnumerator PlayervisibilityRoutine()
    {
        while (_isStationary)
        {
            if (!_canSeePlayer)
            {
                yield return new WaitForSeconds(1f);

                if (!_canSeePlayer && !_newStateRequestAlreadySent)
                {
                    _newStateRequestAlreadySent = true;
                    _eventManager.RequestTargetPursuit(DestinationType.Flank);
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

    private void GetStepsToTry()
    {
        int randomIndex = Random.Range(4, _maxSteps + 1);
        int temp = randomIndex;
        while (temp >= 4)
        {
            _stepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp <= _maxSteps)
        {
            _stepsToTry.Add(temp);
            temp++;
        }
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
            _eventManager.RequestTargetPursuit(DestinationType.Chase);

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

    /// <summary>
    /// Each time a StationaryChaseManagerJob job runs, it calls this function, passing the result of the distance from the player calculation
    /// and the players current position.
    /// If true is passed, a path check is done to determine if the player is reachable.
    /// </summary>
    /// <param name="canChase"> passes true if the distance between agent and player is greater than distance on entry * bufferMultiplier </param>
    /// <param name="targetPosition"></param>
   /* public override void SetChaseEligibility(bool canChase, Vector3 targetPosition)
    {
        if (!_isInState) { return; }

        base.SetChaseEligibility(canChase, targetPosition);

        if (canChase)
        {
           
            if (!_queuedForNewFlankPoint)
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
        
            }

        }

    }*/

    protected override void SetPlayerSeen(bool seen)
    {
        /*if (!_isInState) { return; }

        if(_canSeePlayer == seen && !_initialSeenCheck)
        {
            _initialSeenCheck = false;
            return;
        }*/

        base.SetPlayerSeen(seen);

       /* if (!_canSeePlayer && _coroutine == null)
        {
            _coroutine = CoroutineRunner.Instance.StartCoroutine(AttemptFlankRoutine());
        }*/
        
    }

   
    public override void ExitState()
    {
        _eventManager.OnDestinationRequestStatus -= SetDestinationRequestStatus;
        //_eventManager.OnPlayerSeen -= SetPlayerSeen;
        //_initialSeenCheck = true;
        //_isInState = false;

        /*if (_stepsToTry.Count > 0)
        {
            _stepsToTry.Clear();
        }*/
       
        _eventManager.RotateTowardsTarget(false);
        
       
        _shouldReChasePlayer = false;
        if (_coroutine != null)
        {
            _isStationary = false;
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        StationaryChaseManagerJob.Instance.UnregisterAgent(_agentId);
        //GameManager.OnPlayerMoved -= SetPlayerMoved;
    }

    public override void OnStateDestroyed()
    {
        base.OnStateDestroyed();
        _path = null;
        _stepsToTry.Clear();
        _stepsToTry = null;
    }

   
}
