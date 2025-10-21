using UnityEngine;

public interface IEquippable
{
    void Equip(IWeaponOwner owner = null);

    void UnEquip();

    bool Equipped { get; }

}
