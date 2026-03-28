using UnityEngine;

public interface IFsmSpeedService : IFsmService
{
    float GetSprintEnterDistance();
    float GetSprintExitDistance();
    float GetWalkSpeed();
    float GetSprintSpeed();
}
