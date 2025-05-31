using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class used to to limit concurrent path calculations to a set number per frame
/// </summary>
public class PathRequestManager
{
    private Queue<DestinationRequestData> _pathRequests = new Queue<DestinationRequestData>();
    private int _maxConcurrentRequests = 5;

    public void ExecutePathRequests()
    {
        int processed = 0;

        while (_pathRequests.Count > 0 && processed < _maxConcurrentRequests)
        {
            var request = _pathRequests.Dequeue();

            bool success = HasClearPathToTarget(request.start, request.end, request.path);

            request.externalCallback?.Invoke(success);


            processed++;
        }
    }

    private bool HasClearPathToTarget(Vector3 from, Vector3 to, NavMeshPath path)
    {
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }

    public void AddRequest(DestinationRequestData request)
    {
        _pathRequests.Enqueue(request);
    }

    public int GetPendingRequestCount()
    {
        return _pathRequests.Count;
    }
}
