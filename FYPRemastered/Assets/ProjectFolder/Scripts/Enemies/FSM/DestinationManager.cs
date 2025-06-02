using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class DestinationManager
{

    private UniformZoneGridManager _gridManager;
    private int _maxSteps;
    private List<int> _stepsToTry;
    private List<Vector3> _newPoints;
    private List<Vector3> _patrolPointList = new List<Vector3>(1);
    private EnemyEventManager _eventManager;
    

   
    public DestinationManager(EnemyEventManager eventManager, UniformZoneGridManager gridManager, int maxSteps)
    {
        _eventManager = eventManager;
        
        _gridManager = gridManager;
        _maxSteps = maxSteps;
        _stepsToTry = new List<int>();
        _newPoints = new List<Vector3>();
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
                RequestFlankDestination(destinationData);
                break;
            default:
                _patrolPointList.Clear();
                _patrolPointList.Add(destinationData.end);
                RequestPatrolPointDestination(destinationData);
                break;
        }
    }

    private List<Vector3> GetPatrolPoint()
    {
        return _patrolPointList;
    }

    private List<Vector3> GetPlayerPoint()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        return new List<Vector3> { playerPos };
    }

    private List<Vector3> GetFlankPoints()
    {
        GetStepsToTry(); // modifies _stepsToTry
        List<Vector3> points = new List<Vector3>();

        foreach (int step in _stepsToTry)
        {
            var stepPoints = _gridManager.GetCandidatePointsAtStep(step);
            if (stepPoints.Count > 0)
            {
                points.AddRange(stepPoints.OrderBy(p => Random.value));
            }
        }

        return points;
    }


    
    private IEnumerator AttemptDestinationRoutine(DestinationRequestData data, Func<List<Vector3>> candidatePointProvider)
    {
        var candidates = candidatePointProvider.Invoke();

        foreach (var point in candidates)
        {
            bool resultReceived = false;
            bool isValid = false;

            data.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);

            data.internalCallback = (success) =>
            {
                isValid = success;
                resultReceived = true;
            };

            _eventManager.PathRequested(data);

            yield return new WaitUntil(() => resultReceived);

            if (!isValid) continue;

            data.externalCallback?.Invoke(true, point);
            yield break;
        }

        data.externalCallback?.Invoke(false, Vector3.zero);
    }


    private void RequestPatrolPointDestination(DestinationRequestData destinationData)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationData, GetPatrolPoint));
    }

    private void RequestPlayerDestination(DestinationRequestData destinationData)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationData, GetPlayerPoint));
        //CoroutineRunner.Instance.StartCoroutine(AttemptChaseRoutine(destinationData));
    }

    private void RequestFlankDestination(DestinationRequestData destinationData)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationData, GetFlankPoints));
        //CoroutineRunner.Instance.StartCoroutine(AttemptFlankRoutine(destinationData));
    }

    private IEnumerator AttemptChaseRoutine(DestinationRequestData destinationData)
    {
        bool resultReceived = false;
        bool isValid = false;

        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        destinationData.end = LineOfSightUtility.GetClosestPointOnNavMesh(playerPos);

        destinationData.internalCallback = (success) =>
        {
            isValid = success;
            resultReceived = true;
        };

        _eventManager.PathRequested(destinationData);

        yield return new WaitUntil(() => resultReceived);

        if (isValid)
        {
            destinationData.externalCallback?.Invoke(true, playerPos);

            yield break;
        }

        destinationData.externalCallback?.Invoke(false, Vector3.zero);
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

    private IEnumerator AttemptFlankRoutine(DestinationRequestData destinationData)
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
                
                destinationData.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);
                
                destinationData.internalCallback = (success) =>
                {
                    isValid = success;
                    resultReceived = true;
                };

                _eventManager.PathRequested(destinationData);

                yield return new WaitUntil(() => resultReceived);

                if (!isValid) continue; // No valid path found, try next point

                destinationData.externalCallback?.Invoke(true, point);
                
                
                yield break;

                

            }

        }
        destinationData.externalCallback?.Invoke(false, Vector3.zero);
        
       
    }

    public void StartCarvingRoutine(DestinationRequestData data)
    {
        CoroutineRunner.Instance.StartCoroutine(CarvingRoutine(data));
    }

    private IEnumerator CarvingRoutine(DestinationRequestData data)
    {
        data.carvingCallback?.Invoke();

        yield return new WaitForSeconds(0.15f);

        data.agentActiveCallback?.Invoke();
        
    }

    public void OnInstanceDestroyed()
    {
        _gridManager = null;
        
        _eventManager = null;
        _stepsToTry.Clear();
        _newPoints.Clear();
        _patrolPointList.Clear();
        _patrolPointList = null;
        _stepsToTry = null;
        _newPoints = null;
    }
}
