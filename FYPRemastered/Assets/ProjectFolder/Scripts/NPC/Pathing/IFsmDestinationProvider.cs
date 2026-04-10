using System.Collections.Generic;
using UnityEngine;

public interface IFsmDestinationProvider
{
    float GetArrivalThreshold();
    bool TryGetDestinationCandidates(List<Vector3> buffer);
    void ReleaseCandidates(List<Vector3> buffer);
}
