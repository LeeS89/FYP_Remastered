using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StepEntry
{
    public int step;
    public List<int> reachableIndices = new List<int>();
}
