using Services.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;


public sealed class WaypointSet 
{
    public readonly Vector3[] Points;

    private WaypointSet() { }
    public WaypointSet(Vector3[] points) => Points = points;

}

public class WaypointResources : IPatrolService, IAddressableService
{

   
    //private AsyncOperationHandle[] _handles = new AsyncOperationHandle[2];
    private AsyncOperationHandle<AgentPatrolData>? _patrolDataHandle;
    //private WaypointBlockData _waypointBlockData;
    private Dictionary<IInstanceIdentifiable, BlockData> _inUseBlockTracker = new(20);
    //private Dictionary<object, BlockData> _inUseBlockTracker = new(20);

    private AgentPatrolData _patrolData;
    //private BlockData[] waypointBlocks;

    private Dictionary<WaypointSet, BlockData> _waypointRegistry = new(25);

    [Obsolete]
    private readonly IFsmNavigationQuery _navQuery;

    private WaypointResources() { }

    [Obsolete]
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

        foreach(var block in wpBlockdata.blockDataArray)
        {
            if (block is null || block._waypointPositions is null || block._waypointPositions.Length is 0) continue;

            block._inUse = false;
            var points = (Vector3[])block._waypointPositions.Clone();
            _waypointRegistry.Add(new WaypointSet(points), block);
        }

        /*waypointBlocks = (BlockData[])*//*_waypointBlockData*//*wpBlockdata.blockDataArray.Clone();

        foreach (var block in waypointBlocks*//*_waypointBlockData.blockDataArray*//*)
            block._inUse = false;*/

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

    private bool TryGetWaypoints(IInstanceIdentifiable id, List<Vector3> buffer)
    {
        if (id == null || /*id.GetType().IsValueType ||*/ buffer == null) return false;

       /* if (*//*_waypointBlockData*//*waypointBlocks != null && waypointBlocks.Length > 0)
        {
            foreach (var blockData in waypointBlocks*//*_waypointBlockData.blockDataArray*//*)
            {
                if (!blockData._inUse)
                {
                    if (blockData._waypointPositions == null || blockData._waypointPositions.Length == 0) continue;
                    TryReleaseWaypoints(id, buffer);
                    blockData._inUse = true;
                    buffer.AddRange(blockData._waypointPositions);
                    _inUseBlockTracker[id] = blockData;
                    return true;
                }
            }
        }*/

       


        return false;
    }

    
    public bool TryGetWaypointSet(out WaypointSet set)
    {
        
        foreach(var (key, value) in _waypointRegistry)
        {
            if (value._inUse) continue;
            value._inUse = true;
            set = key;
            return true;
        }
        set = null;
        return false;
    }

    public void ReturnWaypointSet(WaypointSet set)
    {
        if(_waypointRegistry.TryGetValue(set, out var data))
            data._inUse = false;
    }

    // To be deleted
    private bool TryReleaseWaypoints(IInstanceIdentifiable requester, List<Vector3> buffer)
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



    public float GetIdleTimeSeconds()
    {
        if (_patrolData == null) return 1f;
        //Random.Range(minWait, maxWait);
        float min = _patrolData.MinTimeAtPatrolPoint;
        float max = _patrolData.MaxTimeAtPatrolPoint;
        return Random.Range(min, max);
    }

  
}



public class FsmResources : IAddressableService, IFsmControlService
{
    private AgentSpeedData _speedData;

   

    public async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        string addressKey = data.addressKey;
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }

        var spHandle = await AddressableLoader.TryLoadAssetAsync<AgentSpeedData>(addressKey);
        if (!spHandle.HasValue || !spHandle.Value.IsValid())
        {
            DebugLogs.Nre(spHandle, "Agent Speed Handle", this);
            return false;
        }

        _speedData = spHandle.Value.Result;
        if (_speedData == null)
        {
            DebugLogs.Nre(_speedData, "Agent Speed Data asset", this);
            Addressables.Release(spHandle.Value);
            return false;
        }

        DebugLogs.Err($"WalkSpeed: {_speedData.SprintSpeed}");
        return true;
        // await Task.CompletedTask;
    }



    public float GetWalkSpeed()
    {
        throw new System.NotImplementedException();
    }

    public float GetSprintSpeed()
    {
      
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public float GetSprintEnterDistance()
    {
        throw new NotImplementedException();
    }

    public float GetSprintExitDistance()
    {
        throw new NotImplementedException();
    }
}




























