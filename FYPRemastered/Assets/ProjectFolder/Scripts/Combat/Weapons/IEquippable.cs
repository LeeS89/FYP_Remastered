using UnityEngine;

public interface IEquippable
{
    void Equip(EventManager eventManager, IWeaponOwner owner);

    void UnEquip();
}
