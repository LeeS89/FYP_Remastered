using UnityEngine;
using UnityEngine.AI;

public class StationaryState : EnemyState
{
    public StationaryState(NavMeshAgent agent, EnemyEventManager eventManager) : base(agent, eventManager) { }
   

    public override void EnterState(AlertStatus alertStatus = AlertStatus.None)
    {
        base.EnterState(alertStatus);

        switch (_alertStatus)
        {
            case AlertStatus.None:
                _eventManager.DestinationUpdated(_agent.transform.position);
                Debug.LogError("In Stationary State, no alert status");
                break;
            case AlertStatus.Alert:
                Debug.LogError("In Stationary State, with alert status");
                break;
        }
        

        _eventManager.SpeedChanged(0f, 10f);
    }

    public override void ExitState()
    {
        throw new System.NotImplementedException();
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
