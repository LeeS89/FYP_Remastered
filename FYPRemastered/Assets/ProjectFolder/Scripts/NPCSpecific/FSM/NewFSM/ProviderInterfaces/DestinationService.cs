using System.Collections.Generic;
using UnityEngine;

public sealed class DestinationService
{
    private readonly Dictionary<DestinationKind, ICandidateProvider> _map;

    public DestinationService()
    {
        _map = new()
        {
            [DestinationKind.Patrol] = new WaypointProvider(WaypointRepo.Instance)
        };
    }

   /* public List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination request)
    {
        if(_map.TryGetValue(request.Kind, out var p)) return p.TryGet(request);

        return null;
    }*/

    /*public bool GetCurrentZone(out int zone)
    {
        if(_map.TryGetValue(DestinationKind.Patrol, out var p))
        {
            if (p is WaypointProvider pr) 
            {
            //    zone = pr.CurrentWaypointZone;
                return true; 
            }
        }

        zone = -1;
        return false;
    }*/
}
