using UnityEngine;

[CreateAssetMenu(fileName = "WaypointBlockData", menuName = "Scriptable Objects/WaypointBlockData")]
public class WaypointBlockData : ScriptableObject
{
    public BlockData[] blockDataArray;

    public void ClearData()
    {
        blockDataArray = new BlockData[0];
    }
}
