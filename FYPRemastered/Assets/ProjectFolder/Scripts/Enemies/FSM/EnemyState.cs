using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState
{
  
    protected NavMeshAgent _agent;
    protected Coroutine _coroutine;
    protected EnemyEventManager _eventManager;
    protected bool _destinationReached = false;
    protected bool _playerHasMoved = false;
    protected float _remainingDistance;
    protected AlertStatus _alertStatus;
    protected float _walkSpeed;
    protected float _sprintSpeed;

    public EnemyState(NavMeshAgent agent, EnemyEventManager eventManager)
    {
        _agent = agent;
        _eventManager = eventManager;
    }

    protected bool CheckDestinationReached() { /*Debug.LogError("Checking Reached");*/ return _destinationReached; }

    protected bool CheckIfPlayerHasMoved() { return _playerHasMoved; }
    
    protected void SetDestinationReached(bool reached) { _destinationReached = reached; }
   
    public virtual void UpdateDistanceRemainingToDestination(float remainingDistance) { _remainingDistance = remainingDistance; }

    public virtual void EnterState(AlertStatus alertStatus = AlertStatus.None) {  _alertStatus = alertStatus; }
    public virtual void UpdateState() { }

    public virtual void LateUpdateState() { }
    public abstract void ExitState();
    public virtual void OnStateDestroyed()
    {
        _agent = null;
        _eventManager = null;
    }

    
}
