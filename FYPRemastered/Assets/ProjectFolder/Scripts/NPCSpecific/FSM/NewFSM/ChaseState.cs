using UnityEngine;

public sealed class ChaseState : IntentStateBase
{
    public static readonly ChaseState Instance = new();
    private ChaseState() { }

    public override void Enter(IFSMOwner self)
    {
        // public void Chase();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance
        // MinStopDistance
        // MaxStopDistance
    }

    public override void Exit(IFSMOwner self)
    {
        // Public void CancelCurrent(token);
    }

    public override void Handle(IFSMOwner self, StateNotification n)
    {
        switch (n.Kind)
        {
            case NotificationKind.TargetLOSLost:
                if (n.IsCurrentlyMoving) self.SwitchTo(Flank.Instance); // Ensure to wait a few seconds after losing LOS, then check again before sending Notification
                // Plus Notify Zone handler of lost LOS => If all lost LOS switch to Search last known
                break;
            case NotificationKind.PathBlocked or NotificationKind.NoAvailablePath:
                self.SwitchTo(FollowGroup.Instance);
                break;
            case NotificationKind.TargetMoved:
                break;
            default:
                base.Handle(self, n);
                //self.SwitchTo(Patrol.Instance);
                break;
        }
    }

    // FSM Functions For ChaseState
   
}
