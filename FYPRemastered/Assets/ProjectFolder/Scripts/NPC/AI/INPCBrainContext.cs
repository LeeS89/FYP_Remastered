using UnityEngine;

public interface INpcBrainContext : ITargetable
{
    StateId CurrentFsmState { get; }
    CombatOrder CurrentComOrder { get; }
   // RotationOrder CurrentRotOrder { get; }
    FOVResult CurrentFovState { get; }

    void TryBroadcastAlert();
    void SwitchState(StateId intentState);
    void UpdateCombatOrder(CombatOrder newOrder);
    void UpdateCurrentFovStatus(FOVResult newStatus);
    void UpdateAlertPhase(AlertPhase newPhase);
   // void UpdateRotationOrder(RotationOrder newOrder);
   // void RotateToTarget(bool rotate);
    void TriggerDeath();
    void OverrideSpeed(SpeedOverride speedOverride);
    void OverrideRotation(RotationOverride rotOverride);

    void SendAnimationIntent(AnimationCue cue);
    void MapDestinationToZone(Vector3 destination);
}
