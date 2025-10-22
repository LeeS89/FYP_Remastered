using UnityEngine;

public interface IWeaponOwner
{
    GameObject GameObject { get; }
  //  bool IsNPC { get; }

   // void NotifyWeaponReady(bool ready);
    void OnEquippableCue(EquippableCue signal, IEquippable equippable);

    bool HasUnlimitedAmmo { get; }

}
