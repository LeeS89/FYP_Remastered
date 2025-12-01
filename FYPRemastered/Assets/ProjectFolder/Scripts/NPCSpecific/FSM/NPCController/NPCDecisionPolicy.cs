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


    public static BrainDecision Decide(this NPCController self, in OwnerNPCNotification n)
    {
        var state = n.Id;

        return state switch
        {
            StateId.Patrol => DecidePatrol(self, n),
            StateId.Chase => DecideChase(self, n),
            StateId.Flank => DecideFlank(self, n),
            _ => BrainDecision.None,
        };
    }

    private static BrainDecision DecidePatrol(NPCController self, in OwnerNPCNotification n)
    {
        switch (n.Kind)
        {
            case NotificationKind.FOVUpdate:
                if (n.FOVResult == FOVResult.TargetSeen)
                {
                    return new BrainDecision
                    (
                        nextIntent: StateId.Chase,
                        broadcastAlert: true,
                        CombatOrder.None
                    );
                }
                break;
            case NotificationKind.ZoneAlert:
                return new BrainDecision
                    (
                        nextIntent: StateId.Chase,
                        broadcastAlert: false,
                        CombatOrder.None
                    );
               
            default:
                return BrainDecision.None;// Or Log Unhandled
        }

        return BrainDecision.None;
    }

    private static BrainDecision DecideChase(NPCController self, in OwnerNPCNotification n)
    {
        return BrainDecision.None;
    }

    private static BrainDecision DecideFlank(NPCController self, in OwnerNPCNotification n)
    {
        return BrainDecision.None;
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
