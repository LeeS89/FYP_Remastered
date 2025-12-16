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

// Obsolete
public interface IFSMControl : /*IFSMState, */ITickable
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

public interface IFSMControlNew : IFSMStateContext, ITickable
{
    StateId CurrentStateId { get; }
    bool IsInStateTransition { get; }
    void SwitchTo(StateId state);

    delegate void OnNotifyOwner(in NPCNotification n);
    OnNotifyOwner Notification { get; set; }
}

public interface IFSMStateContext : IAnimationCueSource
{
    Action<Vector3> OnMapDestinationToZone { get; set; }
    Vector3? CurrentDestinationForward { get; }
    bool IsStationary();

    void OnDestinationResultReceived(in DestinationResult result);
}


public interface IAgentData : ITargetable
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
    Func<StateId, float> OnRequestAgentStoppingDistance { get; }
    //float GetAgentStoppingDistance(StateId currentState);

}

public interface IFSMOwner // Maybe Obsolete
{
   // void TryBroadcastAlert(); // Remove, NPCControllerBase will handle this
    void LogUnhandled(IntentStateBase state, in NPCNotification notification);
    void SwitchTo(IIntentState next);
    void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles);
    IFSMControl FSM { get; }

}


public interface INotificationListener
{
    void Notify(in NPCNotification n);
  //  void EnterAlertPhase();
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
