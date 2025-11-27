using UnityEngine;


public enum StateId
{
    None,
    Patrol,
    Chase,
    Flank,
    Follow,
    Cover,
    Search,
    Flee
}

public abstract class IntentStateBase : IIntentState
{
    public StateId Id { get; protected set; } = StateId.None;

    public abstract void Enter(IFSMOwner self);


    public virtual void Exit(IFSMOwner self) => self?.FSM?.ExitState(Id);
   

    public virtual void Handle(IFSMOwner self, NotifyOwnerNPC n)
    {
        if(n.Kind == NotificationKind.FOVUpdate) self.HandleFOVSweepResult(n.FOVResult, n.TargetWithinshootingAngles);
        else self.LogUnhandled(this, n);
    }
        

}
