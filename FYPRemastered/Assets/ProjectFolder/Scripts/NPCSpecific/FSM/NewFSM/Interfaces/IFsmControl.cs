using UnityEngine;

public interface IFsmControl : IFSMStateContext, ITickable
{
    bool TestPrint { get; set; }
    StateId CurrentStateId { get; }
    bool IsInStateTransition { get; }
    void SwitchTo(StateId state);
    void OverrideSpeed(SpeedOverride overrideTier);
    // bool RotatingToTarget { get; }
    // void RotateToTarget(bool rotate);
    void OverrideRotation(RotationOverride rotOverride);

    // Notification Notification { get; set; }
}
