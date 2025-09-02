using Meta.XR.MRUtilityKit.SceneDecorator;
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
    public readonly PoolResourceType PoolType;

    // Path request params
    public readonly Vector3 PathStart;
    public readonly Vector3 PathEnd;
    public readonly NavMeshPath Path;

    // Flank Params
    public readonly int FlankCandidateSteps;
    public readonly List<FlankPointData> FlankCandidates;
    public readonly Action<bool> FlankCallback;

    public readonly Action<PoolResourceType, IPoolManager> PoolCallback;
    public readonly Action<LayerMask, LayerMask, LayerMask> FlankPointTargetAndBlockingMasksCallback;
    public readonly Action<BlockData> WaypointCallback;
    public readonly Action<bool> PathRequestCallback;

    private ResourceRequests(
        AIResourceType airt,
        PoolResourceType prt,
        Vector3 start,
        Vector3 end,
        NavMeshPath path,
        List<FlankPointData> flankCandidates,
        int flankCandidateSteps,
        Action<bool> flankCb,
        Action<PoolResourceType, IPoolManager> poolCb,
        Action<LayerMask, LayerMask,LayerMask> fptabmCb,
        Action<BlockData> wpCb,
        Action<bool> prCb)
    {
        AIResourceType = airt;
        PoolType = prt;
        PathStart = start;
        PathEnd = end;
        Path = path;
        FlankCandidates = flankCandidates;
        FlankCallback = flankCb;
        FlankCandidateSteps = flankCandidateSteps;
        PoolCallback = poolCb;
        FlankPointTargetAndBlockingMasksCallback = fptabmCb;
        WaypointCallback = wpCb;
        PathRequestCallback = prCb;
    }

    public static ResourceRequests RequestPool(PoolResourceType type, Action<PoolResourceType, IPoolManager> pool)
        => new(AIResourceType.None, type, default, default, null, null, -1, null, pool, null, null, null);

    public static ResourceRequests FlankPointTargetAndBlockingMasks(AIResourceType type, Action<LayerMask, LayerMask, LayerMask> masks)
        => new(type, PoolResourceType.None, default, default, null, null, -1, null, null, masks, null, null);

    public static ResourceRequests Waypoints(AIResourceType type, Action<BlockData> wp)
        => new(type, PoolResourceType.None, default, default, null, null, -1, null, null, null, wp, null);

    public static ResourceRequests RequestPath(AIResourceType type, Vector3 start, Vector3 end, NavMeshPath path, Action<bool> cb)
        => new(type, PoolResourceType.None, start, end, path, null, -1, null, null, null, null, cb);

    public static ResourceRequests RequestFlankPoints(AIResourceType type, int steps, List<FlankPointData> data, Action<bool> flankCB)
        => new(type, PoolResourceType.None, default, default, null, data, steps, flankCB, null, null, null, null);
}
