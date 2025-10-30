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

    private BlockData _wayPointSet;
    private NavMeshPath _path;
    private WaitUntil _waitUntilPathCheckComplete;
    private bool _pathChecked;
    private BlockData _wayPointBlock;
    private Action<BlockData> _wayPointCallback;
    private Action<bool> PathCheckCallback;
    private List<WaypointPairs> _waypointPairs = new();
    private WaypointPairs? _currentWaypointPair = null;
    public int CurrentWaypointZone { get; private set; } = 0;
  //  private Transform OwnerTransform { get; set; }
    private Coroutine _runningRoutine;
    List<(Vector3 position, Vector3? forward)> samples = new(50);
    private bool _isValid = false;
   // public Action<bool, Vector3, Vector3?> OnSendDestinationResult { get; set; }
    // Plan for Destination providers
    //

    public PathFinder(IFSMEvents owner)
    {
        Owner = owner;
       // OwnerTransform = Owner.Transform;
        PathCheckCallback = OnPathRequestcallback;
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathChecked);
        //_wayPointCallback = OnWaypointBlockReceived;
       // RetrieveWaypoints();
    }

    public void TryChaseTarget(ITargetable target) { }

    private void RetrieveWaypoints() => this.RequestWaypointBlock(callback: _wayPointCallback);

   // private void QueuePathRequest(Vector3 start, Vector3 end) => return;

    private void OnWaypointBlockReceived(BlockData wpb)
    {
        _wayPointBlock = wpb;
        if (_wayPointBlock == null)
        {
            Debug.LogError("Waypoint block data is null. Cannot set waypoints.");
            return;
        }
        CurrentWaypointZone = _wayPointBlock._blockZone;

        _waypointPairs.Clear();
        for (int i = 0; i < wpb._waypointPositions.Length; i++)
            _waypointPairs.Add(new WaypointPairs(_wayPointBlock._waypointPositions[i], _wayPointBlock._waypointForwards[i]));
    }

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

            PathResult result = new PathResult(info.Kind, true, pos, info.Id, fwd);
            Owner.OnPathRequestComplete(result);
            yield break;
        }
        PathResult failResult = new PathResult(info.Kind, false, Vector3.zero, info.Id, null);
        Owner.OnPathRequestComplete(failResult);

    }

    

   

    private void OnPathRequestcallback(bool pathFound/*in PathResult result*/)
    {
        _isValid = pathFound;
        _pathChecked = true;
    }

    private void ShuffleCandidateList<T>(List<T> candidates)
    {

        for (int i = 0; i < candidates.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[randIndex]) = (candidates[randIndex], candidates[i]);
        }
    }


    private struct WaypointPairs
    {
        public Vector3 position;
        public Vector3 forward;

        public WaypointPairs(Vector3 pos, Vector3 fwd)
        {
            position = pos;
            forward = fwd;
        }
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
    public Vector3? GetFollowTarget(MovementIntent intent)
    {
        Vector3? target;
        switch (intent)
        {
            case MovementIntent.FollowPrimary:
                target = _primaryTarget?.GetTargetablePosition();
                break;
            case MovementIntent.FollowSecondary:
                target = _secondaryTarget?.GetTargetablePosition();
                break;
            default:
                target = null;
                break;
        }
        return target;
    }


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
    public readonly DestinationKind Kind;
    public readonly NavMeshPath Path;
    public readonly uint Id;

    public PathRequestInfo(List<(Vector3, Vector3?)> pts, Vector3 startPos, DestinationKind kind, NavMeshPath path, uint id)
    {
        Points = pts;
        StartPos = startPos;
        Kind = kind;
        Path = path;
        Id = id;
    }

}

public readonly struct PathResult
{
    public readonly DestinationKind Kind;
    public readonly bool PathFound;
    public readonly Vector3 Position;
    public readonly Vector3? Forward;
    public readonly uint Id;

    public PathResult(DestinationKind kind, bool found, Vector3 pos, uint id, Vector3? fwd = null)
    {
        Kind = kind;
        Id = id;
        PathFound = found;
        Position = pos;
        Forward = fwd;
    }

}
