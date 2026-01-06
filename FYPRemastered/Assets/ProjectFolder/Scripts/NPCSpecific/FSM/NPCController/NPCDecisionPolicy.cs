using System;
using System.Collections.Generic;
using UnityEngine;


public static class NPCDecisionPolicy
{

    public static void ResolveNextState(IFSMOwner self, NPCNotification n, /*StateId currentState*/IntentStateBase sb)
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


    public static bool TryDecide(this INPCBrainContext self, in NPCNotification n, out BrainDecision decision)
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

    private static bool DecidePatrol(INPCBrainContext self, in NPCNotification n, out BrainDecision d)
    {
        d = default;

        switch (n.Kind)
        {
            case NotificationKind.FOVUpdate:

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
            case NotificationKind.ZoneAlert:

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

    private static bool TargetSeen(FOVResult result) => result == FOVResult.TargetSeen || result == FOVResult.TargetSeenAndWithinMeleeRadius
                    || result == FOVResult.TargetSeenAndWithinShootingAngles;

    private static bool DecideChase(INPCBrainContext self, in NPCNotification n, out BrainDecision d)
    {
        d = default;

        switch (n.Kind)
        {
            case NotificationKind.FOVUpdate:
                if (!FOVStatusChanged(self, n.FOVResult)) return false;
                CombatOrder newCombatOrder = DecideNextCombatOrder(self.CurrentComOrder, n.FOVResult);
                RotationOrder newRotOrder = DecideNextRotationOrder(self.CurrentRotOrder, n.FOVResult);
                d = new BrainDecision
                    (
                        nextIntent: StateId.None,
                        false,
                        newRotOrder,
                        newCombatOrder,
                        n.FOVResult
                    );
                return true;
            default:
                return false;
        }
       
    }

    private static bool DecideFlank(INPCBrainContext self, in NPCNotification n, out BrainDecision d)
    {
        d = default;
        return false;
    }

    private static bool FOVStatusChanged(INPCBrainContext c, FOVResult r) => c.CurrentFovState != r;

    private static RotationOrder DecideNextRotationOrder(RotationOrder currentOrder, FOVResult newFOVStatus)
    {
        RotationOrder newOrder;

        if (newFOVStatus == FOVResult.TargetSeen || newFOVStatus == FOVResult.TargetSeenAndWithinShootingAngles
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


public enum CombatOrder
{
    None,
    HoldFire,
    FireAtWill,
    MeleeAttack
}

public enum RotationOrder
{
    None,
    RotateTowardsTarget,
    StopRotating
}
