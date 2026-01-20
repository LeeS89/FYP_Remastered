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

































public enum NotifyPriority : byte
{
    Critical = 0,
    High = 1,
    Normal = 2,
    Low = 3
}







public readonly struct NpcNotification
{
    public readonly NotificationKind Kind { get; }

  
    public readonly bool HasReachedStaleDestination;
    public readonly Vector3 Destination { get; }
    public readonly FOVResult FOVResult;
    public readonly bool TargetWithinshootingAngles;
    public readonly NotifyPriority Priority;
  

    private NpcNotification(NotificationKind kind, NotifyPriority priority, bool reachedStaleDestination, Vector3 dest, FOVResult result, bool targetInshootAngles)
        => (Kind, Priority, HasReachedStaleDestination, Destination, FOVResult, TargetWithinshootingAngles) = (kind, priority, reachedStaleDestination, dest, result, targetInshootAngles);

    public static NpcNotification SceneBegin()
        => new(NotificationKind.NoCurrentState, NotifyPriority.Critical, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification DestinationReached(/*StateId id,*/ /*bool reachedStaleDestination*/)
        => new(NotificationKind.DestinationReached, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification DestinationSet()
        => new(NotificationKind.DestinationSet, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification ZoneAlert(/*StateId id*/)
        => new(NotificationKind.ZoneAlert, NotifyPriority.Low, false, Vector3.zero, FOVResult.None, false);

   /* public static OwnerNPCNotification TargetMoved(StateId id)
        => new(NotificationKind.TargetMoved, id, false, Vector3.zero, null, FOVResult.None, false);*/

    public static NpcNotification TargetLeftArea(/*StateId id,*/ Vector3 dest)
        => new(NotificationKind.TargetLeftArea, NotifyPriority.High, false, dest, FOVResult.None, false);

    public static NpcNotification PathBlocked(/*StateId id*/)
        => new(NotificationKind.PathBlocked, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification FOVUpdate(/*StateId id,*/ FOVResult result, bool targetInShootingAngles)
        => new(NotificationKind.FOVUpdate, NotifyPriority.Normal, targetInShootingAngles, Vector3.zero, result, targetInShootingAngles);

 /*   public static OwnerNPCNotification TargetFound(StateId id)
        => new(NotificationKind.TargetFound, id, false, Vector3.zero, null, FOVResult.None, false);*/

  /*  public static NotifyOwnerNPC TargetLOSLost(StateId id)
        => new(NotificationKind.TargetLOSLost, id, false, Vector3.zero, null);

    public static NotifyOwnerNPC TargetLOSConfirmed(StateId id)
        => new(NotificationKind.TargetLOSConfirmed, id, false, Vector3.zero, null);*/

    public static NpcNotification NoAvailablePath(/*StateId id*/)
        => new(NotificationKind.NoAvailablePath, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification CoverExposed(/*StateId id*/)
        => new(NotificationKind.CoverExposed, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification PathToPrimaryAvailable(/*StateId id*/)
        => new(NotificationKind.PathToPrimaryAvailable, NotifyPriority.Normal, false, Vector3.zero, FOVResult.None, false);

}
