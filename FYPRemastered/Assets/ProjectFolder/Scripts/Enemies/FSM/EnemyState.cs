using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState
{
  
    //protected NavMeshAgent _agent;
    protected Coroutine _coroutine;
    protected EnemyEventManager _eventManager;
    protected bool _destinationReached = false;
    protected static bool _playerHasMoved = false;
    protected float _remainingDistance;
    protected AlertStatus _alertStatus;
    protected float _walkSpeed;
    protected float _sprintSpeed;

    public EnemyState(EnemyEventManager eventManager)
    {
        _eventManager = eventManager;
    }

    protected bool CheckDestinationReached() { /*Debug.LogError("Checking Reached");*/ return _destinationReached; }

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

    
}
