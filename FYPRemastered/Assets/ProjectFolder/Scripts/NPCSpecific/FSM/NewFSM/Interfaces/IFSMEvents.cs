using Npc.API;
using Npc.Internal;
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;



public interface ITargetRef { ITargetable Target { get; } }







public interface IFsmDeps
{
    ITargetable Owner { get; }
    /* NavMeshAgent Agent();
     NavMeshObstacle Obstacle();
     float WalkSpeed { get; }
     float SprintSpeed { get; }*/

}

public interface IFsmControllerDeps : IFsmDeps, ITargetRef
{
    NavMeshAgent Agent();
    NavMeshObstacle Obstacle();
    float GetAgentStopDistance(bool getRandomDistance);
    float WalkSpeed { get; }
    float SprintSpeed { get; }
    float PathStatusInterval { get; }
    SpeedTier TryUpdateAgentTargetSpeed(SpeedTier currentTier, SpeedOverride speedOverride, float distanceToDestination, out float newSpeed, out float lerp);
}

public interface IFsmStateDeps : IFsmDeps
{
    //ITargetable NpcOwner { get; }
    IPathResolver PathResolver { get; }
    NavMeshPath Path();
}

public interface IPatrolDeps : IFsmStateDeps//, IFsmDeps
{
    IPatrolService WaypointService { get; }
    float MaxTimeAtPatrolPoint { get; }
    float MinTimeAtPatrolPoint { get; }
}

public interface IChaseDeps : IFsmStateDeps, ITargetRef, IFsmDeps
{
    IDistanceMonitoringService DistanceService { get; }
    //float MinStoppingDistance { get; }
    //  float MaxStoppingDistance { get; }

    // Distance Job Service
}

public interface IFlankDeps : IFsmStateDeps, ITargetRef, IFsmDeps
{
    IFlankService FlankService { get; }
    int MaxFlankSteps { get; }
    int MinFlankSteps { get; }
}






public interface INotificationListener
{
    void OnNotifies(in NpcNotification n);
  //  void EnterAlertPhase();
}




public class SharedFsmStateServices
{
    public NavMeshPath Path { get; }
    public Transform OwnerTransform { get; }

    public TryGetTarget OnTryGetCurrentTarget;
    //public Func<ITargetable> GetCurrentTarget { get; }

    public SharedFsmStateServices(NavMeshPath path, Transform ownerTransform, TryGetTarget tryGetCurrentTarget)
    {
        Path = path;
        OwnerTransform = ownerTransform;
        OnTryGetCurrentTarget = tryGetCurrentTarget; 
    }
}





    









