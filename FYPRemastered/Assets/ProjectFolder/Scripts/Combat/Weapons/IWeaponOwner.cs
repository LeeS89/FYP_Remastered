using UnityEngine;

public interface IWeaponOwner
{
    GameObject GameObject { get; }
    bool IsNPC { get; }

    void NotifyWeaponReady(bool ready);


}
