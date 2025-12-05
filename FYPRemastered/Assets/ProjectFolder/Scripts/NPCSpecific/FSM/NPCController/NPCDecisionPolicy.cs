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
        var state = self.CurrentFSMState;
        decision = default;

        return state switch
        {
            StateId.Patrol => DecidePatrol(self, n, out decision),
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

                if (n.FOVResult == self.CurrentFOVState) return false;
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
        return false;
    }

    private static bool DecideFlank(INPCBrainContext self, in NPCNotification n, out BrainDecision d)
    {
        d = default;
        return false;
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
    /*public readonly bool ResetFOVResult;*/

    public BrainDecision(StateId nextIntent, bool broadcastAlert = false, CombatOrder order = CombatOrder.None, FOVResult newFOVStatus = FOVResult.None)
    {
        NextIntent = nextIntent;
        BroadcastZoneAlert = broadcastAlert;
        CombatOrder = order;
        NewFOVStatus = newFOVStatus;
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
