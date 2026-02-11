using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathFinder : IPathResolver
{
    private readonly IPathService _pathService;


    private WaitUntil _waitUntilPathCheckComplete;
    private bool _pathChecked;


    public uint Gen { get; private set; }
    private uint _activeGen;
    private Action<PathResult> PathCheckCallback;



    private Coroutine _runningRoutine;

    private PathResult _lastResult;



    public PathFinder(IPathService pathService)
    {
        _pathService = pathService;
        PathCheckCallback = OnPathRequestcallback;
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
            DestinationResultNew cancelled = new DestinationResultNew(ReasonForDestinationCheck.Cancelled, req.Path, PathResult.RequestCancelled, Vector3.zero, req.StateId);
        }
    }

    private readonly struct DestinationRequest
    {
        public readonly StateId StateId;
        public readonly Vector3 From;
        public readonly List<Vector3> Candidates;
        public readonly NavMeshPath Path;
        public readonly ReasonForDestinationCheck Reason;
        public readonly DestinationValidationCallbackNew Callback;

        public DestinationRequest(StateId id, Vector3 from, List<Vector3> candidates, NavMeshPath path, ReasonForDestinationCheck reason, DestinationValidationCallbackNew cb)
            => (StateId, From, Candidates, Path, Reason, Callback) = (id, from, candidates, path, reason, cb);

    }



    private Queue<DestinationRequest> _requests = new(15);

    public void ProcessDestinationCandidates(StateId id, ReasonForDestinationCheck reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos, DestinationValidationCallbackNew callback)
    {
        if (candidates == null || candidates.Count == 0)
        {
            DestinationResultNew failResult = new DestinationResultNew(reason, path, PathResult.CandidatesNullOrEmpty, Vector3.zero, id);
            callback?.Invoke(failResult);
            return;
        }

        _requests.Enqueue(new DestinationRequest(id, fromPos, candidates, path, reason, callback));
        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutineNewer(_requests));
    }

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
                _lastResult = PathResult.None;
                //_isValid = false;

                Vector3 from = LineOfSightUtility.GetClosestPointOnNavMesh(request.From);
                Vector3 to = LineOfSightUtility.GetClosestPointOnNavMesh(point);
                _pathService?.RequestPath(from, to, request.Path, PathCheckCallback);

                yield return _waitUntilPathCheckComplete;

                if (_activeGen != Gen) break;
                if (_lastResult == PathResult.NullPathParameter)
                {
                    DestinationResultNew nullPath = new DestinationResultNew(request.Reason, request.Path, PathResult.NullPathParameter, Vector3.zero, request.StateId);
                    request.Callback?.Invoke(nullPath);
                    break;
                }

                if (_lastResult == PathResult.Failed) continue;

                // if (!_isValid) continue;
                // Debug.LogError("Sending successful Callback");
                DestinationResultNew success = new DestinationResultNew(request.Reason, request.Path, PathResult.Success, to, request.StateId);
                request.Callback?.Invoke(success);

                found = true;
                break;
            }

            if (_activeGen != Gen) break;
            if (!found)
            {
                DestinationResultNew failed = new DestinationResultNew(request.Reason, request.Path, PathResult.Failed, Vector3.zero, request.StateId);
                request.Callback?.Invoke(failed);
            }

        }

        _runningRoutine = null;

    }



    //public DestinationValidationCallback Callback { get; set; }

    private void OnPathRequestcallback(PathResult result)//bool pathFound/*in PathResult result*/)
    {
        // _isValid = pathFound;
        _lastResult = result;
        _pathChecked = true;
    }



    #region Obsolete

    [Obsolete]
    public void TryGetDestination(in ValidateDestination req)
    {
        /*   List<(Vector3 position, Vector3? forward)> destinations = new();
         //  destinations = TryGet(req);
           if (destinations == null || destinations.Count == 0)
           {
               //DestinationResult failResult = new DestinationResult(req.Reason, req.Path, false, Vector3.zero, req.StateId, null);
              // Callback?.Invoke(failResult);
               //Owner.OnPathRequestComplete(failResult);
               return;
           }
           _pathQueue.Enqueue((destinations, req));
           if (_runningRoutine == null)
               _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutineNew(_pathQueue));*/
    }

    [Obsolete]
    private IEnumerator PathFindRoutineNew(Queue<(List<(Vector3, Vector3?)>, ValidateDestination)> q)
    {

        while (q.Count > 0)
        {
            bool found = false;
            var (points, reqInfo) = q.Dequeue();

            _activeGen = Gen;

            foreach (var (pos, fwd) in points)
            {
                if (_activeGen != Gen) break;

                _pathChecked = false;
                _lastResult = PathResult.None;
                //_isValid = false;

                //  this.RequestValidPath(LineOfSightUtility.GetClosestPointOnNavMesh(reqInfo.Caller.Position()),
                //      LineOfSightUtility.GetClosestPointOnNavMesh(pos), reqInfo.Path, PathCheckCallback);

                yield return _waitUntilPathCheckComplete;

                if (_activeGen != Gen) break;
                //   if (!_isValid) continue;

                //   DestinationResult success = new DestinationResult(reqInfo.Reason, reqInfo.Path, true, pos, reqInfo.StateId, fwd);
                //   Callback?.Invoke(success);
                // Owner.OnPathRequestComplete(success);
                found = true;
                break;
            }

            if (_activeGen != Gen) break;
            if (!found)
            {
                //    DestinationResult failed = new DestinationResult(reqInfo.Reason, reqInfo.Path, false, Vector3.zero, reqInfo.StateId, null);
                //  Callback?.Invoke(failed);
                //Owner.OnPathRequestComplete(failed);
            }
        }

        _runningRoutine = null;

    }

    #endregion
}
