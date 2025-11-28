using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("", true)]
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


    public static void HandleNotification(IFSMOwner delf, OwnerNPCNotification n)
    {

    }

  /*  extension(IEnumerable<int> source)
    {
        public IEnumerable<int> WhereGreaterThan(int threshold)
        => source.Where(x => x > threshold);
    }*/

}
