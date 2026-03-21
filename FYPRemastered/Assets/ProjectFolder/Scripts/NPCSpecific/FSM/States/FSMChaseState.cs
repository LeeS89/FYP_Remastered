using Npc.API;
using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class FSMChaseState : FsmBaseState<ChaseDeps>
{
   // private readonly ChaseDeps _depsNew;
    private Action<float/*, float*/> _distanceCheckCB;
    private int _distanceCheckSubscriberId = -1;
    float? _initialDistance = null;
  
    public FSMChaseState(ChaseDeps deps, SharedFsmStateServices sharedDeps, IFsmStateEvents stateContext)
        : base(deps, sharedDeps, stateContext, StateId.Chase) 
    { 
       // _deps = deps;
      // _depsNew = dpsNew;
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
        
        if (!_isInState || _candidateDestinations == null) return; // Maybe a new notification

        Vector3 chaseTargetPos;
        if (!TryGetTargetPosition(out chaseTargetPos)) return;

        if(_candidateDestinations.Count == 0) _candidateDestinations.Add(chaseTargetPos);
        else _candidateDestinations[0] = chaseTargetPos;

        ValidateCandidateDestinations();
    }

    
    protected override void ValidateCandidateDestinations()
    {
        if (!_isInState || ResolverIsNull()/*||*/ /*OwnerIsNull() ||*/ /*TryGetPath()*/) return;

        Vector3 pos;
        if (!TryGetOwnerPosition(out pos)) return;

        NavMeshPath path;
        if (!TryGetPath(out path)) return;

        DestinationRequest req = new DestinationRequest(_id, pos, _candidateDestinations, path,
            ReasonForDestinationCheck.ValidatePathForDestination, _validationCallback);

        _deps.PathResolver.ProcessDestinationCandidates(in req);
        //_pathResolver?.ProcessDestinationCandidates(in req);

        /* _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
             _candidateDestinations, _path, _owner.Position(), _validationCallback);*/
    }

   
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

        Vector3 targetPos;
        if (!TryGetTargetPosition(out targetPos)) return;

        Vector3 ownerPos;
        if (!TryGetOwnerPosition(out ownerPos)) return;

        _initialDistance = targetPos.SqrDistanceTo(ownerPos);
        RegisterDistanceCheck();
        // Start job to see if player/ target has moved far enough away
        // Add job callback

        // Also, need coroutine for while we havent reached destination
        // Or maybe instead add Virtual State Tick and use that for while destination not reached
    }

    public override bool NeedsNewPath()
    {
        if (!_isInState) return false;
        return !_isAtDestination /*&& !TargetIsNull()*/ /*&& _deps.Target.IsMoving()*/ && TargetIsMoving();
    }

    private void RegisterDistanceCheck()
    {
        Vector3 ownerPos;
        if (!TryGetOwnerPosition(out ownerPos)) return;

        ITargetable target;
        if (!TryGetTarget(out target)) return;

        _distanceCheckSubscriberId = _deps.DistanceService.RegisterSubscriber(
           ownerPos,
           target,
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

public sealed class ChaseDeps : FsmBaseState<ChaseDeps>.FsmBaseStateDeps
{
    public IDistanceMonitoringService DistanceService { get; private set; }
    public float MinStoppingDistance { get; private set; }
    public float MaxStoppingDistance { get; private set; }

    public ChaseDeps(IDistanceMonitoringService distanceService, IPathResolver resolver, ChaseStateConfig config) : base(resolver)
    {
        DistanceService = distanceService;
        MinStoppingDistance = config.minStoppingdistance;
        MaxStoppingDistance = config.maxStoppingdistance;
    }

    public override float GetStoppingDistance() => Random.Range(MinStoppingDistance, MaxStoppingDistance);


}




































public sealed class FSMChaseStateNew : FsmBaseStateNew<IChaseService>
{
    
    private Action<float> _distanceCheckCB;
  //  private int _distanceCheckSubscriberId = -1;
    private float? _initialDistance = null;


    public FSMChaseStateNew(IFsmStateEvents stateController, IChaseService service, IPathResolver pathResolver)
        : base(stateController, service, pathResolver, StateId.Chase) { _candidateDestinations.EnsureCapacity(1); }

 /*   public FSMChaseStateNew(ChaseDeps deps, SharedFsmStateServices sharedDeps, IFsmStateEvents stateContext)
        : base(deps, sharedDeps, stateContext, StateId.Chase)
    {
        // _deps = deps;
        // _depsNew = dpsNew;
        _distanceCheckCB = DistanceCheckCallback;
        _candidateDestinations.EnsureCapacity(1);
    }
*/

    public override void ExitState()
    {
        base.ExitState();
        UnregisterDistanceCheck();
    }


    protected override void RetrieveCandidateDestinations()
    {

        if (!_isInState || _candidateDestinations == null) return; // Maybe a new notification

        /*Vector3 chaseTargetPos;
        if (!TryGetTargetPosition(out chaseTargetPos)) return;*/

        if (!Context.TryGetDestinationCandidates(_stateEvents, _candidateDestinations)) return;
        if (_candidateDestinations.Count == 0) return;

       /* if (_candidateDestinations.Count == 0) _candidateDestinations.Add(chaseTargetPos);
        else _candidateDestinations[0] = chaseTargetPos;*/

        ValidateAndSendCandidateDestinations();
       
    }


    protected override void ValidateAndSendCandidateDestinations()
    {
        if (!_isInState /*|| ResolverIsNull()*//*||*/ /*OwnerIsNull() ||*/ /*TryGetPath()*/) return;

        Vector3 pos;
        NavMeshPath path;
        if (!TryGetCurrentPositionAndPath(out pos, out path)) return;
       /* if (!TryGetCurrentPosition(out pos)) return;
        if (!TryGetPath(out path)) return;*/

        DestinationRequest req = new DestinationRequest(_stateId, pos, _candidateDestinations, path,
            ReasonForDestinationCheck.ValidatePathForDestination, _validationCallback);

        _pathResolver?.ProcessDestinationCandidates(in req);
        
   //     _deps.PathResolver.ProcessDestinationCandidates(in req); // Connented out for now



        //_pathResolver?.ProcessDestinationCandidates(in req);

        /* _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
             _candidateDestinations, _path, _owner.Position(), _validationCallback);*/
    }
   

    protected bool TargetIsMoving() => Context.TargetIsMoving(_stateEvents);
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
        


        Vector3 ownerPos;
        if (!TryGetCurrentPosition(out ownerPos)) return;

        if (!Context.TryGetSqrDistanceToTarget(_stateEvents, ownerPos, out float initialDistance)) return;

        _initialDistance = initialDistance;// = targetPos.SqrDistanceTo(ownerPos);
        RegisterDistanceCheck();
        // Start job to see if player/ target has moved far enough away
        // Add job callback

        // Also, need coroutine for while we havent reached destination
        // Or maybe instead add Virtual State Tick and use that for while destination not reached
    }

    public override bool NeedsNewPath()
    {
        if (!_isInState) return false;
        return !_isAtDestination /*&& !TargetIsNull()*/ /*&& _deps.Target.IsMoving()*/ && TargetIsMoving();
    }

    private void RegisterDistanceCheck()
    {

        if (Context.TryRegisterDistanceToTargetMonitoring(_stateEvents, _distanceCheckCB, out float initDist))
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
        Context.TryUnregisterDistanceToTargetMonitoring(_stateEvents);
        

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
