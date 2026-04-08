using UnityEngine;

public sealed class FsmControlBridge : FsmServiceBridge<IFsmSpeedService>, IFsmSpeedData
{
    private float _targetSpeed;
    private float _currentSpeed;
    private float _accel;

    public FsmControlBridge(IFsmSpeedService service) : base(service) { }

    public float SprintEnterDistance => _service.GetSprintEnterDistance();
    public float SprintExitDistance => _service.GetSprintExitDistance();

    public float WalkSpeed => _service.GetWalkSpeed();

    public float SprintSpeed => _service.GetSprintSpeed();


    public void UpdateTargetSpeed(float remainingDistance) { }
    public float UpdateSpeed(float dt)
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, 5f * dt);
        return _currentSpeed;
    }

    public void OverrideMovement() { }
}
