using UnityEngine;

public interface IFSMState
{
    void EnterState();

    void ExitState();

    void OnDestinationReached();

    void TryGetNewDestination();

    StateId Id { get; }
}
