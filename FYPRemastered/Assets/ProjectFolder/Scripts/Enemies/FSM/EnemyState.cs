using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState
{
  
    protected NavMeshAgent _agent;
    protected Coroutine _coroutine;
    protected EnemyEventManager _eventManager;
    protected bool _destinationReached = false;
    protected float _remainingDistance;

    public EnemyState(NavMeshAgent agent, EnemyEventManager eventManager)
    {
        _agent = agent;
        _eventManager = eventManager;
    }

    protected void SetDestinationReached(bool reached) { _destinationReached = reached; }
   
    public virtual void UpdateDistanceRemainingToDestination(float remainingDistance) { _remainingDistance = remainingDistance; }

    public abstract void EnterState();
    public abstract void UpdateState();

    public abstract void ExitState();
    public abstract void OnStateDestroyed();

    
}
