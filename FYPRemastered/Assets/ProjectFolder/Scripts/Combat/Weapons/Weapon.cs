using System;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    protected EventManager _eventManager;
    protected GameObject _owner;
    public bool Equipped { get; protected set; } = false;

    public virtual void Equip(EventManager eventManager, GameObject owner)
    {
        _eventManager = eventManager;
        _owner = owner;
        Equipped = true;
    }
    
    public virtual void UnEquip()
    {
        Equipped = false;
        _eventManager = null;
        _owner = null;
    }

   
}
