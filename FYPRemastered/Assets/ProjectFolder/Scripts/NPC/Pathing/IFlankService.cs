using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFlankService : IFsmService
{
    void TryGetFlankCandidates(Vector3 flankTargetPos, int numSteps, List<Vector3> buffer, Action<bool> OnRequestComplete);
}
