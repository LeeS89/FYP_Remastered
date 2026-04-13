using System;
using Unity.XR.CoreUtils;
using UnityEngine;

public sealed class FsmChaseState : FsmBaseState<IFsmChaseData>
{

    private Action<float> _distanceCheckCB;
    //  private int _distanceCheckSubscriberId = -1;
    private float? _initialDistance = null;


    public FsmChaseState(IFsmStateContext stateController, IFsmChaseData dataP, IDestinationResolver pathResolver, ICoroutineHost host)
        : base(stateController, dataP, pathResolver, host, StateId.Chase) { _candidateDestinations.EnsureCapacity(1); _distanceCheckCB = DistanceCheckCallback; }



    public override void ExitState()
    {
        base.ExitState();
        UnregisterDistanceCheck();
    }


    protected override void RetrieveCandidateDestinations()
    {

        if (!_isInState || _candidateDestinations == null) return; // Maybe a new notification


        if (!_dataProvider.TryGetDestinationCandidates(_candidateDestinations)) return;
        if (_candidateDestinations.Count is 0) return;

        /* if (_candidateDestinations.Count == 0) _candidateDestinations.Add(chaseTargetPos);
         else _candidateDestinations[0] = chaseTargetPos;*/

        CreateDestinationRequest(DestinationRequestReason.ValidatePathForDestination);
       // ValidateAndSendCandidateDestinations();

    }



    private bool TargetIsMoving() => _dataProvider?.TargetIsMoving() ?? false;
    /* {
         *//*ITargetable target;
         if (!TryGetTarget(out target)) return false;*//*

         return target.IsMoving();
     }*/

    public override void OnDestinationSet()
    {
        base.OnDestinationSet();
        //Debug.LogError("Setting Chase Dest");
        //UnregisterDistanceCheck();

    }

    // private bool TargetIsNull() => _sharedDeps == null || _sharedDeps.GetCurrentTarget?.Invoke() == null;


    public override void OnDestinationReached()
    {
        base.OnDestinationReached();
        Debug.LogError("Destination Reached in Chase");

        //if (TargetIsNull()) return;

        /* Vector3 targetPos;
         if (!TryGetTargetPosition(out targetPos)) return;*/



        /* Vector3 ownerPos;
         if (!TryGetCurrentPosition(out ownerPos)) return;*/

        // if (!Service.TryGetSqrDistanceToTarget(_stateEvents, ownerPos, out float initialDistance)) return;
        //
        // _initialDistance = initialDistance;// = targetPos.SqrDistanceTo(ownerPos);
        RegisterDistanceCheck();
        // Start job to see if player/ target has moved far enough away
        // Add job callback

        // Also, need coroutine for while we havent reached destination
        // Or maybe instead add Virtual State Tick and use that for while destination not reached
    }

    public override bool NeedsNewPath()
    {
        if (!_isInState) return false;
        return !_isAtDestination && TargetIsMoving();
    }

    private void RegisterDistanceCheck()
    {
        if (!TryGetCurrentPosition(out var pos)) return;
        if (_dataProvider.TryRegisterDistanceMonitoring(_stateContext, pos.Value, _distanceCheckCB, out float initDist))
            _initialDistance = initDist;
        else _initialDistance = null;



        /* Vector3 ownerPos;
         if (!TryGetOwnerPosition(out ownerPos)) return;

         ITargetable target;*/

        // if (!Context.TryGetTarget(out target)) return; // COMMENTED OUT FOR NOW

        /*  _distanceCheckSubscriberId = _deps.DistanceService.RegisterSubscriber(
             ownerPos,
             target,
             _distanceCheckCB
         );*/ // COMMENTED OUT FOR NOW
    }

    private void UnregisterDistanceCheck()
    {
        _initialDistance = null;
        _dataProvider.TryUnregisterDistanceMonitoring(_stateContext);


        /* if (_distanceCheckSubscriberId >= 0)
         {
             *//*if (!_deps.DistanceService.UnregisterSubscriber(_distanceCheckSubscriberId))
             {
 #if UNITY_EDITOR || DEVELOPMENT_BUILD
                 Debug.LogError("Failed to unregister distance check subscriber with id: " + _distanceCheckSubscriberId);
 #endif
             }*//* // COMMENTED OUT FOR NO
            // _distanceCheckSubscriberId = -1;
             _initialDistance = null;
         }*/
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
