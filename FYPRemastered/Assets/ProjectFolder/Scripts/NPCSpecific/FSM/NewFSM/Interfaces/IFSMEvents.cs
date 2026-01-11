using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Obsolete]
public interface IFSMEvents : ITickable, IZoneSink
{
    void BeginPatrol(StateId id);
    void BeginChase(StateId id);
    void BeginFlank(StateId id);
    void TakeCover(StateId id);
    void FollowGroup(StateId id);

    bool IsMoving();
  
    Action<StateId> TryRepath { get; }

    //void BeginSearch();
    void ExitState();

    bool DestinationReached { get; }

   // void OnPathRequestComplete(in PathResult result);

    bool HasLOS { get; }

 
    void LookAroundAndContinue();
  
   // StateNotificationProvider Notification { get; set; }

    void OnInstanceDestroyed();
   // bool CurrentZone(out uint zone);
}

[Obsolete]
public interface IFSMControlObsolete : /*IFSMState, */ITickable
{
    StateId CurrentStateId { get; }

    void SwitchTo(StateId state);

    void BeginPatrol(StateId id);
    void BeginChase(StateId id);
    void BeginFlank(StateId id);
    void TakeCover(StateId id);
    void FollowGroup(StateId id);
    // void ExitState();
    bool IsMoving();
    //  bool TryGetPatrolZone(out int zone);

    int? TryGetPatrolZone();

    delegate void OnNotifyOwner(in NPCNotification n);
    OnNotifyOwner Notification { get; set; }
    Action<AnimationCue> OnAnimationIntent { get; set; }


    Action<Vector3> OnMapDestinationToZone { get; set; }

}

public interface IFSMControl : IFSMStateContext, ITickable
{
    StateId CurrentStateId { get; }
    bool IsInStateTransition { get; }
    void SwitchTo(StateId state);

    bool RotatingToTarget { get; }
    void RotateToTarget(bool rotate);
    //delegate void OnNotifyOwner(in NPCNotification n);
   // OnNotifyOwner Notification { get; set; }
    Notification Notification { get; set; }
}

public interface ITargetRef { ITargetable Target { get; } }

public interface IFSMStateContext : IAnimationCueSource//, ITargetRef
{
  //  ITargetable Owner { get; }
    Action<Vector3> OnMapDestinationToZone { get; set; } // Take out
    Vector3? CurrentDestinationForward { get; } // Obsolete
    bool HasReachedDestination(); // Take out

    void OnDestinationResultReceived(in DestinationResultNew result);
}


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
}

public interface IFsmStateDeps : IFsmDeps
{
    //ITargetable NpcOwner { get; }
    IPathResolver PathResolver { get; }
    NavMeshPath Path();
}

public interface IPatrolDeps : IFsmStateDeps, IFsmDeps
{
    IWaypointService WaypointService { get; }
    float MaxTimeAtPatrolPoint { get; }
    float MinTimeAtPatrolPoint { get; }
}

public interface IChaseDeps : IFsmStateDeps, ITargetRef, IFsmDeps
{
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

[Obsolete]
public interface IAgentData// : ITargetable
{
  //  ITargetable PrimaryTarget { get; }
    NavMeshAgent Agent { get; }
    NavMeshObstacle Obstacle { get; }
    NavMeshPath Path { get; }
 //   float MaxPatrolPointWaitTime { get; }
  //  float MinPatrolPointWaitTime { get; }
   // int MaxFlankSteps { get; }
  //  int MinFlankSteps { get; }
 //   float WalkSpeed { get; }
 //   float SprintSpeed { get; }
    float SprintEnterDist { get; }
    float SprintExitDist { get; }
    //Func<StateId, float> OnRequestAgentStoppingDistance { get; }
    //float GetAgentStoppingDistance(StateId currentState);

}

[Obsolete]
public interface IFSMOwner // Maybe Obsolete
{
   // void TryBroadcastAlert(); // Remove, NPCControllerBase will handle this
    void LogUnhandled(IntentStateBaseObsolete state, in NPCNotification notification);
    void SwitchTo(IIntentStateObsolete next);
    void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles);
    IFSMControlObsolete FSM { get; }

}


public interface INotificationListener
{
    void OnNotify(in NPCNotification n);
  //  void EnterAlertPhase();
}


public interface IFieldOfViewRunner : ITickable
{
    void SetAlertPhase(AlertPhase phase);
    //Action<FOVResult, bool> OnFOVSweepComplete { get; set; }
    // Action<NPCNotification> OnFOVSweepCompleted { get; set; }
    Notification OnFOVSweepComplete { get; set; }
}
