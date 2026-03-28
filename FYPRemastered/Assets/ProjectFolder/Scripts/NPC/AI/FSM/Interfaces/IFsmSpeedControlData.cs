using UnityEngine;

public interface IFsmSpeedControlData : IFsmData
{
    float SprintEnterDistance { get; }
    float SprintExitDistance { get; }
    float WalkSpeed { get; }
    float SprintSpeed { get; }

}
