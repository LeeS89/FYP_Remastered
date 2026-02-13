using UnityEngine;

public readonly struct NpcNotification
{
    public readonly NotificationType Kind { get; }
    public readonly AnimationCue Clip;
    public readonly Vector3 Destination { get; }
    public readonly FOVResult FOVResult;
    public readonly NotifyPriority Priority;


    private NpcNotification(NotificationType kind, NotifyPriority priority,Vector3 dest, FOVResult result, AnimationCue clip)
        => (Kind, Priority, Destination, FOVResult, Clip) = (kind, priority, dest, result, clip);

    public static NpcNotification SceneBegin()
        => new(NotificationType.NoCurrentState, NotifyPriority.Critical, Vector3.zero, FOVResult.None, AnimationCue.None);

    

    

    

    /* public static OwnerNPCNotification TargetMoved(StateId id)
         => new(NotificationKind.TargetMoved, id, false, Vector3.zero, null, FOVResult.None, false);*/

    public static NpcNotification TargetLeftArea(Vector3 dest)
        => new(NotificationType.TargetLeftArea, NotifyPriority.High, dest, FOVResult.None, AnimationCue.None);

    

    

   

   /* public static NpcNotification NoAvailablePath(*//*StateId id*//*)
        => new(NotificationType.NoAvailablePath, NotifyPriority.High, false, Vector3.zero, FOVResult.None, false);
*/
    public static NpcNotification CoverExposed()
        => new(NotificationType.CoverExposed, NotifyPriority.High, Vector3.zero, FOVResult.None, AnimationCue.None);

    

    public static class PathNotifications
    {
        public static NpcNotification NoAvailablePath()
        => new(NotificationType.NoAvailablePath, NotifyPriority.High, Vector3.zero, FOVResult.None, AnimationCue.None);

        public static NpcNotification PathToTargetAvailable()
        => new(NotificationType.PathToPrimaryAvailable, NotifyPriority.Normal, Vector3.zero, FOVResult.None, AnimationCue.None);

        public static NpcNotification PathBlocked()
        => new(NotificationType.PathBlocked, NotifyPriority.High, Vector3.zero, FOVResult.None, AnimationCue.None);

        public static NpcNotification DestinationSet()
        => new(NotificationType.DestinationSet, NotifyPriority.High, Vector3.zero, FOVResult.None, AnimationCue.None);

        public static NpcNotification DestinationReached(/*bool reachedStaleDestination*/)
        => new(NotificationType.DestinationReached, NotifyPriority.High, Vector3.zero, FOVResult.None, AnimationCue.None);
    }

    public static class FovNotifications
    {
        public static NpcNotification FOVUpdate(FOVResult result)
        => new(NotificationType.FOVUpdate, NotifyPriority.Normal, Vector3.zero, result, AnimationCue.None);
    }

    public static class AlertNotifications
    {
        public static NpcNotification ZoneAlert()
        => new(NotificationType.ZoneAlert, NotifyPriority.Low, Vector3.zero, FOVResult.None, AnimationCue.None);
    }

    public static class AnimationNotifications
    {
        public static NpcNotification AnimationIntent(AnimationCue cue)
        => new(NotificationType.AnimationRequest, NotifyPriority.Low, Vector3.zero, FOVResult.None, cue);
    }
}
