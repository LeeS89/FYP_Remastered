using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PathFinder
{
    private ITargetable _primaryTarget;
    private ITargetable _secondaryTarget;

    private ITargetable _followTarget;
    public ITargetable _attackTarget;



    IFSMEvents Owner;

    private NavMeshPath _path;
    private WaitUntil _waitUntilPathCheckComplete;
    private bool _pathChecked;

    private Action<bool> PathCheckCallback;
   
    public int CurrentWaypointZone { get; private set; } = 0;
  //  private Transform OwnerTransform { get; set; }
    private Coroutine _runningRoutine;
    List<(Vector3 position, Vector3? forward)> samples = new(50);
    private bool _isValid = false;
    private DestinationService _destService;

    private Queue<(List<(Vector3, Vector3?)>, ValidateDestination)> _pathQueue = new(10);
   
    public PathFinder(IFSMEvents owner)
    {
        Owner = owner;
        PathCheckCallback = OnPathRequestcallback;
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathChecked);
        _destService = new DestinationService();
    }

    public void TryChaseTarget(ITargetable target) { }


    public void TryGetDestination(in ValidateDestination req)
    {
        List<(Vector3 position, Vector3? forward)> destinations;
        destinations = _destService.TryGet(req);
        if (destinations == null || destinations.Count == 0)
        {
            PathResult failResult = new PathResult(req.Reason, false, Vector3.zero, req.RequestId, null);
            Owner.OnPathRequestComplete(failResult);
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
           
            foreach (var (pos, fwd) in points)
            {
                _pathChecked = false;
                _isValid = false;

                this.RequestValidPath(LineOfSightUtility.GetClosestPointOnNavMesh(reqInfo.Caller.GetPosition()),
                     LineOfSightUtility.GetClosestPointOnNavMesh(pos), reqInfo.Path, PathCheckCallback);

                yield return _waitUntilPathCheckComplete;

                if (!_isValid) continue;

                PathResult success = new PathResult(reqInfo.Reason, true, pos, reqInfo.RequestId, fwd);
                Owner.OnPathRequestComplete(success);
                found = true;
                break;
            }

            if (!found)
            {
                PathResult failed = new PathResult(reqInfo.Reason, false, Vector3.zero, reqInfo.RequestId, null);
                Owner.OnPathRequestComplete(failed);
            }
        }
        
        _runningRoutine = null;
       
    }


    // private void QueuePathRequest(Vector3 start, Vector3 end) => return;


    public void TryGetPath(in PathRequestInfo info)
    {
        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PathFindRoutine(info));
    }


    private IEnumerator PathFindRoutine(PathRequestInfo info)
    {
        foreach(var (pos, fwd) in info.Points)
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
    }

    

   

    private void OnPathRequestcallback(bool pathFound/*in PathResult result*/)
    {
        _isValid = pathFound;
        _pathChecked = true;
    }

   

    #region Redundant

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
        if (target == AttackTarget.Primary) return _primaryTarget?.GetTargetableCollider();
        else return _secondaryTarget?.GetTargetableCollider();
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


    public void GetPrimaryTarget()
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
    }
    #endregion
}
public readonly struct PathRequestInfo
{
    public readonly List<(Vector3, Vector3?)> Points;
    public readonly Vector3 StartPos;
    public readonly PathCheckReason Reason;
    public readonly NavMeshPath Path;
    public readonly uint Id;

    public PathRequestInfo(List<(Vector3, Vector3?)> pts, Vector3 startPos, PathCheckReason reason, NavMeshPath path, uint id)
    {
        Points = pts;
        StartPos = startPos;
        Reason = reason;
       // Kind = kind;
        Path = path;
        Id = id;
    }

}

public readonly struct PathResult
{

    public readonly PathCheckReason Reason;
    public readonly bool PathFound;
    public readonly Vector3 Position;
    public readonly Vector3? Forward;
    public readonly uint Id;

    public PathResult(PathCheckReason reason, bool found, Vector3 pos, uint id, Vector3? fwd = null)
    {
   
        Reason = reason;
        Id = id;
        PathFound = found;
        Position = pos;
        Forward = fwd;
    }

}
