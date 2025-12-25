using UnityEngine;
using UnityEngine.AI;

public static class NavMeshZoneMapper
{
    private static readonly int _zoneAAreaMask;
 
    //private static readonly int _zoneBArea;

    static NavMeshZoneMapper()
    {
        int idx = NavMesh.GetAreaFromName("ZoneA");
        if(idx < 0)
        {
#if UNITY_EDITOR
            Debug.LogError("NavMeshZoneMapper: 'ZoneA' area not found in NavMesh areas. Please define it in the Navigation settings.");
#endif
            _zoneAAreaMask = 0;
        }
        else _zoneAAreaMask = 1 << idx;

    }

    public static bool GetZoneId(this NPCControllerObsolete self, Vector3 pos, out ZoneId zone)
    {
       // var pos = self.transform.position;
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            zone = FromAreaIndex(hit);
            return true;
        }
        zone = ZoneId.Unknown;
        return false;
    }

    private static ZoneId FromAreaIndex(NavMeshHit hit)
    {
        int areaIndex = hit.mask;
        if (areaIndex == _zoneAAreaMask) return ZoneId.ZoneA;
        return ZoneId.Unknown;
    }




    public static bool GetZoneId(this NPCControllerNew self, Vector3 pos, out ZoneId zone)
    {
        // var pos = self.transform.position;
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            zone = FromAreaIndex(hit);
            return true;
        }
        zone = ZoneId.Unknown;
        return false;
    }
}

public enum ZoneId
{
    Unknown,
    ZoneA,
    ZoneB
}
