using System;
using UnityEngine;

[Obsolete]
public sealed class Flank : IntentStateBaseObsolete
{
    public static readonly Flank Instance = new();
    private Flank() { }

    public override void Enter(IFSMOwner self)
    {
        // public void BeginFlank();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance = false;
        // MinStopDistance = 0f;
        // MaxStopDistance
    }

    public override void Handle(IFSMOwner self, NPCNotification n)
    {
        switch (n.Kind)
        {
            case NotificationKind.PathBlocked:
                // Try Re path
                break;
            case NotificationKind.TargetMoved or NotificationKind.TargetLeftArea:
                self.SwitchTo(ChaseState.Instance);
                break;
            case NotificationKind.NoAvailablePath://.NoFlankAvailable:
                self.SwitchTo(TakeCoverObsolete.Instance);
                break;
            default:
                base.Handle(self, n);
                break;
        }
    }
}
