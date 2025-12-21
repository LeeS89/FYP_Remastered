using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Obsolete]
public class PathFinderObsolete : IPathResolver
{
   /* private ITargetable _primaryTarget;
    private ITargetable _secondaryTarget;

    private ITargetable _followTarget;
    public ITargetable _attackTarget;*/

  //  private readonly Dictionary<StateId, ICandidateProvider> _map;

   // private readonly IFSMEvents Owner;

    private WaitUntil _waitUntilPathCheckComplete;
    private bool _pathChecked;


    public uint Gen { get; private set; }
    private uint _activeGen;
    private Action<bool> PathCheckCallback;
    
    public int CurrentWaypointZone { get; private set; } = 0;

    private Coroutine _runningRoutine;
    List<(Vector3 position, Vector3? forward)> samples = new(50);
    private bool _isValid = false;
  

    private Queue<(List<(Vector3, Vector3?)>, ValidateDestination)> _pathQueue = new(10);
   // private IReadOnlyDictionary<StateId, ICandidateProvider> _providerMap;
    private ICandidateProvider _destResolver;


    public PathFinderObsolete(ICandidateProvider destResolver/*IReadOnlyDictionary<StateId, ICandidateProvider> providers*/)
    {
      //  _providerMap = providers;
       // _map = providers;
        //Owner = owner;
        _destResolver = destResolver;
        PathCheckCallback = OnPathRequestcallback;
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathChecked);
       /* _map = new()
        {
            [StateId.Patrol] = new WaypointProvider()
        };*/
    }

    public List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination request)
    {
        return _destResolver?.TryGet(request); 
        // if (_providerMap.TryGetValue(request.StateId, out var p)) return p.TryGet(request);

        //  return null;
    }

    

  /*  public bool TryGetCurrentZone(out int zone)
    {
        return _destResolver.TryGetCurrentZone(out zone);
        *//*if (_providerMap.TryGetValue(StateId.Patrol, out var p))
        {
            if (p is WaypointProvider pr)
            {
                zone = pr.CurrentWaypointZone;
                return true;
            }
        }

        zone = 0;
        return false;*//*
    }*/
    public int? TryGetCurrentZone() => _destResolver?.TryGetCurrentZone();



    public bool TrySwitchPatrolZone() => false;
   
   
    public void TryChaseTarget(ITargetable target) { }


    public void CancelAll()
    {
        Gen++;
        if (_runningRoutine != null) { CoroutineRunner.Instance.StopCoroutine(_runningRoutine); 
            _runningRoutine = null; }
        while(_pathQueue.Count > 0)
        {
            var (_, req) = _pathQueue.Dequeue();
            DestinationResult cancelled = new DestinationResult(ReasonForDestinationCheck.Cancelled, req.Path, false, Vector3.zero, req.StateId);
        }
    }


    public void TryGetDestination(in ValidateDestination req)
    {
        List<(Vector3 position, Vector3? forward)> destinations;
        destinations = TryGet(req);
        if (destinations == null || destinations.Count == 0)
        {
            DestinationResult failResult = new DestinationResult(req.Reason, req.Path, false, Vector3.zero, req.StateId, null);
            Callback?.Invoke(failResult);
            //Owner.OnPathRequestComplete(failResult);
            return;
        }
        _pathQueue.Enqueue((destinations, req));
        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutineNew(_pathQueue));
    }
    
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
                _isValid = false;

                this.RequestValidPath(LineOfSightUtility.GetClosestPointOnNavMesh(reqInfo.Caller.Position()),
                     LineOfSightUtility.GetClosestPointOnNavMesh(pos), reqInfo.Path, PathCheckCallback);

                yield return _waitUntilPathCheckComplete;

                if (_activeGen != Gen) break;
                if (!_isValid) continue;
                
                DestinationResult success = new DestinationResult(reqInfo.Reason, reqInfo.Path, true, pos, reqInfo.StateId, fwd);
                Callback?.Invoke(success);
               // Owner.OnPathRequestComplete(success);
                found = true;
                break;
            }
            
            if (_activeGen != Gen) break;
            if (!found)
            {
                DestinationResult failed = new DestinationResult(reqInfo.Reason, reqInfo.Path, false, Vector3.zero, reqInfo.StateId, null);
                Callback?.Invoke(failed);
                //Owner.OnPathRequestComplete(failed);
            }
        }
        
        _runningRoutine = null;
       
    }

    public DestinationValidationCallback Callback { get; set; }

    private void OnPathRequestcallback(bool pathFound/*in PathResult result*/)
    {
        _isValid = pathFound;
        _pathChecked = true;
    }



    #region Redundant
