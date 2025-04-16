using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SamplePointData
{
    public Vector3 position;

    // Step # → list of reachable point indices
    public Dictionary<int, List<int>> reachableSteps = new Dictionary<int, List<int>>();
}
