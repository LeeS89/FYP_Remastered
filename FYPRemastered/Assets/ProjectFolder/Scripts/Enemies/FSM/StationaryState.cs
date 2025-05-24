using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StationaryState : EnemyState
{
    private int _agentId = 0;
    private bool _isStationary = false;
    private float intervalTimer = 0;
    private float checkInterval = 2.5f;
    private bool _pathCheckComplete = false;
    
    private WaitUntil _waitUntilPathCheckComplete;
   
    public StationaryState(EnemyEventManager eventManager, GameObject owner, NavMeshPath path) : base(eventManager, path, owner) 
    { 
      
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathCheckComplete);
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

            if (_shouldUpdateDestination)
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
                    _isStationary = false;
                    UniformZoneGridManager _gridManager = GameObject.FindFirstObjectByType<UniformZoneGridManager>();
                    Vector3 newPoint = _gridManager.GetRandomPointXStepsFromPlayer(4);
                    _eventManager.RequestChasingState(newPoint);
                    
                    yield break;
                }
            }

            yield return null;

        }

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
           _shouldUpdateDestination = HasClearPathToTarget(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), playerPosition, _path);
                  
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
            _shouldUpdateDestination = false;
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
