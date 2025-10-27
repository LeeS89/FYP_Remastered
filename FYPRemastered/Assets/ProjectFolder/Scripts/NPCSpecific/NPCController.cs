using System;
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

    protected override void OnPathValidationResult(bool pathBlocked, FSMPolicy policy)
    {
        if (!_currentPolicy.HasValue) return;
        if (policy.Version != _currentPolicy.Value.Version) return; // Stale
        // if status == false
        // and currentIntent == FollowTarget
        // Send new policy to follow group
        // if currentIntent == FollowGroup
        // new policy to Hold position

        // if status == true
        // and currentIntent == FollowGroup

        
    }

    public Action<FSMPolicy> OnPolicyUpdated;
    public void PolicyUpdated(FSMPolicy policy) => OnPolicyUpdated?.Invoke(policy);

    public Action<bool, FSMPolicy> OnPathValidation;
    public void PathValidation(bool isBlocked, FSMPolicy policy) => OnPathValidation?.Invoke(isBlocked, policy);

    protected override void PolicyResult(in FSMPolicyResult result)
    {
        if(!_currentPolicy.HasValue) return;
        if (result.Version != _currentPolicy.Value.Version) return;
        MovementIntent currentIntent = _currentPolicy.Value.MoveIntent;

        switch (currentIntent)
        {
            case MovementIntent.FollowSecondary:
                HandleFollowGroupIntentHalted(result);
                break;
        }
    }

    protected void HandleFollowGroupIntentHalted(in FSMPolicyResult result)
    {
        if (result.DestinationReached)
        {
            if(!result.PathToPrimaryBlocked)
                _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowPrimary, true);

            
        }
        // if DestinationReached && PathToPrimaryBlocked == false
        // New Policy to Follow Target

        // If DestinationReached && PathToPrimaryBlocked == true
        // New Policy to either go into cover (if future brain calculates so) or remain in place

        // if Not DestinationReached && PathToPrimaryBlocked == false
        // New Policy to follow target

       // All other cases => Remain in place
    }
}
