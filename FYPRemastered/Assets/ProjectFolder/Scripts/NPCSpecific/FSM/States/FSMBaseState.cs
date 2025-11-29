using UnityEngine;

public abstract class FSMBaseState : IFSMState
{
    protected readonly IPathResolver _pathFinder;
    protected readonly IAgentData _ownerData;
    protected Coroutine _runningRoutine;
    public bool ContinueRoutine { get; protected set; } = true;

    public StateId Id { get; protected set; } = StateId.None;

    public FSMBaseState(IAgentData data, IPathResolver resolver)
    {
        _ownerData = data;
        _pathFinder = resolver;
    }

    public abstract void EnterState();

    public virtual void ExitState() { }

    public virtual void OnDestinationReached() { }
    
}
