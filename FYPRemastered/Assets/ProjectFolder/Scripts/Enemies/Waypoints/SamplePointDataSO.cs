using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SamplePointDataSO", menuName = "Scriptable Objects/SamplePointDataSO")]
public class SamplePointDataSO : ScriptableObject
{
    public List<FlankPointData> savedPoints = new();

    public void ClearData()
    {
        savedPoints.Clear();
    }
}
