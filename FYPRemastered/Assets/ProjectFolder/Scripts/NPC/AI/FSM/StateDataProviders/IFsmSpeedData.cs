using UnityEngine;

public interface IFsmSpeedData : IFsmData
{
    float SprintEnterDistance { get; }
    float SprintExitDistance { get; }
    float WalkSpeed { get; }
    float SprintSpeed { get; }

}
