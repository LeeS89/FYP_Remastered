using UnityEngine;

public class NPCController : NPCControllerBase
{
   

    protected /*override*/ void SetAndChaseTarget(Transform targetPosition)
    {
       /* if (CurrentState == State.Death) return;
        else if(CurrentState == State.Chase) OwnerEM.UpdateChaseTarget(targetPosition);
        else ChangeState(State.Chase, targetPosition);*/
    }

   /* protected override void Engage()
    {
        throw new System.NotImplementedException();
    }*/

    protected override void OnAimEnter(bool aiming)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDamageTaken(float remainingHealth)
    {
        throw new System.NotImplementedException();
    }

    
    protected override void OnVisibilityGained(bool seen)
    {
        throw new System.NotImplementedException();
    }


    
    

    public bool TestZone = false;

    protected override void Update()
    {
        base.Update();

        if (TestZone)
        {
          //  int zone;
          //  if (!FSM.TryGetCurrentZone(out zone)) Debug.LogError("No Valid Zone found");
          //  else Debug.LogError("CurrentZone is: "+zone);
            TestZone = false;
        }
    }


  

    /*public override void LogUnhandled(IntentStateBase state, StateNotification notification)
    {
        
    }*/

    /*public override void SwitchTo(IIntentState next)
    {
        if (next == null || _state == next) return;
        _state?.Exit(this);
        _state = next;
        _state?.Enter(this);
    }*/

  //  public override void Notify(in NotifyOwnerNPC n) => _state.Handle(this, n);
    
}
