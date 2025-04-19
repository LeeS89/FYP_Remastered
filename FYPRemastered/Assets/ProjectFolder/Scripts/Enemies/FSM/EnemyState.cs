using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public abstract class EnemyState
{
  
    //protected NavMeshAgent _agent;
    protected Coroutine _coroutine;
    protected EnemyEventManager _eventManager;
    protected NavMeshPath _path;
    protected GameObject _owner;
    protected bool _destinationReached = false;
    protected static bool _playerHasMoved = false;
    protected float _remainingDistance;
    protected AlertStatus _alertStatus;
    protected float _walkSpeed;
    protected float _sprintSpeed;
    protected bool _canSeePlayer = false;

    public EnemyState(EnemyEventManager eventManager, GameObject owner)
    {
        _owner = owner;
        _eventManager = eventManager;
        _eventManager.OnPlayerSeen += SetPlayerSeen;
       
    }

    public EnemyState(EnemyEventManager eventManager, NavMeshPath path, GameObject owner) : this(eventManager, owner)
    {
        _path = path;
    }

   

    protected bool CheckDestinationReached() { /*Debug.LogError("Checking Reached");*/ return _destinationReached; }

    protected void SetPlayerSeen(bool seen) { _canSeePlayer = seen; }

    protected static bool CheckIfPlayerHasMoved() { return _playerHasMoved; }
    
    public static void SetPlayerMoved(bool playerHasMoved) {  _playerHasMoved = playerHasMoved; }

    protected void SetDestinationReached(bool reached) { _destinationReached = reached; }
   
    public virtual void UpdateDistanceRemainingToDestination(float remainingDistance) { _remainingDistance = remainingDistance; }

    public virtual void EnterState(AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0) {  _alertStatus = alertStatus; }
    public virtual void UpdateState() { }

    public virtual void LateUpdateState() { }
    public abstract void ExitState();
    public virtual void OnStateDestroyed()
    {
        _eventManager = null;
        ExitState();
    }


    public static bool IsTargetMovingAndReachable(
    Vector3 from,
    Vector3 to,
    float threshold,
    NavMeshPath path,
    float bufferMultiplier = 1.3f)
    {
        float thresholdSqr = threshold * threshold;
        float distSqr = (to - from).sqrMagnitude;

        if (distSqr <= (thresholdSqr * bufferMultiplier))
            return false;

        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            return false;

        Debug.LogError("Path status: " + path.status);
        return path.status == NavMeshPathStatus.PathComplete;
    }
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

}
