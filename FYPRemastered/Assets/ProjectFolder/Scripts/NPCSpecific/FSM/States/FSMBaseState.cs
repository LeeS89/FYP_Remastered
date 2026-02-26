using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class FsmBaseState<TDeps> : IFsmState where TDeps : FsmBaseState<TDeps>.FsmBaseStateDeps
{
    protected readonly IPathResolver _pathResolver;
    protected readonly IFsmStateEvents _stateEvents;
    //protected readonly IFsmNotificationSource _stateContext;
    protected Coroutine _runningRoutine;
    protected bool _isAtDestination = false;
    protected bool _isInState = false;
    protected DestinationResultCallback _validationCallback;


    protected List<Vector3> _candidateDestinations = new();

    //new
    //protected readonly ITargetable _owner;
    protected TDeps _deps;
    protected SharedFsmStateServices _sharedDeps;
   // protected NavMeshPath _path;
    // end new

    public StateId GetId() => _id;
    protected readonly StateId _id = StateId.None;




    public FsmBaseState(TDeps deps, SharedFsmStateServices sharedDeps, IFsmStateEvents stateEvents, StateId id)
    {
        _deps = deps;
        _sharedDeps = sharedDeps;
      //  _owner = deps.Owner;
      //  _path = deps.Path();
      //  _pathResolver = deps.PathResolver;
        _stateEvents = stateEvents;
        _id = id;
        _validationCallback = OnProcessedDestinationsResult;
    }

    private bool SharedDepsIsNull() => _sharedDeps == null; // Maybe new Notification

    // Need to make private once the Distance job is updated to accept transform instead of ITargetable
    /*private*/protected bool TryGetTarget(out ITargetable target)
    {
        if (SharedDepsIsNull()) { target = null; return false; }

        target = _sharedDeps.GetCurrentTarget?.Invoke();
        return target != null; // Maybe new notification if target is null or Func is not set
    }

    protected bool TryGetTargetPosition(out Vector3 pos)
    {
        ITargetable t;
        if (!TryGetTarget(out t)) { pos = Vector3.zero; return false; }
        pos = t.Position();
        return true;
    }

    private bool OwnerIsNull()
    {
        if (!SharedDepsIsNull()) return _sharedDeps.OwnerTransform == null; // Maybe new Notification
        return true;
    }

    protected bool TryGetPath(out NavMeshPath path)
    {
        if (SharedDepsIsNull()) { path = null; return false; };
        path = _sharedDeps.Path;
        return path != null; // Maybe new notification if path is null
    }

    protected bool TryGetOwnerPosition(out Vector3 pos)
    {
        if (OwnerIsNull()) { pos = Vector3.zero; return false; }

        pos = _sharedDeps.OwnerTransform.position;
        return true;
    }
    
    protected bool TargetIsMoving()
    {
        ITargetable target;
        if (!TryGetTarget(out target)) return false;

        return target.IsMoving();
    }

    public virtual bool NeedsNewPath() => false;
    
    //protected bool OwnerDataNull() => _owner == null || _path == null;
   // protected bool IsStationary() => _stateContext?.HasReachedDestination() ?? true;

    public virtual void EnterState() { _isInState = true; RetrieveCandidateDestinations(); }
    protected abstract void ValidateCandidateDestinations();
    protected abstract void RetrieveCandidateDestinations();
    public void TryRepath()
    {
        if (!_isInState) return;
        RetrieveCandidateDestinations();
    }

    protected virtual void OnProcessedDestinationsResult(in DestinationResultInfo result)
    {
        if (!_isInState) return;
       // Debug.LogError("Sending Dest Result from: "+ _id.ToString());
        _stateEvents?.ProcessDestinationResult(in result);
    }
    
    public virtual void ExitState()
    {
        _isInState = false;
        _pathResolver?.CancelAll();
        if (_runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }
    }
    public virtual void OnDestinationReached() => _isAtDestination = true;

    public virtual void OnDestinationSet() => _isAtDestination = false;

    public virtual void Tick(float dt) { }
    
    public virtual void LateTick(float dt) { }

    protected virtual void ShuffleCandidateList<T>(List<T> candidates)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            int randIndex = Random.Range(i, candidates.Count);
            (candidates[i], candidates[randIndex]) = (candidates[randIndex], candidates[i]);
        }
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public float GetDesiredStoppingDistance()
    {
        throw new System.NotImplementedException();
    }

    public abstract class FsmBaseStateDeps 
    { 
        public virtual float GetStoppingDistance() => 0f; 
    }
}
