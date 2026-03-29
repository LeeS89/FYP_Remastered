using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPatrolService : IFsmService
{
    float GetIdleTimeSeconds();
    // bool TryGetWaypoints(object requester, List<Vector3> buffer);

    // bool TryReleaseWaypoints(object requester, List<Vector3> buffer);
}

