using UnityEngine;

public sealed class FsmControlBridge : FsmServiceBridge<IFsmSpeedService>, IFsmSpeedControl
{
    private float _targetSpeed;
    private float _currentSpeed;
    private float _accel;
    private SpeedOverride _currentSpeedOverride = SpeedOverride.None;

    public FsmControlBridge(IFsmSpeedService service) : base(service) { }

    public float SprintEnterDistance => _service.GetSprintEnterDistance();
    public float SprintExitDistance => _service.GetSprintExitDistance();

    public float WalkSpeed => _service.GetWalkSpeed();

    public float SprintSpeed => _service.GetSprintSpeed();


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

