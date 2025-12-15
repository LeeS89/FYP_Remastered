using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public interface ICandidateProvider : IZoneSink
{
    List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req);
  //  List<(Vector3, Vector3?)> Candidates { get; }

}



public interface IPathResolver// : IZoneSink
{
    /*bool TryGetWaypointZone(out uint zone);

    bool TrySwitchWaypoints();*/
    DestinationRequestCallback Callback { get; set; }

    void CancelAll();

    void ProcessDestinationCandidates(StateId id, PathCheckReason reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos);

    [Obsolete]
    void TryGetDestination(in ValidateDestination req);
   // List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination request);
}

[Obsolete]
public interface IZoneSink
{
//    bool TryGetCurrentZone(out int zone);
    int? TryGetCurrentZone();
    bool TrySwitchPatrolZone();
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

public class DestinationResolver : ICandidateProvider
{
    private IReadOnlyDictionary<StateId, ICandidateProvider> _providers;

    public DestinationResolver(IReadOnlyDictionary<StateId, ICandidateProvider> providers)
    => _providers = providers;

    

    public List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req)
    {
        if (_providers.TryGetValue(req.StateId, out var p)) return p.TryGet(req);

        return null;
    }

    /*public bool TryGetCurrentZone(out int zone)
    {
        if(_providers.TryGetValue(StateId.Patrol, out var z))
        {
            if(z is IZoneSink zs)             {
                return zs.TryGetCurrentZone(out zone);
            }
        }
        zone = -1;
        return false;
    }*/

    public int? TryGetCurrentZone()
    {
        if (_providers.TryGetValue(StateId.Patrol, out var z))
        {
            if (z is IZoneSink zs)
                return zs.TryGetCurrentZone();
        }
        return null;
    }

    public bool TrySwitchPatrolZone()
    {
        throw new NotImplementedException();
    }
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

   // public abstract bool TryGetCurrentZone(out int zone);

    public virtual bool TrySwitchPatrolZone() => false;

    public abstract int? TryGetCurrentZone();
    
}


public sealed class TargetPointProvider : DestinationProvider
{
    private ITargetable _target;

    public TargetPointProvider(ITargetable target)
    {
        Candidates.EnsureCapacity(1);
        _target = target;
    }


    public override List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req)
    {
        if (Candidates == null || /*Candidates.Count == 0 || */_target == null || _target.Transform == null) return null;
        Candidates.Clear();
        Candidates.Add((_target.Transform.position, _target.Transform.forward));
        return Candidates;
    }

   /* public override bool TryGetCurrentZone(out int zone)
    {
        zone = -1;
        return false;
       *//* throw new NotImplementedException();*//*
    }*/

    public override int? TryGetCurrentZone() => null;
    
}

public sealed class WaypointProvider : DestinationProvider
{
    private IWaypointRepository _repo;
    public int CurrentWaypointZone { get; private set; } = -1;

    //public List<(Vector3, Vector3?)> Candidates { get; set; } = new();
    private BlockData _wayPointBlock;
    private Action<BlockData> _wpRequestCB;

    public WaypointProvider(IWaypointRepository repo)
    {
        _repo = repo;
        Candidates.EnsureCapacity(15);
        _wpRequestCB = OnWaypointBlockReceived;
        // this.RequestWaypointBlock(callback: _wpRequestCB);
        _repo.GetWaypointBlock(requestCallback: _wpRequestCB);
    }

    public override List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req)
    {
        if (Candidates == null || Candidates.Count == 0)
        {
            req.WaypointZoneCallback?.Invoke(false, -1);
            return null;
        }
     
        if(Candidates.Count > 1)
        {
            var temp = Candidates[0];
            Candidates.RemoveAt(0);
            ShuffleCandidateList(Candidates);
            Candidates.Add(temp);
        }
        req.WaypointZoneCallback?.Invoke(true, CurrentWaypointZone);
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

    /*public override bool TryGetCurrentZone(out int zone)
    {
        zone = CurrentWaypointZone.Value;
        return CurrentWaypointZone >= 0;
    }*/


    public override bool TrySwitchPatrolZone()
    {
        return false;
       // throw new NotImplementedException();
    }

    public override int? TryGetCurrentZone() => CurrentWaypointZone;

}



