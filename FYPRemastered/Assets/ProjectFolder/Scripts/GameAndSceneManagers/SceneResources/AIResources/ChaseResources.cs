using Services.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ChaseResources : IChaseService, IAddressableService
{
    private AsyncOperationHandle<AgentChaseData>? _chaseDataHandle;
    private AgentChaseData _data;
    private readonly IFsmTargetQuery _targetQuery;
    private readonly IFsmNavigationQuery _navQuery;

    private ChaseResources() { }

    public ChaseResources(IFsmNavigationQuery navQuery, IFsmTargetQuery targetRegistry)
    {
        _navQuery = navQuery;
        _targetQuery = targetRegistry;
    }

    public async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        string addressKey = data.addressKey;
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }

        _chaseDataHandle = await AddressableLoader.TryLoadAssetAsync<AgentChaseData>(addressKey);
        
        if(!_chaseDataHandle.HasValue || !_chaseDataHandle.Value.IsValid())
        {
            DebugLogs.Nre(_chaseDataHandle, "Chase data handle", this);
            return false;
        }

        _data = _chaseDataHandle.Value.Result;

        if(_data == null)
        {
            DebugLogs.Nre(_data, "Agent chase data", this);
            Addressables.Release(_chaseDataHandle.Value);
            return false;
        }

        return true;
    }

    public bool TryGetOwnerPosition(IInstanceIdentifiable id, out Vector3 pos)
    {
        pos = default;
        if (id == null) { DebugLogs.RequireNotNull(id, "InstancIdentifiable"); return false; }

        return _navQuery.TryGetOwnerPosition(id, out pos);
    }

    public bool TryGetChaseCandidates(IInstanceIdentifiable id, List<Vector3> buffer)
    {
        if (id == null || buffer == null) { DebugLogs.RequireNotNull(id,"Id or buffer", this); return false; }

        if (!_targetQuery.TryGetTargetPosition(id, out Vector3 pos)) return false;
        
        buffer.Clear();

        buffer.Add(pos);

        return true;
    }

    public bool TryGetPath(IInstanceIdentifiable id, out NavMeshPath path)
    {
        path = null;
        if (id == null) { DebugLogs.RequireNotNull(id, "InstancIdentifiable"); return false; }
        return _navQuery.TryGetPath(id, out path);
    }

    public bool TargetIsMoving(IInstanceIdentifiable id)
    {
        if (id == null) { DebugLogs.RequireNotNull(id, "InstancIdentifiable"); return false; }
        return _targetQuery.TargetIsMoving(id);
    }

    public bool TryGetSqrDistanceToTarget(IInstanceIdentifiable id, Vector3 from, out float sqrDistance)
    {
        sqrDistance = float.MinValue;
        if (id == null) { DebugLogs.RequireNotNull(id, "InstancIdentifiable"); return false; }

        if (!_targetQuery.TryGetTargetPosition(id, out var pos)) return false;

        sqrDistance = pos.SqrDistanceTo(from);
        return true;
    }


    public void Dispose()
    {
        if(_chaseDataHandle.HasValue && _chaseDataHandle.Value.IsValid())
            Addressables.Release(_chaseDataHandle.Value);
    }

   

    
}
