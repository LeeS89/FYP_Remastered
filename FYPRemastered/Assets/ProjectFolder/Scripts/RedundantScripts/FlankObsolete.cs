using System;
using UnityEngine;

[Obsolete]
public sealed class FlankObsolete : IntentStateBaseObsolete
{
    public static readonly FlankObsolete Instance = new();
    private FlankObsolete() { }

    public override void Enter(/*IFSMOwner self*/)
    {
        // public void BeginFlank();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance = false;
        // MinStopDistance = 0f;
        // MaxStopDistance
    }

    public override void Handle(/*IFSMOwner self, */NpcNotification n)
    {
        switch (n.Kind)
        {
            case NotificationType.PathBlocked:
                // Try Re path
                break;
            case NotificationType.TargetMoved or NotificationType.TargetLeftArea:
                //self.SwitchTo(ChaseStateObsolete.Instance);
                break;
            case NotificationType.NoAvailablePath://.NoFlankAvailable:
              //  self.SwitchTo(TakeCoverObsolete.Instance);
                break;
            default:
              //  base.Handle(self, n);
                break;
        }
    }
}
