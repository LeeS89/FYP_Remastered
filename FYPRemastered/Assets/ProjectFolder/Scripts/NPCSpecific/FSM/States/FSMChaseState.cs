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
        //_timeSinceLastRepath = _repathInterval;
        RetrieveCandidateDestinations();
        //ValidateCandidateDestinations();
    }

    protected override void RetrieveCandidateDestinations()
    {
        
        if (_candidateDestinations == null || TargetIsNull()) return;

        Vector3 chaseTargetPos = _deps.Target.Position();

        if(_candidateDestinations.Count == 0) _candidateDestinations.Add(chaseTargetPos);
        else _candidateDestinations[0] = chaseTargetPos;

        ValidateCandidateDestinations();
    }


    public override void ValidateCandidateDestinations()
    {
        if (OwnerDataNull()) return;
        Debug.LogError("Sending Chase request to path manager");
        _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
            _candidateDestinations, _path, _owner.Position(), _validationCallback);
       // var request = ValidateDestination.GetTargetPosition(/*_ownerData.Path*/_path, ReasonForDestinationCheck.ValidatePathForDestination, /*_ownerData*/_owner, _deps.Target/*_ownerData.PrimaryTarget*/);
       // _pathResolver?.TryGetDestination(request);
    }

   
    public override void OnDestinationSet()
    {
        Debug.LogError("Destination Set In Chase State");
        base.OnDestinationSet();
        if (!_isInState || TargetIsNull()) return;

        // Target moved - Repath
        if (!_deps.Target.IsStationary)
            RetrieveCandidateDestinations();
    }

    private bool TargetIsNull() => _deps == null || _deps.Target == null;

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
        return;
        if (!_isInState /*|| !_hasDestination*/ || OwnerDataNull() || _deps.Target == null/*_ownerData.PrimaryTarget == null*/) return;
        _timeSinceLastRepath -= dt;

        if (_timeSinceLastRepath <= 0f)
        {
            if(!_deps.Target.IsStationary/*!_ownerData.PrimaryTarget.IsStationary*/) // Target is moving, need to repath
            {
               // _hasDestination = false;
                ValidateCandidateDestinations();
            }

            _timeSinceLastRepath = _repathInterval;
        }
    }

    
}
