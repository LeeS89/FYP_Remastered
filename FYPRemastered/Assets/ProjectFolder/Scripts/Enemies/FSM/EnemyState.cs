using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState
{
  
    protected NavMeshAgent _agent;
    protected Coroutine _coroutine;
    protected EnemyEventManager _eventManager;

    public EnemyState(NavMeshAgent agent, EnemyEventManager eventManager)
    {
        _agent = agent;
        _eventManager = eventManager;
    }
    


    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void OnStateDestroyed();
}
