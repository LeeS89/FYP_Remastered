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
