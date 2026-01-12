using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class FSMBaseState : IFSMState
{
    protected readonly IPathResolver _pathResolver;
    //protected readonly IAgentData _ownerData;
    protected readonly IFSMStateContext _stateContext;
    protected Coroutine _runningRoutine;
    protected bool _isAtDestination = false;
    protected bool _isInState = false;
    protected DestinationValidationCallbackNew _validationCallback;


    protected List<Vector3> _candidateDestinations = new();

    //new
    protected readonly ITargetable _owner;
    protected NavMeshPath _path;
    // end new

    public StateId GetId() => _id;
    protected readonly StateId _id = StateId.None;

    private readonly bool _usesRandomStopDistance;
    public bool UsesRandomAgentStopDistance => _usesRandomStopDistance;

    /*public FSMBaseState(IAgentData data, IPathResolver resolver, IFSMStateContext stateContext, StateId id)
    {
        _ownerData = data;
        _pathResolver = resolver;
        _stateContext = stateContext;
        _id = id;
        _validationCallback = OnPathResultReceived;
    }*/
    public FSMBaseState(IFsmStateDeps deps, IFSMStateContext stateContext, bool useRandomStopDistance, StateId id)
    {
        _usesRandomStopDistance = useRandomStopDistance;
        _owner = deps.Owner;
        _path = deps.Path();
        _pathResolver = deps.PathResolver;
        _stateContext = stateContext;
        _id = id;
        _validationCallback = OnPathResultReceived;
    }

    protected bool OwnerDataNull() => _owner == null || _path == null;
    protected bool IsStationary() => _stateContext?.HasReachedDestination() ?? true;

    public virtual void EnterState() { _isInState = true; /*_hasDestination = false;*/ }
    public abstract void ValidateCandidateDestinations();
    protected virtual void RetrieveCandidateDestinations() { }
    protected virtual void OnPathResultReceived(in DestinationResultNew result)
    {
        if (!_isInState) return;
        Debug.LogError("Sending Dest Result from: "+ _id.ToString());
        _stateContext?.OnDestinationResultReceived(in result);
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

    
   
}
