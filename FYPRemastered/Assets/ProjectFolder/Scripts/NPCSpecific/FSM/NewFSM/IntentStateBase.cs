using UnityEngine;

public abstract class IntentStateBase : IIntentState
{
    public abstract void Enter(NPCController self);


    public virtual void Exit(NPCController self) { }// => CancelCurrent(Token)
   

    public virtual void Handle(NPCController self, StateNotification notification) 
        => self.LogUnhandled(this, notification);

}
