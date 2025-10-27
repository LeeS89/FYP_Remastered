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
    TakeCover,
    Flee,
    SearchArea
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