/*
    public void TryGetPath(in PathRequestInfo info)
    {
        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutineOld(info));
    }


    private IEnumerator PathFindRoutineOld(PathRequestInfo info)
    {
        foreach (var (pos, fwd) in info.Points)
        {
            _pathChecked = false;
            _isValid = false;

            this.RequestValidPath(LineOfSightUtility.GetClosestPointOnNavMesh(info.StartPos),
                LineOfSightUtility.GetClosestPointOnNavMesh(pos), info.Path, PathCheckCallback);

            yield return _waitUntilPathCheckComplete;

            if (!_isValid) continue;

            PathResult result = new PathResult(info.Reason, true, pos, info.Id, fwd);
            Owner.OnPathRequestComplete(result);
            _runningRoutine = null;
            yield break;
        }
        PathResult failResult = new PathResult(info.Reason, false, Vector3.zero, info.Id, null);
        Owner.OnPathRequestComplete(failResult);
        _runningRoutine = null;
    }*/
    /*public void TryGetWaypoint(DestinationKind target)
    {
        if (_currentWaypointPair.HasValue)
        {
            _waypointPairs.Remove(_currentWaypointPair.Value);
            ShuffleCandidateList(_waypointPairs);
            _waypointPairs.Add(_currentWaypointPair.Value);
        }
        else
        {
            ShuffleCandidateList(_waypointPairs);
        }
        for(int i = 0; i < _waypointPairs.Count; i++)
        {
            samples.Add((_waypointPairs[i].position, _waypointPairs[i].forward));
        }

        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(WaypointRoutine(target, samples));
    }*/
    /*
        private IEnumerator WaypointRoutine(DestinationKind target, List<(Vector3 position, Vector3? forward)> samples)
        {
            foreach (var (pos, fwd) in samples)
            {
                _pathChecked = false;
                _isValid = false;

                this.RequestValidPath(LineOfSightUtility.GetClosestPointOnNavMesh(Owner.Transform.position),
                    LineOfSightUtility.GetClosestPointOnNavMesh(pos), _path, PathCheckCallback);

                yield return _waitUntilPathCheckComplete;

                if (!_isValid) continue;

                Owner.OnPathRequestComplete(target, _isValid, pos, fwd);
                yield break;
            }
            Owner.OnPathRequestComplete(target, false, Vector3.zero, null);
        }*/
    public Transform FollowTarget { get; private set; } = null;
    public Vector3 LastKnownTargetPos { get; private set; }
   

    public Collider GetAttackTarget(AttackTarget target)
    {
        /* if (target == AttackTarget.Primary) return _primaryTarget?.GetTargetableCollider();
         else return _secondaryTarget?.GetTargetableCollider();*/
        return null;
    }

    public void ProcessDestinationCandidates(StateId id, ReasonForDestinationCheck reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos)
    {
        throw new NotImplementedException();
    }

    public void ProcessDestinationCandidates(StateId id, ReasonForDestinationCheck reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos, DestinationValidationCallback callBack)
    {
        throw new NotImplementedException();
    }

    public void ProcessDestinationCandidates(StateId id, ReasonForDestinationCheck reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos, DestinationValidationCallbackNew callBack)
    {
        throw new NotImplementedException();
    }




    /* public Vector3? GetFollowTarget(MovementIntent intent)
{
    Vector3? target;
    switch (intent)
    {
        case MovementIntent.FollowPrimary:
            target = _primaryTarget?.GetTargetablePositionAndForward();
            break;
        case MovementIntent.FollowSecondary:
            target = _secondaryTarget?.GetTargetablePositionAndForward();
            break;
        default:
            target = null;
            break;
    }
    return target;
}*/


    /*  public void GetPrimaryTarget()
      {
          if(!GameManager.Instance.TryGetPlayer(out _primaryTarget))
          {
  #if UNITY_EDITOR
              Debug.LogError("Player ITargetable not found");
  #endif
          }
          else
          {
  #if UNITY_EDITOR
              Debug.LogError("Player ITargetable found");
  #endif
          }
      }*/




    #endregion
}

