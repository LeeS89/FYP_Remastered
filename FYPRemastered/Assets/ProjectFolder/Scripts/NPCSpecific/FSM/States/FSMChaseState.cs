using Unity.XR.CoreUtils;
using UnityEngine;

public class FSMChaseState : FSMBaseState
{
    private IChaseDeps _deps;
    private float _repathInterval = 0.25f;
    private float _timeSinceLastRepath = 0f;
  //  private ITargetable _target;

   /* public FSMChaseState(ITargetable target, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext)
        : base(data, resolver, stateContext, StateId.Chase) { _target = target; }*/
    
    
    public FSMChaseState(IChaseDeps deps, IFSMStateContext stateContext)
        : base(deps, stateContext, StateId.Chase) 
    { 
        _deps = deps;
        _candidateDestinations.EnsureCapacity(1);
    }

    

    public override void EnterState()
    {
        base.EnterState();
        _timeSinceLastRepath = _repathInterval;
        ValidateCandidateDestinations();
    }


    public override void ValidateCandidateDestinations()
    {
        if (OwnerDataNull() || _deps.Target == null /*_ownerData.PrimaryTarget == null*/) return;
        var request = ValidateDestination.GetTargetPosition(/*_ownerData.Path*/_path, ReasonForDestinationCheck.ValidatePathForDestination, /*_ownerData*/_owner, _deps.Target/*_ownerData.PrimaryTarget*/);
        _pathResolver?.TryGetDestination(request);
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
        if (!_isInState || !_hasDestination || OwnerDataNull() || _deps.Target == null/*_ownerData.PrimaryTarget == null*/) return;
        _timeSinceLastRepath -= dt;

        if (_timeSinceLastRepath <= 0f)
        {
            if(!_deps.Target.IsStationary/*!_ownerData.PrimaryTarget.IsStationary*/) // Target is moving, need to repath
            {
                _hasDestination = false;
                ValidateCandidateDestinations();
            }

            _timeSinceLastRepath = _repathInterval;
        }
    }

    
}
