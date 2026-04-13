using System.Collections.Generic;
using UnityEngine;

public abstract class StateServiceBridge<TService> : FsmServiceBridge<TService>, IFsmStateData
    where TService : IFsmService

{

    public StateServiceBridge(TService service) : base(service) { }

    public float GetArrivalThreshold() => _service.GetStoppingDistance();

    public abstract void ReleaseCandidates(List<Vector3> buffer);

    public abstract bool TryGetDestinationCandidates(List<Vector3> buffer);

}
