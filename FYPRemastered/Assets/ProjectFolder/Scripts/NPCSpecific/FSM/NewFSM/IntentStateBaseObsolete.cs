using System;
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

[Obsolete]
public abstract class IntentStateBaseObsolete : IIntentStateObsolete
{
    public StateId Id { get; protected set; } = StateId.None;

    public abstract void Enter(IFSMOwner self);


    public virtual void Exit(IFSMOwner self) { }/* => self?.FSM?.ExitState(*//*Id*//*);*/
   

    public virtual void Handle(IFSMOwner self, NpcNotification n)
    {
        if(n.Kind == NotificationKind.FOVUpdate) self.HandleFOVSweepResult(n.FOVResult, n.TargetWithinshootingAngles);
        else self.LogUnhandled(this, n);
    }
        

}
