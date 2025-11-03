using UnityEngine;

public sealed class FollowGroup : IntentStateBase
{
    public static readonly FollowGroup Instance = new();
    private FollowGroup() { }

    public override void Enter(IFSMOwner self)
    {
        // public void FollowGroup();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance
        // MinStopDistance
        // MaxStopDistance

    }

    public override void Handle(IFSMOwner self, NotifyOwnerNPC n)
    {
        switch (n.Kind)
        {
            case NotificationKind.PathBlocked:
                // Try Repath
                break;
            case NotificationKind.PathToPrimaryAvailable:
                self.SwitchTo(ChaseState.Instance);
                break;
            case NotificationKind.NoAvailablePath://.NoAvailableGroupToFollow:
                // Take Cover, Flee, Try re-Chase, Flank
                break;
            default:
                base.Handle(self, n);
                break;
        }
    }
}
