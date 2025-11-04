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


    void DestinationApproved(NavMeshPath path, Vector3 newDestination, StateId ApprovalState, float newAgentpeed, float lerp);
    Action TryRepath { get; }

    //void BeginSearch();
    void ExitState();

    bool DestinationReached { get; }

    void OnPathRequestComplete(in PathResult result);

    bool HasLOS { get; }

  //  Action GetNext { get; }

    void LookAroundAndContinue();
   // Transform Transform { get; }

    StateNotificationProvider Notification { get; set; }

   // bool CurrentZone(out uint zone);
}

public interface IFSMOwner : ITargetable
{
  //  Transform Transform { get; }

    ITargetable PrimaryTarget { get; }

   // ITargetable GameObject { get; }

    uint CurrentStateId { get; set; }

    EnemyEventManager OwnerEM { get; }

    NavMeshAgent Agent { get; }

    NavMeshObstacle Obstacle { get; }

    float MaxWaitTime { get; }

    float MinWaitTime { get; }

    NavMeshPath Path { get; }

    float WalkSpeed { get; }
    float SprintSpeed { get; }

    void LogUnhandled(IntentStateBase state, NotifyOwnerNPC notification) { }

    //  void SetAgentTargetSpeed(float speed = 0, float lerpSpeed = 10);

   // (float, float) GetSpeedAndLerp(StateId id);

    void DestinationReached(StateId reachedInState, bool isStale);

    void OnDestinationFound(StateId id, Vector3 destination, NavMeshPath path);

    void SwitchTo(IIntentState next) { }
    IFSMEvents FSM { get; }

}
