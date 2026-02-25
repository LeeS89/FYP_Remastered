using Npc.Internal;
using System;
using UnityEngine;
using UnityEngine.AI;



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
    IWaypointService WaypointService { get; }
    float MaxTimeAtPatrolPoint { get; }
    float MinTimeAtPatrolPoint { get; }
}

public interface IChaseDeps : IFsmStateDeps, ITargetRef, IFsmDeps
{
    IDistanceService DistanceService { get; }
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
    public IPathResolver PathResolver { get; }
    public NavMeshPath Path { get; }
    public Transform OwnerTransform { get; }
    public IFsmStateEvents Events { get; }
    public Func<ITargetable> GetCurrentTarget { get; }

    public SharedFsmStateServices(IPathResolver pResolver, NavMeshPath path, Transform ownerTransform, IFsmStateEvents events, Func<ITargetable> getCurrentTarget)
    {
        PathResolver = pResolver;
        Path = path;
        OwnerTransform = ownerTransform;
        Events = events;
        GetCurrentTarget = getCurrentTarget; 
    }
}

public class FsmManagerServices
{
    public NavMeshAgent Agent { get; }
    public NavMeshObstacle Obstacle { get; }
    public MovementConfig Movement { get; }

    public FsmManagerServices(NavMeshAgent agent, NavMeshObstacle obstacle, MovementConfig config)
    {
        Agent = agent;
        Obstacle = obstacle;
        Movement = config;
    }

    
}

[System.Serializable]
public class MovementConfig
{
    [Header("Agent Base Speeds")]
    public float walkSpeed = 0.9f;
    public float sprintSpeed = 3.6f;

    [Header("Sprint/ Walk thresholds")]
    public float sprintEnterDistance = 15;
    public float sprintExitDistance = 12;

    [Header("Stopping - When remaining distance is <= stopping distance + threshold")]
    public float stopDistancethreshold = 0.25f;

    [Header("Path status check interval")]
    public float pathStatusInterval = 0.1f;

    [Header("Lerp Settings")]
    public float idleLerp = 10f;
    public float moveLerp = 2f;
}

