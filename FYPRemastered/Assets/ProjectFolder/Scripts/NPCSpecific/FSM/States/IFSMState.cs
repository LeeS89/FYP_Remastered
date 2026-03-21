using UnityEngine;

public interface IFsmState : ITickable
{
    void EnterState();

    void ExitState();

    void OnDestinationReached();

    void OnDestinationSet();

    // void ValidateCandidateDestinations();

    void TryRepath();

    bool NeedsNewPath();
    //void RetrieveCandidateDestinations();

   // bool UsesRandomAgentStopDistance { get; }
    StateId GetId();

    float GetDesiredStoppingDistance();
}


public interface IFsmStateNew<out TContext> : ITickable
{
    TContext Context { get; }
   
    void EnterState();

    void ExitState();

    void OnDestinationReached();

    void OnDestinationSet();

    // void ValidateCandidateDestinations();

    void TryRepath();

    bool NeedsNewPath();
    //void RetrieveCandidateDestinations();

   // bool UsesRandomAgentStopDistance { get; }
    StateId GetId();

    float GetDesiredStoppingDistance();
}


public interface IFsmController : ITickable
{
    bool IsInStateTransition { get; }
    bool HasReachedDestination();
    ///Above to keep NPCController working for now

    void SwitchTo(StateId state);
    StateId CurrentState { get; }

    void OverrideSpeed(SpeedOverride speedOverride);
    void OverrideRotation(RotationOverride rotOverride);

    void Reset();

    bool StateExists(StateId id);
}