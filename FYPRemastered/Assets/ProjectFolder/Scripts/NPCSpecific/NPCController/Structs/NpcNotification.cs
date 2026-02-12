using UnityEngine;

public readonly struct NpcNotification
{
    public readonly NotificationType Kind { get; }


    public readonly bool HasReachedStaleDestination;
    public readonly Vector3 Destination { get; }
    public readonly FOVResult FOVResult;
    public readonly bool TargetWithinshootingAngles;
    public readonly NotifyPriority Priority;


    private NpcNotification(NotificationType kind, NotifyPriority priority, bool reachedStaleDestination, Vector3 dest, FOVResult result, bool targetInshootAngles)
        => (Kind, Priority, HasReachedStaleDestination, Destination, FOVResult, TargetWithinshootingAngles) = (kind, priority, reachedStaleDestination, dest, result, targetInshootAngles);

    public static NpcNotification SceneBegin()
        => new(NotificationType.NoCurrentState, NotifyPriority.Critical, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification DestinationReached(/*StateId id,*/ /*bool reachedStaleDestination*/)
        => new(NotificationType.DestinationReached, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification DestinationSet()
        => new(NotificationType.DestinationSet, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification ZoneAlert(/*StateId id*/)
        => new(NotificationType.ZoneAlert, NotifyPriority.Low, false, Vector3.zero, FOVResult.None, false);

    /* public static OwnerNPCNotification TargetMoved(StateId id)
         => new(NotificationKind.TargetMoved, id, false, Vector3.zero, null, FOVResult.None, false);*/

    public static NpcNotification TargetLeftArea(/*StateId id,*/ Vector3 dest)
        => new(NotificationType.TargetLeftArea, NotifyPriority.High, false, dest, FOVResult.None, false);

    public static NpcNotification PathBlocked(/*StateId id*/)
        => new(NotificationType.PathBlocked, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification FOVUpdate(/*StateId id,*/ FOVResult result, bool targetInShootingAngles)
        => new(NotificationType.FOVUpdate, NotifyPriority.Normal, targetInShootingAngles, Vector3.zero, result, targetInShootingAngles);

    /*   public static OwnerNPCNotification TargetFound(StateId id)
           => new(NotificationKind.TargetFound, id, false, Vector3.zero, null, FOVResult.None, false);*/

    /*  public static NotifyOwnerNPC TargetLOSLost(StateId id)
          => new(NotificationKind.TargetLOSLost, id, false, Vector3.zero, null);

      public static NotifyOwnerNPC TargetLOSConfirmed(StateId id)
          => new(NotificationKind.TargetLOSConfirmed, id, false, Vector3.zero, null);*/

    public static NpcNotification NoAvailablePath(/*StateId id*/)
        => new(NotificationType.NoAvailablePath, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification CoverExposed(/*StateId id*/)
        => new(NotificationType.CoverExposed, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);

    public static NpcNotification PathToPrimaryAvailable(/*StateId id*/)
        => new(NotificationType.PathToPrimaryAvailable, NotifyPriority.Normal, false, Vector3.zero, FOVResult.None, false);

}
