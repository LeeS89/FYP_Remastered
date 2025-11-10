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

    void OnPathRequestComplete(in PathResult result);

    bool HasLOS { get; }

 
    void LookAroundAndContinue();
  
   // StateNotificationProvider Notification { get; set; }

    void OnInstanceDestroyed();
   // bool CurrentZone(out uint zone);
}

public interface IFSMOwner : ITargetable
{
 
    ITargetable PrimaryTarget { get; }

    uint CurrentStateId { get; set; }

    EnemyEventManager OwnerEM { get; }

    NavMeshAgent Agent { get; }

    NavMeshObstacle Obstacle { get; }

    float MaxPatrolPointWaitTime { get; }

    float MinPatrolPointWaitTime { get; }

    NavMeshPath Path { get; }

    float WalkSpeed { get; }
    float SprintSpeed { get; }

    void LogUnhandled(IntentStateBase state, NotifyOwnerNPC notification) { }

  
    void DestinationReached(StateId reachedInState, bool isStale);

    void OnDestinationFound(StateId id, Vector3 destination, NavMeshPath path);

    void SwitchTo(IIntentState next) { }

    void Notify(in NotifyOwnerNPC n);

    IFSMEvents FSM { get; }

    float SprintEnterDist { get; }
    float SprintExitDist { get; }

}

public interface IFieldOfViewOwner : IFSMEvents
{
    void FieldOfViewSweepResult(FOVResult result, bool withinAttackAngles);

}

public interface IFieldOfViewRunner : ITickable
{
    void SetFOVSweepFrequency(AlertPhase phase);
}
