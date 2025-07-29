using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SamplePointData
{
    public Vector3 position;

    // Step # → list of reachable point indices
    [NonSerialized] public Dictionary<int, List<int>> reachableSteps = new Dictionary<int, List<int>>();

    public List<StepEntry> serializedReachableSteps = new();
}
