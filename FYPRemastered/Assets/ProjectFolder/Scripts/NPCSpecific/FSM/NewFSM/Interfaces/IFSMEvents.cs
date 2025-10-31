using System;
using UnityEngine;
using UnityEngine.AI;

public interface IFSMEvents : ITickable
{
    void BeginPatrol();
    void BeginChase();
    void BeginFlank();
    void TakeCover();
    void FollowGroup();

    //void BeginSearch();
    void ClearState();

    void OnPathRequestComplete(in PathResult result);

    bool HasLOS { get; }
   // Transform Transform { get; }

    StateNotificationProvider Notification { get; set; }
}

public interface FSMOwner : ITargetable
{
    Transform Transform { get; }

    ITargetable PrimaryTarget { get; }

    EnemyEventManager OwnerEM { get; }

    NavMeshAgent Agent { get; }

    NavMeshObstacle Obstacle { get; }

    NavMeshPath Path { get; }
}
