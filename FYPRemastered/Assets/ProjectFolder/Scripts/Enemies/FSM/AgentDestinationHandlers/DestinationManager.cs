using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

/// <summary>
/// Processes destination requests from AI agents via event sent from FSMController class
/// Based on type of destination requested i.e Chase, Flank, Patrol, the request retrieves the 
/// relevant candidate destinations from the DestinationManagerHelper class
/// Requests are queued and processed 1 at a time.
/// Onve a point is chosen and validated, an event is fired, providing the calling agent with the Vector3 and request type 
/// A Chase or patrol point is valid if it is reachable, while a flank point is only valid if it is both reachable and proides a clear LOS to the player 
/// When a point is being evaluated, it gets queued for path finding, and the request waits until the path request returns success/ failure.
/// The final callback to the calling agent only gets fired once a point is validated and that reequests type matches to most recently queued destination type to prevent rapid Destination setting
/// </summary>
public class DestinationManager
{
    private EnemyEventManager _eventManager;
    private Transform _owner;
    private AIDestinationType _globalDestinationType = AIDestinationType.None;

   

    // Coroutine stuff
    private Coroutine _runningRoutine;
    private WaitUntil _waitUntilResultReceived;
    
    private Queue<AIDestinationType> _destinationQueue;
    private bool _resultReceived = false;
    private bool _isValid = false;
    ////////////////////////////////////


    // Path Calculation Request callback function and data provided to request
    private NavMeshPath _path;
    private readonly Action<bool, Vector3, AIDestinationType> _onRequestComplete;
    private AIDestinationRequestData _destinationRequest;
    /////////////////////////////////////////////

    /// Helper class which stores and provides Destination candidates
    private DestinationManagerHelper _candidatePointProvider;
   

    /// TESTING
    private GameObject testCube; // OLD

    public DestinationManager(EnemyEventManager eventManager, int maxFlankingSteps, GameObject cube, Transform owner, Action<bool, Vector3, AIDestinationType> callback)
    {
        _eventManager = eventManager;
        _eventManager.OnDeathStatusUpdated += OwnerDeathStatusUpdated;

        testCube = cube;
        _path = new NavMeshPath();
        _owner = owner;

        _onRequestComplete = callback;
       
        _waitUntilResultReceived = new WaitUntil(() => _resultReceived);
        _destinationRequest = new AIDestinationRequestData();
        _eventManager.OnDestinationRequested += DestinationRequested;

        _destinationQueue = new Queue<AIDestinationType>();

        _destinationRequest.path = _path;
        _candidatePointProvider = new DestinationManagerHelper(_destinationRequest, _owner, maxFlankingSteps, testCube);
        _candidatePointProvider?.InitializeWaypoints();
    }

    public int GetCurrentWPZone() => _candidatePointProvider.CurrentWaypointZone;


    public bool OwnerIsDead { get; private set; } = false;

    private void OwnerDeathStatusUpdated(bool isDead)
    {
        OwnerIsDead = isDead;

        if (OwnerIsDead && _runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
            _destinationQueue.Clear();

            CoroutineRunner.Instance.StopCoroutine(_candidatePointProvider.FlankDestinationRoutine());
            _candidatePointProvider?.ReleaseCurrentFlankPointIfExists();
            _candidatePointProvider?.ClearCandidates();
        }
    }

    private void DestinationRequested(AIDestinationType destType)
    {
        if (OwnerIsDead) { return; }

        _globalDestinationType = destType;
        _destinationQueue.Enqueue(destType);

        if (_runningRoutine == null)
        {
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(ProcessDestinationQueue());
        }


    }

    

    private void PathRequestInternalCallback(bool status)
    {

        _isValid = status;
        _resultReceived = true;
    }

