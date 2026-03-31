using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Obsolete("", true)]
public readonly struct PoolRequestObsolete<TPool> where TPool : IPoolManager
{
    public readonly PoolResourceTypeObsolete PoolType;
    public readonly Action<PoolResourceTypeObsolete, TPool> Callback;

    public PoolRequestObsolete(PoolResourceTypeObsolete type, Action<PoolResourceTypeObsolete, TPool> cb)
    {
        PoolType = type;
        Callback = cb;
    }
}


[Obsolete("", true)]
public readonly struct ResourceRequestsObsolete
{
    // For AI and Pool requests
    public readonly AIResourceTypeObsolete AIResourceType;
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

    private ResourceRequestsObsolete(
        AIResourceTypeObsolete airt,
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

    public static ResourceRequestsObsolete RequestPool(PoolIdSO type, Action<string, IPoolManager> pool)
        => new(AIResourceTypeObsolete.None, type, default, default, null, null, -1, null, pool, null, null, null);

    public static ResourceRequestsObsolete FlankPointTargetAndBlockingMasks(Action<LayerMask, LayerMask, LayerMask> masks)
        => new(AIResourceTypeObsolete.FlankPointEvaluationMasks, null, default, default, null, null, -1, null, null,masks, null, null);

    public static ResourceRequestsObsolete RequestWaypoints(Action<BlockData> callback)
        => new(AIResourceTypeObsolete.WaypointBlock, null, default, default, null, null, -1, null, null, null, callback, null);

    public static ResourceRequestsObsolete RequestPath(Vector3 start, Vector3 end, NavMeshPath path, Action<bool> cb)
        => new(AIResourceTypeObsolete.Path, null, start, end, path, null, -1, null, null, null, null, cb);

    public static ResourceRequestsObsolete RequestFlankPoints(int steps, List<FlankPointData> data, Action<bool> flankCB)
        => new(AIResourceTypeObsolete.FlankPointCandidates, null, default, default, null, data, steps, flankCB, null, null, null, null);
}

