using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class PathDistanceBatcher
{
    private static readonly List<PathCheckRequest> _queue = new();
    private static bool _isRunning;
    private static float _nextCheckTime;
    private const float _checkInterval = 0.1f; // Time interval between checks in seconds

    public static void RequestCheck(PathCheckRequest request)
    {
        _queue.Add(request);

        if (!_isRunning)
        {
            _isRunning = true;
            CoroutineRunner.Instance.StartCoroutine(ProcessQueue());
        }
    }

    private static IEnumerator ProcessQueue()
    {
        while (_queue.Count > 0)
        {
            if(Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + _checkInterval;

                int checkCount = Mathf.Min(2, _queue.Count); // Process up to 2 requests at a time
                for(int i = 0; i < checkCount; i++)
                {
                    var request = _queue[0];
                    _queue.RemoveAt(0);

                    float distance = LineOfSightUtility.GetNavMeshPathDistance(request.From, request.To, request.Path);
                    request.OnComplete?.Invoke(distance);
                }
            }
            yield return null; // Wait for the next frame
        }
        _isRunning = false;
    }
}
