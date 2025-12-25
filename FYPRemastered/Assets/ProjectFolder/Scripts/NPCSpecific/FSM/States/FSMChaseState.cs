using UnityEngine;

public class FSMChaseState : FSMBaseState
{
    private float _repathInterval = 0.25f;
    private float _timeSinceLastRepath = 0f;
    private ITargetable _target;

    public FSMChaseState(ITargetable target, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext)
        : base(data, resolver, stateContext, StateId.Chase) { _target = target; }

    

    public override void EnterState()
    {
        base.EnterState();
        _timeSinceLastRepath = _repathInterval;
        ValidateCandidateDestinations();
    }


    public override void ValidateCandidateDestinations()
    {
        if (_ownerData == null || _ownerData.Path == null || _target == null /*_ownerData.PrimaryTarget == null*/) return;
        var request = ValidateDestination.GetTargetPosition(_ownerData.Path, ReasonForDestinationCheck.ValidatePathForDestination, _ownerData, _target/*_ownerData.PrimaryTarget*/);
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
        if (!_hasDestination || _ownerData == null || _target == null/*_ownerData.PrimaryTarget == null*/) return;
        _timeSinceLastRepath -= dt;

        if (_timeSinceLastRepath <= 0f)
        {
            if(!_target.IsStationary/*!_ownerData.PrimaryTarget.IsStationary*/) // Target is moving, need to repath
            {
                _hasDestination = false;
                ValidateCandidateDestinations();
            }

            _timeSinceLastRepath = _repathInterval;
        }
    }

    
}
