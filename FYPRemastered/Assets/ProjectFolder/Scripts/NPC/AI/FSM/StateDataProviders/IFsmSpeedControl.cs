using UnityEngine;

public interface IFsmSpeedControl : IFsmData
{
    #region To be made redundant by the service bridge
   /* float SprintEnterDistance { get; }
    float SprintExitDistance { get; }
    float WalkSpeed { get; }
    float SprintSpeed { get; }*/
    #endregion

    void UpdateTargetSpeed(float remainingDistance);
    float UpdateSpeed(float dt);
    void OverrideMovement(SpeedOverride overrideTier);

}