[Obsolete]
public readonly struct PathRequestInfo
{
    public readonly List<(Vector3, Vector3?)> Points;
    public readonly Vector3 StartPos;
    public readonly ReasonForDestinationCheck Reason;
    public readonly NavMeshPath Path;
    public readonly uint Id;

    public PathRequestInfo(List<(Vector3, Vector3?)> pts, Vector3 startPos, ReasonForDestinationCheck reason, NavMeshPath path, uint id)
    {
        Points = pts;
        StartPos = startPos;
        Reason = reason;
       // Kind = kind;
        Path = path;
        Id = id;
    }

}


[Obsolete]
public readonly struct DestinationResult
{

    public readonly ReasonForDestinationCheck Reason;
    public readonly NavMeshPath Path;
    public readonly bool PathFound;
    public readonly Vector3 Destination;
    public readonly Vector3? Forward;
    public readonly StateId Id;

    public DestinationResult(ReasonForDestinationCheck reason, NavMeshPath path, bool found, Vector3 dest, StateId id, Vector3? fwd = null)
    {
        Reason = reason;
        Path = path;
        Id = id;
        PathFound = found;
        Destination = dest;
        Forward = fwd;
    }

}


[Obsolete]
public delegate void DestinationValidationCallback(in DestinationResult result);


























public delegate void DestinationValidationCallbackNew(in DestinationResultNew result);






public readonly struct DestinationResultNew
{

    public readonly ReasonForDestinationCheck Reason;
    public readonly NavMeshPath Path;
    public readonly PathResult PathResult;
    public readonly Vector3 Destination;
    public readonly Vector3? Forward;
    public readonly StateId Id;

    public DestinationResultNew(ReasonForDestinationCheck reason, NavMeshPath path, PathResult result, Vector3 dest, StateId id, Vector3? fwd = null)
    {
        Reason = reason;
        Path = path;
        Id = id;
        PathResult = result;
        //PathFound = found;
        Destination = dest;
        Forward = fwd;
    }

}





public class PathFinderNew : IPathResolver
{

    private readonly IPathService _pathService;


    private WaitUntil _waitUntilPathCheckComplete;
    private bool _pathChecked;


    public uint Gen { get; private set; }
    private uint _activeGen;
    private Action<PathResult> PathCheckCallback;

    

    private Coroutine _runningRoutine;

    private PathResult _lastResult;



    public PathFinderNew(IPathService pathService)
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
        _requests.Enqueue( new DestinationRequest(id, fromPos, candidates, path, reason, callback));
        if(_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutineNewer(_requests));
    }

    private IEnumerator PathFindRoutineNewer(Queue<DestinationRequest> q)
    {

        while (q.Count > 0)
        {
            bool found = false;
            var request = q.Dequeue();

            _activeGen = Gen;

            foreach(var point in request.Candidates)
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
                if(_lastResult == PathResult.NullPathParameter)
                {
                    DestinationResultNew nullPath = new DestinationResultNew(request.Reason, request.Path, PathResult.NullPathParameter, Vector3.zero, request.StateId);
                    request.Callback?.Invoke(nullPath);
                    break;
                }

                if (_lastResult == PathResult.Failed) continue;
                
               // if (!_isValid) continue;

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
