using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class ResourceRequestExtension
{
    public static void RequestPool<T>(this T _, PoolIdSO poolId, Action<string, IPoolManager> componentCallback, Action<bool> ownerCallback = null)
    {
        var pool = ResourceRequests.RequestPool(poolId, componentCallback);
        SceneEventAggregator.Instance.ResourceRequested(pool);
    }

    public static void RequestFlankPointEvaluationMasks<T>(this T _, Action<LayerMask, LayerMask, LayerMask> callback)
    {
        var masks = ResourceRequests.FlankPointTargetAndBlockingMasks(callback);
        SceneEventAggregator.Instance.ResourceRequested(masks);
    }
    public static void RequestValidPath<T>(this T _, Vector3 start, Vector3 end, NavMeshPath path, Action<bool> callback)
    {
        var pathReq = ResourceRequests.RequestPath(start, end, path, callback);
        SceneEventAggregator.Instance.ResourceRequested(pathReq);
    }

    public static void RequestFlankPointCandidates<T>(this T _, int steps, List<FlankPointData> buffer, Action<bool> callback)
    {
        var points = ResourceRequests.RequestFlankPoints(steps, buffer, callback);
        SceneEventAggregator.Instance.ResourceRequested(points);
    }

    public static void RequestWaypointBlock<T>(this T _, Action<BlockData> callback)
    {
        var waypoints = ResourceRequests.RequestWaypoints(callback);
        SceneEventAggregator.Instance.ResourceRequested(waypoints);
    }
}
