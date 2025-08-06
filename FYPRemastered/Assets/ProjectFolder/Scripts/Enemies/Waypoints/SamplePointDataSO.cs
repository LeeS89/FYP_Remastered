using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SamplePointDataSO", menuName = "Scriptable Objects/SamplePointDataSO")]
public class SamplePointDataSO : ScriptableObject
{
    public List<FlankPointData> savedPoints = new();

    public LayerMask flankBlockingMask;
    public LayerMask flankTargetMask;
    public LayerMask flankSecondaryTargetMask;

    public void ClearData()
    {
        savedPoints.Clear();
    }
}
