using System;
using UnityEngine;

public interface IChaseService : IFsmService
{
    //bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer);
    bool TargetIsMoving(IInstanceIdentifiable id);
    bool TryGetSqrDistanceToTarget(IInstanceIdentifiable id, Vector3 from, out float sqrDistance);

    bool TryRegisterDistanceToTargetMonitoring(IInstanceIdentifiable id, Action<float> onDistanceUpdate, out float initialDistance);
    bool TryUnregisterDistanceToTargetMonitoring(IInstanceIdentifiable id);
}
