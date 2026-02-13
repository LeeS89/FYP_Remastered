using UnityEngine;

public enum NotificationType
{
    NoCurrentState,
    TargetMoved,
    TargetLeftArea,
    PathBlocked, // Obsolete
    TargetFound, // Obsolete
    /*TargetLOSLost,
    TargetLOSConfirmed,*/
    ZoneAlert,
    FOVUpdate,
    NoAvailablePath,
    CoverExposed,
    PathToPrimaryAvailable,
    DestinationFound, // Obsolete
    DestinationSet,
    DestinationReached,
    AnimationRequest
}
