using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("", true)]
public struct WaypointDataObsolete
{
    public List<Vector3> _waypointPositions;
    public List<Vector3> _waypointForwards;

    public void UpdateData(List<Vector3> positions, List<Vector3> forwards)
    {
        this._waypointPositions = positions;
        this._waypointForwards = forwards;
    }
}
