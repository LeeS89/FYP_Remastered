using UnityEngine;

public class FSMChaseState : FSMBaseState
{
    public FSMChaseState(IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) : base(data, resolver, stateContext)
        => Id = StateId.Chase;
    

    public override void EnterState() => TryGetNewDestination();


    public override void TryGetNewDestination()
    {
        var request = ValidateDestination.GetTargetPosition(_ownerData.Path, PathCheckReason.ValidatePathForDestination, _ownerData, _ownerData.PrimaryTarget);
        _pathFinder?.TryGetDestination(request);
    }

    public override void OnDestinationReached()
    {
        // Start job to see if player/ target has moved far enough away
        // Add job callback

        // Also, need coroutine for while we havent reached destination
        // Or maybe instead add Virtual State Tick and use that for while destination not reached
    }
}
