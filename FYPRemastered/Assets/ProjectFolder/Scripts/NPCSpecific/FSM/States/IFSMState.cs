using UnityEngine;

public interface IFSMState : ITickable
{
    void EnterState();

    void ExitState();

    void OnDestinationReached();

    void OnDestinationSet();

    // void ValidateCandidateDestinations();

    void TryRepath();

    bool NeedsNewPath();
    //void RetrieveCandidateDestinations();

    bool UsesRandomAgentStopDistance { get; }
    StateId GetId();
}
