using System;
using Unity.XR.CoreUtils;
using UnityEngine;

public class FSMChaseState : FSMBaseState
{
    private readonly IChaseDeps _deps;
    private float _repathInterval = 0.25f;
    private float _timeSinceLastRepath = 0f;
    private bool _timerRunning = false;
    private Action<float, float> _distanceCheckCB;
    private int _distanceCheckSubscriberId = -1;
    //  private ITargetable _target;

    /* public FSMChaseState(ITargetable target, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext)
         : base(data, resolver, stateContext, StateId.Chase) { _target = target; }*/


    public FSMChaseState(IChaseDeps deps, IFSMStateContext stateContext, bool useRandomStopDistance = false)
        : base(deps, stateContext, useRandomStopDistance, StateId.Chase) 
    { 
        _deps = deps;
        _distanceCheckCB = DistanceCheckCallback;
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
        
        if (!_isInState || _candidateDestinations == null || TargetIsNull()) return;

        Vector3 chaseTargetPos = _deps.Target.Position();

        if(_candidateDestinations.Count == 0) _candidateDestinations.Add(chaseTargetPos);
        else _candidateDestinations[0] = chaseTargetPos;

        ValidateCandidateDestinations();
    }


    public override void ValidateCandidateDestinations()
    {
        if (!_isInState || OwnerDataNull()) return;
        _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
            _candidateDestinations, _path, _owner.Position(), _validationCallback);
    }

   
    public override void OnDestinationSet()
    {
        base.OnDestinationSet();
        if (!_isInState || TargetIsNull()) return;

        _timeSinceLastRepath = _repathInterval;
        _timerRunning = true;
    }

    private bool TargetIsNull() => _deps == null || _deps.Target == null;

    public override void OnDestinationReached()
    {
        base.OnDestinationReached();
        _timerRunning = false;
        _distanceCheckSubscriberId = _deps.DistanceService.RegisterSubscriber(
            _owner.Position(),
            _deps.Target/*.Position()*/,
            1.0f,
            _distanceCheckCB
        );
        // Start job to see if player/ target has moved far enough away
        // Add job callback

        // Also, need coroutine for while we havent reached destination
        // Or maybe instead add Virtual State Tick and use that for while destination not reached
    }

    public override void LateTick(float dt)
    {
    
        if (!_isInState || !_timerRunning || _isAtDestination) return;
        _timeSinceLastRepath -= dt;

        if (_timeSinceLastRepath <= 0f)
        {
            if(_deps.Target.IsMoving()) // Target is still moving, need to repath
            {
                _timerRunning = false;
                RetrieveCandidateDestinations();
            }

            _timeSinceLastRepath = _repathInterval;
        }
    }

    /// Update Late Tick & OnDestinationSet
    /// Start timer once destination set
    /// check if target is stationary at each interval
    /// If not, repath + Stop timer
    /// Also, in Late Tick, if destination reached, stop timer

    private void DistanceCheckCallback(float initialDistance, float currentDistance)
    {
        if (!_isInState) return;
        
        Debug.LogError($"Distance Check Callback: Initial Distance: {initialDistance}, Current Distance: {currentDistance}");
        if (currentDistance > (2* initialDistance))
        {
            _deps.DistanceService.UnregisterSubscriber(_distanceCheckSubscriberId);
            RetrieveCandidateDestinations();
            return;
        }

        /* if (!_isInState) return;
         float distanceDelta = Mathf.Abs(currentDistance - initialDistance);
         float threshold = 1.5f; // Could be param
         if (distanceDelta >= threshold)
         {
             // Target has moved enough, repath
             _timerRunning = false;
             RetrieveCandidateDestinations();
         }
         else
         {
             // Restart distance check
             StartDistanceCheck();
         }*/
    }

}
