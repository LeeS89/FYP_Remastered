using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SamplePointDataSO", menuName = "Scriptable Objects/SamplePointDataSO")]
public class SamplePointDataSO : ScriptableObject
{
    public List<SamplePointData> savedPoints = new();

    public void ClearData()
    {
        savedPoints.Clear();
    }
}
