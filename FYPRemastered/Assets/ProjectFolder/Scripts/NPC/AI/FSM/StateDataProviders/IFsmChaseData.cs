using System;
using UnityEngine;

public interface IFsmChaseData : IFsmData
{
    bool TryRegisterDistanceMonitoring(IInstanceIdentifiable id, Vector3 currentPosition, /*ITargetable targetToCompare,*/ Action<float> callback, out float initDist);
    bool TryUnregisterDistanceMonitoring(IInstanceIdentifiable id);
    bool TargetIsMoving();
}
