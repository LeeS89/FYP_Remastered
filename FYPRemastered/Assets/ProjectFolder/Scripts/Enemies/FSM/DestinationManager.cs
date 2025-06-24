using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class DestinationManager
{

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


    public DestinationManager(EnemyEventManager eventManager, int maxSteps)
    {
        _eventManager = eventManager;
      
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
        return _points;
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
        _points.Clear();

        destinationRequest.resourceType = AIResourceType.FlankPointCandidates; // Set the resource type for the request
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            destinationRequest.numSteps = step; // Set the step for the request

            destinationRequest.FlankPointCandidatesCallback = (points) =>
            {
                if (points != null && points.Count > 0)
                {
                    _points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;
                
            };

            SceneEventAggregator.Instance.RequestResource(destinationRequest);

            yield return _waitUntilResultReceived;
        }
       
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetFlankPoints));

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
