using System;
using System.Collections.Generic;
using UnityEngine;


public static class NPCDecisionPolicy
{

    public static void ResolveNextState(IFSMOwner self, OwnerNPCNotification n, /*StateId currentState*/IntentStateBase sb)
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


    public static bool TryDecide(this INPCBrainContext self, in OwnerNPCNotification n, out BrainDecision decision)
    {
        var state = n.Id;
        decision = default;

        return state switch
        {
            StateId.Patrol => DecidePatrol(self, n, out decision),
            StateId.Chase => DecideChase(self, n, out decision),
            StateId.Flank => DecideFlank(self, n, out decision),
            _ => false
        };
    }

    private static bool DecidePatrol(INPCBrainContext self, in OwnerNPCNotification n, out BrainDecision d)
    {
        d = default;

        switch (n.Kind)
        {
            case NotificationKind.FOVUpdate:
                if (n.FOVResult == FOVResult.TargetSeen)
                {
                    d = new BrainDecision
                    (
                        nextIntent: StateId.Chase,
                        broadcastAlert: true,
                        CombatOrder.None
                    );
                    return true;
                }
                break;
            case NotificationKind.ZoneAlert:

                d = new BrainDecision
                    (
                        nextIntent: StateId.Chase,
                        broadcastAlert: false,
                        CombatOrder.None
                    );
                return true;
               
            default:
                return false;// Or Log Unhandled
        }

        return false;
    }

    private static bool DecideChase(INPCBrainContext self, in OwnerNPCNotification n, out BrainDecision d)
    {
        d = default;
        return false;
    }

    private static bool DecideFlank(INPCBrainContext self, in OwnerNPCNotification n, out BrainDecision d)
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

    public BrainDecision(StateId nextIntent, bool broadcastAlert, CombatOrder order)
    {
        NextIntent = nextIntent;
        BroadcastZoneAlert = broadcastAlert;
        CombatOrder = order;
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
