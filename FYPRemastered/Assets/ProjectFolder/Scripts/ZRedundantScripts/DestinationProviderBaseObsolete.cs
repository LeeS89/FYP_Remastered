
using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[Obsolete("", true)]
public abstract class DestinationProviderBaseObsolete
{
    protected List<(Vector3, Vector3?)> _candidates = new();

    public virtual List<(Vector3, Vector3?)> GetCandidates() => null;

    public virtual List<(Vector3, Vector3?)> GetCandidates(DynamicDestinationKind kind, ITargetable target) => null;

    protected virtual void LoadDestinations() { }

    protected virtual void ShuffleCandidates<T>(List<T> candidates) { }
}


[Obsolete("", true)]
public sealed class DestinationProviderOld 
{
    public int CurrentWaypointZone { get; private set; } = 0;
    private Dictionary<DestinationKind, DestinationProviderBaseObsolete> _providers = new(5);
    private List<(Vector3, Vector3?)> _candidates = new(1);

    public DestinationProviderOld(params DestinationKind[] kinds)
    {
        if (kinds is null || kinds.Length == 0) 
        {
#if UNITY_EDITOR
            Debug.LogError("At least one destination kind is required - " +
                "Adding Patrol by Default");
#endif
            _providers.TryAdd(DestinationKind.Patrol, GetProvider(DestinationKind.Patrol));
            return;
        }
        foreach (var kind in kinds)
            _providers.TryAdd(kind, GetProvider(kind));
    }

    private DestinationProviderBaseObsolete GetProvider(DestinationKind kind)
    {
        return kind switch
        {
            DestinationKind.Patrol => new WaypointProviderOld(),
            _ => new WaypointProviderOld()
        };
    }

    public List<(Vector3, Vector3?)> TryGetDestinations(DestinationKind Kind, ITargetable destinationOverride = null)
    {
        if(destinationOverride != null)
        {
            _candidates.Clear();
           // _candidates.Add(destinationOverride.GetTargetablePositionAndForward());
            return _candidates;
        }
        else if(_providers.TryGetValue(Kind, out var p)) return p.GetCandidates();
        return null;
    }

   /* public List<(Vector3, Vector3?)> TryGetDestinations(DynamicDestinationKind kind, ITargetable target = null)
    {
        _candidates.Clear();
        if (target != null) _candidates.Add(target.GetTargetablePositionAndForward());
        else
        {
            if(kind == DynamicDestinationKind.Player)
            {
                if (GameManager.Instance.TryGetPlayer(out target)) _candidates.Add(target.GetTargetablePositionAndForward());
                else
                {
#if UNITY_EDITOR
                    Debug.LogError("Failed to retrieve Player Position");
#endif
                }
            }
        }
            return _candidates;
    }*/



   
}
[Obsolete("", true)]
public class WaypointProviderOld : DestinationProviderBaseObsolete
{
    private BlockData _wayPointBlock;
    private Action<BlockData> _wayPointCallback;
   // private List<WaypointPairings> _waypointPairs = new();
    public int CurrentWaypointZone { get; private set; } = 0;

    /*private struct WaypointPairings
    {
        public Vector3 position;
        public Vector3 forward;

        public WaypointPairings(Vector3 pos, Vector3 fwd)
        {
            position = pos;
            forward = fwd;
        }
    }
*/
    public WaypointProviderOld()
    {
        _wayPointCallback = OnWaypointBlockReceived;
        _candidates.EnsureCapacity(15);
        LoadDestinations();
    }

    private void OnWaypointBlockReceived(BlockData wpb)
    {
        _wayPointBlock = wpb;
        if (_wayPointBlock == null)
        {
            Debug.LogError("Waypoint block data is null. Cannot set waypoints.");
            return;
        }
     //   CurrentWaypointZone = _wayPointBlock._blockZone;


        //_waypointPairs.Clear();
        /*for (int i = 0; i < _wayPointBlock._waypointPositions.Length; i++)
            _waypointPairs.Add(new WaypointPairing(_wayPointBlock._waypointPositions[i], _wayPointBlock._waypointForwards[i]));*/

        for (int i = 0; i < _wayPointBlock._waypointPositions.Length; i++)
            _candidates.Add((_wayPointBlock._waypointPositions[i], _wayPointBlock._waypointForwards[i]));
    }

    public override List<(Vector3, Vector3?)> GetCandidates()
    {
        if (_candidates.Count > 1)
        {
            var temp = _candidates[0];
            _candidates.RemoveAt(0);
            ShuffleCandidates(_candidates);
            _candidates.Add(temp);
        }
        return _candidates;
    }

    protected override void LoadDestinations() => this.RequestWaypointBlock(callback: _wayPointCallback);

}
