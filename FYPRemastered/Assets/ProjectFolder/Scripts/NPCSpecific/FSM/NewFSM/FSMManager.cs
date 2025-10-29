using System;
using UnityEngine;
using UnityEngine.AI;

public class FSMManager : IFSMEvents
{
    public Action<float> Tick { get; private set; }
    public StateNotificationProvider Notification { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private Action<float> OnPatrol;

    private NavMeshAgent _agent;
    private NavMeshObstacle _obstacle;
    private EnemyEventManager _eventManager;
   
    public FSMManager(EnemyEventManager em, NavMeshAgent agt, NavMeshObstacle ob)
    {
        _eventManager = em;
        _agent = agt;
        _obstacle = ob;
    }



    private bool IsDestinationReached() => false;

    public void BeginPatrol()
    {
        throw new NotImplementedException();
    }

    public void BeginChase()
    {
        throw new NotImplementedException();
    }

    public void BeginFlank()
    {
        throw new NotImplementedException();
    }

    public void TakeCover()
    {
        throw new NotImplementedException();
    }

    public void FollowGroup()
    {
        throw new NotImplementedException();
    }

    public void ClearState()
    {
        throw new NotImplementedException();
    }

    //  public required NavMeshAgent agent { get; init; }
}
