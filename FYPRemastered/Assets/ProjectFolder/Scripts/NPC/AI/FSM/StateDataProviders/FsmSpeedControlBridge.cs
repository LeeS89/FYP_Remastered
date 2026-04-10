using UnityEngine;

public sealed class FsmSpeedControlBridge : FsmServiceBridge<IFsmSpeedService>, IFsmSpeedControl
{
    private float _targetSpeed;
    private float _currentSpeed;
    private float _accel; // Future fix to replace lerp with actual acceleration value
    private SpeedOverride _currentSpeedOverride = SpeedOverride.None;

    public FsmSpeedControlBridge(IFsmSpeedService service) : base(service) { }

    private float SprintEnterDistance => _service.GetSprintEnterDistance();
    private float SprintExitDistance => _service.GetSprintExitDistance();

    private float WalkSpeed => _service.GetWalkSpeed();

    private float SprintSpeed => _service.GetSprintSpeed();


    public void UpdateTargetSpeed(float remainingDistance)
    {
        if(remainingDistance <= 0.25f)
        {
            _targetSpeed = 0f;
            return;
        }

        if (HasOverride()) return;

        if (remainingDistance > SprintEnterDistance) { _targetSpeed = SprintSpeed;  return; }
        if (remainingDistance < SprintExitDistance) { _targetSpeed = WalkSpeed; return; }

        if (_targetSpeed < WalkSpeed) _targetSpeed = WalkSpeed;
    }

    private bool HasOverride()
    {
        if (_currentSpeedOverride == SpeedOverride.None) return false;

        if(_currentSpeedOverride == SpeedOverride.ForceWalk) _targetSpeed = WalkSpeed;
        else if(_currentSpeedOverride == SpeedOverride.ForceSprint) _targetSpeed = SprintSpeed;
        else if(_currentSpeedOverride == SpeedOverride.ForceIdle) _targetSpeed = 0f;

        return true;
    }

    public float UpdateSpeed(float dt)
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, 5f * dt);
        return _currentSpeed;
    }

    public void OverrideMovement(SpeedOverride overrideTier) 
        => _currentSpeedOverride = overrideTier;
}