// Testing below











public abstract class FsmServiceBridge<TService> : IFsmDataProvider
{
    protected readonly TService _service;
    public FsmServiceBridge(TService service) => _service = service;
}


public sealed class FsmControlBridge : FsmServiceBridge<IFsmControlService>, IFsmControlDataProvider
{
    public FsmControlBridge(IFsmControlService service) : base(service) { }

    public float SprintEnterDistance => _service.GetSprintEnterDistance();
    public float SprintExitDistance => _service.GetSprintExitDistance();

    public float WalkSpeed => _service.GetWalkSpeed();

    public float SprintSpeed => _service.GetSprintSpeed();
}


public abstract class StateServiceBridge<TService> : FsmServiceBridge<TService>, IFsmDestinationProvider
{

    public StateServiceBridge(TService service) : base(service) { }

    public abstract void ReleaseCandidates(List<Vector3> buffer);

    public abstract bool TryGetDestinationCandidates(List<Vector3> buffer);
   
}

public sealed class PatrolServiceBridge : StateServiceBridge<IPatrolService>, IFsmPatrolDataProvider
{
    private WaypointSet _wpSet;

    public PatrolServiceBridge(IPatrolService service) : base(service) { }
    

    public override void ReleaseCandidates(List<Vector3> buffer)
    {
        if(_service is WaypointResources r)
        {
            r.ReturnWaypointSet(_wpSet);
            _wpSet = null;
        }
    }

    public override bool TryGetDestinationCandidates(List<Vector3> buffer)
    {
        if(_wpSet is null)
        {
            if (_service is WaypointResources r)
            {
                if (!r.TryGetWaypointSet(out _wpSet)) return false;
            }
        }
        buffer.Clear();

        foreach(var point in _wpSet.Points)
            buffer.Add(point);

        return true;
    }
}

public sealed class ChaseServiceBridge : StateServiceBridge<IChaseService>, IFsmChaseDataProvider
{
    private readonly ITargetProvider _targetProvider;
    private readonly IDistanceMonitoringService _distanceService;

    public ChaseServiceBridge(IChaseService service, IDistanceMonitoringService distService, ITargetProvider targetProvider) : base(service) 
    { (_distanceService, _targetProvider) = (distService, targetProvider); }

    public override void ReleaseCandidates(List<Vector3> buffer)
    {
        throw new NotImplementedException();
    }

    

    public override bool TryGetDestinationCandidates(List<Vector3> buffer)
    {
        if (buffer is null || _targetProvider is null) return false;
        buffer.Clear();
        if (!_targetProvider.TryGetTargetPosition(out var pos) || pos is null) return false;
        buffer.Add(pos.Value);
        return true;
    }

    // Needs targets ITargetable
    private bool TryGetTarget(out ITargetable target) => _targetProvider.TryGetTarget(out target);

    public bool TargetIsMoving()
    {
        if (!TryGetTarget(out var target)) return false;
        return target.IsMoving();
    }

    public bool TryRegisterDistanceMonitoring(IInstanceIdentifiable id, Vector3 currentPosition, Action<float> callback, out float initDist)
    {
        initDist = 0f; // Remember to get
        if (id is null || callback is null) return false;
        if (!_targetProvider.TryGetTarget(out var target)) return false;
        return _distanceService.TryRegisterSubscriber(id, currentPosition, target, callback);
    }

    public bool TryUnregisterDistanceMonitoring(IInstanceIdentifiable id)
    {
        if (id is null) return false;
        return _distanceService.TryUnregisterSubscriber(id);
    }
}