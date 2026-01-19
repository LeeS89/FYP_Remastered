using System;
using Unity.XR.CoreUtils;
using UnityEngine;

public class FSMChaseState : FSMBaseState
{
    private readonly IChaseDeps _deps;
    private Action<float/*, float*/> _distanceCheckCB;
    private int _distanceCheckSubscriberId = -1;
    float? _initialDistance = null;
  
    public FSMChaseState(IChaseDeps deps, IFSMStateContext stateContext, bool useRandomStopDistance = false)
        : base(deps, stateContext, useRandomStopDistance, StateId.Chase) 
    { 
        _deps = deps;
        _distanceCheckCB = DistanceCheckCallback;
        _candidateDestinations.EnsureCapacity(1);
    }


    public override void ExitState()
    {
        base.ExitState();
        UnregisterDistanceCheck();
    }
    

    protected override void RetrieveCandidateDestinations()
    {
        
        if (!_isInState || _candidateDestinations == null || TargetIsNull()) return;

        Vector3 chaseTargetPos = _deps.Target.Position();

        if(_candidateDestinations.Count == 0) _candidateDestinations.Add(chaseTargetPos);
        else _candidateDestinations[0] = chaseTargetPos;

        ValidateCandidateDestinations();
    }


    protected override void ValidateCandidateDestinations()
    {
        if (!_isInState || OwnerDataNull()) return;
        _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
            _candidateDestinations, _path, _owner.Position(), _validationCallback);
    }

   
    public override void OnDestinationSet()
    {
        base.OnDestinationSet();
        //Debug.LogError("Setting Chase Dest");
        //UnregisterDistanceCheck();
    
    }

    private bool TargetIsNull() => _deps == null || _deps.Target == null;

    public override void OnDestinationReached()
    {
        base.OnDestinationReached();
        Debug.LogError("Destination Reached in Chase");
      
        if (TargetIsNull()) return;
        _initialDistance = _deps.Target.Position().SqrDistanceTo(_owner.Position());
        RegisterDistanceCheck();
        // Start job to see if player/ target has moved far enough away
        // Add job callback

        // Also, need coroutine for while we havent reached destination
        // Or maybe instead add Virtual State Tick and use that for while destination not reached
    }

    public override bool NeedsNewPath()
    {
        if (!_isInState) return false;
        return !_isAtDestination && !TargetIsNull() && _deps.Target.IsMoving();
    }

    private void RegisterDistanceCheck()
    {
        _distanceCheckSubscriberId = _deps.DistanceService.RegisterSubscriber(
           _owner.Position(),
           _deps.Target,
           _distanceCheckCB
       );
    }

    private void UnregisterDistanceCheck()
    {
        if(_distanceCheckSubscriberId >= 0)
        {
            if (!_deps.DistanceService.UnregisterSubscriber(_distanceCheckSubscriberId))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Failed to unregister distance check subscriber with id: " + _distanceCheckSubscriberId);
#endif
            }
            _distanceCheckSubscriberId = -1;
            _initialDistance = null;
        }
    }

   

    /// Update Late Tick & OnDestinationSet
    /// Start timer once destination set
    /// check if target is stationary at each interval
    /// If not, repath + Stop timer
    /// Also, in Late Tick, if destination reached, stop timer

    private void DistanceCheckCallback(float currentDistance)
    {
        if (!_isInState) return;
     
       // Debug.LogError($"Distance Check Callback: Initial Distance: {_initialDistance}, Current Distance: {currentDistance}");
        if (_initialDistance.HasValue && currentDistance.IsSqrDistanceGreaterThan(_initialDistance.Value, 2f))
        {
            UnregisterDistanceCheck();
            RetrieveCandidateDestinations();
            return;
        }

    }

}
