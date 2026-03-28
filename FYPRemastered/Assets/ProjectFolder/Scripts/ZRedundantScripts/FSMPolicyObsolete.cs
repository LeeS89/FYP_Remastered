using System;
using UnityEngine;


[Obsolete("", true)]
public readonly struct FSMPolicyObsolete
{
    public readonly MovementIntent MoveIntent;
    public readonly bool UseRandomStoppingdistance;
    public readonly float MinStoppingdistance;
    public readonly float MaxStoppingdistance;
    public readonly uint Version;

    public FSMPolicyObsolete(uint version, MovementIntent intent, bool useRandomStoppingdistance = false, float minStopdist = 5f, float maxStopdist = 12f)
    {
        Version = version;
        MoveIntent = intent;
        UseRandomStoppingdistance = useRandomStoppingdistance;
        MinStoppingdistance = minStopdist;
        MaxStoppingdistance = maxStopdist;
        if (!useRandomStoppingdistance) MinStoppingdistance = 0f;
    }
}
[Obsolete("", true)]
public readonly struct StateNotificationObsolete
{
    public readonly NotificationType Kind { get; }

    public readonly DestinationKind DestKind;
    public readonly StateId Id;
    public readonly bool IsCurrentlyMoving { get; }
    public readonly Vector3 Destination { get; }
    public readonly Vector3? Forward { get; }

    private StateNotificationObsolete(NotificationType kind, DestinationKind destKind, StateId stateId, bool currentlyMoving, Vector3 dest, Vector3? fwd = null)
        => (Kind, DestKind, Id, IsCurrentlyMoving, Destination, Forward) = (kind, destKind, stateId, currentlyMoving, dest, fwd);


   /* public static StateNotificationObsolete DestinationFound(bool currentlyMoving, StateId stateId, DestinationKind destKind, Vector3 dest, Vector3? fwd = null)
        => new(NotificationType.DestinationFound, destKind, stateId, currentlyMoving, dest, fwd);*/

    public static StateNotificationObsolete TargetMoved(bool currentlyMoving, StateId id)
        => new(NotificationType.TargetMoved, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotificationObsolete TargetLeftArea(bool currentlyMoving, StateId id, Vector3 dest, Vector3? fwd = null)
        => new(NotificationType.TargetLeftArea, DestinationKind.None, id, currentlyMoving, dest, fwd);

    public static StateNotificationObsolete PathBlocked(bool currentlyMoving, StateId id)
        => new(NotificationType.PathBlocked, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

  /*  public static StateNotification TargetLOSLost(bool currentlyMoving, StateId id)
        => new(NotificationKind.TargetLOSLost, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotification TargetLOSConfirmed(bool currentlyMoving, StateId id)
        => new(NotificationKind.TargetLOSConfirmed, DestinationKind.None, 0, currentlyMoving, Vector3.zero, null);*/

    public static StateNotificationObsolete NoAvailablePath(bool currentlyMoving, StateId id)
        => new(NotificationType.NoAvailablePath, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotificationObsolete CoverExposed(bool currentlyMoving, StateId id)
        => new(NotificationType.CoverExposed, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotificationObsolete PathToPrimaryAvailable(bool currentlyMoving, StateId id)
        => new(NotificationType.PathToPrimaryAvailable, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotificationObsolete DestinationReached(DestinationKind destKind, StateId id, Vector3? forward = null)
        => new(NotificationType.DestinationReached, destKind, id, false, Vector3.zero, forward);
}

//public delegate void StateNotificationProvider(in NotifyOwnerNPC n);










