    private IEnumerator ProcessDestinationQueue()
    {
        while (_destinationQueue.Count > 0)
        {
            var destinationType = _destinationQueue.Dequeue();

            if (destinationType == AIDestinationType.FlankDestination)
            {
                yield return CoroutineRunner.Instance.StartCoroutine(_candidatePointProvider.FlankDestinationRoutine());
                yield return CoroutineRunner.Instance.StartCoroutine(EvaluateDestinationRoutine(_candidatePointProvider.GetFlankCandidates(), DestinationManagerHelper.GetFlankPointCandidatePosition, destinationType, DestinationManagerHelper.MarkFlankPointInUse));
            }
            else
            {
                yield return CoroutineRunner.Instance.StartCoroutine(EvaluateDestinationRoutine(_candidatePointProvider.GetCandidates(destinationType), DestinationManagerHelper.ReturnSelf, destinationType));
            }
            _candidatePointProvider?.ClearCandidates();
        }
        _runningRoutine = null;
    }

    private IEnumerator EvaluateDestinationRoutine<T>(
    List<T> candidates,
    Func<T, Vector3> getPositionFunc,
    AIDestinationType destType = AIDestinationType.None,
    Action<T> markInUseFunc = null
    )
    {
        foreach (var candidate in candidates)
        {
            Vector3 point = getPositionFunc(candidate);

            _resultReceived = false;
            _isValid = false;

            _destinationRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.position);
            _destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);
            _destinationRequest.internalCallback = PathRequestInternalCallback;

            SceneEventAggregator.Instance.PathRequested(_destinationRequest);

            yield return _waitUntilResultReceived;

            if (destType != _globalDestinationType)
            {
                yield break;
            }

            if (!_isValid) continue;

            if (destType == AIDestinationType.PatrolDestination)
            {
                Vector3 wpForward = _candidatePointProvider.GetWaypointForward(point);
                _eventManager.RotateAtPatrolPoint(wpForward);
                
            }

            if (candidate is FlankPointData fp)
            {
                if (fp.inUse) { continue; }
            }

            _candidatePointProvider?.ReleaseCurrentFlankPointIfExists();

            if (candidate is FlankPointData flankPoint)
            {
                _candidatePointProvider.SetCurrentFlankPoint(flankPoint);
            }

               
             markInUseFunc?.Invoke(candidate);

