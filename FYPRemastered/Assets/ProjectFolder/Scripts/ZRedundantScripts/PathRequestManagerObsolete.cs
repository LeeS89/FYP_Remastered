using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class used to to limit concurrent path calculations to a set number per frame
/// </summary>
[Obsolete("", true)]
public class PathRequestManagerObsolete : SceneResourcesObsolete, IUpdateableResource // Change to ITickable
{
 
    private Queue<ResourceRequestsObsolete> _pathRequestQueue = new Queue<ResourceRequestsObsolete>();
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

    protected override void ResourceRequested(in ResourceRequestsObsolete request)
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











