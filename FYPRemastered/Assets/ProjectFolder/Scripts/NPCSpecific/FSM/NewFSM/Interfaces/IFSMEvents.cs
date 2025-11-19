using System;
using UnityEngine;
using UnityEngine.AI;

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

public interface IFSMControl : ITickable
{
    void BeginPatrol(StateId id);
    void BeginChase(StateId id);
    void BeginFlank(StateId id);
    void TakeCover(StateId id);
    void FollowGroup(StateId id);
    void ExitState();
    bool IsMoving();
  //  bool TryGetPatrolZone(out int zone);

    int? TryGetPatrolZone();

    delegate void OnNotifyOwner(in NotifyOwnerNPC n);
    OnNotifyOwner Notification { get; set; }
    Action<AnimationCue> OnAnimationIntent { get; set; }
    Action<bool, int> OnWaypointZoneReceived { get; set; }
    }


public interface IFSMData : ITargetable
{
    ITargetable PrimaryTarget { get; }
    NavMeshAgent Agent { get; }
    NavMeshObstacle Obstacle { get; }
    NavMeshPath Path { get; }
    float MaxPatrolPointWaitTime { get; }
    float MinPatrolPointWaitTime { get; }
    float WalkSpeed { get; }
    float SprintSpeed { get; }
    float SprintEnterDist { get; }
    float SprintExitDist { get; }
    float GetAgentStoppingDistance(StateId currentState);

}

public interface IFSMOwner
{
   // void TryBroadcastAlert(); // Remove, NPCControllerBase will handle this
    void LogUnhandled(IntentStateBase state, in NotifyOwnerNPC notification);
    void SwitchTo(IIntentState next);
    void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles);
    IFSMControl FSM { get; }

}

public interface IZoneAlertListener
{
    void EnterAlertPhase();
}


public interface IFieldOfViewOwner // Obsolete
{
    void FieldOfViewSweepResult(FOVResult result, bool withinAttackAngles);

}

public interface IFieldOfViewRunner : ITickable
{
    void SetFOVSweepFrequency(AlertPhase phase);
    Action<FOVResult, bool> OnFOVSweepComplete { get; set; }
}