            _destinationRequest.resourceType = AIResourceType.None;
            _onRequestComplete?.Invoke(true, point, destType);
            yield break;
        }

        if (destType != _globalDestinationType)
        {
            yield break;
        }

        _onRequestComplete?.Invoke(false, Vector3.zero, destType);
    }
    



    public void StartCarvingRoutine(AIDestinationRequestData destinationRequest)
    {
        CoroutineRunner.Instance.StartCoroutine(CarvingRoutine(destinationRequest));
    }

    private IEnumerator CarvingRoutine(AIDestinationRequestData destinationRequest)
    {
        destinationRequest.carvingCallback?.Invoke();

        yield return null;
       
        destinationRequest.agentActiveCallback?.Invoke();
        
    }

    public void OnInstanceDestroyed()
    {
        _eventManager.OnDeathStatusUpdated -= OwnerDeathStatusUpdated;
        _candidatePointProvider.OnInstanceDestroyed();
        _candidatePointProvider = null;
        _owner = null;
        _path = null;
       
        _eventManager = null;
      
         _waitUntilResultReceived = null;
        _destinationRequest = null;
        _destinationQueue.Clear();
        _destinationQueue = null;
    }


    #region Backup imp[lementaiton but now Obsolete
    private LayerMask _flankBlockingMask; // OLD
    private LayerMask _flankTargetMask; // OLD
    private LayerMask _flankBackupTargetMask; //OLD

    private int _maxFlankingSteps; // OLD

    // Waypoint Data => OLD
    private BlockData _blockData;
    // public int CurrentWaypointZone { get; private set; } = 0; // OLD
    private List<WaypointPair> _waypointPairs = new(); // OLD
    private WaypointPair? currentWaypointPair = null; // OLD
    ///private int _currentWaypointZone = 0;

    private List<Vector3> _candidatePoints = new List<Vector3>(); // OLD
    private Collider[] _flankCandidateLOSColliders; // OLD
    private List<int> _stepsToTry; ///// Remove

    #region Waypoint Initialization
    private void InitializeWaypoints() // OLD => Call helper
    {
        _destinationRequest.flankPointCandidates = _candidatePoints; // OLD
        _destinationRequest.path = _path;
        _destinationRequest.resourceType = AIResourceType.WaypointBlock; // OLD
        _destinationRequest.waypointCallback = SetWayPoints; // OLD
        SceneEventAggregator.Instance.RequestResource(_destinationRequest); // OLd
    }

    private void SetWayPoints(BlockData data) // OLD
    {
        _blockData = data;
        _destinationRequest.resourceType = AIResourceType.None;
        if (_blockData == null)
        {
            Debug.LogError("Waypoint block data is null. Cannot set waypoints.");
            return;
        }

        // CurrentWaypointZone = _blockData._blockZone;
        //  _currentWaypointZone = _blockData._blockZone;
        LoadWaypointData(_blockData);
    }



    public void LoadWaypointData(BlockData wpData)
    {

        _waypointPairs.Clear();

        for (int i = 0; i < wpData._waypointPositions.Length; i++)
        {
            _waypointPairs.Add(new WaypointPair(wpData._waypointPositions[i], wpData._waypointForwards[i]));
        }

    }

    private struct WaypointPair // OLD
    {
        public Vector3 position;
        public Vector3 forward;

        public WaypointPair(Vector3 pos, Vector3 fwd)
        {
            position = pos;
            forward = fwd;
        }
    }




    private void ShuffleWaypointPairs() // OLD
    {
        for (int i = 0; i < _waypointPairs.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, _waypointPairs.Count);
            (_waypointPairs[i], _waypointPairs[randIndex]) = (_waypointPairs[randIndex], _waypointPairs[i]);
        }
    }

    #endregion
    #region Populating Candidate Points for processing
    private void AddWaypointsToCandidatePoints() // OLD
    {
        if (currentWaypointPair.HasValue)
        {

            _waypointPairs.Remove(currentWaypointPair.Value);
            ShuffleWaypointPairs();
            _waypointPairs.Add(currentWaypointPair.Value);
        }
        else
        {
            ShuffleWaypointPairs();
        }


        foreach (var pair in _waypointPairs)
        {
            _candidatePoints.Add(pair.position);
        }

    }

    private void AddPlayerDestinationToCandidatePoints() // OLD
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;

        _candidatePoints.Add(playerPos);
    }

    private List<Vector3> GetCandidatePoints(AIDestinationType destType)
    {

        // when destType == AIDestinationType.FlankDestination, _candidatePoints is populated in the Flank coroutine
        if (destType == AIDestinationType.ChaseDestination)
        {
            AddPlayerDestinationToCandidatePoints();
        }
        else if (destType == AIDestinationType.PatrolDestination)
        {
            AddWaypointsToCandidatePoints();
        }

        return _candidatePoints;

    }



    private void GetStepsToTry()
    {
        _stepsToTry.Clear();

        int randomIndex = Random.Range(4, _maxFlankingSteps + 1);
        int temp = randomIndex;
        while (temp >= 4) // 4 will eventually be changed to a passed minSteps parameter
        {
            _stepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp <= _maxFlankingSteps)
        {
            _stepsToTry.Add(temp);
            temp++;
        }
    }
    #endregion

    #region Destination Request Processing
    private IEnumerator ProcessDestinationQueueOld()
    {
        while (_destinationQueue.Count > 0)
        {
            var request = _destinationQueue.Dequeue();

            if (request == AIDestinationType.FlankDestination)
            {
                yield return CoroutineRunner.Instance.StartCoroutine(FlankDestinationRoutine());
                yield return CoroutineRunner.Instance.StartCoroutine(DestinationRoutine(request));
            }
            else
            {
                yield return CoroutineRunner.Instance.StartCoroutine(DestinationRoutine(request));
            }
            _candidatePoints.Clear();
        }
        _runningRoutine = null;
    }

    private IEnumerator DestinationRoutine(AIDestinationType destType)
    {

        var candidates = GetCandidatePoints(destType);


        foreach (var point in candidates)
        {

            _resultReceived = false;
            _isValid = false;

            _destinationRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.position);
            _destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);


            _destinationRequest.internalCallback = PathRequestInternalCallback;

            SceneEventAggregator.Instance.PathRequested(_destinationRequest);

            yield return _waitUntilResultReceived;


            if (destType != _globalDestinationType)
            {
                yield break;
            }

            if (!_isValid) continue;


            if (destType == AIDestinationType.PatrolDestination)
            {
                var match = _waypointPairs.FirstOrDefault(p => p.position == point);
                _eventManager.RotateAtPatrolPoint(match.forward);

                currentWaypointPair = match;
            }



            _destinationRequest.resourceType = AIResourceType.None;
            _onRequestComplete?.Invoke(true, point, destType);

            yield break;
        }

        if (destType != _globalDestinationType)
        {
            yield break;
        }

        _onRequestComplete?.Invoke(false, Vector3.zero, destType);


    }

    private IEnumerator FlankDestinationRoutine()
    {
        GetStepsToTry();

        _destinationRequest.resourceType = AIResourceType.FlankPointCandidates;
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            _destinationRequest.numSteps = step; // Set the step for the request

            _destinationRequest.FlankPointCandidatesCallback = OnReceivedFlankPointCandidates; /*(points) =>
            {
                if (points != null && points.Count > 0)
                {
                    foreach (var point in points)
                    {
                        Vector3 startPoint = point + Vector3.up;
                        if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

                        _candidatePoints.Add(point);
                    }


                    //_points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;

            };*/

            SceneEventAggregator.Instance.RequestResource(_destinationRequest);

            yield return _waitUntilResultReceived;
        }

        foreach (var point in _candidatePoints)
        {

            GameObject obj = UnityEngine.Object.Instantiate(testCube, point, Quaternion.identity);
        }

    }

    private void OnReceivedFlankPointCandidates(/*List<Vector3> points*/bool success)
    {
        ///// Later implementation => Based on returned bool => decide what happens when it fails
        Debug.LogError("Flank Candidates before filtering: " + _candidatePoints.Count);
        // foreach (var point in _candidatePoints)
        // {
        // Vector3 startPoint = point + Vector3.up;
        Vector3 losTargetPoint = _owner.position + Vector3.up * 0.9f;
        // if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

        _candidatePoints.RemoveAll(p => !LineOfSightUtility.HasLineOfSight(p + Vector3.up, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask));
        //   if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask)) { continue; }
        Debug.LogError("Flank Candidates after filtering: " + _candidatePoints.Count);
        // _candidatePoints.Add(point);
        // }

        //_points.AddRange(points.OrderBy(p => Random.value));


        /* if (points != null && points.Count > 0)
         {
             foreach (var point in points)
             {
                 Vector3 startPoint = point + Vector3.up;
                 if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

                 _candidatePoints.Add(point);
             }

             //_points.AddRange(points.OrderBy(p => Random.value));
         }*/
        _resultReceived = true;
    }


    #endregion

    #endregion

    #region Obsolete 
    private bool ValidateTargetVisibilityFromPoint(Vector3 point)
    {
        int mask = LayerMask.GetMask("Default", "Water", "PlayerDefence", "Player");
        Collider playerCollider = GameManager.Instance.GetPlayerCollider(PlayerPart.Position);
        Vector3[] testPoints = new Vector3[]
        {
            playerCollider.bounds.center,
            playerCollider.bounds.center + Vector3.up * playerCollider.bounds.extents.y, // top
            playerCollider.bounds.center - Vector3.up * playerCollider.bounds.extents.y, // bottom
            playerCollider.bounds.center + Vector3.right * playerCollider.bounds.extents.x, // right shoulder
            playerCollider.bounds.center - Vector3.right * playerCollider.bounds.extents.x  // left shoulder
        };

        foreach (var colPoint in testPoints)
        {
            if (Physics.Linecast(point, colPoint, out RaycastHit hit, mask))
            {
                if (hit.collider == playerCollider)
                {
                    Debug.DrawLine(point, colPoint, Color.green, 25f);
                    return true;
                }
            }
        }

        return false;
    }

    public void RequestNewDestination(AIDestinationRequestData destinationRequest)
    {

        switch (destinationRequest.destinationType)
        {
            case AIDestinationType.ChaseDestination:
                RequestPlayerDestination(destinationRequest);
                break;
            case AIDestinationType.FlankDestination:
                CoroutineRunner.Instance.StartCoroutine(RequestFlankDestination(destinationRequest));
                break;
            case AIDestinationType.PatrolDestination:
                RequestPatrolPointDestination(destinationRequest);
                break;
            default:
                Debug.LogError($"Unknown destination type: {destinationRequest.destinationType}");

                break;
        }
    }

    public void LoadWaypointData(WaypointData wpData)
    {

        _waypointPairs.Clear();

        for (int i = 0; i < wpData._waypointPositions.Count; i++)
        {
            _waypointPairs.Add(new WaypointPair(wpData._waypointPositions[i], wpData._waypointForwards[i]));
        }

    }

    private void RequestPatrolPointDestination(AIDestinationRequestData destinationRequest)
    {
        if (currentWaypointPair.HasValue)
        {
            // If there's a previously selected waypoint, remove it, shuffle the list, and then add it back at the end
            _waypointPairs.Remove(currentWaypointPair.Value);
            ShuffleWaypointPairs();  // Shuffle the remaining list
            _waypointPairs.Add(currentWaypointPair.Value);  // Add the selected waypoint to the end
        }
        else
        {
            ShuffleWaypointPairs();  // Shuffle if no waypoint has been selected yet
        }

        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetWaypointPositions));
    }

    private IEnumerator RequestFlankDestination(AIDestinationRequestData destinationRequest)
    {
        GetStepsToTry();
        //_waypoints.Clear();

        destinationRequest.resourceType = AIResourceType.FlankPointCandidates; // Set the resource type for the request
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            destinationRequest.numSteps = step; // Set the step for the request

       /*     destinationRequest.FlankPointCandidatesCallback = (points) =>
            {
                if (points != null && points.Count > 0)
                {
                    foreach (var point in points)
                    {
                        Vector3 startPoint = point + Vector3.up;
                        if (!LineOfSightUtility.HasLineOfSight(startPoint, destinationRequest.flankTargetColliders, destinationRequest.flankBlockingMask, destinationRequest.flankTargetMask)) { continue; }

                        _candidatePoints.Add(point);
                    }


                    //_points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;

            };*/

            SceneEventAggregator.Instance.RequestResource(destinationRequest);

            yield return _waitUntilResultReceived;
        }

        foreach (var point in _candidatePoints)
        {

            GameObject obj = UnityEngine.Object.Instantiate(testCube, point, Quaternion.identity);
        }

        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetFlankPoints));

    }

    private void RequestPlayerDestination(AIDestinationRequestData destinationRequest)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetPlayerPoint));

    }

    private IEnumerator AttemptDestinationRoutine(AIDestinationRequestData destinationRequest, Func<List<Vector3>> candidatePointProvider)
    {
        var candidates = candidatePointProvider.Invoke();


        foreach (var point in candidates)
        {


            _resultReceived = false;
            _isValid = false;

            destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);

            destinationRequest.internalCallback = (success) =>
            {
                _isValid = success;
                _resultReceived = true;

            };


            SceneEventAggregator.Instance.PathRequested(destinationRequest);

            yield return _waitUntilResultReceived;

            if (!_isValid) continue;

            if (destinationRequest.destinationType == AIDestinationType.PatrolDestination)
            {
                var match = _waypointPairs.FirstOrDefault(p => p.position == point);
                _eventManager.RotateAtPatrolPoint(match.forward);

                currentWaypointPair = match;
            }
            destinationRequest.resourceType = AIResourceType.None;
            destinationRequest.externalCallback?.Invoke(true, point);
            yield break;
        }

        destinationRequest.externalCallback?.Invoke(false, Vector3.zero);
    }
    private List<Vector3> GetWaypointPositions()
    {
        return _waypointPairs.Select(p => p.position).ToList();
    }






    private List<Vector3> GetPlayerPoint()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        return new List<Vector3> { playerPos };
    }

    private List<Vector3> GetFlankPoints()
    {
        return _candidatePoints;
    }
    #endregion
}
