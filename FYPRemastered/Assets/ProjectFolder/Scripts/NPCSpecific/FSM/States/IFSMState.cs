using UnityEngine;

public interface IFSMState : ITickable
{
    void EnterState();

    void ExitState();

    void OnDestinationReached();

    void OnDestinationSet();

    void ValidateCandidateDestinations();

    bool UsesRandomAgentStopDistance { get; }
    StateId GetId();
}
