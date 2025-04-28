/*using UnityEngine;

public class TriangleCounter : MonoBehaviour
{
    private int triangleCount;

    void Update()
    {
        triangleCount = 0;
        var camera = Camera.main;
        if (camera == null) return;

        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        foreach (var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.InstanceID))
        {
            var renderer = filter.GetComponent<Renderer>();
            if (renderer == null) continue;

            if (!GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
                continue;

            if (filter.sharedMesh != null && filter.sharedMesh.isReadable)
            {
                triangleCount += filter.sharedMesh.triangles.Length / 3;
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.cyan;
        GUI.Label(new Rect(20, 20, 400, 50), "Triangles (in view): " + triangleCount, style);
    }
}
*/