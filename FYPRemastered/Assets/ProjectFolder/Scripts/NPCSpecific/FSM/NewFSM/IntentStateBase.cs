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


    public virtual void Exit(IFSMOwner self) { }// => CancelCurrent(Token)
   

    public virtual void Handle(IFSMOwner self, NotifyOwnerNPC n)
    {
       // if (n.Kind == NotificationKind.DestinationReached) self.DestinationReached(n.Id, n.HasReachedStaleDestination);
       // if (n.Kind == NotificationKind.DestinationFound) self.OnDestinationFound(n.Id, n.Destination, n.Path); // Obsolete line
       //else
            self.LogUnhandled(this, n);
    }
        

}
