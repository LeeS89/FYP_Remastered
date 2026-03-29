using System.Collections.Generic;
using UnityEngine;

public abstract class StateServiceBridge<TService> : FsmServiceBridge<TService>, IFsmDestinationProvider
{

    public StateServiceBridge(TService service) : base(service) { }

    public abstract void ReleaseCandidates(List<Vector3> buffer);

    public abstract bool TryGetDestinationCandidates(List<Vector3> buffer);

}
