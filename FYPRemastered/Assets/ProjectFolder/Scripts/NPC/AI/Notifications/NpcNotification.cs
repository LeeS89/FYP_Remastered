using Npc.API;
using UnityEngine;


namespace Npc.Internal
{
    public delegate void Notification(in NpcNotification n);

    public readonly struct NpcNotification
    {
        public readonly NotificationType Kind { get; }
        public readonly AnimationCue Clip;
        public readonly Vector3 Destination { get; }
        public readonly FOVResult FOVResult;
        public readonly NotifyPriority Priority;


        private NpcNotification(NotificationType kind, NotifyPriority priority, Vector3 dest, FOVResult result, AnimationCue clip)
            => (Kind, Priority, Destination, FOVResult, Clip) = (kind, priority, dest, result, clip);

        public static NpcNotification SceneBegin()
            => new(NotificationType.NoCurrentState, NotifyPriority.Critical, Vector3.zero, FOVResult.None, AnimationCue.None);



        public static NpcNotification TargetLeftArea(Vector3 dest)
            => new(NotificationType.TargetLeftArea, NotifyPriority.High, dest, FOVResult.None, AnimationCue.None);




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

            public static NpcNotification DestinationSet(Vector3 destination)
            => new(NotificationType.DestinationSet, NotifyPriority.High, destination, FOVResult.None, AnimationCue.None);

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





    public readonly struct PathNotificationSender : IPathNotifications
    {
        private readonly Notification send;

        public PathNotificationSender(Notification send) => this.send = send;

        public void DestinationReached()
            => send(NpcNotification.PathNotifications.DestinationReached());

        public void DestinationSet(Vector3 destination)
            => send(NpcNotification.PathNotifications.DestinationSet(destination));

        public void NoAvailablePath()
            => send(NpcNotification.PathNotifications.NoAvailablePath());

        public void PathBlocked()
            => send(NpcNotification.PathNotifications.PathBlocked());

        public void PathToTargetAvailable()
            => send(NpcNotification.PathNotifications.PathToTargetAvailable());
    }

    public readonly struct FovNotificationSender : IFovNotifications
    {
        private readonly Notification send;
        public FovNotificationSender(Notification send) => this.send = send;
        public void FovUpdate(FOVResult result)
            => send(NpcNotification.FovNotifications.FOVUpdate(result));
    }

    public readonly struct AlertNotificationSender : IAlertNotifications
    {
        private readonly Notification send;
        public AlertNotificationSender(Notification send) => this.send = send;
        public void ZoneAlert()
            => send(NpcNotification.AlertNotifications.ZoneAlert());
    }

    public readonly struct AnimationNotificationSender : IAnimationRequestNotifications
    {
        private readonly Notification send;
        public AnimationNotificationSender(Notification send) => this.send = send;
        public void RequestAnimation(AnimationCue cue)
            => send(NpcNotification.AnimationNotifications.AnimationIntent(cue));
    }
}

namespace Npc.API
{


    public interface IPathNotifications
    {
        void NoAvailablePath();
        void PathToTargetAvailable();
        void PathBlocked();
        void DestinationSet(Vector3 destination);
        void DestinationReached();
    }

    public interface IAnimationRequestNotifications
    {
        void RequestAnimation(AnimationCue cue);
        /*   void AnimationFinished(AnimationCue cue);*/
        /*   void AnimationEvent(AnimationCue cue, string eventName);*/
    }

    public interface IFovNotifications
    {
        void FovUpdate(FOVResult result);
        /*   void TargetEnteredFov(ITargetable target);
           void TargetLeftFov(ITargetable target);*/
    }
    public interface IAlertNotifications
    {
        void ZoneAlert();
        /*   void TargetEnteredFov(ITargetable target);
           void TargetLeftFov(ITargetable target);*/
    }
}

