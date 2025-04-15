using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ZoneCellLabelDrawer
{
   /* static ZoneCellLabelDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!Application.isPlaying) return;

        var zoneManager = Object.FindFirstObjectByType<UniformZoneGridManager>();
        if (zoneManager == null || zoneManager.Cells == null) return;

        foreach (var cell in zoneManager.Cells.Values)
        {
            if (cell.Walkability <= 0f) continue; // Only show walkable cell labels

            Vector3 worldPos = cell.Center + Vector3.up * 0.1f;
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

            Handles.BeginGUI();

            GUIStyle labelStyle = new GUIStyle
            {
                normal = { textColor = Color.yellow },
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            Vector2 size = labelStyle.CalcSize(new GUIContent(cell.Name));
            Rect rect = new Rect(screenPos.x - size.x / 2, screenPos.y - size.y / 2, size.x, size.y);

            GUI.Label(rect, cell.Name, labelStyle);

            Handles.EndGUI();
        }
    }*/
}
