using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathFinder : IPathResolver
{
    private readonly IPathService _pathService;


    private WaitUntil _waitUntilPathCheckComplete;
    private bool _pathChecked;


    public uint Gen { get; private set; }
    private uint _activeGen;
    private Action<DestinationResult> PathValidationCallback;



    private Coroutine _runningRoutine;

    private DestinationResult _lastResult;



    public PathFinder(IPathService pathService)
    {
        _pathService = pathService;
        PathValidationCallback = OnPathValidationCallback;
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathChecked);

    }


    public void CancelAll()
    {
        Gen++;
        if (_runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }
        while (_requests.Count > 0)
        {
            var req = _requests.Dequeue();
            DestinationResultInfo cancelled = new DestinationResultInfo(ReasonForDestinationCheck.Cancelled, req.Path, DestinationResult.RequestCancelled, Vector3.zero, req.StateId);
        }
    }

    private Queue<DestinationRequest> _requests = new(15);

    public void ProcessDestinationCandidates(in DestinationRequest request)
    {
        if (request.Candidates == null || request.Candidates.Count == 0)
        {
            DestinationResultInfo failresult = new DestinationResultInfo(request.Reason, request.Path, DestinationResult.CandidatesNullOrEmpty, Vector3.zero, request.StateId);
            request.Callback?.Invoke(failresult);
            return;
        }
        _requests.Enqueue(request);
        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutineNewer(_requests));
    }
/*
    public void ProcessDestinationCandidates(StateId id, ReasonForDestinationCheck reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos, DestinationResultCallback callback)
    {
        if (candidates == null || candidates.Count == 0)
        {
            DestinationResult failResult = new DestinationResult(reason, path, PathValidationResult.CandidatesNullOrEmpty, Vector3.zero, id);
            callback?.Invoke(failResult);
            return;
        }

        _requests.Enqueue(new DestinationRequest(id, fromPos, candidates, path, reason, callback));
        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutineNewer(_requests));
    }*/

    private IEnumerator PathFindRoutineNewer(Queue<DestinationRequest> q)
    {

        while (q.Count > 0)
        {
            bool found = false;
            var request = q.Dequeue();

            _activeGen = Gen;

            foreach (var point in request.Candidates)
            {
                if (_activeGen != Gen) break;

                _pathChecked = false;
                _lastResult = DestinationResult.None;
                //_isValid = false;

                Vector3 from = LineOfSightUtility.GetClosestPointOnNavMesh(request.From);
                Vector3 to = LineOfSightUtility.GetClosestPointOnNavMesh(point);
                _pathService?.RequestPath(from, to, request.Path, PathValidationCallback);

                yield return _waitUntilPathCheckComplete;

                if (_activeGen != Gen) break;
                if (_lastResult == DestinationResult.NullPathParameter)
                {
                    DestinationResultInfo nullPath = new DestinationResultInfo(request.Reason, request.Path, DestinationResult.NullPathParameter, Vector3.zero, request.StateId);
                    request.Callback?.Invoke(nullPath);
                    break;
                }

                if (_lastResult == DestinationResult.Failed) continue;

                // if (!_isValid) continue;
                // Debug.LogError("Sending successful Callback");
                DestinationResultInfo success = new DestinationResultInfo(request.Reason, request.Path, DestinationResult.Success, to, request.StateId);
                request.Callback?.Invoke(success);

                found = true;
                break;
            }

            if (_activeGen != Gen) break;
            if (!found)
            {
                DestinationResultInfo failed = new DestinationResultInfo(request.Reason, request.Path, DestinationResult.Failed, Vector3.zero, request.StateId);
                request.Callback?.Invoke(failed);
            }

        }

        _runningRoutine = null;

    }



    private void OnPathValidationCallback(DestinationResult result)//bool pathFound/*in PathResult result*/)
    {
        // _isValid = pathFound;
        _lastResult = result;
        _pathChecked = true;
    }

    
}

