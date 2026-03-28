using System.Collections.Generic;
using UnityEngine;

public sealed class PatrolServiceBridge : StateServiceBridge<IPatrolService>, IFsmPatrolData
{
    private WaypointSet _wpSet;

    public PatrolServiceBridge(IPatrolService service) : base(service) { }


    public override void ReleaseCandidates(List<Vector3> buffer)
    {
        if (_service is WaypointResources r)
        {
            r.ReturnWaypointSet(_wpSet);
            _wpSet = null;
        }
    }

    public override bool TryGetDestinationCandidates(List<Vector3> buffer)
    {
        if (_wpSet is null)
        {
            if (_service is WaypointResources r)
            {
                if (!r.TryGetWaypointSet(out _wpSet)) return false;
            }
        }
        buffer.Clear();

        foreach (var point in _wpSet.Points)
            buffer.Add(point);

        return true;
    }
}
