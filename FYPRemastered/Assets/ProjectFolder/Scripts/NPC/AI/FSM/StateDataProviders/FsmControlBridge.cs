using UnityEngine;

public sealed class FsmControlBridge : FsmServiceBridge<IFsmSpeedService>, IFsmSpeedData
{
    public FsmControlBridge(IFsmSpeedService service) : base(service) { }

    public float SprintEnterDistance => _service.GetSprintEnterDistance();
    public float SprintExitDistance => _service.GetSprintExitDistance();

    public float WalkSpeed => _service.GetWalkSpeed();

    public float SprintSpeed => _service.GetSprintSpeed();
}
