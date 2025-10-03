using UnityEngine;

public interface IEquippable
{
    void Equip(EventManager eventManager, GameObject owner);

    void UnEquip();
}
