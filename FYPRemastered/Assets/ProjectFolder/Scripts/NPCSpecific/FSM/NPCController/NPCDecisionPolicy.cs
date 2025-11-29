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


    public static BrainDecision HandleNotification(this IFSMOwner self, in OwnerNPCNotification n)
    {
        var state = self.FSM.CurrentStateId;

        return state switch
        {
            StateId.Patrol => DecidePatrol(self, n),
            StateId.Chase => DecideChase(self, n),
            StateId.Flank => DecideFlank(self, n),
            _ => BrainDecision.None,
        };
    }

    private static BrainDecision DecidePatrol(IFSMOwner self, in OwnerNPCNotification n)
    {
        return BrainDecision.None;
    }

    private static BrainDecision DecideChase(IFSMOwner self, in OwnerNPCNotification n)
    {
        return BrainDecision.None;
    }

    private static BrainDecision DecideFlank(IFSMOwner self, in OwnerNPCNotification n)
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
    public StateId NextIntent { get; }
    public bool BroadcastZoneAlert { get; }

    public static BrainDecision None => new BrainDecision();

}



