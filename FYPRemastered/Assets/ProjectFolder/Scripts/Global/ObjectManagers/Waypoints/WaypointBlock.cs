using UnityEngine;

public class WaypointBlock : MonoBehaviour
{
    [SerializeField] private GameObject _waypointPrefab;
    [SerializeField] private Color _waypointColor = Color.yellow;
    [SerializeField] private float _spacing = 2f;
    public int _numberOfWaypoints = 5;
    private GameObject[] _waypoints;

    
    public void InstantiateWaypoints(GameObject blockParent)
    {
        Debug.LogError("Instantiate called");
        _waypoints = new GameObject[_numberOfWaypoints];
        
        for(int i = 0; i < _numberOfWaypoints; i++)
        {
            Vector3 position = new Vector3(i * _spacing, 0, 0);

            GameObject waypoint = Instantiate(_waypointPrefab, position, Quaternion.identity, blockParent.transform);
            waypoint.name = "Waypoint " + (i + 1);

            Renderer waypointRenderer = waypoint.GetComponent<Renderer>();
            if (waypointRenderer != null)
            {
                waypointRenderer.material.color = _waypointColor;
            }

            _waypoints[i] = waypoint;

            AddLabelToWaypoint(waypoint, i + 1);
        }
    }

    private void AddLabelToWaypoint(GameObject waypoint, int waypointNumber)
    {
        GameObject label = new GameObject("WaypointLabel");
        label.transform.SetParent(waypoint.transform);
        label.transform.localPosition = Vector3.up * 1.5f; // Position the label above the waypoint

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = waypointNumber.ToString();
        textMesh.fontSize = 16;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.black;
        
    }
}
