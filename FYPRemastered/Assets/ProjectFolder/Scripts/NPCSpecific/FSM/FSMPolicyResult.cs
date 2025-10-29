using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public readonly struct FSMPolicyResult
{
   // public readonly bool PathBlocked;
    //public readonly bool PathToPrimaryBlocked;
    public readonly bool DestinationReached;
    public readonly PolicyHaltReason Reason;
    public readonly FSMPolicy CurrentPolicy;
    public readonly uint Version;

    public FSMPolicyResult(in FSMPolicy currentPolicy, PolicyHaltReason reason, /*bool pathBlocked, bool pathToPrimaryBlocked, */bool destinationReached)
    {
        CurrentPolicy = currentPolicy;
        Reason = reason;
       // PathBlocked = pathBlocked;
       // PathToPrimaryBlocked = pathToPrimaryBlocked;
        DestinationReached = destinationReached;
        this.Version = currentPolicy.Version;
    }
}

public readonly struct FSMPolicyValidation
{
    public readonly PolicyIntentResult PathResult;
    public readonly FSMPolicy CurrentPolicy;
    public readonly Vector3 Destination;
    public readonly uint Version;
    public readonly bool DestinationReached;

    public FSMPolicyValidation(in FSMPolicy currentPolicy, PolicyIntentResult pathResult, Vector3 destination, bool destinationReached)
    {
        CurrentPolicy = currentPolicy;
        PathResult = pathResult;
        Destination = destination;
        DestinationReached = destinationReached;
        Version = currentPolicy.Version;
    }
}


public readonly struct PathRequest
{
   // public readonly AIResourceType AIResourceType;
   // public readonly PathTarget Target;


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

    private PathRequest(
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
        Action<LayerMask, LayerMask, LayerMask> fptabmCb,
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

    public static PathRequest RequestPool(PoolIdSO type, Action<string, IPoolManager> pool)
        => new(AIResourceType.None, type, default, default, null, null, -1, null, pool, null, null, null);

    public static PathRequest FlankPointTargetAndBlockingMasks(Action<LayerMask, LayerMask, LayerMask> masks)
        => new(AIResourceType.FlankPointEvaluationMasks, null, default, default, null, null, -1, null, null, masks, null, null);

    public static PathRequest RequestWaypoints(Action<BlockData> callback)
        => new(AIResourceType.WaypointBlock, null, default, default, null, null, -1, null, null, null, callback, null);

    public static PathRequest RequestPath(Vector3 start, Vector3 end, NavMeshPath path, Action<bool> cb)
        => new(AIResourceType.Path, null, start, end, path, null, -1, null, null, null, null, cb);

    public static PathRequest RequestFlankPoints(int steps, List<FlankPointData> data, Action<bool> flankCB)
        => new(AIResourceType.FlankPointCandidates, null, default, default, null, data, steps, flankCB, null, null, null, null);
}


public readonly struct StateInfo
{

}
