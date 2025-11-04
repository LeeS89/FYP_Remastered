using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public interface ICandidateProvider
{
    List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req);
  //  List<(Vector3, Vector3?)> Candidates { get; }

}

public interface IDestinationResolver : IZoneSink
{
    /*bool TryGetWaypointZone(out uint zone);

    bool TrySwitchWaypoints();*/
    void CancelAll();

    void TryGetDestination(in ValidateDestination req);
   // List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination request);
}

public interface IZoneSink
{
    bool TryGetCurrentZone(out int zone);

    bool TrySwitchZone();
}

public interface IWaypointRepository
{
    void GetWaypointBlock(Action<BlockData> requestCallback);
    void SwitchWaypointBlock(BlockData oldBlock, Action<BlockData> requestCallback);
}

public interface IFlankPointSampler
{
    List<FlankPointData> GetFlankPoints();
}

public abstract class DestinationProvider : ICandidateProvider
{
    public List<(Vector3, Vector3?)> Candidates { get; set; } = new();
    
    protected virtual void ShuffleCandidateList<T>(List<T> candidates)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[randIndex]) = (candidates[randIndex], candidates[i]);
        }
    }
   
    public abstract List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req);
   
}

public sealed class WaypointProvider : DestinationProvider
{
    private IWaypointRepository _repo;
    public int CurrentWaypointZone { get; private set; }

    //public List<(Vector3, Vector3?)> Candidates { get; set; } = new();
    private BlockData _wayPointBlock;
    private Action<BlockData> _wpRequestCB;

    public WaypointProvider()
    {
        _repo = WaypointRepo.Instance;
        Candidates.EnsureCapacity(15);
        _wpRequestCB = OnWaypointBlockReceived;
        // this.RequestWaypointBlock(callback: _wpRequestCB);
        _repo.GetWaypointBlock(requestCallback: _wpRequestCB);
    }

    public override List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req)
    {
        if (Candidates == null || Candidates.Count == 0) return null;
     
        if(Candidates.Count > 1)
        {
            var temp = Candidates[0];
            Candidates.RemoveAt(0);
            ShuffleCandidateList(Candidates);
            Candidates.Add(temp);
        }
   
        return Candidates;
    }

   

    private void OnWaypointBlockReceived(BlockData wpb)
    {
        _wayPointBlock = wpb;
        if (_wayPointBlock == null)
        {
            Debug.LogError("Waypoint block data is null. Cannot set waypoints.");
            return;
        }
        CurrentWaypointZone = _wayPointBlock._blockZone;

        for (int i = 0; i < _wayPointBlock._waypointPositions.Length; i++)
            Candidates.Add((_wayPointBlock._waypointPositions[i], _wayPointBlock._waypointForwards[i]));
    }

   
}


public readonly struct ValidateDestination
{
    public readonly StateId StateId;
  //  public readonly DestinationKind Kind;
    public readonly ITargetable Caller;
    public readonly ITargetable Target;
    public readonly NavMeshPath Path;
    public readonly PathCheckReason Reason;
    public readonly int Zone;
    public readonly uint MaxFlankSteps;
    public readonly uint MinFlankSteps;

    private ValidateDestination(
        StateId stateId,
        PathCheckReason reason,
     //   DestinationKind kind,
        ITargetable caller,
        ITargetable target,
        NavMeshPath path,
        int zone,
        uint maxFlankSteps,
        uint minFlankSteps
        )
    {
        StateId = stateId;
        Reason = reason;
      //  Kind = kind; 
        Caller = caller; 
        Target = target; 
        Path = path;
        Zone = zone;
        MaxFlankSteps = maxFlankSteps; 
        MinFlankSteps = minFlankSteps;
    }

    public static ValidateDestination GetPatrolPoint(StateId id, ITargetable caller, NavMeshPath path)
        => new ValidateDestination(id, PathCheckReason.ValidatePathForDestination, /*DestinationKind.Patrol,*/ caller, null, path, 0, 0, 0);

    public static ValidateDestination GetFlankPoint(StateId id, NavMeshPath path, ITargetable caller, ITargetable flankTarget, uint maxFlankSteps = 15, uint minFlankSteps = 4)
        => new ValidateDestination(id, PathCheckReason.ValidatePathForDestination, /*DestinationKind.Flank,*/ caller, flankTarget, path, 0, maxFlankSteps, minFlankSteps);

    public static ValidateDestination GetTargetPosition(StateId id, NavMeshPath path, PathCheckReason reason, ITargetable caller, ITargetable target)
        => new ValidateDestination(id, reason, /*DestinationKind.Target,*/ caller, target, path, 0, 0, 0);
}