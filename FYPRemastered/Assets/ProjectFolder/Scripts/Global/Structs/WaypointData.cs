using System.Collections.Generic;
using UnityEngine;

public struct WaypointData
{
    public List<Vector3> _waypointPositions;
    public List<Vector3> _waypointForwards;

    public void UpdateData(List<Vector3> positions, List<Vector3> forwards)
    {
        this._waypointPositions = positions;
        this._waypointForwards = forwards;
    }
}
