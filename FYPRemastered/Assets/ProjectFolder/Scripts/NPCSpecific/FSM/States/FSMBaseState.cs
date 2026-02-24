using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class FsmBaseState : IFsmState
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
    protected readonly ITargetable _owner;
    protected NavMeshPath _path;
    // end new

    public StateId GetId() => _id;
    protected readonly StateId _id = StateId.None;

    private readonly bool _usesRandomStopDistance;
    public bool UsesRandomAgentStopDistance => _usesRandomStopDistance;


    public FsmBaseState(IFsmStateDeps deps, IFsmStateEvents stateEvents, bool useRandomStopDistance, StateId id)
    {
        _usesRandomStopDistance = useRandomStopDistance;
        _owner = deps.Owner;
        _path = deps.Path();
        _pathResolver = deps.PathResolver;
        _stateEvents = stateEvents;
        _id = id;
        _validationCallback = OnProcessedDestinationsResult;
    }


    public virtual bool NeedsNewPath() => false;
    
    protected bool OwnerDataNull() => _owner == null || _path == null;
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
}
