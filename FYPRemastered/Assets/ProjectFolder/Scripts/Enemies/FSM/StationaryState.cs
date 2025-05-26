using Meta.XR.MRUtilityKit.SceneDecorator;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class StationaryState : EnemyState
{
    private int _agentId = 0;
    private bool _isStationary = false;
    private float intervalTimer = 0;
    private float checkInterval = 2.5f;
    private bool _pathCheckComplete = false;
    private int _maxSteps = 0;

    private WaitUntil _waitUntilPathCheckComplete;
    private UniformZoneGridManager _gridManager;
    private PathRequest _chasePlayerPathRequest;
    private PathRequest _flankePlayerPathRequest;

    private bool _queuedForNewFlankPoint = false;
    private bool _newDestinationAlreadyApplied = false;
    private bool _initialSeenCheck = true;
    List<Vector3> _newPoints;
    List<int> _stepsToTry;

    public StationaryState(EnemyEventManager eventManager, GameObject owner, NavMeshPath path, int maxSteps) : base(eventManager, path, owner) 
    { 
        _maxSteps = maxSteps;
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathCheckComplete);
        _gridManager = GameObject.FindFirstObjectByType<UniformZoneGridManager>();
        _chasePlayerPathRequest = new PathRequest();
        _flankePlayerPathRequest = new PathRequest();
        _stepsToTry = new List<int>();
        _newPoints = new List<Vector3>();
    }
   

    public override void EnterState(Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {
        _eventManager.OnPlayerSeen += SetPlayerSeen;
        switch (alertStatus)
        {
            case AlertStatus.None:

                break;
            case AlertStatus.Alert:
                _queuedForNewFlankPoint = false;
                _newDestinationAlreadyApplied = false;
                _eventManager.SpeedChanged(0f, 10f);

                // if (_coroutine == null)
                // {
                /*float bufferMultiplier = Random.Range(1.2f, 1.45f);
                _agentId = StationaryChaseManagerJob.Instance.RegisterAgent(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), bufferMultiplier, this);*/
                CoroutineRunner.Instance.StartCoroutine(DelayJob());
                _isStationary = true;
                //_coroutine = CoroutineRunner.Instance.StartCoroutine(PlayerProximityRoutine());
                //_coroutine = CoroutineRunner.Instance.StartCoroutine(PlayerProximityRoutine());
                _eventManager.RotateTowardsTarget(true);

                // }

                break;
            default:
                _eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                break;
        }
        _owner.GetComponent<NavMeshAgent>().updateRotation = false;
       
    }

    private IEnumerator DelayJob()
    {
        yield return new WaitForSeconds(0.5f);
        float bufferMultiplier = Random.Range(1.2f, 1.45f);
        _agentId = StationaryChaseManagerJob.Instance.RegisterAgent(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), bufferMultiplier, this);
        //Debug.LogError("Agent Registered with ID: " + _agentId);
    }

    private IEnumerator PlayerProximityRoutine()
    {
       
        yield return new WaitForSeconds(0.5f); // Small buffer before coroutine starts running

        /// When the agent registers with the StationaryChaseManagerJob, this calculates the agents distance from the player
        /// and if the distance between the agent and player becomes > that distance multiplied by the bufferMultiplier, the agent will start chasing again
        float bufferMultiplier = Random.Range(1.2f, 1.45f);
        _agentId = StationaryChaseManagerJob.Instance.RegisterAgent(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), bufferMultiplier, this);


        while (_isStationary)
        {
           
            yield return _waitUntilPathCheckComplete;
            _pathCheckComplete = false;

            if (_shouldReChasePlayer)
            {
                _isStationary = false;
                _eventManager.RequestChasingState();

                yield break;
            }

            if (intervalTimer >= checkInterval)
            {
                intervalTimer = 0f;

                if (!_canSeePlayer)
                {    
                    Vector3 newPoint = RequestFlankingPoint();
                    if (newPoint != Vector3.zero) // Vector3.zero = No valid point found
                    {
                        _isStationary = false;
                        _eventManager.RequestChasingState(newPoint);

                        yield break;
                    }
                    // => else, if newPoint = Vector3.zero, We stop the interval timer to prevent unneccessary checks
                }

            }

            yield return null;

        }

    }

    private Vector3 RequestFlankingPoint()
    {
        _stepsToTry = new List<int>();
        int randomIndex = Random.Range(4, _maxSteps +1);
        int temp = randomIndex;
        while (temp >= 4)
        {
            _stepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp < 13)
        {
            _stepsToTry.Add(temp);
            temp++;
        }

        foreach (int step in _stepsToTry)
        {
            List<Vector3> newPoints = _gridManager.GetCandidatePointsAtStep(step);

            if (newPoints.Count == 0) { continue; }
            
            newPoints = newPoints.OrderBy(p => Random.value).ToList();

            foreach (var point in newPoints)
            {
                if (HasClearPathToTarget(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), point, _path))
                {
                    Debug.LogError("Point Index: "+step);
                    return point;
                }
            }
           
        }

        return Vector3.zero; 
    }

    //private IEnumerator PlayerProximityRoutine()
    //{

    //    yield return new WaitForSeconds(0.5f); // Small buffer before coroutine starts running

    //    /// When the agent registers with the StationaryChaseManagerJob, this calculates the agents distance from the player
    //    /// and if the distance between the agent and player becomes > that distance multiplied by the bufferMultiplier, the agent will start chasing again
    //    float bufferMultiplier = Random.Range(1.2f, 1.45f);
    //    _agentId = StationaryChaseManagerJob.Instance.RegisterAgent(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), bufferMultiplier, this);


    //    while (_isStationary)
    //    {

    //        yield return _waitUntilPathCheckComplete;
    //        _pathCheckComplete = false;

    //        if (_shouldReChasePlayer)
    //        {
    //            _isStationary = false;
    //            _eventManager.RequestChasingState();

    //            yield break;
    //        }

    //        if (intervalTimer >= checkInterval)
    //        {
    //            intervalTimer = 0f;

    //            /* if (!_canSeePlayer)
    //             {
    //                 //_flankingPointSearchComplete = false;
    //                 // _validFlankingPoint = Vector3.zero;

    //                 // RequestFlankingPointAsync(); // ← starts request, sets _flankingPointSearchComplete when done

    //                 // Wait for either a new point or re-chase trigger
    //                 //  yield return new WaitUntil(() => _flankingPointSearchComplete || _waitUntilPathCheckComplete);
    //                 yield return _waitUntilPathCheckComplete;
    //                 _pathCheckComplete = false;

    //                 if (_shouldReChasePlayer)
    //                 {
    //                     _isStationary = false;
    //                     _eventManager.RequestChasingState();
    //                     yield break;
    //                 }

    //                 if (_validFlankingPoint != Vector3.zero)
    //                 {
    //                     _isStationary = false;
    //                     _eventManager.RequestChasingState(_validFlankingPoint);
    //                     yield break;
    //                 }

    //                 // else: no valid flank point found, stay stationary for now
    //             }*/
    //            if (!_canSeePlayer)
    //            {
    //                Vector3 newPoint = RequestFlankingPoint();
    //                if (newPoint != Vector3.zero) // Vector3.zero = No valid point found
    //                {
    //                    _isStationary = false;
    //                    _eventManager.RequestChasingState(newPoint);

    //                    yield break;
    //                }
    //                //    // => else, if newPoint = Vector3.zero, We stop the interval timer to prevent unneccessary checks
    //            }

    //        }

    //        yield return null;

    //    }

    //}

    private IEnumerator AttemptFlankRoutine()
    {

        int randomIndex = Random.Range(4, _maxSteps + 1);
        int temp = randomIndex;
        while (temp >= 4)
        {
            _stepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp < 13)
        {
            _stepsToTry.Add(temp);
            temp++;
        }

        //Debug.LogError("Reaching Flank routine");
        foreach (int step in _stepsToTry)
        {
            //Debug.LogError("Reaching insiode first for each loop");
            _newPoints = _gridManager.GetCandidatePointsAtStep(step);

            if (_newPoints.Count == 0) { continue; }

            _newPoints = _newPoints.OrderBy(p => Random.value).ToList();

            foreach (var point in _newPoints)
            {
                //Debug.LogError("Reaching 2nd foreach loop");
                bool resultReceived = false;
                bool isValid = false;
                _flankePlayerPathRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position);
                _flankePlayerPathRequest.end = point;
                _flankePlayerPathRequest.path = _path;
                _flankePlayerPathRequest.callback = (success) =>
                {
                    isValid = success;
                    resultReceived = true;
                };

                _eventManager.PathRequested(_flankePlayerPathRequest);
               // Debug.LogError("Yielding in routine");
                yield return new WaitUntil(() => resultReceived);
                //Debug.LogError("Finished Yielding in routine");
                if (!isValid) continue; // No valid path found, try next point

                if (_newDestinationAlreadyApplied)
                {
                    //Debug.LogError("New dest Already applied, exit routine");
                    yield break;
                }
                else
                {
                    if (!_canSeePlayer)
                    {
                        //Debug.LogError("Found new point");
                        _newDestinationAlreadyApplied = true;
                        _eventManager.RequestChasingState(point);
                        yield break;
                    }
                }

            }

        }
        _stepsToTry.Clear();
        _coroutine = null;
        //Debug.LogError("Reached end of routine");

    }

    /* public override void SetChaseEligibility(bool canChase, Vector3 targetPosition)
     {
         base.SetChaseEligibility(canChase, targetPosition);

         if (canChase)
         {
             _shouldReChasePlayer = false;

             if (!_queuedForNewFlankPoint)
             {
                 _queuedForNewFlankPoint = true;
                 _pathRequest.path = _path;
                 _pathRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position);
                 _pathRequest.end = targetPosition;
                 _eventManager.PathRequested(_pathRequest);

                 _pathRequest.callback = (success) =>
                 {
                     _shouldReChasePlayer = success;
                     _queuedForNewFlankPoint = false;
                 };

                 if (!canChase) // Extra safety check in the unlikely event canChase is set to false while the path request is being processed
                 {
                     _shouldReChasePlayer = false;
                 }


                 _pathCheckComplete = true;
             }

         }

         if (!canChase)
         {
             _pathCheckComplete = true;
         }
     }*/

    public override void SetChaseEligibility(bool canChase, Vector3 targetPosition)
    {
        base.SetChaseEligibility(canChase, targetPosition);

        if (canChase)
        {
           
            if (!_queuedForNewFlankPoint)
            {
                _queuedForNewFlankPoint = true;
                _chasePlayerPathRequest.path = _path;
                _chasePlayerPathRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position);
                _chasePlayerPathRequest.end = targetPosition;
                _eventManager.PathRequested(_chasePlayerPathRequest);

                _chasePlayerPathRequest.callback = (success) =>
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

    }

    protected override void SetPlayerSeen(bool seen)
    {
        if(_canSeePlayer == seen && !_initialSeenCheck)
        {
            _initialSeenCheck = false;
            return;
        }

        base.SetPlayerSeen(seen);

        if (!_canSeePlayer && _coroutine == null)
        {
            _coroutine = CoroutineRunner.Instance.StartCoroutine(AttemptFlankRoutine());
        }
        
    }

    /// <summary>
    /// Each time a StationaryChaseManagerJob job runs, it calls this function, passing the result of the distance from the player calculation
    /// and the players current position.
    /// If true is passed, a path check is done to determine if the player is reachable.
    /// </summary>
    /// <param name="canChase"> passes true if the distance between agent and player is greater than distance on entry * bufferMultiplier </param>
    /// <param name="targetPosition"></param>
    /*public override void SetChaseEligibility(bool canChase, Vector3 targetPosition)
    {
        base.SetChaseEligibility(canChase, targetPosition);

        if (canChase)
        {
           
            _shouldReChasePlayer = HasClearPathToTarget(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), targetPosition, _path);
                  
        }

        _pathCheckComplete = true;
    }*/



    public override void ExitState()
    {
        _eventManager.OnPlayerSeen -= SetPlayerSeen;
        _initialSeenCheck = true;

        if(_stepsToTry.Count > 0)
        {
            _stepsToTry.Clear();
        }
        //_owner.GetComponent<NavMeshAgent>().updateRotation = true;
        _eventManager.RotateTowardsTarget(false);
        _isStationary = false;
        _pathCheckComplete = false;
        _shouldReChasePlayer = false;
        if (_coroutine != null)
        {
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
    }

    public override void LateUpdateState()
    {
        intervalTimer += Time.deltaTime;
    }

   
}
