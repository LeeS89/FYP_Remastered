using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class FSMBaseState : IFSMState
{
    protected readonly IPathResolver _pathFinder;
    protected readonly IAgentData _ownerData;
    protected readonly IFSMStateContext _stateContext;
    protected Coroutine _runningRoutine;
    protected bool _hasDestination = false;
    protected DestinationValidationCallback _validationCallback;


    protected List<Vector3> _candidateDestinations = new();
    public bool ContinueRoutine { get; protected set; } = true;

    public StateId GetId() => _id;
    protected readonly StateId _id = StateId.None;

    public FSMBaseState(IAgentData data, IPathResolver resolver, IFSMStateContext stateContext, StateId id)
    {
        _ownerData = data;
        _pathFinder = resolver;
        _stateContext = stateContext;
        _id = id;
        _validationCallback = OnPathResultReceived;
    }

    protected bool IsStationary() => _stateContext?.IsStationary() ?? true;

    public virtual void EnterState() { _hasDestination = false; }
    public abstract void TryGetNewDestination();
    protected virtual void OnPathResultReceived(in DestinationResult result)
        => _stateContext?.OnDestinationResultReceived(in result);

    public virtual void ExitState()
    {
        _pathFinder?.CancelAll();
        if (_runningRoutine != null)
        {
            ContinueRoutine = false;
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }
    }
    public virtual void OnDestinationReached() => _hasDestination = false;

    public virtual void OnDestinationSet() => _hasDestination = true;

    public virtual void Tick(float dt) { }
    
    public virtual void LateTick(float dt) { }
    
}
