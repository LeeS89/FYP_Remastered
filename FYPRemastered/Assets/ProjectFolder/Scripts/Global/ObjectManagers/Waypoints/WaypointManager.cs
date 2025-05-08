using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    [SerializeField] private GameObject _waypointPrefab;
    [SerializeField] private Color _waypointColor = Color.yellow;
    [SerializeField] private float _spacing = 2f;
    [SerializeField] private int _numberOfWaypoints = 5;
    private GameObject[] _waypoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
