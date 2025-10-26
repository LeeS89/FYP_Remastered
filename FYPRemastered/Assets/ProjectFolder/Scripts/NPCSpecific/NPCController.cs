using UnityEngine;

public class NPCController : NPCControllerBase
{
    protected override void ChangeState(State state, Transform target = null)
    {
        StateChangeResult result = _eEventManager.ChangeState(state, target);
        if(result == StateChangeResult.Success) CurrentState = state;
    }

    protected override void SetAndChaseTarget(Transform targetPosition)
    {
        if (CurrentState == State.Death) return;
        else if(CurrentState == State.Chase) _eEventManager.UpdateChaseTarget(targetPosition);
        else ChangeState(State.Chase, targetPosition);
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

    protected override void OnPathValidationResult(bool status, MovementIntent currentIntent)
    {
        // if status == false
        // and currentIntent == FollowTarget
        // Send new policy to follow group
        // if currentIntent == FollowGroup
        // new policy to Hold position

        // if status == true
        // and currentIntent == FollowGroup

        
    }
}
