using System;
using UnityEngine;

[Obsolete]
public interface IFovDeps
{
    public void DebugFrequency();

    public LayerMask WorldMask();
    public LayerMask BlockingMask();
    public Transform OwnerOrigin();
    public Transform SweepOrigin();
    public float FovRadius();
    public float FovHalfAngle();
    public float HalfShootAngle();
    public float GetSweepFrequency();
    public int MaxTargets();
    public bool UseShootingAngleRestriction();
    public void SetTargetProximityStatus(bool targetInsideRadius);
    public ITargetable Target { get; }

}
