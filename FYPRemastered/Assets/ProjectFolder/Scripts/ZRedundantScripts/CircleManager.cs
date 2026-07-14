using System.Collections.Generic;
using UnityEngine;

public class CircleManager : MonoBehaviour
{
    [SerializeField] private List<CircletSet> _wpSets = new();

    public bool TryGetWaypointSet(out List<Transform> waypoints)
    {
        // Debug.LogError($"_wpSets Count is: {_wpSets.Count.ToString()}");
        waypoints = null;
        foreach (var set in _wpSets)
        {
            if (set.Get(out waypoints) && waypoints != null)
            {
                // Debug.LogError("Success in Getting");
                return true;
            }
            else
                continue;

        }
        return false;
    }

}



[System.Serializable]
public class CircletSet
{
    public List<Transform> _waypoints = new();
    private bool _indexZeroInUse = false;
    private bool _indexOneInUse = false;

    public bool Get(out List<Transform> points)
    {
        points = null;
        if (_indexZeroInUse && _indexOneInUse) return false;
        if (_indexZeroInUse)
        {
            points = new List<Transform>();
            points.Add(_waypoints[1]);
            points.Add(_waypoints[0]);
            _indexOneInUse = true;
            return true;
        }
        else// if (_indexOneInUse)
        {
            points = new List<Transform>();
            points.Add(_waypoints[0]);
            points.Add(_waypoints[1]);
            _indexZeroInUse = true;
            return true;
        }

    }
}