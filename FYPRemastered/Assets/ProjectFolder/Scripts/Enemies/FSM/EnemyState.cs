using UnityEngine;
using UnityEngine.AI;


public abstract class EnemyState
{
  
    //protected NavMeshAgent _agent;
    protected Coroutine _coroutine;
    protected EnemyEventManager _eventManager;
 
    protected GameObject _owner;
    protected bool _destinationReached = false;
    protected static bool _playerHasMoved = false;
    protected float _remainingDistance;
    protected AlertStatus _alertStatus = AlertStatus.None;
    protected float _walkSpeed;
    protected float _sprintSpeed;
    protected bool _canSeePlayer = false;
    protected bool _shouldReChasePlayer = false;

    protected bool _newDestinationPending = false;
 
    protected bool _destinationRequestComplete = false;
    protected bool _destinationRequestSuccess = false;

    protected WaitUntil _waitUntilDestinationApplied;
    protected bool _destinationApplied = false;

    public EnemyState(EnemyEventManager eventManager, GameObject owner)
    {
        _owner = owner;
        _eventManager = eventManager;
        _eventManager.OnTargetSeen += SetPlayerSeen;
        _waitUntilDestinationApplied = new WaitUntil(() => _destinationApplied);
        _alertStatus = AlertStatus.None;
        //_eventManager.OnAlertStatusChanged += UpdateAlertStatus;
        _eventManager.OnDestinationApplied += SetDestinationApplied;
        _eventManager.OnDestinationReached += SetDestinationReached;
      
    }


    protected void UpdateAlertStatus(AlertStatus alertStatus)
    {
        if (_alertStatus == alertStatus) { return; }

        _alertStatus = alertStatus;
    }

    protected void SetDestinationApplied()
    {
        _destinationApplied = true;
    }
   
  
    /*protected void SetDestinationRequestStatus(bool complete, bool success)
    {
        _destinationRequestSuccess = success;
        _destinationRequestComplete = complete;
        //_eventManager.OnDestinationRequestStatus?.Invoke(success, complete);
    }*/

    protected bool CheckDestinationReached() { /*Debug.LogError("Checking Reached");*/ return _destinationReached; }

    protected virtual void SetPlayerSeen(bool seen) { _canSeePlayer = seen; }

    protected static bool CheckIfPlayerHasMoved() { return _playerHasMoved; }
    
    public static void SetPlayerMoved(bool playerHasMoved) {  _playerHasMoved = playerHasMoved; }

    protected void SetDestinationReached(bool reached) { _destinationReached = reached; }
   
    public virtual void UpdateDistanceRemainingToDestination(float remainingDistance) { _remainingDistance = remainingDistance; }

  //  public virtual void EnterState(Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0) {  }

    public virtual void EnterState(AlertStatus alertStatus = AlertStatus.None) {  }
    //public virtual void EnterState(Vector3? destination = null) { }

    public virtual void UpdateState() { }

    public virtual void LateUpdateState() { }
    public abstract void ExitState();

    public virtual void SetChaseEligibility(bool canChase, Vector3 playerPosition) { _shouldReChasePlayer = canChase; }

    public virtual void OnStateDestroyed()
    {
        ExitState();
        _eventManager.OnDestinationReached -= SetDestinationReached;
        _eventManager.OnTargetSeen -= SetPlayerSeen;
        //_eventManager.OnAlertStatusChanged -= UpdateAlertStatus;
        _eventManager.OnDestinationApplied -= SetDestinationApplied;
        _eventManager = null;
        
    }


    

    protected static bool HasClearPathToTarget(Vector3 from, Vector3 to, NavMeshPath path)
    {
        if(!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }


    #region Old Code
    /* protected static bool IsDistanceGreaterThan(Vector3 from, Vector3 to, float threshold)
    {
        float thresholdSqr = threshold * threshold;
        float distSqr = (to - from).sqrMagnitude;
        return distSqr > (thresholdSqr * 1.3f);
    }

    protected bool IsPathToTargetComplete(Vector3 destination)
    {
        if (NavMesh.CalculatePath(_owner.transform.position, destination, NavMesh.AllAreas, _path))
        {
            return _path.status == NavMeshPathStatus.PathComplete;
        }

        return false;
    }*/

    /*public static bool IsTargetMovingAndReachable(
    Vector3 from,
    Vector3 to,
    float thresholdSqr,
    NavMeshPath path
    )
    {
        float bufferMultiplier = Random.Range(1.2f, 1.45f);
        float bufferedThresholdSqr = thresholdSqr * (bufferMultiplier * bufferMultiplier);

        float distSqr = (to - from).sqrMagnitude;

        Debug.LogError("Current Distance: " + distSqr + " | Threshold with buffer: " + bufferedThresholdSqr);

        if (distSqr <= bufferedThresholdSqr)
            return false;


        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            return false;

        //Debug.LogError("Path status: " + path.status);
        return path.status == NavMeshPathStatus.PathComplete;
    }*/
    #endregion

}
