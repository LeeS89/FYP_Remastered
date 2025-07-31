using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class DestinationManager
{

    private int _maxFlankingSteps;
    private List<int> _stepsToTry;
    private List<Vector3> _newPoints;
   
    private EnemyEventManager _eventManager;
   
    private List<WaypointPair> _waypointPairs = new();
    private WaypointPair? currentWaypointPair = null;

    private Coroutine _runningRoutine;// => For a later optimization, to prevent possible in progress coroutines from continuing
   // private Coroutine _flankRoutine;

    private WaitUntil _waitUntilResultReceived;
    private bool _resultReceived = false;
    private bool _isValid = false;
    private List<Vector3> _waypoints = new List<Vector3>();
    private List<Vector3> _flankPoints = new List<Vector3>();
    private List<Vector3> _playerPoints = new List<Vector3>();

    private AIDestinationRequestData _destinationRequest;
    private LayerMask _flankBlockingMask;
    private LayerMask _flankTargetMask;
    private BlockData _blockData;
    private Collider[] _targetColliders;
    private WaypointData _wpData;
    private NavMeshPath _path;
    private readonly Action<bool, Vector3, AIDestinationType> _onRequestComplete;
    private Transform _owner;

    AIDestinationType _globalDestinationType = AIDestinationType.None;
    private int _requestToken = 0;
   // private int _currentCallbackToken = 0;

    /// TESTING
    private GameObject testCube;

    public DestinationManager(EnemyEventManager eventManager, int maxFlankingSteps, NavMeshPath path, GameObject cube, Transform owner, Action<bool, Vector3, AIDestinationType> callback, LayerMask flankPointBlockingMask, LayerMask flankPointTargetMask)
    {
        _eventManager = eventManager;
        testCube = cube;
        _path = path;
        _owner = owner;

        _onRequestComplete = callback;
        _maxFlankingSteps = maxFlankingSteps;
        _stepsToTry = new List<int>();
        _newPoints = new List<Vector3>();
        _flankBlockingMask = flankPointBlockingMask;
        _flankTargetMask = flankPointTargetMask;
        _targetColliders = GameManager.Instance.GetPlayerTargetPoints();
        _waitUntilResultReceived = new WaitUntil(() => _resultReceived);
        _destinationRequest = new AIDestinationRequestData();
        _eventManager.OnDestinationRequested += DestinationRequested;

        InitializeWaypoints();
    }

    #region new Implementation
    private void InitializeWaypoints()
    {
        _destinationRequest.path = _path;
        _destinationRequest.resourceType = AIResourceType.WaypointBlock;
        _destinationRequest.waypointCallback = SetWayPoints;
        SceneEventAggregator.Instance.RequestResource(_destinationRequest);
    }

    private void SetWayPoints(BlockData data)
    {
        _blockData = data;
        _destinationRequest.resourceType = AIResourceType.None;
        if (_blockData == null)
        {
            Debug.LogError("Waypoint block data is null. Cannot set waypoints.");
            return;
        }

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


    private void DestinationRequested(AIDestinationType destType)
    {
       if(_runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _destinationRequest._requestID.Clear();
            _destinationRequest.internalCallback = null;
            _destinationRequest.FlankPointCandidatesCallback = null;
            _waypoints.Clear();
            _runningRoutine = null;
        }
       

        switch (destType)
        {
            case AIDestinationType.ChaseDestination:
                AddPlayerDestinationToCandidatePoints();
                break;
            case AIDestinationType.FlankDestination:
                /*_runningRoutine = */CoroutineRunner.Instance.StartCoroutine(FlankDestinationRoutine());
                break;
            case AIDestinationType.PatrolDestination:
                AddWaypointsToCandidatePoints();
                break;
            default:
                break;
        }
        _requestToken++;
        int _currentCallbackToken = _requestToken;
        Debug.LogError("Request id at Entry point stage: " + _currentCallbackToken);
        _destinationRequest._requestID.Enqueue(_currentCallbackToken);
        // Skip immediate destination handling; flank points are gathered asynchronously before routing
        if (destType == AIDestinationType.FlankDestination) { return; }
        _runningRoutine = CoroutineRunner.Instance.StartCoroutine(DestinationRoutine(/*GetCandidatePoints, */destType, _currentCallbackToken));
    }

    private IEnumerator FlankDestinationRoutine()
    {
        GetStepsToTry();
      //  _points.Clear();

        _destinationRequest.resourceType = AIResourceType.FlankPointCandidates; // Set the resource type for the request
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            _destinationRequest.numSteps = step; // Set the step for the request

            _destinationRequest.FlankPointCandidatesCallback = (points) =>
            {
                if (points != null && points.Count > 0)
                {
                    foreach (var point in points)
                    {
                        Vector3 startPoint = point + Vector3.up;
                        if (!LineOfSightUtility.HasLineOfSight(startPoint, _destinationRequest.flankTargetColliders, _destinationRequest.flankBlockingMask, _destinationRequest.flankTargetMask)) { continue; }

                        _flankPoints.Add(point);
                    }


                    //_points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;

            };

            SceneEventAggregator.Instance.RequestResource(_destinationRequest);

            yield return _waitUntilResultReceived;
        }

        foreach (var point in _flankPoints)
        {

            GameObject obj = UnityEngine.Object.Instantiate(testCube, point, Quaternion.identity);
        }

        _runningRoutine = CoroutineRunner.Instance.StartCoroutine(DestinationRoutine(/*GetFlankPoints, */AIDestinationType.FlankDestination));

    }

    private void AddWaypointsToCandidatePoints()
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
            _waypoints.Add(pair.position);
        }
       
    }

    /*private void PatrolRequested()
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

       // CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetWaypointPositions));
    }*/

    private void AddPlayerDestinationToCandidatePoints()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;

        _playerPoints.Add(playerPos);
        //_waypoints.Add(playerPos);
        //return new List<Vector3> { playerPos };
    }

    private List<Vector3> GetCandidatePoints(AIDestinationType destType)
    {
        return destType switch
        {
            AIDestinationType.PatrolDestination => _waypoints,
            AIDestinationType.ChaseDestination => _playerPoints,
            AIDestinationType.FlankDestination => _flankPoints,
            _ => _waypoints
        };

    }

    private IEnumerator DestinationRoutine(/*Func<List<Vector3>> candidatePointProvider, */AIDestinationType destType, int requestID = 0)
    {

        var candidates = GetCandidatePoints(destType);
        //var candidates = candidatePointProvider.Invoke();

        foreach (var point in candidates)
        {

            _resultReceived = false;
            _isValid = false;

            _destinationRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.position);
            _destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);
           
           /* // Skip if a new request interrupted this one
            if (requestID != _requestToken)
                yield break;*/

            _destinationRequest.internalCallback = PathRequestInternalCallback;
           
            SceneEventAggregator.Instance.PathRequested(_destinationRequest);

            yield return _waitUntilResultReceived;

            /*// Skip if a new request interrupted this one
            if (requestID != _requestToken)
                yield break;*/

            if (!_isValid) continue;

            if (destType == AIDestinationType.PatrolDestination)
            {
                var match = _waypointPairs.FirstOrDefault(p => p.position == point);
                _eventManager.RotateAtPatrolPoint(match.forward);

                currentWaypointPair = match;
            }
            // Skip if a new request interrupted this one
            if (requestID != _requestToken)
                yield break;
            _destinationRequest.resourceType = AIResourceType.None;
            _onRequestComplete?.Invoke(true, point, destType);
            _runningRoutine = null;
            candidates.Clear();
            yield break;
        }
        // Skip if a new request interrupted this one
        if (requestID != _requestToken)
            yield break;
        _onRequestComplete?.Invoke(false, Vector3.zero, destType);
        candidates.Clear();
        _runningRoutine = null;
        
    }

    private void PathRequestInternalCallback(bool status, int requestID)
    {
       /* Debug.LogError("Request id at callback stage: " + requestID);
        if (requestID != _requestToken)
            return;*/

        _isValid = status;
        _resultReceived = true;
    }
    #endregion


    /* public DestinationManager(EnemyEventManager eventManager, int maxFlankingSteps, GameObject cube, LayerMask flankBlockingMask, LayerMask flankTargetMask) : this(eventManager, maxFlankingSteps, cube)
     {
         _flankBlockingMask = flankBlockingMask;
         _flankTargetMask = flankTargetMask;
     }*/


    private struct WaypointPair
    {
        public Vector3 position;
        public Vector3 forward;

        public WaypointPair(Vector3 pos, Vector3 fwd)
        {
            position = pos;
            forward = fwd;
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

    private void ShuffleWaypointPairs()
    {
        for (int i = 0; i < _waypointPairs.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, _waypointPairs.Count);
            (_waypointPairs[i], _waypointPairs[randIndex]) = (_waypointPairs[randIndex], _waypointPairs[i]);
        }
    }

   

    private List<Vector3> GetWaypointPositions()
    {
        return _waypointPairs.Select(p => p.position).ToList();
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


    private List<Vector3> GetPlayerPoint()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        return new List<Vector3> { playerPos };
    }

    private List<Vector3> GetFlankPoints()
    {
        return _waypoints;
    }

   
    
    private IEnumerator AttemptDestinationRoutine(AIDestinationRequestData destinationRequest, Func<List<Vector3>> candidatePointProvider)
    {
        var candidates = candidatePointProvider.Invoke();

       
        foreach (var point in candidates)
        {

          
            _resultReceived = false;
            _isValid = false;

            destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);

            destinationRequest.internalCallback = (success, id) =>
            {
                _isValid = success;
                _resultReceived = true;
               
            };

           
            SceneEventAggregator.Instance.PathRequested(destinationRequest);

            yield return _waitUntilResultReceived;

            if (!_isValid) continue;

            if(destinationRequest.destinationType == AIDestinationType.PatrolDestination)
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
  

    private void RequestPlayerDestination(AIDestinationRequestData destinationRequest)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetPlayerPoint));
      
    }

    private IEnumerator RequestFlankDestination(AIDestinationRequestData destinationRequest)
    {
        GetStepsToTry();
        _waypoints.Clear();

        destinationRequest.resourceType = AIResourceType.FlankPointCandidates; // Set the resource type for the request
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            destinationRequest.numSteps = step; // Set the step for the request

            destinationRequest.FlankPointCandidatesCallback = (points) =>
            {
                if (points != null && points.Count > 0)
                {
                    foreach (var point in points)
                    {
                        Vector3 startPoint = point + Vector3.up;
                        if (!LineOfSightUtility.HasLineOfSight(startPoint, destinationRequest.flankTargetColliders, destinationRequest.flankBlockingMask, destinationRequest.flankTargetMask)) { continue; }

                        _waypoints.Add(point);
                    }


                    //_points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;
                
            };

            SceneEventAggregator.Instance.RequestResource(destinationRequest);

            yield return _waitUntilResultReceived;
        }

        foreach (var point in _waypoints)
        {

            GameObject obj = UnityEngine.Object.Instantiate(testCube, point, Quaternion.identity);
        }

        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetFlankPoints));

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
        _owner = null;
        _path = null;
        _targetColliders = null;
        _eventManager = null;
        _stepsToTry.Clear();
        _newPoints.Clear();
        _waypoints.Clear();
        _waypoints = null;
        _stepsToTry = null;
        _newPoints = null;
        _waitUntilResultReceived = null;
        _destinationRequest = null;
    }



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
    #endregion
}
