using UnityEngine;

public abstract class FSMBaseState : IFSMState
{
    protected readonly IPathResolver _pathFinder;
    protected readonly IAgentData _ownerData;
    protected Coroutine _runningRoutine;
    public bool ContinueRoutine { get; protected set; } = true;

    public FSMBaseState(IAgentData data, IPathResolver resolver)
    {
        _ownerData = data;
        _pathFinder = resolver;
    }

    public abstract void EnterState(StateId id);

    public virtual void ExitState(StateId id) { }

    public virtual void OnDestinationReached() { }
    
}
