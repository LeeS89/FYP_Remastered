using ProjectDawn.Navigation.Hybrid;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CircleDestination : DestinationBase
{
    private readonly CircleManager _cManager;
    private List<Transform> _waypoints;
    private int _index = 0;

    public CircleDestination(CircleManager cManager, AgentAuthoring agent) : base(agent) { _cManager = cManager; }

    public override void Init()
    {
        if (!_cManager.TryGetWaypointSet(out _waypoints))
            Debug.LogError("Failed to retrieve circle points");
    }

    public override bool TryGetPath(out Vector3 destination)
    {
        destination = Vector3.zero;

        if (_waypoints == null || _waypoints.Count < 2) return false;

        //bool hasClearPath = HasClearPathToTarget(_agent.transform.position, _waypoints[_index++].position, out destination);

        Vector3 resolvedPoint = GetNearestPointOnNavMesh(_waypoints[_index++].position, 2f);
        destination = resolvedPoint;

        if (_index > 1) _index = 0;

        return true;//hasClearPath;
    }

    public override Vector3 GetWaypointPositionOnNavMesh()
    {
        Vector3 pos = _waypoints[_index].position;
        _index++;
        return GetNearestPointOnNavMesh(pos, 2f);
    }
}
