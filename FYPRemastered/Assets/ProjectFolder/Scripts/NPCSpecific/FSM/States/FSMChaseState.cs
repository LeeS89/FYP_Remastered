using UnityEngine;

public class FSMChaseState : FSMBaseState
{
    private float _repathInterval = 0.25f;
    private float _timeSinceLastRepath = 0f;

    public FSMChaseState(IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) : base(data, resolver, stateContext)
        => Id = StateId.Chase;
    

    public override void EnterState()
    {
        base.EnterState();
        _timeSinceLastRepath = _repathInterval;
        TryGetNewDestination();
    }


    public override void TryGetNewDestination()
    {
        if (_ownerData == null || _ownerData.Path == null || _ownerData.PrimaryTarget == null) return;
        var request = ValidateDestination.GetTargetPosition(_ownerData.Path, PathCheckReason.ValidatePathForDestination, _ownerData, _ownerData.PrimaryTarget);
        _pathFinder?.TryGetDestination(request);
    }

    public override void OnDestinationReached()
    {
        base.OnDestinationReached();
        // Start job to see if player/ target has moved far enough away
        // Add job callback

        // Also, need coroutine for while we havent reached destination
        // Or maybe instead add Virtual State Tick and use that for while destination not reached
    }

    public override void LateTick(float dt)
    {
        if (!_hasDestination || _ownerData == null || _ownerData.PrimaryTarget == null) return;
        _timeSinceLastRepath -= dt;

        if (_timeSinceLastRepath <= 0f)
        {
            if(!_ownerData.PrimaryTarget.IsStationary)
            {
                _hasDestination = false;
                TryGetNewDestination();
            }

            _timeSinceLastRepath = _repathInterval;
        }
    }
}
