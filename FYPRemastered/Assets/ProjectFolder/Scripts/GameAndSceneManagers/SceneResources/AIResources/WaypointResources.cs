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

    private readonly IFsmNavigationQuery _navQuery;

    private WaypointResources() { }

    public WaypointResources(IFsmNavigationQuery navQuery) => _navQuery = navQuery;
   

    public bool TryGetDestinationCandidates(ITargetContext id, List<Vector3> buffer)
    {
        return false;
    }


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

    public bool TryGetCurrentPosition(IInstanceIdentifiable id, out Vector3 pos)
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

    [Obsolete]
    public bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer)
        => TryGetWaypoints(id, buffer);

   /* bool TryGetDestinationCandidates(ITargetContext context, List<Vector3> buffer)
        => TryGetWaypoints(null, buffer);
*/
    public void ReleaseDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer)
        => TryReleaseWaypoints(id, buffer);

    public bool TryGetCurrentPositionAndPath(IInstanceIdentifiable id, out Vector3 currentPos, out NavMeshPath path)
    {
        currentPos = default;
        path = null;
        if (id == null) return false;
        return _navQuery.TryGetOwnerPositionAndPath(id, out currentPos, out path);
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

    private readonly IFsmNavigationControl _navControl;
    private AgentSpeedData _speedData;

    private FsmResources() { }

    public FsmResources(IFsmNavigationControl navControl) => _navControl = navControl;
    

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




    public bool TryGetOwnerTransform(IInstanceIdentifiable id, out Transform t)
        => _navControl.TryGetOwnerTransform(id, out t);

    public bool TryGetAgent(IInstanceIdentifiable id, out NavMeshAgent agent)
        => _navControl.TryGetAgent(id, out agent);

    public bool TryGetObstacle(IInstanceIdentifiable id, out NavMeshObstacle obstacle)
        => _navControl.TryGetObstacle(id, out obstacle);

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
}




























// Testing below



public abstract class CandidateDestinationProvider/*<TContext>*/ : IFsmStateServiceNew// where TContext : IContext
{
    //  public abstract bool TryGetDestinationCandidates(T id, List<Vector3> buffer);

    //  public abstract bool TryGetDestinationCandidates<IContext>(IContext context, List<Vector3> buffer);
    public abstract bool TryGetDestinationCandidates<TContext>(TContext context, List<Vector3> buffer) where TContext : IContext;
    /*{
        throw new NotImplementedException();
    }*/
}


public class OtherTest : CandidateDestinationProvider//<ITargetContext>
{
    /*   public override bool TryGetDestinationCandidates<ITargetContext>(ITargetContext id, List<Vector3> buffer)
       {
           throw new NotImplementedException();
       }*/
    /*public override bool TryGetDestinationCandidates(ITargetContext id, List<Vector3> buffer)
    {
        throw new NotImplementedException();
    }*/
    public override bool TryGetDestinationCandidates<ITargetContext>(ITargetContext context, List<Vector3> buffer)
    {
     
        return true;
        //throw new NotImplementedException();
    }
}

public interface INewTest
{
    void TryGet<T>(T id);
}

public interface IFsmStateServiceNew//<TContext>  where TContext : IContext
{
    bool TryGetDestinationCandidates<TContext>(TContext context, List<Vector3> buffer) where TContext : IContext;
    /*   [Obsolete("Use INpcBody instead")]
       bool TryGetCurrentPosition(IInstanceIdentifiable id, out Vector3 currentPosition);

       [Obsolete("Use INpcBody instead")]
       bool TryGetPath(IInstanceIdentifiable id, out NavMeshPath path);
       [Obsolete("Use INpcBody instead")]
       bool TryGetCurrentPositionAndPath(IInstanceIdentifiable id, out Vector3 currentPos, out NavMeshPath path);


     //  bool TryGetDestinationCandidates(ITargetContext context, List<Vector3> buffer);
       [Obsolete("Use INpcBody instead")]
       bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer);
       void ReleaseDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer);*/

}

public class otherClass
{
    public IFsmStateServiceNew _new;
    public IContext _iContext;
    public ITargetContext _tContext;

    public void returnNew()
    {
        _new.TryGetDestinationCandidates(_tContext, new List<Vector3>());
    }
}








public interface IFsmDestinationProvider
{
    bool TryGetDestinationCandidates(List<Vector3> buffer);
    void ReleaseCandidates(List<Vector3> buffer);
}



public interface IFsmDataProvider { }

public interface IFsmPatrolDataProvider : IFsmDataProvider { }
public interface IFsmChaseDataProvider : IFsmDataProvider
{
    bool TryRegisterDistanceMonitoring(IInstanceIdentifiable id, Vector3 currentPosition, /*ITargetable targetToCompare,*/ Action<float> callback, out float initDist);
    bool TryUnregisterDistanceMonitoring(IInstanceIdentifiable id);
    bool TargetIsMoving();
}









public abstract class StateServiceBridge<TService> : IFsmDestinationProvider, IFsmDataProvider
{
    protected readonly TService _service;

    public StateServiceBridge(TService service) => _service = service;

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