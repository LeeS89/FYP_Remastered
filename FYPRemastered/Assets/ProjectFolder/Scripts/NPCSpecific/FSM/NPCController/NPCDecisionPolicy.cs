using System;
using UnityEngine;

[Obsolete("", true)]
public sealed class NPCDecisionPolicy
{
    public static readonly NPCDecisionPolicy Instance = new();
    private NPCDecisionPolicy() { }

    public void ResolveNextState(IFSMOwner self, NotifyOwnerNPC n, /*StateId currentState*/IntentStateBase sb)
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
}
