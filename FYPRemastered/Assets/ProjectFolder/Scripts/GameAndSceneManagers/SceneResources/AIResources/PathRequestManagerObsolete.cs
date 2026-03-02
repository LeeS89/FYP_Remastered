using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class used to to limit concurrent path calculations to a set number per frame
/// </summary>
[Obsolete]
public class PathRequestManagerObsolete : SceneResources, IUpdateableResource // Change to ITickable
{
 
    private Queue<ResourceRequests> _pathRequestQueue = new Queue<ResourceRequests>();
    private int _maxConcurrentRequests = 5;

    public override async Task LoadResources()
    {

        SceneEventAggregatorObsolete.Instance.OnResourceRequested += ResourceRequested;

        await Task.CompletedTask;
       
    }

    public void ExecutePathRequests()
    {
        int processed = 0;

        while (_pathRequestQueue.Count > 0 && processed < _maxConcurrentRequests)
        {
            var request = _pathRequestQueue.Dequeue();
          
            bool success = HasClearPathToTarget(request.PathStart, request.PathEnd, request.Path);

           
            //Debug.LogError($"Path request from {request.start} to {request.end} success: {success}, please");
            //request.externalCallback?.Invoke(success);
            request.PathRequestCallback?.Invoke(success);

            processed++;
        }
    }

    private bool HasClearPathToTarget(Vector3 from, Vector3 to, NavMeshPath path)
    {
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }

    protected override void ResourceRequested(in ResourceRequests request)
    {
        if (request.AIResourceType != AIResourceType.Path) return;
        _pathRequestQueue.Enqueue(request);
    }

  
    public int GetPendingRequestCount() => _pathRequestQueue.Count;
    

    public void UpdateResource()
    {
        if (_pathRequestQueue.Count == 0) { return; }

        ExecutePathRequests();
    }
}








/// <summary>
/// Class used to to limit concurrent path calculations to a set number per frame
/// </summary>
public class PathRequestManagerNew : SceneResources, IPathService, ITickable // Change to ITickable
{
 
    private Queue<PathRequest> _pathRequestQueue = new(25);
    private int _maxConcurrentRequests = 5;

    public void ExecutePathRequests()
    {
        int processed = 0;

        while (_pathRequestQueue.Count > 0 && processed < _maxConcurrentRequests)
        {
            var request = _pathRequestQueue.Dequeue();
          
            //bool success = HasClearPathToTarget(request.From, request.To, request.Path);
            DestinationResult result = HasClearPathToTarget(request.From, request.To, request.Path) ? DestinationResult.Success : DestinationResult.Failed;

            //Debug.LogError($"Path request from {request.start} to {request.end} success: {success}, please");
            //request.externalCallback?.Invoke(success);
            request.OnRequestComplete?.Invoke(result);

            processed++;
        }
    }

    private bool HasClearPathToTarget(Vector3 from, Vector3 to, NavMeshPath path)
    {
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }

  

    public void RequestPath(Vector3 from, Vector3 to, NavMeshPath path, Action<DestinationResult> onRequestComplete)
    {
        if (path == null)
        {
            onRequestComplete?.Invoke(DestinationResult.NullPathParameter);
            return;
        }
        PathRequest req = new PathRequest
        (
            from,
            to,
            path,
            onRequestComplete
        );
        _pathRequestQueue.Enqueue(req);
    }


    public int GetPendingRequestCount() => _pathRequestQueue.Count;
    


    public void Tick(float dt)
    {
        if (_pathRequestQueue.Count == 0) { return; }
       // Debug.LogError("Ticking Path Requests");
        ExecutePathRequests();
    }

    public void LateTick(float dt) { }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private readonly struct PathRequest
    {
        public readonly Vector3 From;
        public readonly Vector3 To;
        public readonly NavMeshPath Path;
        public readonly Action<DestinationResult> OnRequestComplete;

        public PathRequest(Vector3 from, Vector3 to, NavMeshPath path, Action<DestinationResult> onRequestComplete)
            => (From, To, Path, OnRequestComplete) = (from, to, path, onRequestComplete);
           
    }
}


