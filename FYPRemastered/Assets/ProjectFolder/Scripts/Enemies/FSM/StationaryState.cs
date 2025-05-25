using Meta.XR.MRUtilityKit.SceneDecorator;
using NUnit.Framework;
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


    public StationaryState(EnemyEventManager eventManager, GameObject owner, NavMeshPath path, int maxSteps) : base(eventManager, path, owner) 
    { 
        _maxSteps = maxSteps;
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathCheckComplete);
        _gridManager = GameObject.FindFirstObjectByType<UniformZoneGridManager>();
    }
   

    public override void EnterState(Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {
       
        switch (alertStatus)
        {
            case AlertStatus.None:
                
                break;
            case AlertStatus.Alert:
               
                _eventManager.SpeedChanged(0f, 10f);
               
                if (_coroutine == null)
                {
                    _isStationary = true;
                    _coroutine = CoroutineRunner.Instance.StartCoroutine(PlayerProximityRoutine());
                    _eventManager.RotateTowardsTarget(true);

                }
               
                break;
            default:
                _eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                break;
        }
        _owner.GetComponent<NavMeshAgent>().updateRotation = false;
       
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
        List<int> stepsToTry = new List<int>();
        int randomIndex = Random.Range(4, _maxSteps +1);
        int temp = randomIndex;
        while (temp >= 4)
        {
            stepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp < 13)
        {
            stepsToTry.Add(temp);
            temp++;
        }

        foreach (int step in stepsToTry)
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

    /// <summary>
    /// Each time a StationaryChaseManagerJob job runs, it calls this function, passing the result of the distance from the player calculation
    /// and the players current position.
    /// If true is passed, a path check is done to determine if the player is reachable.
    /// </summary>
    /// <param name="canChase"> passes true if the distance between agent and player is greater than distance on entry * bufferMultiplier </param>
    /// <param name="playerPosition"></param>
    public override void SetChaseEligibility(bool canChase, Vector3 playerPosition)
    {
        base.SetChaseEligibility(canChase, playerPosition);

        if (canChase)
        {
           _shouldReChasePlayer = HasClearPathToTarget(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), playerPosition, _path);
                  
        }

        _pathCheckComplete = true;
    }



    public override void ExitState()
    {
        if (_coroutine != null)
        {
            StationaryChaseManagerJob.Instance.UnregisterAgent(_agentId);
            _owner.GetComponent<NavMeshAgent>().updateRotation = true;
            _eventManager.RotateTowardsTarget(false);
            _isStationary = false;
            _pathCheckComplete = false;
            _shouldReChasePlayer = false;
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
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
