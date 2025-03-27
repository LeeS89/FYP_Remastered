using UnityEngine;
using UnityEngine.AI;

public class ChasingState : EnemyState
{
    public ChasingState(NavMeshAgent agent, EnemyEventManager eventManager) : base(agent, eventManager) { }
   

    public override void EnterState()
    {
        
    }

    public override void ExitState()
    {
        
    }

    public override void OnStateDestroyed()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
