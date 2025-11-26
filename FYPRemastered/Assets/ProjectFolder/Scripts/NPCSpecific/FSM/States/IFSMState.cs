using UnityEngine;

public interface IFSMState
{
    void EnterState(StateId id);

    void ExitState();

    void OnDestinationReached();
}
