using System.Collections.Generic;
using UnityEngine;

public interface IFsmDestinationProvider
{
    float GetStoppingDistance();
    bool TryGetDestinationCandidates(List<Vector3> buffer);
    void ReleaseCandidates(List<Vector3> buffer);
}
