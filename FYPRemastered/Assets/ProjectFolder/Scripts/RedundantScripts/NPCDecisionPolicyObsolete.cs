using System;


[Obsolete("", true)]
public static class NPCDecisionPolicyObsolete
{

   /* public static void ResolveNextState(IFSMOwner self, NpcNotification n, *//*StateId currentState*//*IntentStateBaseObsolete sb)
    {
        NotificationKind kind = n.Kind;
        switch (kind)
        {
            case NotificationKind.NoCurrentState:
                self.SwitchTo(Patrol.Instance);
                break;
            default:
                self.LogUnhandled(sb, n); // Change
                break;
        }

    }

*/
    public static bool TryDecide(this INPCBrainContext self, in NpcNotification n, out BrainDecision decision)
    {
        var state = self.CurrentFsmState;
        decision = default;

        return state switch
        {
            StateId.Patrol or StateId.Search => DecidePatrol(self, n, out decision),
            StateId.Chase => DecideChase(self, n, out decision),
            StateId.Flank => DecideFlank(self, n, out decision),
            _ => false
        };
    }

    private static bool DecidePatrol(INPCBrainContext self, in NpcNotification n, out BrainDecision d)
    {
        d = default;

        switch (n.Kind)
        {
            case NotificationType.FOVUpdate:

                if (!FOVStatusChanged(self, n.FOVResult)) return false;
                if (TargetSeen(n.FOVResult))
                {
                    d = new BrainDecision
                    (
                        nextIntent: StateId.Chase,
                        broadcastAlert: true
                    // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                    );
                    return true;
                }
                break;
            case NotificationType.ZoneAlert:

                d = new BrainDecision
                    (
                        nextIntent: StateId.Chase
                        // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                    );
                return true;
               
            default:
                return false;// Or Log Unhandled
        }

        return false;
    }

    private static bool TargetSeen(FOVResult result) => result == FOVResult.ClearFov || result == FOVResult.TargetSeenAndWithinMeleeRadius
                    || result == FOVResult.TargetSeenAndWithinShootingAngles;

    private static bool DecideChase(INPCBrainContext self, in NpcNotification n, out BrainDecision d)
    {
        d = default;
        switch (n.Kind)
        {
            case NotificationType.FOVUpdate:
                if (!FOVStatusChanged(self, n.FOVResult)) return false;
                CombatOrder newCombatOrder = DecideNextCombatOrder(self.CurrentComOrder, n.FOVResult);
               // RotationOrder newRotOrder = DecideNextRotationOrder(self.CurrentRotOrder, n.FOVResult);
                d = new BrainDecision
                    (
                      //  nextIntent: StateId.None,
                      //  false,
                     //   newRotOrder,
                    //    newCombatOrder,
                      //  n.FOVResult
                    );
                return true;
            default:
                return false;
        }
       
    }

    private static bool DecideFlank(INPCBrainContext self, in NpcNotification n, out BrainDecision d)
    {
        d = default;
        return false;
    }

    private static bool FOVStatusChanged(INPCBrainContext c, FOVResult r) => c.CurrentFovState != r;

    private static RotationOrder DecideNextRotationOrder(RotationOrder currentOrder, FOVResult newFOVStatus)
    {
        RotationOrder newOrder;

        if (newFOVStatus == FOVResult.ClearFov || newFOVStatus == FOVResult.TargetSeenAndWithinShootingAngles
            || newFOVStatus == FOVResult.TargetSeenAndWithinMeleeRadius) { newOrder = RotationOrder.RotateTowardsTarget; }
        else newOrder = RotationOrder.StopRotating;

        if (newOrder == currentOrder) return RotationOrder.None; // Already executing order, do nothing

        return newOrder;
    }

    private static CombatOrder DecideNextCombatOrder(CombatOrder currentOrder, FOVResult newFOVStatus)
    {
        CombatOrder newOrder;
        if (newFOVStatus == FOVResult.TargetSeenAndWithinShootingAngles)
            newOrder = CombatOrder.FireAtWill;
        else if (newFOVStatus == FOVResult.TargetSeenAndWithinMeleeRadius)
            newOrder = CombatOrder.MeleeAttack;
        else newOrder = CombatOrder.FireAtWill;

        if(newOrder == currentOrder) return CombatOrder.None; // Already executing order, do nothing

        return newOrder;
    }
   /* extension(IEnumerable<int> source)
    {
        public IEnumerable<int> WhereGreaterThan(int threshold)
        => source.Where(x => x > threshold);
    }*/
}

[Obsolete("", true)]
public readonly struct BrainDecision
{
    public readonly StateId NextIntent;
    public readonly bool BroadcastZoneAlert;
    public readonly CombatOrder CombatOrder;
    public readonly FOVResult NewFOVStatus;
    public readonly RotationOrder RotationOrder;
    /*public readonly bool ResetFOVResult;*/

    public BrainDecision(StateId nextIntent, bool broadcastAlert = false, RotationOrder rOrder = RotationOrder.None, 
        CombatOrder cOrder = CombatOrder.None, FOVResult newFOVStatus = FOVResult.None)
    {
        NextIntent = nextIntent;
        BroadcastZoneAlert = broadcastAlert;
        CombatOrder = cOrder;
        NewFOVStatus = newFOVStatus;
        RotationOrder = rOrder;
      //  ResetFOVResult = resetFOVResult;
    }

    public static BrainDecision None => new BrainDecision();

}




[Obsolete("", true)]
public enum RotationOrder
{
    None,
    RotateTowardsTarget,
    StopRotating
}
































/*public static class NPCDecisionPolicyNew
{

    public static void Decide(this INPCBrainContext self, in NpcNotification n)
    {
        if(n.Kind == NotificationKind.NoCurrentState)
        {
            self.OverrideSpeed(SpeedOverride.ForceWalk);
            self.SwitchState(StateId.Patrol);
            return;
        }

        // If IsDead => return, unless notification is Death related
        var state = self.CurrentFsmState;
      
        switch (state)
        {
            case StateId.Patrol or StateId.Search:
                DecidePatrol(self, n);
                break;
            case StateId.Chase:
                DecideChase(self, n);
                break;
                case StateId.Flank:
                DecideFlank(self, n);
                break;
                default:
                // Undefined State - Log
                break;
        }
    }

    private static void DecidePatrol(INPCBrainContext self, in NpcNotification n)
    {
      
        switch (n.Kind)
        {
            case NotificationKind.FOVUpdate:
                //Debug.LogError("Receiving FOV update of: "+n.FOVResult.ToString());
               // if (!FOVStatusChanged(self, n.FOVResult)) return;

               // self.UpdateCurrentFovStatus(n.FOVResult);
                if (TargetSeen(n.FOVResult))
                {
                    self.SwitchState(StateId.Chase);
                    self.UpdateAlertPhase(AlertPhase.Alerted);
                    self.OverrideSpeed(SpeedOverride.None);
                    self.TryBroadcastAlert();
                   
                    // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                    return;
                }
                break;
            case NotificationKind.ZoneAlert:

                self.SwitchState(StateId.Chase);
               
                // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                return;

            default:
                return;// Or Log Unhandled
        }

    }

    private static bool TargetSeen(FOVResult result) => result == FOVResult.ClearFov || result == FOVResult.PartialFov || result == FOVResult.TargetSeenAndWithinMeleeRadius
                    || result == FOVResult.TargetSeenAndWithinShootingAngles;

    private static void DecideChase(INPCBrainContext self, in NpcNotification n)
    {
       
        switch (n.Kind)
        {
            case NotificationKind.FOVUpdate:
                if (!FOVStatusChanged(self, n.FOVResult)) return;
                self.UpdateCurrentFovStatus(n.FOVResult);

                *//*if (self.IsMoving())*//* TryUpdateRotationToTarget(self, TargetSeen(n.FOVResult));
                
                self.UpdateCombatOrder(DecideNextCombatOrder(self.CurrentComOrder, n.FOVResult));
               
                break;
           *//* case NotificationKind.DestinationReached:
                //TryUpdateRotationToTarget(self, true); // Always rotate to target on destination reached
                break;*//*
            case NotificationKind.DestinationSet:
              //  TryUpdateRotationToTarget(self, TargetSeen(self.CurrentFovState)); // Rotate if target is seen while moving
                break;
            default:
                break;
        }
    }

    /// Update function to implement new enum RotationOverride
    private static void TryUpdateRotationToTarget(INPCBrainContext self, bool shouldRotate)
    {
        RotationOverride rotationOverride = shouldRotate ? RotationOverride.ForceLookAtTarget : RotationOverride.None;
        self.OverrideRotation(rotationOverride);
        //if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
    }
    
  *//*  [Obsolete]
    private static void DecideRotateToTarget(INPCBrainContext self, in NpcNotification n)
    {
        NotificationKind kind = n.Kind;
        if (kind != NotificationKind.FOVUpdate || kind != NotificationKind.DestinationReached) return;

        StateId currentState = self.CurrentFsmState;
        if (currentState != StateId.Chase || currentState != StateId.Flank || currentState != StateId.Cover)
        {
            if(self.IsRotatingToTarget()) self.RotateToTarget(false);
            return;
        }
        
        if(currentState == StateId.Chase)
        {
            if(kind == NotificationKind.DestinationReached && !self.IsRotatingToTarget()) self.RotateToTarget(true);
            else if(kind == NotificationKind.DestinationSet)
            {
                bool shouldRotate = TargetSeen(self.CurrentFovState);
                if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
            }
            else if (kind == NotificationKind.FOVUpdate && self.IsMoving())
            {
                bool shouldRotate = TargetSeen(n.FOVResult);
                if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
            }
        }
    }*//*
    

    private static void DecideFlank(INPCBrainContext self, in NpcNotification n)
    {
        return;
    }

    private static bool FOVStatusChanged(INPCBrainContext c, FOVResult r) => c.CurrentFovState != r;


    private static CombatOrder DecideNextCombatOrder(CombatOrder currentOrder, FOVResult newFOVStatus)
    {
        CombatOrder newOrder;
        if (newFOVStatus == FOVResult.TargetSeenAndWithinShootingAngles)
            newOrder = CombatOrder.FireAtWill;
        else if (newFOVStatus == FOVResult.TargetSeenAndWithinMeleeRadius)
            newOrder = CombatOrder.MeleeAttack;
        else newOrder = CombatOrder.HoldFire;

        if (newOrder == currentOrder) return currentOrder;//CombatOrder.None; // Already executing order, do nothing

        return newOrder;
    }
    *//* extension(IEnumerable<int> source)
     {
         public IEnumerable<int> WhereGreaterThan(int threshold)
         => source.Where(x => x > threshold);
     }*//*
}*/
