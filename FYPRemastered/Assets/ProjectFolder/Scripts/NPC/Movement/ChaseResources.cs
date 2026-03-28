using Services.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class ChaseResources : IChaseService, IAddressableService
{
    private AsyncOperationHandle<AgentChaseData>? _chaseDataHandle;
    private AgentChaseData _data;

    [Obsolete]
    private readonly IFsmTargetQuery _targetQuery;
    [Obsolete]
    private readonly IFsmNavigationQuery _navQuery;
    private readonly IDistanceMonitoringService _distService;

    private ChaseResources() { }

    [Obsolete]
    public ChaseResources(IFsmNavigationQuery navQuery, IFsmTargetQuery targetRegistry, IDistanceMonitoringService distanceService)
    {
        _navQuery = navQuery;
        _targetQuery = targetRegistry;
        _distService = distanceService;
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

    public bool TryGetCurrentPosition(IInstanceIdentifiable id, out Vector3 pos)
    {
        pos = default;
        if (id == null) { DebugLogs.RequireNotNull(id, "InstancIdentifiable", this); return false; }

        return _navQuery.TryGetOwnerPosition(id, out pos);
    }

    public bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer)
    {
        if (id == null || buffer == null) { DebugLogs.RequireNotNull(id,"Id or buffer", this); return false; }

        buffer.Clear();
        if (!_targetQuery.TryGetTargetPosition(id, out Vector3 pos)) return false;
        
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

    public bool TryRegisterDistanceToTargetMonitoring(IInstanceIdentifiable id, Action<float> onDistanceUpdate, out float initialDistance)
    {
        initialDistance = float.MinValue;
        if (id == null || onDistanceUpdate == null) return false;
        
        if(!_navQuery.TryGetOwnerPosition(id, out var pos)) return false;
        if (!_targetQuery.TryGetTarget(id, out var target)) return false;
        if (target == null || target.Position() == null) return false;

        initialDistance = pos.SqrDistanceTo(target.Position().Value);

        return _distService.TryRegisterSubscriber(id, pos, target, onDistanceUpdate);
    }

    public bool TryUnregisterDistanceToTargetMonitoring(IInstanceIdentifiable id)
    {
        if (id == null) return false;
        return _distService.TryUnregisterSubscriber(id);
    }

    public void ReleaseDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer)
    {
        throw new NotImplementedException();
    }

    public bool TryGetCurrentPositionAndPath(IInstanceIdentifiable id, out Vector3 currentPos, out NavMeshPath path)
    {
        currentPos = default;
        path = null;
        if (id == null) return false;
        return _navQuery.TryGetOwnerPositionAndPath(id, out currentPos, out path);
    }

 
}
