using UnityEngine;

public class NPCController : NPCControllerBase
{
    protected override void ChangeState(State state)
    {
        StateChangeResult result = _eEventManager.ChangeState(state);
        if(result == StateChangeResult.Success) CurrentState = state;
    }

    protected override void Engage()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnAimEnter(bool aiming)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDamageTaken(float remainingHealth)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnMeleeRangeEnter(bool targetInRange)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnVisibilityGained(bool seen)
    {
        throw new System.NotImplementedException();
    }
}