public readonly struct ValidateDestination
{
    public readonly StateId StateId;
    public readonly ITargetable Caller;
    public readonly ITargetable Target;
    public readonly NavMeshPath Path;
    public readonly PathCheckReason Reason;
    public readonly Action<bool, int> WaypointZoneCallback;
    public readonly uint MaxFlankSteps;
    public readonly uint MinFlankSteps;

    private ValidateDestination(
        StateId stateId,
        PathCheckReason reason,
        ITargetable caller,
        ITargetable target,
        NavMeshPath path,
        uint maxFlankSteps,
        uint minFlankSteps,
        Action<bool, int> waypointZoneCallback = null // Obsolete
        )
    {
        StateId = stateId;
        Reason = reason;
        Caller = caller; 
        Target = target; 
        Path = path;
        MaxFlankSteps = maxFlankSteps; 
        MinFlankSteps = minFlankSteps;
        WaypointZoneCallback = waypointZoneCallback;
    }

    public static ValidateDestination GetPatrolPoint(ITargetable caller, NavMeshPath path, Action<bool, int> waypointZoneCB = null)
        => new ValidateDestination(StateId.Patrol, PathCheckReason.ValidatePathForDestination, caller, null, path, 0, 0, waypointZoneCB);

    public static ValidateDestination GetFlankPoint(NavMeshPath path, ITargetable caller, ITargetable flankTarget, uint maxFlankSteps = 15, uint minFlankSteps = 4)
        => new ValidateDestination(StateId.Flank, PathCheckReason.ValidatePathForDestination, caller, flankTarget, path, maxFlankSteps, minFlankSteps);

    public static ValidateDestination GetTargetPosition(NavMeshPath path, PathCheckReason reason, ITargetable caller, ITargetable target)
        => new ValidateDestination(StateId.Chase, reason, caller, target, path, 0, 0);
}






















public interface ICandidateProviderNew : IZoneSink
{
    List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req);
    //  List<(Vector3, Vector3?)> Candidates { get; }

}




public abstract class DestinationProviderNew : ICandidateProviderNew
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

    // public abstract bool TryGetCurrentZone(out int zone);

    public virtual bool TrySwitchPatrolZone() => false;

    public abstract int? TryGetCurrentZone();

}

public sealed class WaypointProviderNew : DestinationProviderNew
{
    private IWaypointService _service;
    //private IWaypointRepository _repo;
    public int CurrentWaypointZone { get; private set; } = -1;

    //public List<(Vector3, Vector3?)> Candidates { get; set; } = new();
    private BlockData _wayPointBlock;
    private Action<BlockData> _wpRequestCB;
    private Action<bool, int> OnDelayedCandidateRequest;

    public WaypointProviderNew(IWaypointService waypointService)
    {
        _service = waypointService;
        Candidates.EnsureCapacity(15);
        _wpRequestCB = OnWaypointBlockReceived;
        // this.RequestWaypointBlock(callback: _wpRequestCB);
      //  _service.RequestWaypointBlock(requestCallback: _wpRequestCB);
    }

  //  private void SendRequest() => _service.RequestWaypointBlock(requestCallback: _wpRequestCB);


    public override List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req)
    {
        if (Candidates == null) { Candidates = new List<(Vector3, Vector3?)>(); /*SendRequest();*/ return null; }
        
        if (Candidates == null || Candidates.Count == 0)
        {
            req.WaypointZoneCallback?.Invoke(false, -1);
            return null;
        }

        if (Candidates.Count > 1)
        {
            var temp = Candidates[0];
            Candidates.RemoveAt(0);
            ShuffleCandidateList(Candidates);
            Candidates.Add(temp);
        }
        req.WaypointZoneCallback?.Invoke(true, CurrentWaypointZone); // Change later to remove
        return Candidates;
    }

    private void SortCandidates()
    {
        if (Candidates.Count > 1)
        {
            var temp = Candidates[0];
            Candidates.RemoveAt(0);
            ShuffleCandidateList(Candidates);
            Candidates.Add(temp);
        }

    }
/*
    public override List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req)
    {
        if (Candidates == null || Candidates.Count == 0)
        {
            req.WaypointZoneCallback?.Invoke(false, -1);
            return null;
        }

        if (Candidates.Count > 1)
        {
            var temp = Candidates[0];
            Candidates.RemoveAt(0);
            ShuffleCandidateList(Candidates);
            Candidates.Add(temp);
        }
        req.WaypointZoneCallback?.Invoke(true, CurrentWaypointZone);
        return Candidates;
    }*/



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

    /*public override bool TryGetCurrentZone(out int zone)
    {
        zone = CurrentWaypointZone.Value;
        return CurrentWaypointZone >= 0;
    }*/


    public override bool TrySwitchPatrolZone()
    {
        return false;
        // throw new NotImplementedException();
    }

    public override int? TryGetCurrentZone() => CurrentWaypointZone;

}