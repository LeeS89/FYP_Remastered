using UnityEngine;

public sealed class ChaseState : IntentStateBase
{
    public static readonly ChaseState Instance = new();
    private ChaseState() { }

    public override void Enter(NPCController self)
    {
        // public void Chase();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance
        // MinStopDistance
        // MaxStopDistance
    }

    public override void Exit(NPCController self)
    {
        // Public void CancelCurrent(token);
    }

    public override void Handle(NPCController self, StateNotification n)
    {
        switch (n.Kind)
        {
            case NotificationKind.TargetLOSLost:
                if (n.DestinationReached) self.SwitchTo(Flank.Instance);
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
