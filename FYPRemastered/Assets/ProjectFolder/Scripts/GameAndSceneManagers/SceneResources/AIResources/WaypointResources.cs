using Services.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;


public class WaypointResources : IWaypointService, IAddressableService
{

    //private AsyncOperationHandle[] _handles = new AsyncOperationHandle[2];
    private AsyncOperationHandle<AgentPatrolData>? _patrolDataHandle;
    //private WaypointBlockData _waypointBlockData;
    private Dictionary<object, BlockData> _inUseBlockTracker = new(20);

    private AgentPatrolData _patrolData;
    private BlockData[] waypointBlocks;

    private readonly IFsmNavigationQuery _navQuery;

    private WaypointResources() { }

    public WaypointResources(IFsmNavigationQuery navQuery) => _navQuery = navQuery;
   

    public async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        string addressKey = data.addressKey;
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }
        
        // Load the asset from Addressables
       var wpHandle = await AddressableLoader.TryLoadAssetAsync<WaypointBlockData>(addressKey);

        if (!wpHandle.HasValue || !wpHandle.Value.IsValid())
        {
            DebugLogs.Nre(wpHandle, "_wpHandle", this);
          //  Dispose();
            return false;
        }

        var wpBlockdata /*_waypointBlockData*/ = wpHandle.Value.Result;

        if (/*_waypointBlockData*/wpBlockdata == null)
        {
            DebugLogs.Nre(/*_waypointBlockData*/wpBlockdata, "_waypointBlockData", this);
            Addressables.Release(wpHandle.Value);
            //  Dispose();
            return false;
        }

        //var blocks = _waypointBlockData.blockDataArray;

        if (/*_waypointBlockData*/wpBlockdata.blockDataArray == null || /*_waypointBlockData*/wpBlockdata.blockDataArray.Length == 0)
        {
            DebugLogs.Err("Waypoint block data array is null or contains no elements", this);
            Addressables.Release(wpHandle.Value);
            //Dispose();
            return false;
        }

        waypointBlocks = (BlockData[])/*_waypointBlockData*/wpBlockdata.blockDataArray.Clone();

        foreach (var block in waypointBlocks/*_waypointBlockData.blockDataArray*/)
            block._inUse = false;

        DebugLogs.Log("Successfully initialized waypoint blocks", this);
        Addressables.Release(wpHandle.Value);
        //_handles[0] = wpHandle.Value;


        return await TryLoadSubData(data.subDataKeys);
       
    }


    private async Task<bool> TryLoadSubData(List<string> addressKeys)
    {
        if (addressKeys == null) { DebugLogs.Nre(addressKeys, "addressKeys", this); return false; }
        if (addressKeys.Count == 0) { DebugLogs.Err("addressKeys contains no elements", this); return false; }

        var key = addressKeys[0];

        /*var so*/
        _patrolDataHandle = await AddressableLoader.TryLoadAssetAsync<AgentPatrolData>(key);
        if (!_patrolDataHandle.HasValue || !_patrolDataHandle.Value.IsValid()) { DebugLogs.Nre(_patrolDataHandle, $"{_patrolDataHandle.GetType().Name}", this); return false; }

        _patrolData = _patrolDataHandle.Value.Result;

        if (_patrolData == null)
        {
            Addressables.Release(key);
            DebugLogs.Err("_patrol data SO was loaded but was null", this);
            return false;
        }

       // _handles[0] = so.Value;
        DebugLogs.Log("LOADED PATROL DATA SUCCESS", this);
        return true;
    }

    public void Dispose()
    {
        /*for (int i = 0; i < _handles.Length; i++)
            if (_handles[i].IsValid()) Addressables.Release(_handles[i]);*/
        if (_patrolDataHandle.HasValue && _patrolDataHandle.Value.IsValid())
            Addressables.Release(_patrolDataHandle.Value);
    }

    public bool TryGetWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType || buffer == null) return false;

        if (/*_waypointBlockData*/waypointBlocks != null && waypointBlocks.Length > 0)
        {
            foreach (var blockData in waypointBlocks/*_waypointBlockData.blockDataArray*/)
            {
                if (!blockData._inUse)
                {
                    TryReleaseWaypoints(requester, buffer);
                    blockData._inUse = true;
                    buffer.AddRange(blockData._waypointPositions);
                    _inUseBlockTracker[requester] = blockData;
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryReleaseWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType) return false;

        if (_inUseBlockTracker.Remove(requester, out var block))
        {
            block._inUse = false;
            if (buffer != null) buffer.Clear();
            return true;
        }

        return false;
    }

    public bool TryGetOwnerPosition(IInstanceIdentifiable id, out Vector3 pos)
    {
        pos = default;
        if (id == null) { DebugLogs.RequireNotNull(id, "InstancIdentifiable"); return false; }

        return _navQuery.TryGetOwnerPosition(id, out pos);
    }

    public bool TryGetPath(IInstanceIdentifiable id, out NavMeshPath path)
    {
        path = null;
        if (id == null) { DebugLogs.RequireNotNull(id, "InstancIdentifiable"); return false; }
        return _navQuery.TryGetPath(id, out path);
    }

    public bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer)
    {
        throw new System.NotImplementedException();
    }

    public void ReleaseDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer)
    {
        throw new System.NotImplementedException();
    }
}
