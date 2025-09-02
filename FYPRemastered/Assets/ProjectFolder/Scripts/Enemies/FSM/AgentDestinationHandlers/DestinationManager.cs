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
    private readonly Action<bool, Vector3, AIDestinationType> _onDestinationRequestComplete;
    private AIDestinationRequestData _destinationRequest;
    /////////////////////////////////////////////

    /// Helper class which stores and provides Destination candidates
    private DestinationManagerHelper _candidatePointProvider;
    private Action<bool> _pathRequestCallback;

    /// TESTING
    private GameObject testCube; // OLD

    public DestinationManager(EnemyEventManager eventManager, int maxFlankingSteps, GameObject cube, Transform owner, Action<bool, Vector3, AIDestinationType> callback)
    {
        _eventManager = eventManager;
        _eventManager.OnDeathStatusUpdated += OwnerDeathStatusUpdated;

        testCube = cube;
        _path = new NavMeshPath();
        _owner = owner;

        _onDestinationRequestComplete = callback;
       
        _waitUntilResultReceived = new WaitUntil(() => _resultReceived);
        _destinationRequest = new AIDestinationRequestData();
        _eventManager.OnDestinationRequested += DestinationRequested;

        _destinationQueue = new Queue<AIDestinationType>();
        _pathRequestCallback = PathRequestInternalCallback;
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

            var pathRequest = ResourceRequests.RequestPath(AIResourceType.Path, LineOfSightUtility.GetClosestPointOnNavMesh(_owner.position), LineOfSightUtility.GetClosestPointOnNavMesh(point), _path, _pathRequestCallback);
            SceneEventAggregator.Instance.ResourceRequested(pathRequest);
           /* _destinationRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.position);
            _destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);
            _destinationRequest.internalCallback = PathRequestInternalCallback;*/

           //SceneEventAggregator.Instance.PathRequested(_destinationRequest);

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
            _onDestinationRequestComplete?.Invoke(true, point, destType);
            yield break;
        }

        if (destType != _globalDestinationType)
        {
            yield break;
        }

        _onDestinationRequestComplete?.Invoke(false, Vector3.zero, destType);
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
        _pathRequestCallback = null;
         _waitUntilResultReceived = null;
        _destinationRequest = null;
        _destinationQueue.Clear();
        _destinationQueue = null;
    }

}
