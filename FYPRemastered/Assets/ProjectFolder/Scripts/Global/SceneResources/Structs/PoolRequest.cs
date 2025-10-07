using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public readonly struct PoolRequest<TPool> where TPool : IPoolManager
{
    public readonly PoolResourceType PoolType;
    public readonly Action<PoolResourceType, TPool> Callback;

    public PoolRequest(PoolResourceType type, Action<PoolResourceType, TPool> cb)
    {
        PoolType = type;
        Callback = cb;
    }
}



public readonly struct ResourceRequests
{
    // For AI and Pool requests
    public readonly AIResourceType AIResourceType;
  //  public readonly PoolResourceType PoolType;
   // public readonly string PoolId;
    public readonly PoolIdSO PoolId;

    // Path request params
    public readonly Vector3 PathStart;
    public readonly Vector3 PathEnd;
    public readonly NavMeshPath Path;

    // Flank Params
    public readonly int FlankCandidateSteps;
    public readonly List<FlankPointData> FlankCandidates;
    public readonly Action<bool> FlankCallback;

    public readonly Action<string, IPoolManager> PoolRequesterCallback;
   
    public readonly Action<LayerMask, LayerMask, LayerMask> FlankPointTargetAndBlockingMasksCallback;
    public readonly Action<BlockData> WaypointCallback;
    public readonly Action<bool> PathRequestCallback;

    private ResourceRequests(
        AIResourceType airt,
       // PoolResourceType prt,
        //string pid,
        PoolIdSO pid,
        Vector3 start,
        Vector3 end,
        NavMeshPath path,
        List<FlankPointData> flankCandidates,
        int flankCandidateSteps,
        Action<bool> flankCb,
        Action<string, IPoolManager> poolRequesterCallback,
        Action<LayerMask, LayerMask,LayerMask> fptabmCb,
        Action<BlockData> wpCb,
        Action<bool> prCb)
    {
        AIResourceType = airt;
       // PoolType = prt;
        PoolId = pid;
        PathStart = start;
        PathEnd = end;
        Path = path;
        FlankCandidates = flankCandidates;
        FlankCallback = flankCb;
        FlankCandidateSteps = flankCandidateSteps;
        PoolRequesterCallback = poolRequesterCallback;
        FlankPointTargetAndBlockingMasksCallback = fptabmCb;
        WaypointCallback = wpCb;
        PathRequestCallback = prCb;
    }

    public static ResourceRequests RequestPool(PoolIdSO type, Action<string, IPoolManager> pool)
        => new(AIResourceType.None, type, default, default, null, null, -1, null, pool, null, null, null);

    public static ResourceRequests FlankPointTargetAndBlockingMasks(Action<LayerMask, LayerMask, LayerMask> masks)
        => new(AIResourceType.FlankPointEvaluationMasks, null, default, default, null, null, -1, null, null,masks, null, null);

    public static ResourceRequests RequestWaypoints(Action<BlockData> callback)
        => new(AIResourceType.WaypointBlock, null, default, default, null, null, -1, null, null, null, callback, null);

    public static ResourceRequests RequestPath(Vector3 start, Vector3 end, NavMeshPath path, Action<bool> cb)
        => new(AIResourceType.Path, null, start, end, path, null, -1, null, null, null, null, cb);

    public static ResourceRequests RequestFlankPoints(int steps, List<FlankPointData> data, Action<bool> flankCB)
        => new(AIResourceType.FlankPointCandidates, null, default, default, null, data, steps, flankCB, null, null, null, null);
}
