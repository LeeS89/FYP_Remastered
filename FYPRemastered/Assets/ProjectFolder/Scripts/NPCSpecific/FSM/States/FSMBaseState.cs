using UnityEngine;

public abstract class FSMBaseState : IFSMState
{
    protected readonly IPathResolver _pathFinder;
    protected readonly IAgentData _ownerData;
    protected readonly IFSMStateContext _stateContext;
    protected Coroutine _runningRoutine;
    protected bool _hasDestination = false;


    public bool ContinueRoutine { get; protected set; } = true;

    public StateId Id { get; protected set; } = StateId.None;

    public FSMBaseState(IAgentData data, IPathResolver resolver, IFSMStateContext stateContext)
    {
        _ownerData = data;
        _pathFinder = resolver;
        _stateContext = stateContext;
    }

    public virtual void EnterState() { _hasDestination = false; }
    public abstract void TryGetNewDestination();
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
