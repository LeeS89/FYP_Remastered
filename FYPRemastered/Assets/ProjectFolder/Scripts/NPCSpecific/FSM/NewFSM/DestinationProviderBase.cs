
using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public abstract class DestinationProviderBase
{
    protected List<(Vector3, Vector3?)> _candidates = new();

    public abstract List<(Vector3, Vector3?)> GetCandidates();

    protected abstract void LoadDestinations();

    protected virtual void ShuffleCandidates<T>(List<T> candidates) { }
}

public sealed class DestinationProvider 
{
    public int CurrentWaypointZone { get; private set; } = 0;
    private Dictionary<DestinationKind, DestinationProviderBase> _providers = new(5);

    public DestinationProvider()
    {
        _providers.TryAdd(DestinationKind.Patrol, new WaypointProvider());
    }

    public List<(Vector3, Vector3?)> RetrieveDestinations(DestinationKind Kind)
    {
        if(_providers.TryGetValue(Kind, out var p)) return p.GetCandidates();
        return null;
    }

    private class WaypointProvider : DestinationProviderBase
    {
        private BlockData _wayPointBlock;
        private Action<BlockData> _wayPointCallback;
        private List<WaypointPairing> _waypointPairs = new();
        public int CurrentWaypointZone { get; private set; } = 0;

        private struct WaypointPairing
        {
            public Vector3 position;
            public Vector3 forward;

            public WaypointPairing(Vector3 pos, Vector3 fwd)
            {
                position = pos;
                forward = fwd;
            }
        }

        public WaypointProvider()
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
            CurrentWaypointZone = _wayPointBlock._blockZone;

            
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
}
