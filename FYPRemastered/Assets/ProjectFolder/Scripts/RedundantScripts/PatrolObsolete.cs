using System;
using UnityEngine;

[Obsolete]
public sealed class PatrolObsolete : IntentStateBaseObsolete
{
    public static readonly PatrolObsolete Instance = new();
    private PatrolObsolete() { Id = StateId.Patrol; }

 

    public override void Enter(/*IFSMOwner self*/)
    {
       // self.FSM.BeginPatrol(Id);
        // public void BeginPatrol();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance
        // MinStopDistance
        // MaxStopDistance
    }

    public override void Handle(/*IFSMOwner self,*/ NpcNotification n)
    {
        switch (n.Kind)
        {
            case NotificationType.PathBlocked:
                // Try- Repath
                break;
            case NotificationType.NoAvailablePath://.NoPatrolPointAvailable:
                // Error - Hold
                break;
           /* case NotificationKind.TargetFound:
                self.TryBroadcastAlert();
                break;*/
            //case NotificationKind.TargetLOSConfirmed:
            // Notify Zone
            //  break;
            default:
               // base.Handle(self, n);
                break;
        }
    }
}
