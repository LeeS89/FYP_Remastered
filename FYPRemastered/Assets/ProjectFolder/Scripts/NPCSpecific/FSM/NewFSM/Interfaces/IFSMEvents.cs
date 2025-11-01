using System;
using UnityEngine;
using UnityEngine.AI;

public interface IFSMEvents : ITickable, IZoneSink
{
    void BeginPatrol();
    void BeginChase();
    void BeginFlank();
    void TakeCover();
    void FollowGroup();

    //void BeginSearch();
    void ClearState();

    bool DestinationReached { get; }

    void OnPathRequestComplete(in PathResult result);

    bool HasLOS { get; }
   // Transform Transform { get; }

    StateNotificationProvider Notification { get; set; }

   // bool CurrentZone(out uint zone);
}

public interface IFSMOwner : ITargetable
{
  //  Transform Transform { get; }

    ITargetable PrimaryTarget { get; }

   // ITargetable GameObject { get; }

    EnemyEventManager OwnerEM { get; }

    NavMeshAgent Agent { get; }

    NavMeshObstacle Obstacle { get; }

    NavMeshPath Path { get; }

    float WalkSpeed { get; }
    float SprintSpeed { get; } 
}
