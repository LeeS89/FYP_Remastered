using System;
using UnityEngine;

public class NPCController : NPCControllerBase
{
    protected override void ChangeState(State state, Transform target = null)
    {
        StateChangeResult result = OwnerEM.ChangeState(state, target);
        if(result == StateChangeResult.Success) CurrentState = state;
    }

    protected override void SetAndChaseTarget(Transform targetPosition)
    {
        if (CurrentState == State.Death) return;
        else if(CurrentState == State.Chase) OwnerEM.UpdateChaseTarget(targetPosition);
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

    /*protected override void OnPathValidationResult(bool pathBlocked, FSMPolicy policy)
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

        
    }*/

    public Action<FSMPolicy> OnPolicyUpdated;
    public void PolicyUpdated(FSMPolicy policy) => OnPolicyUpdated?.Invoke(policy);

    public Action<bool, FSMPolicy> OnPathValidation;
    public void PathValidation(bool isBlocked, FSMPolicy policy) => OnPathValidation?.Invoke(isBlocked, policy);

   
    protected void HandleFollowGroupIntentHalted(in FSMPolicyResult result)
    {
      /*  PolicyHaltReason reason = result.Reason;

        if(reason == PolicyHaltReason.PathUnAvailable) 
            _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowPrimary, true);

        else if(reason == PolicyHaltReason.NoAvailableGroupToFollow) 
            _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FindAvailableCover, false);

        else 
            _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.Flee);
*/

        /* if (!result.PathToPrimaryBlocked)
         {
             _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowPrimary, true);
             return;
         }

         if (result.PathBlocked)
         {
             _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.TakeCover, false);
             return;
         }
 */

        // if DestinationReached && PathToPrimaryBlocked == false
        // New Policy to Follow Target

        // If DestinationReached && PathToPrimaryBlocked == true
        // New Policy to either go into cover (if future brain calculates so) or remain in place

        // if Not DestinationReached && PathToPrimaryBlocked == false
        // New Policy to follow target

        // All other cases => Remain in place
    }

    protected void HandleFollowPrimaryIntentHalted(in FSMPolicyResult result)
    {
        // => Eventually check brain for next decision
      /*  PolicyHaltReason reason = result.Reason;
        if (reason == PolicyHaltReason.PathBlocked)
        {
            if(!result.DestinationReached)
                _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowSecondary, true);
            // else => No need for further action at this point
        }
        else if(reason == PolicyHaltReason.TargetLOSLost)
        {
            if (result.DestinationReached)
                _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FindAvailableFlank, false);
          
        }
        else if(reason == PolicyHaltReason.TargetMoved)
        {
            //if(result.DestinationReached)
                // Re-send current policy
        }*/



  /*      if (!result.DestinationReached)
        {
            if (reason == PolicyHaltReason.PathBlocked)
                _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowSecondary, true);
        }
        else
        {
            if (reason == PolicyHaltReason.TargetLOSLost)
                _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowPrimary, true);//=> Inform ZoneClass of lost LOS
            //if(reason == )
        }*/

        /*if (result.PathToPrimaryBlocked)
            _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowSecondary, true);*/


    }
    public bool TestZone = false;

    protected override void Update()
    {
        base.Update();

        if (TestZone)
        {
            int zone;
            if (!FSM.TryGetCurrentZone(out zone)) Debug.LogError("No Valid Zone found");
            else Debug.LogError("CurrentZone is: "+zone);
            TestZone = false;
        }
    }


    protected /*override*/ void ValidateNewPolicy(MovementIntent intent)
    {

    }

    protected void OnPolicyValidationResult(in FSMPolicyValidation result)
    {
      /*  if (!_currentPolicy.HasValue || result.Version != _currentPolicy.Value.Version) return;
        MovementIntent currentIntent = _currentPolicy.Value.MoveIntent;
        PolicyIntentResult pathResult = result.PathResult;
        bool destinationReached = result.DestinationReached;

        switch (currentIntent)
        {
            case MovementIntent.FollowPrimary:
                HandlePrimaryResult(pathResult, destinationReached);
                break;
            case MovementIntent.FollowSecondary:
                HandleSecondaryResult(pathResult, destinationReached);
                break;
            default:
                return;
        }

        *//*else*//* if(currentIntent == MovementIntent.FollowSecondary)
        {
            //if(pathResult == PathCheckResult.PathAvailable) CommitPolicy
            if (pathResult == PolicyIntentResult.PathToPrimaryAvailable)
            {
                _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowPrimary, true);
                // CommitPolicy(_currentPolicy.Value);
            }
            else if(pathResult == PolicyIntentResult.NoAvailableSecondaryToFollow)
            {

            }
               

        }*/
    }



    protected void HandlePrimaryResult(PolicyIntentResult result, bool destinationReached)
    {
        // if (currentIntent == MovementIntent.FollowPrimary)
        // {
        if (result == PolicyIntentResult.TargetMoved) { ValidateNewPolicy(MovementIntent.FollowPrimary); return; }

        if (destinationReached)
        {
            if (result == PolicyIntentResult.TargetLOSLost)
                ValidateNewPolicy(MovementIntent.FindAvailableFlank);
        }
        else
        {
            if (result == PolicyIntentResult.PathToPrimaryBlocked) ValidateNewPolicy(MovementIntent.FollowSecondary);
        }
        //if(pathResult == PathCheckResult.PathToPrimaryAvailable) CommitPolicy
        /*else */
        
        
        // }
    }

    protected void HandleSecondaryResult(PolicyIntentResult result, bool destinationReached)
    {

        if (result == PolicyIntentResult.PathAvailable) return; // => CommitPolicy(_currentPolicy.Value);
        else if (result == PolicyIntentResult.PathToPrimaryAvailable)
        {
          //  _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FollowPrimary, true);
            // CommitPolicy(_currentPolicy.Value);
        }
        else if (result == PolicyIntentResult.PathBlocked)
        {
            // Re-Validate current Policy
        }
        else if (result == PolicyIntentResult.NoAvailableSecondaryToFollow)
        {
          //  _currentPolicy = new FSMPolicy(_currentPolicyVersion++, MovementIntent.FindAvailableCover, false);
            // CommitPolicy(_currentPolicy.Value);
        }
        else
        {
            // Flank maybe, or flee, Hold etc...
        }
    }

    public override void LogUnhandled(IntentStateBase state, StateNotification notification)
    {
        
    }

    public override void SwitchTo(IIntentState next)
    {
        if (next == null || _state == next) return;
        _state?.Exit(this);
        _state = next;
        _state?.Enter(this);
    }

    public override void OnNotification(in NotifyOwnerNPC n) => _state.Handle(this, n);
    
}
