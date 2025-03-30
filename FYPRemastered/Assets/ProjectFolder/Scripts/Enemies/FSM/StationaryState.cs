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
                //_eventManager.StopImmediately();
                //_eventManager.DestinationUpdated(_agent.transform.position);
                //_eventManager.SpeedChanged(0f, 10f);
                Debug.LogError("In Stationary State, no alert status");
                break;
            case AlertStatus.Alert:
                Debug.LogError("In Stationary State, with alert status");
                break;
            default:
                _eventManager.DestinationUpdated(_agent.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
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
        base.OnStateDestroyed();
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
