using UnityEngine;

public interface IFSMState
{
    void EnterState(StateId id);

    void ExitState(StateId id);

    void OnDestinationReached();
}
