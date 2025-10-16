using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlankPointData
{
    public Vector3 position;

    // Step # → list of reachable point indices
    [NonSerialized] public Dictionary<int, List<int>> reachableSteps = new Dictionary<int, List<int>>();

    public List<StepEntry> serializedReachableSteps = new();


    /// <summary>
    /// /////////////////////////////////////// NEW
    /// </summary>
    public List<StepEntry> stepLinks = new();

    [NonSerialized] public bool inUse = false;
}
