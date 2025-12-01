using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

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
    public readonly StateId Id;
    public readonly bool IsCurrentlyMoving { get; }
    public readonly Vector3 Destination { get; }
    public readonly Vector3? Forward { get; }

    private StateNotification(NotificationKind kind, DestinationKind destKind, StateId stateId, bool currentlyMoving, Vector3 dest, Vector3? fwd = null)
        => (Kind, DestKind, Id, IsCurrentlyMoving, Destination, Forward) = (kind, destKind, stateId, currentlyMoving, dest, fwd);


    public static StateNotification DestinationFound(bool currentlyMoving, StateId stateId, DestinationKind destKind, Vector3 dest, Vector3? fwd = null)
        => new(NotificationKind.DestinationFound, destKind, stateId, currentlyMoving, dest, fwd);

    public static StateNotification TargetMoved(bool currentlyMoving, StateId id)
        => new(NotificationKind.TargetMoved, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotification TargetLeftArea(bool currentlyMoving, StateId id, Vector3 dest, Vector3? fwd = null)
        => new(NotificationKind.TargetLeftArea, DestinationKind.None, id, currentlyMoving, dest, fwd);

    public static StateNotification PathBlocked(bool currentlyMoving, StateId id)
        => new(NotificationKind.PathBlocked, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

  /*  public static StateNotification TargetLOSLost(bool currentlyMoving, StateId id)
        => new(NotificationKind.TargetLOSLost, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotification TargetLOSConfirmed(bool currentlyMoving, StateId id)
        => new(NotificationKind.TargetLOSConfirmed, DestinationKind.None, 0, currentlyMoving, Vector3.zero, null);*/

    public static StateNotification NoAvailablePath(bool currentlyMoving, StateId id)
        => new(NotificationKind.NoAvailablePath, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotification CoverExposed(bool currentlyMoving, StateId id)
        => new(NotificationKind.CoverExposed, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotification PathToPrimaryAvailable(bool currentlyMoving, StateId id)
        => new(NotificationKind.PathToPrimaryAvailable, DestinationKind.None, id, currentlyMoving, Vector3.zero, null);

    public static StateNotification DestinationReached(DestinationKind destKind, StateId id, Vector3? forward = null)
        => new(NotificationKind.DestinationReached, destKind, id, false, Vector3.zero, forward);
}

//public delegate void StateNotificationProvider(in NotifyOwnerNPC n);









































public readonly struct OwnerNPCNotification
{
    public readonly NotificationKind Kind { get; }

    public readonly StateId Id;

    

    public readonly bool HasReachedStaleDestination;
    public readonly Vector3 Destination { get; }
    public readonly NavMeshPath Path;
    public readonly FOVResult FOVResult;
    public readonly bool TargetWithinshootingAngles;
  

    private OwnerNPCNotification(NotificationKind kind, StateId stateId, bool reachedStaleDestination, Vector3 dest, NavMeshPath path, FOVResult result, bool targetInshootAngles)
        => (Kind, Id, HasReachedStaleDestination, Destination, Path, FOVResult, TargetWithinshootingAngles) = (kind, stateId, reachedStaleDestination, dest, path, result, targetInshootAngles);


    public static OwnerNPCNotification ZoneAlertReceived(StateId id)
        => new(NotificationKind.ZoneAlert, id, false, Vector3.zero, null, FOVResult.None, false);

    public static OwnerNPCNotification DestinationFound(StateId stateId, Vector3 dest, NavMeshPath path)
        => new(NotificationKind.DestinationFound, stateId, false, dest, path, FOVResult.None, false);

    public static OwnerNPCNotification TargetMoved(StateId id)
        => new(NotificationKind.TargetMoved, id, false, Vector3.zero, null, FOVResult.None, false);

    public static OwnerNPCNotification TargetLeftArea(StateId id, Vector3 dest)
        => new(NotificationKind.TargetLeftArea, id, false, dest, null, FOVResult.None, false);

    public static OwnerNPCNotification PathBlocked(StateId id)
        => new(NotificationKind.PathBlocked, id, false, Vector3.zero, null, FOVResult.None, false);

    public static OwnerNPCNotification FOVUpdate(StateId id, FOVResult result, bool targetInShootingAngles)
        => new(NotificationKind.FOVUpdate, id, false, Vector3.zero, null, result, targetInShootingAngles);

    public static OwnerNPCNotification TargetFound(StateId id)
        => new(NotificationKind.TargetFound, id, false, Vector3.zero, null, FOVResult.None, false);

  /*  public static NotifyOwnerNPC TargetLOSLost(StateId id)
        => new(NotificationKind.TargetLOSLost, id, false, Vector3.zero, null);

    public static NotifyOwnerNPC TargetLOSConfirmed(StateId id)
        => new(NotificationKind.TargetLOSConfirmed, id, false, Vector3.zero, null);*/

    public static OwnerNPCNotification NoAvailablePath(StateId id)
        => new(NotificationKind.NoAvailablePath, id, false, Vector3.zero, null, FOVResult.None, false);

    public static OwnerNPCNotification CoverExposed(StateId id)
        => new(NotificationKind.CoverExposed, id, false, Vector3.zero, null, FOVResult.None, false);

    public static OwnerNPCNotification PathToPrimaryAvailable(StateId id)
        => new(NotificationKind.PathToPrimaryAvailable, id, false, Vector3.zero, null, FOVResult.None, false);

}
