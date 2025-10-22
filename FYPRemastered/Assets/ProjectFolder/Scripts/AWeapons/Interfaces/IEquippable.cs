using UnityEngine;

public interface IEquippable
{
    void Equip(IWeaponOwner owner = null);

    void HandleOwnerCue(EquippableCue cue);

    void UnEquip();

    bool Equipped { get; }

    EquippableType EquippableType { get; }

}
