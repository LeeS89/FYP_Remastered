using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DestinationManager
{

    private UniformZoneGridManager _gridManager;
    private int _maxSteps;
    private List<int> _stepsToTry;
    private List<Vector3> _newPoints;
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
                RequestTargetDestination(destinationData);
                break;
            case DestinationType.Flank:
                RequestFlankDestination(destinationData);
                break;
            default:
                RequestTargetDestination(destinationData);
                break;
        }
    }


    private void RequestTargetDestination(DestinationRequestData destinationData)
    {
        // Not yet implemented, to be used for all other destination requests, i.e. Chase, Patrol, etc.
    }

    private void RequestFlankDestination(DestinationRequestData destinationData)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptFlankRoutine(destinationData));
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

                destinationData.externalCallback?.Invoke(true);
                
                
                yield break;

                

            }

        }
        destinationData.externalCallback?.Invoke(false);
        
       
    }

    public void OnInstanceDestroyed()
    {
        _gridManager = null;
        _eventManager = null;
        _stepsToTry.Clear();
        _newPoints.Clear();
        _stepsToTry = null;
        _newPoints = null;
    }
}
