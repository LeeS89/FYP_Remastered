using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class used to to limit concurrent path calculations to a set number per frame
/// </summary>
public class PathRequestManager : SceneResources, IUpdateableResource
{
    private Queue<AIDestinationRequestData> _pathRequests = new Queue<AIDestinationRequestData>();
    private int _maxConcurrentRequests = 5;

    public override async Task LoadResources()
    {
        SceneEventAggregator.Instance.OnPathRequested += AddRequest;

        await Task.CompletedTask;
        /* SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
         SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;*/
        // return base.LoadResources();
    }

    public void ExecutePathRequests()
    {
        int processed = 0;

        

        while (_pathRequests.Count > 0 && processed < _maxConcurrentRequests)
        {
            var request = _pathRequests.Dequeue();

            bool success = HasClearPathToTarget(request.start, request.end, request.path);
            //Debug.LogError($"Path request from {request.start} to {request.end} success: {success}, please");
            //request.externalCallback?.Invoke(success);
            request.internalCallback?.Invoke(success);


            processed++;
        }
    }

    private bool HasClearPathToTarget(Vector3 from, Vector3 to, NavMeshPath path)
    {
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void AddRequest(AIDestinationRequestData request)
    {
        _pathRequests.Enqueue(request);
    }

    public int GetPendingRequestCount()
    {
        return _pathRequests.Count;
    }

    public void UpdateResource()
    {
        if (_pathRequests.Count == 0) { return; }

        ExecutePathRequests();
    }
}
