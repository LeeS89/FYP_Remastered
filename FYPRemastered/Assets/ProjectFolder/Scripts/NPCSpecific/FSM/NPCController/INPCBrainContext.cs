using UnityEngine;

public interface INPCBrainContext// : ITargetable
{
    StateId CurrentFsmState { get; }
    CombatOrder CurrentComOrder { get; }
    RotationOrder CurrentRotOrder { get; }
    FOVResult CurrentFovState { get; }

    void TryBroadcastAlert();
    void SwitchState(StateId intentState);
    void UpdateCombatOrder(CombatOrder newOrder);
    void UpdateCurrentFovStatus(FOVResult newStatus);
    void UpdateFovAlertPhase(AlertPhase newPhase);
    void UpdateRotationOrder(RotationOrder newOrder);
    void RotateToTarget(bool rotate);
}
