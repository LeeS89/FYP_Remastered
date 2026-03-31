using UnityEngine;

public enum NotificationType
{
    NoCurrentState,
    TargetMoved,
    TargetLeftArea,
    PathBlocked, // Obsolete
    ZoneAlert,
    FOVUpdate,
    NoAvailablePath,
    CoverExposed,
    PathToPrimaryAvailable,
    DestinationSet,
    DestinationReached,
    AnimationRequest
}
