using UnityEngine;

public interface INPCBrainContext : ITargetable
{
    StateId CurrentFSMState { get; }
    CombatOrder CurrentComOrder { get; }
    RotationOrder CurrentRotOrder { get; }
    FOVResult CurrentFOVState { get; }

    void TryBroadcastAlert();
    void SwitchState(StateId intentState);
    void UpdateCombatOrder(CombatOrder newOrder);
    void UpdateFovStatus(FOVResult newStatus);
    void UpdateRotationOrder(RotationOrder newOrder);
}
