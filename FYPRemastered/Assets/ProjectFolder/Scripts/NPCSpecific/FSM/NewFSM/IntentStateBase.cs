using UnityEngine;

public abstract class IntentStateBase : IIntentState
{
    public abstract void Enter(IFSMOwner self);


    public virtual void Exit(IFSMOwner self) { }// => CancelCurrent(Token)
   

    public virtual void Handle(IFSMOwner self, StateNotification n)
    {
        if (n.Kind == NotificationKind.DestinationReached) self.OnDestinationReached(n.DestKind, n.Forward);
        else if (n.Kind == NotificationKind.DestinationFound) self.OnDestinationFound(n.DestKind, n.Destination, n.Forward);
        else
            self.LogUnhandled(this, n);
    }
        

}
