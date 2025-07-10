/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaypointManager waypointManager = (WaypointManager)target;

        // Display the default inspector
        DrawDefaultInspector();

        // Add an input field for the number of waypoints per block
        waypointManager._waypointsPerBlock = EditorGUILayout.IntField("Waypoints Per Block", waypointManager._waypointsPerBlock);
        waypointManager._blockZone = EditorGUILayout.IntField("Waypoints Per Block", waypointManager._blockZone);

        // Add a button to create the waypoint block
        if (GUILayout.Button("Create Waypoint Block"))
        {
            //waypointManager.CreateWaypointBlock(waypointManager._waypointsPerBlock, waypointManager._blockZone);
        }
    }
}
*/