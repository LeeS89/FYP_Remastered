using UnityEngine;

public readonly struct FSMPolicy
{
    public readonly MovementIntent MoveIntent;
    public readonly bool UseRandomStoppingdistance;
    public readonly float MinStoppingdistance;
    public readonly float MaxStoppingdistance;
    public readonly uint Version;

    public FSMPolicy(uint version, MovementIntent intent, bool useRandomStoppingdistance = false, float minStopdist = 5f, float maxStopdist = 12f)
    {
        Version = version;
        MoveIntent = intent;
        UseRandomStoppingdistance = useRandomStoppingdistance;
        MinStoppingdistance = minStopdist;
        MaxStoppingdistance = maxStopdist;
        if (!useRandomStoppingdistance) MinStoppingdistance = 0f;
    }
}

public readonly struct StateNotification
{
    public readonly NotificationKind Kind { get; }

    public readonly DestinationKind DestKind;
    public readonly bool IsCurrentlyMoving { get; }
    public readonly Vector3 Destination { get; }
    public readonly Vector3? Forward { get; }

    private StateNotification(NotificationKind kind, DestinationKind destKind, bool currentlyMoving, Vector3 dest, Vector3? fwd = null)
        => (Kind, DestKind, IsCurrentlyMoving, Destination, Forward) = (kind, destKind, currentlyMoving, dest, fwd);


    public static StateNotification DestinationFound(bool currentlyMoving, DestinationKind destKind, Vector3 dest, Vector3? fwd = null)
        => new(NotificationKind.DestinationFound, destKind, currentlyMoving, dest, fwd);

    public static StateNotification TargetMoved(bool currentlyMoving)
        => new(NotificationKind.TargetMoved, DestinationKind.None, currentlyMoving, Vector3.zero, null);

    public static StateNotification TargetLeftArea(bool currentlyMoving, Vector3 dest, Vector3? fwd = null)
        => new(NotificationKind.TargetLeftArea, DestinationKind.None, currentlyMoving, dest, fwd);

    public static StateNotification PathBlocked(bool currentlyMoving)
        => new(NotificationKind.PathBlocked, DestinationKind.None, currentlyMoving, Vector3.zero, null);

    public static StateNotification TargetLOSLost(bool currentlyMoving)
        => new(NotificationKind.TargetLOSLost, DestinationKind.None, currentlyMoving, Vector3.zero, null);

    public static StateNotification TargetLOSConfirmed(bool currentlyMoving)
        => new(NotificationKind.TargetLOSConfirmed, DestinationKind.None, currentlyMoving, Vector3.zero, null);

    public static StateNotification NoAvailablePath(bool currentlyMoving)
        => new(NotificationKind.NoAvailablePath, DestinationKind.None, currentlyMoving, Vector3.zero, null);

    public static StateNotification CoverExposed(bool currentlyMoving)
        => new(NotificationKind.CoverExposed, DestinationKind.None, currentlyMoving, Vector3.zero, null);

    public static StateNotification PathToPrimaryAvailable(bool currentlyMoving)
        => new(NotificationKind.PathToPrimaryAvailable, DestinationKind.None, currentlyMoving, Vector3.zero, null);

    public static StateNotification DestinationReached(DestinationKind destKind, Vector3? forward = null)
        => new(NotificationKind.DestinationReached, destKind, false, Vector3.zero, forward);
}

public delegate void StateNotificationProvider(in StateNotification n);

