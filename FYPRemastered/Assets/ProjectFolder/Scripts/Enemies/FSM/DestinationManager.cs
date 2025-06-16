using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class DestinationManager
{

    //private UniformZoneGridManager _gridManager;
    private int _maxSteps;
    private List<int> _stepsToTry;
    private List<Vector3> _newPoints;
   
    private EnemyEventManager _eventManager;
   
    private List<WaypointPair> _waypointPairs = new();
    private WaypointPair? currentWaypointPair = null;

    private Coroutine _coroutine;// => For a later optimization, to prevent possible in progress coroutines from continuing
    private WaitUntil _waitUntilResultReceived;
    private bool _resultReceived = false;
    private bool _isValid = false;
    private List<Vector3> _points = new List<Vector3>();
    private ResourceRequest _flankPointRequest;

    public DestinationManager(EnemyEventManager eventManager, /*UniformZoneGridManager gridManager,*/ int maxSteps)
    {
        _eventManager = eventManager;
        _flankPointRequest = new ResourceRequest();
        _flankPointRequest.resourceType = Resourcetype.FlankPointCandidates;
       // _gridManager = gridManager;
        _maxSteps = maxSteps;
        _stepsToTry = new List<int>();
        _newPoints = new List<Vector3>();
        _waitUntilResultReceived = new WaitUntil(() => _resultReceived);
    }

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
        //Debug.LogError($"Loading Waypoint Data: {wpData._waypointPositions.Count} waypoints.");
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



    public void RequestNewDestination(DestinationRequestData destinationData) 
    {
        DestinationType destinationType = destinationData.destinationType;

        switch (destinationType)
        {
            case DestinationType.Chase:
                RequestPlayerDestination(destinationData);
                break;
            case DestinationType.Flank:
                CoroutineRunner.Instance.StartCoroutine(RequestFlankDestination(destinationData, _flankPointRequest));
                break;
            case DestinationType.Patrol:
                RequestPatrolPointDestination(destinationData);
                break;
            default:
                Debug.LogError($"Unknown destination type: {destinationType}");
             
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
        //GetStepsToTry(); 
        //_points.Clear();
        //List<Vector3> points = new List<Vector3>();

        /*foreach (int step in _stepsToTry)
        {
            var stepPoints = _gridManager.GetCandidatePointsAtStep(step); // => Changing to event in Scene event aggregator
            if (stepPoints.Count > 0)
            {
                _points.AddRange(stepPoints.OrderBy(p => Random.value));
            }
        }*/

        return _points;
    }

    private IEnumerator ValidateFlankPointsRoutine(AIResourceRequest request)
    {
        GetStepsToTry();
        _points.Clear();

        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;

            request.FlankPointCandidatesCallback = (points) =>
            {
                if (points != null && points.Count > 0)
                {
                    _points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;
            };

            SceneEventAggregator.Instance.RequestResource(request);

            yield return _waitUntilResultReceived;

           /* var stepPoints = _gridManager.GetCandidatePointsAtStep(step); // => Changing to event in Scene event aggregator
            if (stepPoints.Count > 0)
            {
                _points.AddRange(stepPoints.OrderBy(p => Random.value));
            }*/
        }
    }


    
    private IEnumerator AttemptDestinationRoutine(DestinationRequestData data, Func<List<Vector3>> candidatePointProvider)
    {
        var candidates = candidatePointProvider.Invoke();

       
        foreach (var point in candidates)
        {
            _resultReceived = false;
            _isValid = false;

            data.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);

            data.internalCallback = (success) =>
            {
                _isValid = success;
                _resultReceived = true;
               
            };

            //_eventManager.PathRequested(data);
            SceneEventAggregator.Instance.PathRequested(data);

            yield return _waitUntilResultReceived;

            if (!_isValid) continue;

            if(data.destinationType == DestinationType.Patrol)
            {
                var match = _waypointPairs.FirstOrDefault(p => p.position == point);
                _eventManager.RotateAtPatrolPoint(match.forward);

                currentWaypointPair = match;
            }

            data.externalCallback?.Invoke(true, point);
            yield break;
        }

        data.externalCallback?.Invoke(false, Vector3.zero);
    }


    private void RequestPatrolPointDestination(DestinationRequestData destinationData)
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

        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationData, GetWaypointPositions));
    }
   /* private void RequestPatrolPointDestination(DestinationRequestData destinationData)
    {
        ShuffleWaypointPairs(); 
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationData, GetWaypointPositions));
    }*/

    private void RequestPlayerDestination(DestinationRequestData destinationData)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationData, GetPlayerPoint));
      
    }

    private IEnumerator RequestFlankDestination(DestinationRequestData destinationData, ResourceRequest request)
    {
        GetStepsToTry();
        _points.Clear();

        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            request.numSteps = step; // Set the step for the request

            request.FlankPointCandidatesCallback = (points) =>
            {
                if (points != null && points.Count > 0)
                {
                    _points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;
            };

            SceneEventAggregator.Instance.RequestResource(request);

            yield return _waitUntilResultReceived;
        }
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationData, GetFlankPoints));

    }

   

    private void GetStepsToTry()
    {
        _stepsToTry.Clear();

        int randomIndex = Random.Range(4, _maxSteps + 1);
        int temp = randomIndex;
        while (temp >= 4) // 4 will eventually be changed to a passed minSteps parameter
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

   

    public void StartCarvingRoutine(DestinationRequestData data)
    {
        CoroutineRunner.Instance.StartCoroutine(CarvingRoutine(data));
    }

    private IEnumerator CarvingRoutine(DestinationRequestData data)
    {
        data.carvingCallback?.Invoke();

        yield return null;
        //yield return new WaitForSeconds(0.15f);

        data.agentActiveCallback?.Invoke();
        
    }

    public void OnInstanceDestroyed()
    {
        
        //_gridManager = null;
        
        _eventManager = null;
        _stepsToTry.Clear();
        _newPoints.Clear();
        _points.Clear();
        _points = null;
        _stepsToTry = null;
        _newPoints = null;
        _waitUntilResultReceived = null;
    }
}
