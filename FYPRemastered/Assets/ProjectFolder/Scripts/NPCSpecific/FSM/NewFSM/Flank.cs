using UnityEngine;

public sealed class Flank : IntentStateBase
{
    public static readonly Flank Instance = new();
    private Flank() { }

    public override void Enter(NPCController self)
    {
        // public void BeginFlank();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance = false;
        // MinStopDistance = 0f;
        // MaxStopDistance
    }

    public override void Handle(NPCController self, StateNotification n)
    {
        switch (n.Kind)
        {
            case NotificationKind.PathBlocked:
                // Try Re path
                break;
            case NotificationKind.TargetMoved or NotificationKind.TargetLeftArea:
                self.SwitchTo(ChaseState.Instance);
                break;
            case NotificationKind.NoFlankAvailable:
                self.SwitchTo(TakeCover.Instance);
                break;
            case NotificationKind.TargetLOSLost:
                //if(n.DestinationReached) Try Repath
                break;
            default:
                base.Handle(self, n);
                break;
        }
    }
}
