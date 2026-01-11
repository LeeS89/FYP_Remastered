using UnityEngine;


public enum WeaponTypeObsolete
{
    Ranged,
    Melee
}


public enum AmmoType
{
    None,
    Normal,
    Poison
}

public enum FireRate
{
    Single,
    SingleAutomatic,
    Burst,
    FullAutomatic
}

public enum EquippableCue
{
    Ready,
    Empty,
    ClipEmpty,
    Fire
}

//public enum 

public enum WeaponType
{
    Basic,
    Poison,
    Melee
}

public enum EquipResult
{
    Success,
    NoAvailableSlot,
    SlotOccupied,
    AlreadyEquipped,
    Failed,
    EquipIsNull
}

public enum State
{
    Patrol,
    Chase,
    Stationary,
    Flank,
    Cover,
    Death
}

public enum StateChangeResult
{
    Success,
    AlreadyInState,
    Failed
}

public enum MovementIntent
{
    Patrol,
    FollowPrimary,
    FollowSecondary,
    FindAvailableFlank,
    FindAvailableCover,
    Flee,
    SearchArea
}

public enum DynamicDestinationKind
{
    Player,
    AgentInSameZone
}

public enum DestinationKind
{
    None,
    Target,//
    FollowGroup, //
    TakeCover, //
    Flank, //
    Search,
    Patrol, //
    Flee //


    /*ChasePrimary,
    CheckPrimary,
    Group,
    Cover,
    Flank,
    Search,
    Patrol,
    Flee*/
}

public enum ReasonForDestinationCheck
{
    ProbePath,
    ValidatePathForDestination,
    Cancelled // Obsolete
}

public enum PolicyIntentResult
{
    PathBlocked,
    PathAvailable,
    TargetLOSLost,
    TargetMoved,
    PathToPrimaryBlocked,
    PathToPrimaryAvailable,
    NoAvailableSecondaryToFollow,
  //  PathToGroupAvailable,
    NoCoverAvailable,
   // CoverAvailable
}



public enum NotificationKind
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
    DestinationReached 
}

public enum PolicyHaltReason
{
    TargetMoved,
    TargetLOSLost,
    NoAvailableGroupToFollow,
    NoFlankAvailable,
    NoCoverAvailable,
    CoverExposed,
    PathBlocked,
    PathUnAvailable
}

public enum AttackTarget
{
    Primary,
    Secondary
}

public enum SanityStatus
{
    Normal,
    Confused
}

public enum ReloadPolicy
{
    InPlace,
    TakecoverThenReload
}

