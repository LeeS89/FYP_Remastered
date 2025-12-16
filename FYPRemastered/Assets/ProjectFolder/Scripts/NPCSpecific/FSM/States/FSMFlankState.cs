using Unity.XR.CoreUtils;
using UnityEngine;

public class FSMFlankState : FSMBaseState
{
    public FSMFlankState(IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) 
        : base(data, resolver, stateContext, StateId.Flank)
    {
        _candidateDestinations.EnsureCapacity(25);
    }

    public override void TryGetNewDestination()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnPathResultReceived(in DestinationResult result)
    {
        throw new System.NotImplementedException();
    }
}
