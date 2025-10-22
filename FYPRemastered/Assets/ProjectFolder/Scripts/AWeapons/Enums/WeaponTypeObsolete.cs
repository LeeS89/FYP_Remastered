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

public enum EquippableType
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

