using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChasingState : EnemyState
{
    public ChasingState(NavMeshAgent agent, EnemyEventManager eventManager) : base(agent, eventManager) { }
   

    public override void EnterState(AlertStatus alertStatus = AlertStatus.None)
    {
        Vector3 _playerPos = GameManager.Instance.GetPlayerPosition().position;
        int randomStoppingDistance = Random.Range(5,11);
        _eventManager.DestinationUpdated(_playerPos, randomStoppingDistance);

        _eventManager.SpeedChanged(0.9f, 2f);
    }

   
   
    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public override void LateUpdateState()
    {
        
    }

    public override void ExitState()
    {

    }

    public override void OnStateDestroyed()
    {
        throw new System.NotImplementedException();
    }

}
